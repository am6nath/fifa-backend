using fifa_backend.Data;
using fifa_backend.Models;
using fifa_backend.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace fifa_backend.Extensions;

public static class DatabaseExtensions
{
    public static async Task SeedDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Run migrations automatically
        await context.Database.MigrateAsync();

        // Seed Admin user if they do not exist
        const string adminEmail = "admin@fifavote.com";
        var adminExists = await context.Users.AnyAsync(u => u.Email == adminEmail);

        if (!adminExists)
        {
            var adminUser = new User
            {
                UserName = "admin",
                Email = adminEmail,
                Role = UserRole.Admin,
                EmailVerified = true,
                IsActive = true
            };

            var hasher = new PasswordHasher<User>();
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin@123!");

            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
            Console.WriteLine("Successfully seeded default Admin user.");
        }

        // Seed 32 FIFA World Cup Teams if they do not exist
        if (!await context.Teams.AnyAsync())
        {
            var teams = new List<Team>
            {
                // Group A
                new() { Name = "Qatar", CountryCode = "QAT", FlagUrl = "https://flags.com/qat.png", GroupName = "Group A", CoachName = "Félix Sánchez", Description = "The Maroon", Region = "Asia", IsActive = true },
                new() { Name = "Ecuador", CountryCode = "ECU", FlagUrl = "https://flags.com/ecu.png", GroupName = "Group A", CoachName = "Gustavo Alfaro", Description = "La Tri", Region = "South America", IsActive = true },
                new() { Name = "Senegal", CountryCode = "SEN", FlagUrl = "https://flags.com/sen.png", GroupName = "Group A", CoachName = "Aliou Cissé", Description = "Lions of Teranga", Region = "Africa", IsActive = true },
                new() { Name = "Netherlands", CountryCode = "NED", FlagUrl = "https://flags.com/ned.png", GroupName = "Group A", CoachName = "Louis van Gaal", Description = "Oranje", Region = "Europe", IsActive = true },

                // Group B
                new() { Name = "England", CountryCode = "ENG", FlagUrl = "https://flags.com/eng.png", GroupName = "Group B", CoachName = "Gareth Southgate", Description = "Three Lions", Region = "Europe", IsActive = true },
                new() { Name = "Iran", CountryCode = "IRN", FlagUrl = "https://flags.com/irn.png", GroupName = "Group B", CoachName = "Dragan Skočić", Description = "Team Melli", Region = "Asia", IsActive = true },
                new() { Name = "USA", CountryCode = "USA", FlagUrl = "https://flags.com/usa.png", GroupName = "Group B", CoachName = "Gregg Berhalter", Description = "The Stars & Stripes", Region = "North America", IsActive = true },
                new() { Name = "Wales", CountryCode = "WAL", FlagUrl = "https://flags.com/wal.png", GroupName = "Group B", CoachName = "Rob Page", Description = "The Dragons", Region = "Europe", IsActive = true },

                // Group C
                new() { Name = "Argentina", CountryCode = "ARG", FlagUrl = "https://flags.com/arg.png", GroupName = "Group C", CoachName = "Lionel Scaloni", Description = "La Albiceleste", Region = "South America", IsActive = true },
                new() { Name = "Saudi Arabia", CountryCode = "KSA", FlagUrl = "https://flags.com/ksa.png", GroupName = "Group C", CoachName = "Hervé Renard", Description = "Green Falcons", Region = "Asia", IsActive = true },
                new() { Name = "Mexico", CountryCode = "MEX", FlagUrl = "https://flags.com/mex.png", GroupName = "Group C", CoachName = "Gerardo Martino", Description = "El Tri", Region = "North America", IsActive = true },
                new() { Name = "Poland", CountryCode = "POL", FlagUrl = "https://flags.com/pol.png", GroupName = "Group C", CoachName = "Czesław Michniewicz", Description = "Biało-czerwoni", Region = "Europe", IsActive = true },

                // Group D
                new() { Name = "France", CountryCode = "FRA", FlagUrl = "https://flags.com/fra.png", GroupName = "Group D", CoachName = "Didier Deschamps", Description = "Les Bleus", Region = "Europe", IsActive = true },
                new() { Name = "Australia", CountryCode = "AUS", FlagUrl = "https://flags.com/aus.png", GroupName = "Group D", CoachName = "Graham Arnold", Description = "Socceroos", Region = "Asia", IsActive = true },
                new() { Name = "Denmark", CountryCode = "DEN", FlagUrl = "https://flags.com/den.png", GroupName = "Group D", CoachName = "Kasper Hjulmand", Description = "De Rød-Hvide", Region = "Europe", IsActive = true },
                new() { Name = "Tunisia", CountryCode = "TUN", FlagUrl = "https://flags.com/tun.png", GroupName = "Group D", CoachName = "Jalel Kadri", Description = "Eagles of Carthage", Region = "Africa", IsActive = true },

                // Group E
                new() { Name = "Spain", CountryCode = "ESP", FlagUrl = "https://flags.com/esp.png", GroupName = "Group E", CoachName = "Luis Enrique", Description = "La Roja", Region = "Europe", IsActive = true },
                new() { Name = "Costa Rica", CountryCode = "CRC", FlagUrl = "https://flags.com/crc.png", GroupName = "Group E", CoachName = "Luis Fernando Suárez", Description = "Los Ticos", Region = "North America", IsActive = true },
                new() { Name = "Germany", CountryCode = "GER", FlagUrl = "https://flags.com/ger.png", GroupName = "Group E", CoachName = "Hansi Flick", Description = "Die Mannschaft", Region = "Europe", IsActive = true },
                new() { Name = "Japan", CountryCode = "JPN", FlagUrl = "https://flags.com/jpn.png", GroupName = "Group E", CoachName = "Hajime Moriyasu", Description = "Samurai Blue", Region = "Asia", IsActive = true },

                // Group F
                new() { Name = "Belgium", CountryCode = "BEL", FlagUrl = "https://flags.com/bel.png", GroupName = "Group F", CoachName = "Roberto Martínez", Description = "Red Devils", Region = "Europe", IsActive = true },
                new() { Name = "Canada", CountryCode = "CAN", FlagUrl = "https://flags.com/can.png", GroupName = "Group F", CoachName = "John Herdman", Description = "Les Rouges", Region = "North America", IsActive = true },
                new() { Name = "Morocco", CountryCode = "MAR", FlagUrl = "https://flags.com/mar.png", GroupName = "Group F", CoachName = "Walid Regragui", Description = "Atlas Lions", Region = "Africa", IsActive = true },
                new() { Name = "Croatia", CountryCode = "CRO", FlagUrl = "https://flags.com/cro.png", GroupName = "Group F", CoachName = "Zlatko Dalić", Description = "Vatreni", Region = "Europe", IsActive = true },

                // Group G
                new() { Name = "Brazil", CountryCode = "BRA", FlagUrl = "https://flags.com/bra.png", GroupName = "Group G", CoachName = "Tite", Description = "Seleção", Region = "South America", IsActive = true },
                new() { Name = "Serbia", CountryCode = "SRB", FlagUrl = "https://flags.com/srb.png", GroupName = "Group G", CoachName = "Dragan Stojković", Description = "Orlovi", Region = "Europe", IsActive = true },
                new() { Name = "Switzerland", CountryCode = "SUI", FlagUrl = "https://flags.com/sui.png", GroupName = "Group G", CoachName = "Murat Yakin", Description = "Nati", Region = "Europe", IsActive = true },
                new() { Name = "Cameroon", CountryCode = "CMR", FlagUrl = "https://flags.com/cmr.png", GroupName = "Group G", CoachName = "Rigobert Song", Description = "Indomitable Lions", Region = "Africa", IsActive = true },

                // Group H
                new() { Name = "Portugal", CountryCode = "POR", FlagUrl = "https://flags.com/por.png", GroupName = "Group H", CoachName = "Fernando Santos", Description = "A Seleção", Region = "Europe", IsActive = true },
                new() { Name = "Ghana", CountryCode = "GHA", FlagUrl = "https://flags.com/gha.png", GroupName = "Group H", CoachName = "Otto Addo", Description = "Black Stars", Region = "Africa", IsActive = true },
                new() { Name = "Uruguay", CountryCode = "URU", FlagUrl = "https://flags.com/uru.png", GroupName = "Group H", CoachName = "Diego Alonso", Description = "La Celeste", Region = "South America", IsActive = true },
                new() { Name = "South Korea", CountryCode = "KOR", FlagUrl = "https://flags.com/kor.png", GroupName = "Group H", CoachName = "Paulo Bento", Description = "Taegeuk Warriors", Region = "Asia", IsActive = true }
            };

            context.Teams.AddRange(teams);
            await context.SaveChangesAsync();
            Console.WriteLine("Successfully seeded 32 FIFA World Cup teams.");
        }

