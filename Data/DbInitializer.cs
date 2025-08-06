using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using TSG_Commex_BE.Models;
using TSG_Commex_BE.Models.Domain;
using TSG_Commex_BE.Models.Enums;

namespace TSG_Commex_BE.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Ensure database is created
            context.Database.EnsureCreated();

            // Check if data already exists
            if (context.GlobalStatuses.Any())
            {
                // Check if we need to seed CommunicationTypeStatuses
                if (!context.CommunicationTypeStatuses.Any())
                {
                    SeedCommunicationTypeStatuses(context);
                }
                return; // Core data has been seeded
            }

            // Seed Users first
            var users = new User[]
            {
                new User { Email = "admin@zelis.com", FirstName = "Admin", LastName = "User", Role = "Admin", IsActive = true, LastLoginUtc = DateTime.UtcNow.AddDays(-1) },
                new User { Email = "john.doe@zelis.com", FirstName = "John", LastName = "Doe", Role = "User", IsActive = true, LastLoginUtc = DateTime.UtcNow.AddDays(-2) },
                new User { Email = "jane.smith@zelis.com", FirstName = "Jane", LastName = "Smith", Role = "User", IsActive = true, LastLoginUtc = DateTime.UtcNow.AddHours(-5) },
                new User { Email = "cs.rep@zelis.com", FirstName = "Customer", LastName = "Service", Role = "User", IsActive = true, LastLoginUtc = DateTime.UtcNow.AddHours(-3) }
            };

            foreach (var user in users)
            {
                context.Users.Add(user);
            }
            context.SaveChanges();

            // Seed Global Statuses with phases (organized categories)
            var globalStatuses = new GlobalStatus[]
            {
                // Creation Phase
                new GlobalStatus { StatusCode = "Created", DisplayName = "Created", Description = "Initial creation", Phase = StatusPhase.Creation, IsActive = true },
                new GlobalStatus { StatusCode = "ReadyForRelease", DisplayName = "Ready for Release", Description = "Ready to be released", Phase = StatusPhase.Creation, IsActive = true },
                new GlobalStatus { StatusCode = "Released", DisplayName = "Released", Description = "Released to production", Phase = StatusPhase.Creation, IsActive = true },
                
                // Production Phase
                new GlobalStatus { StatusCode = "QueuedForPrinting", DisplayName = "Queued for Printing", Description = "In print queue", Phase = StatusPhase.Production, IsActive = true },
                new GlobalStatus { StatusCode = "Printed", DisplayName = "Printed", Description = "Document printed", Phase = StatusPhase.Production, IsActive = true },
                new GlobalStatus { StatusCode = "Inserted", DisplayName = "Inserted", Description = "Inserted into envelope", Phase = StatusPhase.Production, IsActive = true },
                new GlobalStatus { StatusCode = "WarehouseReady", DisplayName = "Warehouse Ready", Description = "Ready at warehouse", Phase = StatusPhase.Production, IsActive = true },
                
                // Logistics Phase
                new GlobalStatus { StatusCode = "Shipped", DisplayName = "Shipped", Description = "Shipped to member", Phase = StatusPhase.Logistics, IsActive = true },
                new GlobalStatus { StatusCode = "InTransit", DisplayName = "In Transit", Description = "In transit to member", Phase = StatusPhase.Logistics, IsActive = true },
                new GlobalStatus { StatusCode = "Delivered", DisplayName = "Delivered", Description = "Delivered to member", Phase = StatusPhase.Logistics, IsActive = true },
                new GlobalStatus { StatusCode = "Returned", DisplayName = "Returned", Description = "Returned to sender", Phase = StatusPhase.Logistics, IsActive = true },
                
                // Universal/Error Phase
                new GlobalStatus { StatusCode = "Failed", DisplayName = "Failed", Description = "Processing failed", Phase = StatusPhase.Terminal, IsActive = true },
                new GlobalStatus { StatusCode = "Cancelled", DisplayName = "Cancelled", Description = "Process cancelled", Phase = StatusPhase.Terminal, IsActive = true },
                new GlobalStatus { StatusCode = "OnHold", DisplayName = "On Hold", Description = "Temporarily paused", Phase = StatusPhase.Terminal, IsActive = true }
            };

            foreach (var status in globalStatuses)
            {
                context.GlobalStatuses.Add(status);
            }
            context.SaveChanges();

            // Seed Communication Types
            var communicationTypes = new CommunicationType[]
            {
                new CommunicationType { TypeCode = "EOB", DisplayName = "Explanation of Benefits", Description = "Medical benefits explanation", IsActive = true },
                new CommunicationType { TypeCode = "EOP", DisplayName = "Explanation of Payments", Description = "Payment explanation document", IsActive = true },
                new CommunicationType { TypeCode = "ID_CARD", DisplayName = "ID Card", Description = "Member identification card", IsActive = true }
            };

            foreach (var type in communicationTypes)
            {
                context.CommunicationTypes.Add(type);
            }
            context.SaveChanges();

            // Get the IDs for foreign keys
            var eobType = context.CommunicationTypes.First(ct => ct.TypeCode == "EOB");
            var eopType = context.CommunicationTypes.First(ct => ct.TypeCode == "EOP");
            var idCardType = context.CommunicationTypes.First(ct => ct.TypeCode == "ID_CARD");

            var createdStatus = context.GlobalStatuses.First(gs => gs.StatusCode == "Created");
            var readyForReleaseStatus = context.GlobalStatuses.First(gs => gs.StatusCode == "ReadyForRelease");
            var printedStatus = context.GlobalStatuses.First(gs => gs.StatusCode == "Printed");
            var shippedStatus = context.GlobalStatuses.First(gs => gs.StatusCode == "Shipped");
            var deliveredStatus = context.GlobalStatuses.First(gs => gs.StatusCode == "Delivered");
            var failedStatus = context.GlobalStatuses.First(gs => gs.StatusCode == "Failed");

            // Seed CommunicationTypeStatus - Admin-configured valid statuses per type
            var communicationTypeStatuses = new CommunicationTypeStatus[]
            {
                // EOB statuses (complex document workflow)
                new CommunicationTypeStatus { CommunicationTypeId = eobType.Id, GlobalStatusId = createdStatus.Id, IsActive = true, Description = "EOB document created" },
                new CommunicationTypeStatus { CommunicationTypeId = eobType.Id, GlobalStatusId = readyForReleaseStatus.Id, IsActive = true, Description = "EOB ready for release" },
                new CommunicationTypeStatus { CommunicationTypeId = eobType.Id, GlobalStatusId = printedStatus.Id, IsActive = true, Description = "EOB printed by vendor" },
                new CommunicationTypeStatus { CommunicationTypeId = eobType.Id, GlobalStatusId = shippedStatus.Id, IsActive = true, Description = "EOB shipped to member" },
                new CommunicationTypeStatus { CommunicationTypeId = eobType.Id, GlobalStatusId = deliveredStatus.Id, IsActive = true, Description = "EOB delivered to member" },
                new CommunicationTypeStatus { CommunicationTypeId = eobType.Id, GlobalStatusId = failedStatus.Id, IsActive = true, Description = "EOB processing failed" },
                
                // ID Card statuses (simpler workflow)
                new CommunicationTypeStatus { CommunicationTypeId = idCardType.Id, GlobalStatusId = createdStatus.Id, IsActive = true, Description = "Card design created" },
                new CommunicationTypeStatus { CommunicationTypeId = idCardType.Id, GlobalStatusId = printedStatus.Id, IsActive = true, Description = "Card printed" },
                new CommunicationTypeStatus { CommunicationTypeId = idCardType.Id, GlobalStatusId = shippedStatus.Id, IsActive = true, Description = "Card shipped" },
                new CommunicationTypeStatus { CommunicationTypeId = idCardType.Id, GlobalStatusId = deliveredStatus.Id, IsActive = true, Description = "Card delivered" },
                new CommunicationTypeStatus { CommunicationTypeId = idCardType.Id, GlobalStatusId = failedStatus.Id, IsActive = true, Description = "Card processing failed" },
                
                // EOP statuses (document workflow)
                new CommunicationTypeStatus { CommunicationTypeId = eopType.Id, GlobalStatusId = createdStatus.Id, IsActive = true, Description = "EOP document created" },
                new CommunicationTypeStatus { CommunicationTypeId = eopType.Id, GlobalStatusId = readyForReleaseStatus.Id, IsActive = true, Description = "EOP ready for release" },
                new CommunicationTypeStatus { CommunicationTypeId = eopType.Id, GlobalStatusId = printedStatus.Id, IsActive = true, Description = "EOP printed" },
                new CommunicationTypeStatus { CommunicationTypeId = eopType.Id, GlobalStatusId = deliveredStatus.Id, IsActive = true, Description = "EOP delivered" },
                new CommunicationTypeStatus { CommunicationTypeId = eopType.Id, GlobalStatusId = failedStatus.Id, IsActive = true, Description = "EOP processing failed" }
            };

            foreach (var typeStatus in communicationTypeStatuses)
            {
                context.CommunicationTypeStatuses.Add(typeStatus);
            }
            context.SaveChanges();

            // Seed Members with funky names
            var members = new Member[]
            {
                new Member 
                { 
                    MemberId = "12345",
                    FirstName = "Bartholomew",
                    LastName = "McFizzlebrow",
                    Email = "b.mcfizzle@example.com",
                    PhoneNumber = "555-123-4567",
                    Address = "742 Evergreen Terrace",
                    City = "Springfield",
                    State = "CA",
                    ZipCode = "90210",
                    DateOfBirth = new DateTime(1980, 5, 15),
                    EnrollmentDate = new DateTime(2020, 1, 1),
                    IsActive = true
                },
                new Member 
                { 
                    MemberId = "67890",
                    FirstName = "Persephone",
                    LastName = "Thunderwhistle",
                    Email = "p.thunder@example.com",
                    PhoneNumber = "555-987-6543",
                    Address = "1337 Hacker Way",
                    City = "Cyberville",
                    State = "NY",
                    ZipCode = "10001",
                    DateOfBirth = new DateTime(1975, 8, 22),
                    EnrollmentDate = new DateTime(2019, 6, 15),
                    IsActive = true
                },
                new Member 
                { 
                    MemberId = "11111",
                    FirstName = "Wolfgang",
                    LastName = "von Snickerdoodle",
                    Email = "w.snickerdoodle@example.com",
                    PhoneNumber = "555-555-5555",
                    Address = "999 Infinity Loop",
                    City = "Quantumburg",
                    State = "TX",
                    ZipCode = "75001",
                    DateOfBirth = new DateTime(1990, 12, 3),
                    EnrollmentDate = new DateTime(2021, 3, 20),
                    IsActive = true
                },
                new Member 
                { 
                    MemberId = "99999",
                    FirstName = "Maximilian",
                    LastName = "Buttercup-Jones",
                    Email = "max.buttercup@example.com",
                    PhoneNumber = "555-777-8888",
                    Address = "42 Answer Street",
                    City = "Hitchhiker",
                    State = "FL",
                    ZipCode = "33101",
                    DateOfBirth = new DateTime(1985, 4, 1),
                    EnrollmentDate = new DateTime(2022, 7, 4),
                    IsActive = true
                }
            };

            foreach (var member in members)
            {
                context.Members.Add(member);
            }
            context.SaveChanges();

            // Get seeded users and members for foreign key references
            var adminUser = context.Users.First(u => u.Email == "admin@zelis.com");
            var johnUser = context.Users.First(u => u.Email == "john.doe@zelis.com");
            var bartMember = context.Members.First(m => m.MemberId == "12345");
            var persephoneMember = context.Members.First(m => m.MemberId == "67890");

            // Seed Sample Communications with user and member tracking
            var communications = new Communication[]
            {
                new Communication 
                { 
                    Title = "Bartholomew McFizzlebottom - EOB Q3 2024", 
                    CommunicationTypeId = eobType.Id, 
                    CurrentStatusId = printedStatus.Id, 
                    MemberId = bartMember.Id,
                    CreatedUtc = DateTime.UtcNow.AddDays(-10), 
                    LastUpdatedUtc = DateTime.UtcNow.AddDays(-2), 
                    CreatedByUserId = adminUser.Id,
                    LastUpdatedByUserId = johnUser.Id,
                    IsActive = true
                },
                new Communication 
                { 
                    Title = "Persephone Thunderwhistle - ID Card", 
                    CommunicationTypeId = idCardType.Id, 
                    CurrentStatusId = shippedStatus.Id, 
                    MemberId = persephoneMember.Id,
                    CreatedUtc = DateTime.UtcNow.AddDays(-5), 
                    LastUpdatedUtc = DateTime.UtcNow.AddDays(-1), 
                    CreatedByUserId = johnUser.Id,
                    LastUpdatedByUserId = adminUser.Id,
                    IsActive = true
                }
            };

            foreach (var comm in communications)
            {
                context.Communications.Add(comm);
            }
            context.SaveChanges();
        }

        private static void SeedCommunicationTypeStatuses(ApplicationDbContext context)
        {
            // Get the IDs for foreign keys
            var eobType = context.CommunicationTypes.First(ct => ct.TypeCode == "EOB");
            var eopType = context.CommunicationTypes.First(ct => ct.TypeCode == "EOP");
            var idCardType = context.CommunicationTypes.First(ct => ct.TypeCode == "ID_CARD");

            var createdStatus = context.GlobalStatuses.First(gs => gs.StatusCode == "Created");
            var readyForReleaseStatus = context.GlobalStatuses.First(gs => gs.StatusCode == "ReadyForRelease");
            var printedStatus = context.GlobalStatuses.First(gs => gs.StatusCode == "Printed");
            var shippedStatus = context.GlobalStatuses.First(gs => gs.StatusCode == "Shipped");
            var deliveredStatus = context.GlobalStatuses.First(gs => gs.StatusCode == "Delivered");
            var failedStatus = context.GlobalStatuses.First(gs => gs.StatusCode == "Failed");

            // Seed CommunicationTypeStatus - Admin-configured valid statuses per type
            var communicationTypeStatuses = new CommunicationTypeStatus[]
            {
                // EOB statuses (complex document workflow)
                new CommunicationTypeStatus { CommunicationTypeId = eobType.Id, GlobalStatusId = createdStatus.Id, IsActive = true, Description = "EOB document created" },
                new CommunicationTypeStatus { CommunicationTypeId = eobType.Id, GlobalStatusId = readyForReleaseStatus.Id, IsActive = true, Description = "EOB ready for release" },
                new CommunicationTypeStatus { CommunicationTypeId = eobType.Id, GlobalStatusId = printedStatus.Id, IsActive = true, Description = "EOB printed by vendor" },
                new CommunicationTypeStatus { CommunicationTypeId = eobType.Id, GlobalStatusId = shippedStatus.Id, IsActive = true, Description = "EOB shipped to member" },
                new CommunicationTypeStatus { CommunicationTypeId = eobType.Id, GlobalStatusId = deliveredStatus.Id, IsActive = true, Description = "EOB delivered to member" },
                new CommunicationTypeStatus { CommunicationTypeId = eobType.Id, GlobalStatusId = failedStatus.Id, IsActive = true, Description = "EOB processing failed" },
                
                // ID Card statuses (simpler workflow)
                new CommunicationTypeStatus { CommunicationTypeId = idCardType.Id, GlobalStatusId = createdStatus.Id, IsActive = true, Description = "Card design created" },
                new CommunicationTypeStatus { CommunicationTypeId = idCardType.Id, GlobalStatusId = printedStatus.Id, IsActive = true, Description = "Card printed" },
                new CommunicationTypeStatus { CommunicationTypeId = idCardType.Id, GlobalStatusId = shippedStatus.Id, IsActive = true, Description = "Card shipped" },
                new CommunicationTypeStatus { CommunicationTypeId = idCardType.Id, GlobalStatusId = deliveredStatus.Id, IsActive = true, Description = "Card delivered" },
                new CommunicationTypeStatus { CommunicationTypeId = idCardType.Id, GlobalStatusId = failedStatus.Id, IsActive = true, Description = "Card processing failed" },
                
                // EOP statuses (document workflow)
                new CommunicationTypeStatus { CommunicationTypeId = eopType.Id, GlobalStatusId = createdStatus.Id, IsActive = true, Description = "EOP document created" },
                new CommunicationTypeStatus { CommunicationTypeId = eopType.Id, GlobalStatusId = readyForReleaseStatus.Id, IsActive = true, Description = "EOP ready for release" },
                new CommunicationTypeStatus { CommunicationTypeId = eopType.Id, GlobalStatusId = printedStatus.Id, IsActive = true, Description = "EOP printed" },
                new CommunicationTypeStatus { CommunicationTypeId = eopType.Id, GlobalStatusId = deliveredStatus.Id, IsActive = true, Description = "EOP delivered" },
                new CommunicationTypeStatus { CommunicationTypeId = eopType.Id, GlobalStatusId = failedStatus.Id, IsActive = true, Description = "EOP processing failed" }
            };

            foreach (var typeStatus in communicationTypeStatuses)
            {
                context.CommunicationTypeStatuses.Add(typeStatus);
            }
            context.SaveChanges();
        }
    }
}