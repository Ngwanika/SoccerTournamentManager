using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SoccerTournament.Models;

namespace SoccerTournament.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {


        }

        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<Player> Players { get; set; }

        public DbSet<TournamentTeam> TournamentTeams { get; set; }

        public DbSet<GroupTeam> GroupTeams { get; set; }
        public DbSet<Group> Groups { get; set; }

        public DbSet<TournamentFormat> TournamentFormat { get; set; }

        public DbSet<Match> Matches { get; set; }

        public DbSet<TournamentRegistration> TournamentRegistrations { get; set; }

        public DbSet<Goal> Goals { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        //public DbSet<Match> Matches { get; set; }
        //public DbSet<MatchEvent> MatchEvents { get; set; }
        //public DbSet<Standing> Standings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<Match>()
            //  .HasOne(m => m.HomeTeam)
            //  .WithMany()
            //  .HasForeignKey(m => m.HomeTeamId)
            //  .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<Match>()
            //    .HasOne(m => m.AwayTeam)
            //    .WithMany()
            //    .HasForeignKey(m => m.AwayTeamId)
            //    .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Team>().HasData(
               new Team { TeamId = 1, TeamName = "Red Warriors", Abbreviation = "RWS", CoachName = "John Smith", LogoUrl = "https://example.com/logos/red_warriors.png", Homestadium = "Dube", ContactNumber1 = "0811111111", Established = DateTime.Parse("2020-06-09"), Email = "test@email.com" },
               new Team { TeamId = 2, TeamName = "Blue Knights", Abbreviation = "BKs", CoachName = "Emily Johnson", LogoUrl = "https://example.com/logos/blue_knights.png", Homestadium = "Zola 1", ContactNumber1 = "0822222222", Established = DateTime.Parse("2020-06-09"), Email = "test@email.com" },
               new Team { TeamId = 3, TeamName = "Green Giants", Abbreviation = "GG", CoachName = "Michael Brown", LogoUrl = "https://example.com/logos/green_giants.png", Homestadium = "Zone 10 Meadolwands", ContactNumber1 = "0833333333", Established = DateTime.Parse("2020-06-09"), Email = "test@email.com" },
               new Team { TeamId = 4, TeamName = "Yellow Tigers", Abbreviation = "YT", CoachName = "Sarah Davis", LogoUrl = "https://example.com/logos/yellow_tigers.png", Homestadium = "Grren Village", ContactNumber1 = "0844444444", Established = DateTime.Parse("2020-06-09"), Email = "test@email.com" }
            );
            modelBuilder.Entity<Player>().HasData(
              new Player { PlayerId = 1, FirstName = "David", LastName = "Miller", JerseyNumber = 10, Position = "Forward", NationalIdNumber = "1234567890", DateOfBirth = DateTime.Parse("1995-05-15"), PlayerPhotoUrl = "https://example.com/players/david_miller.png", TeamId = 1 },
              new Player { PlayerId = 2, FirstName = "Emily", LastName = "Johnson", JerseyNumber = 8, Position = "Midfielder", NationalIdNumber = "0987654321", DateOfBirth = DateTime.Parse("1997-08-20"), PlayerPhotoUrl = "https://example.com/players/emily_johnson.png", TeamId = 2 },
              new Player { PlayerId = 3, FirstName = "Michael", LastName = "Brown", JerseyNumber = 5, Position = "Defender", NationalIdNumber = "1122334455", DateOfBirth = DateTime.Parse("1994-12-10"), PlayerPhotoUrl = "https://example.com/players/michael_brown.png", TeamId = 3 },
              new Player { PlayerId = 4, FirstName = "Sarah", LastName = "Davis", JerseyNumber = 1, Position = "Goalkeeper", NationalIdNumber = "5566778899", DateOfBirth = DateTime.Parse("1996-03-25"), PlayerPhotoUrl = "https://example.com/players/sarah_davis.png", TeamId = 4 },
              new Player { PlayerId = 5, FirstName = "John", LastName = "Smith", JerseyNumber = 9, Position = "Forward", NationalIdNumber = "1234567890", DateOfBirth = DateTime.Parse("1995-05-15"), PlayerPhotoUrl = "https://example.com/players/john_smith.png", TeamId = 1 },
              new Player { PlayerId = 6, FirstName = "Anna", LastName = "Taylor", JerseyNumber = 7, Position = "Midfielder", NationalIdNumber = "0987654321", DateOfBirth = DateTime.Parse("1997-08-20"), PlayerPhotoUrl = "https://example.com/players/anna_taylor.png", TeamId = 2 },
              new Player { PlayerId = 7, FirstName = "James", LastName = "Wilson", JerseyNumber = 4, Position = "Defender", NationalIdNumber = "1122334455", DateOfBirth = DateTime.Parse("1994-12-10"), PlayerPhotoUrl = "https://example.com/players/james_wilson.png", TeamId = 3 },
              new Player { PlayerId = 8, FirstName = "Olivia", LastName = "Martinez", JerseyNumber = 2, Position = "Goalkeeper", NationalIdNumber = "5566778899", DateOfBirth = DateTime.Parse("1996-03-25"), PlayerPhotoUrl = "https://example.com/players/olivia_martinez.png", TeamId = 4 },
              new Player { PlayerId = 9, FirstName = "Chris", LastName = "Anderson", JerseyNumber = 11, Position = "Forward", NationalIdNumber = "1234567890", DateOfBirth = DateTime.Parse("1995-05-15"), PlayerPhotoUrl = "https://example.com/players/chris_anderson.png", TeamId = 1 },
              new Player { PlayerId = 10, FirstName = "Sophia", LastName = "Garcia", JerseyNumber = 6, Position = "Midfielder", NationalIdNumber = "0987654321", DateOfBirth = DateTime.Parse("1997-08-20"), PlayerPhotoUrl = "https://example.com/players/sophia_garcia.png", TeamId = 2 }
              );

            modelBuilder.Entity<Tournament>().HasData(
               new Tournament { TournamentId = 1, Name = "Champions Summer Cup", Season = "", Format = "League", Status = "Live", StartDate = new DateTime(2026, 08, 15), Location = "Main Stadium" },
               new Tournament { TournamentId = 2, Name = "Gauteng Amateur League", Season = "", Status = "Upcoming", Format = "Knockout", StartDate = new DateTime(2026, 09, 01), Location = "Sandton Field" },
               new Tournament { TournamentId = 3, Name = "Midnight Pro Series", Season = "", Status = "Registration Open", Format = "Group Stage", StartDate = new DateTime(2026, 10, 12), Location = "The Arena" },
               new Tournament { TournamentId = 4, Name = "Annual spring", Season = "", Format = "League", Status = "Live", StartDate = new DateTime(2026, 08, 15), Location = "Main Stadium" },
               new Tournament { TournamentId = 5, Name = "featuring top regional teams", Season = "", Status = "Upcoming", Format = "Knockout", StartDate = new DateTime(2026, 09, 01), Location = "Sandton Field" },
               new Tournament { TournamentId = 6, Name = "competing", Season = "", Status = "Registration Open", Format = "Group Stage", StartDate = new DateTime(2026, 10, 12), Location = "The Arena" }


            );

            modelBuilder.Entity<TournamentFormat>().HasData(
                new TournamentFormat { TournamentFormatId = 1, FormatName = "League", Description = "Teams play each other in a round-robin format. Points are awarded for wins and draws, and the team with the most points at the end of the season wins." },
                new TournamentFormat { TournamentFormatId = 2, FormatName = "Knockout", Description = "Teams are paired in a single-elimination format. The winner of each match advances to the next round, while the loser is eliminated from the tournament." },
                new TournamentFormat { TournamentFormatId = 3, FormatName = "Group Stage", Description = "Teams are divided into groups, and each team plays against the others in their group. The top teams from each group advance to the knockout stage." },
                new TournamentFormat { TournamentFormatId = 4, FormatName = "Single Elimination", Description = "Teams play each other in a round-robin format. Points are awarded for wins and draws, and the team with the most points at the end of the season wins." },
                new TournamentFormat { TournamentFormatId = 5, FormatName = "Double Elimination", Description = "Teams are paired in a single-elimination format. The winner of each match advances to the next round, while the loser is eliminated from the tournament." },
                new TournamentFormat { TournamentFormatId = 6, FormatName = "Round Robin", Description = "Teams are divided into groups, and each team plays against the others in their group. The top teams from each group advance to the knockout stage." }

                );
        }
    }

}