        // Seed 2 active and 1 upcoming voting sessions if they do not exist
        if (!await context.VotingSessions.AnyAsync())
        {
            var allTeams = await context.Teams.ToListAsync();
            
            // Session 1: World Cup Group A Winner (Active)
            var groupATeams = allTeams.Where(t => t.GroupName == "Group A").ToList();
            var session1 = new VotingSession
            {
                Title = "Group A Winner Prediction",
                VotingStartAt = DateTime.UtcNow.AddDays(-1),
                VotingEndAt = DateTime.UtcNow.AddDays(7),
                IsVotingClosedManually = false,
                ResultsPublished = false,
                Notes = "Predict which team from Group A will advance as the group winner.",
                RegionFilter = null,
                WinnersCount = 1,
                Teams = groupATeams
            };

            // Session 2: South American Giant Clash (Active, Region restricted)
            var conmebolTeams = allTeams.Where(t => t.Region == "South America").ToList();
            var session2 = new VotingSession
            {
                Title = "CONMEBOL Top Contender",
                VotingStartAt = DateTime.UtcNow.AddDays(-2),
                VotingEndAt = DateTime.UtcNow.AddDays(5),
                IsVotingClosedManually = false,
                ResultsPublished = false,
                Notes = "Which South American powerhouse looks strongest ahead of the tournament?",
                RegionFilter = "South America",
                WinnersCount = 2,
                Teams = conmebolTeams
            };

            // Session 3: Group B Stage Match Day 1 (Upcoming)
            var session3 = new VotingSession
            {
                Title = "Group B Stage Match Day 1",
                VotingStartAt = DateTime.UtcNow.AddDays(2),
                VotingEndAt = DateTime.UtcNow.AddDays(9),
                IsVotingClosedManually = false,
                ResultsPublished = false,
                Notes = "Cast your stage prediction for England vs USA.",
                RegionFilter = null,
                WinnersCount = 1,
                Teams = allTeams.Where(t => t.GroupName == "Group B").ToList()
            };

            context.VotingSessions.AddRange(session1, session2, session3);
            await context.SaveChangesAsync();
            Console.WriteLine("Successfully seeded sample voting sessions.");
        }
    }
}
