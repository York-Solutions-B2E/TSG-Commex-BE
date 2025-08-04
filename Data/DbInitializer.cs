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
                return; // DB has been seeded
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

            // Seed CommunicationTypeStatus - Admin-configured valid statuses per type
            var communicationTypeStatuses = new CommunicationTypeStatus[]
            {
                // EOB statuses (complex document workflow)
                new CommunicationTypeStatus { TypeCode = "EOB", StatusCode = "Created", Description = "EOB document created" },
                new CommunicationTypeStatus { TypeCode = "EOB", StatusCode = "ReadyForRelease", Description = "EOB ready for release" },
                new CommunicationTypeStatus { TypeCode = "EOB", StatusCode = "Printed", Description = "EOB printed by vendor" },
                new CommunicationTypeStatus { TypeCode = "EOB", StatusCode = "Shipped", Description = "EOB shipped to member" },
                new CommunicationTypeStatus { TypeCode = "EOB", StatusCode = "Delivered", Description = "EOB delivered to member" },
                new CommunicationTypeStatus { TypeCode = "EOB", StatusCode = "Failed", Description = "EOB processing failed" },
                
                // ID Card statuses (simpler workflow)
                new CommunicationTypeStatus { TypeCode = "ID_CARD", StatusCode = "Created", Description = "Card design created" },
                new CommunicationTypeStatus { TypeCode = "ID_CARD", StatusCode = "Printed", Description = "Card printed" },
                new CommunicationTypeStatus { TypeCode = "ID_CARD", StatusCode = "Shipped", Description = "Card shipped" },
                new CommunicationTypeStatus { TypeCode = "ID_CARD", StatusCode = "Delivered", Description = "Card delivered" },
                new CommunicationTypeStatus { TypeCode = "ID_CARD", StatusCode = "Failed", Description = "Card processing failed" },
                
                // EOP statuses (document workflow)
                new CommunicationTypeStatus { TypeCode = "EOP", StatusCode = "Created", Description = "EOP document created" },
                new CommunicationTypeStatus { TypeCode = "EOP", StatusCode = "ReadyForRelease", Description = "EOP ready for release" },
                new CommunicationTypeStatus { TypeCode = "EOP", StatusCode = "Printed", Description = "EOP printed" },
                new CommunicationTypeStatus { TypeCode = "EOP", StatusCode = "Delivered", Description = "EOP delivered" },
                new CommunicationTypeStatus { TypeCode = "EOP", StatusCode = "Failed", Description = "EOP processing failed" }
            };

            foreach (var typeStatus in communicationTypeStatuses)
            {
                context.CommunicationTypeStatuses.Add(typeStatus);
            }
            context.SaveChanges();

            // Get seeded users for foreign key references
            var adminUser = context.Users.First(u => u.Email == "admin@zelis.com");
            var johnUser = context.Users.First(u => u.Email == "john.doe@zelis.com");

            // Seed Sample Communications with user tracking
            var communications = new Communication[]
            {
                new Communication 
                { 
                    Title = "John Doe - EOB Q3 2024", 
                    TypeCode = "EOB", 
                    CurrentStatus = "Printed", 
                    CreatedUtc = DateTime.UtcNow.AddDays(-10), 
                    LastUpdatedUtc = DateTime.UtcNow.AddDays(-2), 
                    MemberInfo = "Member ID: 12345",
                    CreatedByUserId = adminUser.Id,
                    LastUpdatedByUserId = johnUser.Id,
                    IsActive = true
                },
                new Communication 
                { 
                    Title = "Jane Smith - ID Card", 
                    TypeCode = "ID_CARD", 
                    CurrentStatus = "Shipped", 
                    CreatedUtc = DateTime.UtcNow.AddDays(-5), 
                    LastUpdatedUtc = DateTime.UtcNow.AddDays(-1), 
                    MemberInfo = "Member ID: 67890",
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
    }
}