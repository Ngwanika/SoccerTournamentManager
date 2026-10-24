using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoccerTournament.DataAccess.Data;
using SoccerTournament.Models;
using SoccerTournament.Models.ViewModels;
using SoccerTournament.Utility;
using System.Data;

namespace SoccerTournamentManager.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UsersController : Controller
    {
        private static readonly string[] AssignableRoles =
        [
            SD.Role_Admin,
            SD.Role_Customer,
            SD.Role_MatchOfficial,
            SD.Role_TeamManager,
            SD.Role_TournamentManager
        ];

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext dbContext,
            ILogger<UsersController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var teams = await _dbContext.Teams
                .AsNoTracking()
                .OrderBy(team => team.TeamName)
                .ToListAsync();
            var users = await _userManager.Users
                .AsNoTracking()
                .OrderBy(user => user.Email)
                .ToListAsync();

            var viewModels = new List<AdminUserRoleViewModel>(users.Count);
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var selectedRole = GetSelectedRole(roles);

                viewModels.Add(new AdminUserRoleViewModel
                {
                    UserId = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    CurrentRoles = roles.Count == 0 ? "None" : string.Join(", ", roles),
                    SelectedRole = selectedRole,
                    TeamId = user.TeamId,
                    Roles = AssignableRoles,
                    Teams = teams.Select(team => new AdminTeamOption
                    {
                        TeamId = team.TeamId,
                        TeamName = team.TeamName
                    }).ToList()
                });
            }

            return View(viewModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(string userId, string role, int? teamId)
        {
            if (string.IsNullOrWhiteSpace(userId) || !AssignableRoles.Contains(role, StringComparer.Ordinal))
            {
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            if (teamId.HasValue && !await _dbContext.Teams.AnyAsync(team => team.TeamId == teamId.Value))
            {
                ModelState.AddModelError(nameof(teamId), "Select an existing team.");
                TempData["error"] = "The selected team could not be found.";
                return RedirectToAction(nameof(Index));
            }

            if (role == SD.Role_TeamManager && !teamId.HasValue)
            {
                TempData["error"] = "Assign a team before assigning the Team Manager role.";
                return RedirectToAction(nameof(Index));
            }

            if (!await _roleManager.RoleExistsAsync(role))
            {
                _logger.LogError("Cannot assign missing identity role {Role} to user {UserId}.", role, user.Id);
                return Problem("The selected role is not configured.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (user.Id == _userManager.GetUserId(User)
                && currentRoles.Contains(SD.Role_Admin)
                && role != SD.Role_Admin)
            {
                await transaction.RollbackAsync();
                TempData["error"] = "You cannot remove your own Admin role.";
                return RedirectToAction(nameof(Index));
            }

            if (currentRoles.Contains(SD.Role_Admin) && role != SD.Role_Admin)
            {
                var administrators = await _userManager.GetUsersInRoleAsync(SD.Role_Admin);
                if (administrators.Count <= 1)
                {
                    await transaction.RollbackAsync();
                    TempData["error"] = "The last Admin account cannot be demoted.";
                    return RedirectToAction(nameof(Index));
                }
            }

            var rolesToRemove = currentRoles
                .Where(currentRole => !string.Equals(currentRole, role, StringComparison.Ordinal))
                .ToArray();
            if (rolesToRemove.Length > 0)
            {
                var removal = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removal.Succeeded)
                {
                    await transaction.RollbackAsync();
                    LogIdentityErrors("removing roles", user.Id, removal.Errors);
                    TempData["error"] = "The user's role could not be updated.";
                    return RedirectToAction(nameof(Index));
                }
            }

            if (!currentRoles.Contains(role, StringComparer.Ordinal))
            {
                var addition = await _userManager.AddToRoleAsync(user, role);
                if (!addition.Succeeded)
                {
                    await transaction.RollbackAsync();
                    LogIdentityErrors("assigning a role", user.Id, addition.Errors);
                    TempData["error"] = "The user's role could not be updated.";
                    return RedirectToAction(nameof(Index));
                }
            }

            user.TeamId = teamId;
            var update = await _userManager.UpdateAsync(user);
            if (!update.Succeeded)
            {
                await transaction.RollbackAsync();
                LogIdentityErrors("updating a team assignment", user.Id, update.Errors);
                TempData["error"] = "The user's team assignment could not be updated.";
                return RedirectToAction(nameof(Index));
            }

            await transaction.CommitAsync();
            TempData["success"] = "User access updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        private static string GetSelectedRole(IList<string> roles)
        {
            if (roles.Contains(SD.Role_TeamManagerLegacy, StringComparer.Ordinal))
            {
                return SD.Role_TeamManager;
            }

            return AssignableRoles.FirstOrDefault(role => roles.Contains(role, StringComparer.Ordinal))
                ?? SD.Role_Customer;
        }

        private void LogIdentityErrors(string operation, string userId, IEnumerable<IdentityError> errors)
        {
            foreach (var error in errors)
            {
                _logger.LogError(
                    "Identity failed while {Operation} for user {UserId}: {Code} {Description}",
                    operation,
                    userId,
                    error.Code,
                    error.Description);
            }
        }
    }
}
