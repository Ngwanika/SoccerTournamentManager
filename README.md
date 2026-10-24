# SoccerTournamentManager

PitchPerfect is an ASP.NET Core MVC application for browsing and managing soccer tournaments, teams, players, fixtures, and results.

## Requirements

- .NET 9 SDK
- SQL Server (LocalDB is suitable for local development)

## Configure the database

The web application reads the `ConnectionStrings:DefaultConnection` setting and uses SQL Server. Keep credentials out of tracked configuration files. For local development, configure it with user secrets:

```powershell
dotnet user-secrets init --project .\SoccerTournamentManager
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\MSSQLLocalDB;Database=SoccerTournamentManager;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True" --project .\SoccerTournamentManager
```

For a deployed server, set `ConnectionStrings__DefaultConnection` through the host's secret/environment configuration instead.

## Create the initial administrator

Public registration creates Customer accounts only. Configure an initial administrator using secrets before the first run; the account is created and assigned the Admin role only when no administrator exists:

```powershell
dotnet user-secrets set "BootstrapAdmin:Email" "admin@example.com" --project .\SoccerTournamentManager
dotnet user-secrets set "BootstrapAdmin:Password" "<a-strong-unique-password>" --project .\SoccerTournamentManager
```

For production, provide `BootstrapAdmin__Email` and `BootstrapAdmin__Password` through the deployment secret store. Remove those bootstrap settings after the first administrator has been provisioned. Administrators can then assign user roles and team access from **Admin → Users**. Team Manager accounts must have a team assigned.

## Restore, migrate, and run

From the repository root:

```powershell
dotnet restore .\SoccerTournamentManager.sln
dotnet ef database update --project .\SoccerTournament.DataAccess --startup-project .\SoccerTournamentManager
dotnet run --project .\SoccerTournamentManager
```

Open the HTTPS address printed by `dotnet run`. Visual Studio users can select the `SoccerTournamentManager` project and use its `https` launch profile.

## Tournament operations

Administrators and Tournament Managers can manage tournament registrations, add or remove teams before fixtures exist, arrange a seed order or save a random draw, generate format-specific fixtures, assign match officials, and correct recorded results. The public tournament schedule links to match details and brackets. League standings and group tables are calculated from completed results; group-stage qualifiers enter a knockout bracket after all group matches are complete.

Assigned Match Officials can use **Match workspace** to start a match, record goals and match events, and finish play. Match changes are stored in an audit history. Statistics are shown only when events have been recorded; existing matches without event history are marked unavailable rather than assigned guessed values.

Team Managers can edit player email addresses, send an email to one or all roster members with an address on file, save a WhatsApp invite link, and send authenticated messages to the Admin inbox. The application stores the WhatsApp invite URL; creating and administering the external WhatsApp group remains with the team manager.

## Configure email delivery

Set SMTP configuration in user secrets for development and in the deployment's secret/environment configuration for production. For example:

```powershell
dotnet user-secrets set "Email:Smtp:Host" "smtp.example.com" --project .\SoccerTournamentManager
dotnet user-secrets set "Email:Smtp:Port" "587" --project .\SoccerTournamentManager
dotnet user-secrets set "Email:Smtp:EnableSsl" "true" --project .\SoccerTournamentManager
dotnet user-secrets set "Email:Smtp:Username" "smtp-user" --project .\SoccerTournamentManager
dotnet user-secrets set "Email:Smtp:Password" "<smtp-password>" --project .\SoccerTournamentManager
dotnet user-secrets set "Email:From" "noreply@example.com" --project .\SoccerTournamentManager
```

For production, use `Email__Smtp__Host`, `Email__Smtp__Port`, `Email__Smtp__EnableSsl`, `Email__Smtp__Username`, `Email__Smtp__Password`, and `Email__From` environment variables. Use credentials supplied by your SMTP provider; never commit them to `appsettings.json`.

## Production diagnostics

The application writes ASP.NET Core logs to standard output through the built-in console logger. Unhandled production exceptions are logged by the exception-handler middleware and users are shown a request ID on the error page. Configure the hosting provider to retain application stdout/stderr logs and use the request ID when correlating a reported error with server logs. Never log passwords, tokens, or connection strings.
