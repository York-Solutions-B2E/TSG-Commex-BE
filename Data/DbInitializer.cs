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

            // Seed Global Statuses with enum values
            var globalStatuses = new GlobalStatus[]
            {
                new GlobalStatus { StatusCode = "Created", DisplayName = "Created", Description = "Initial creation", Phase = StatusPhase.Creation, IsActive = true },
                new GlobalStatus { StatusCode = "ReadyForRelease", DisplayName = "Ready for Release", Description = "Ready to be released", Phase = StatusPhase.Creation, IsActive = true },
                new GlobalStatus { StatusCode = "Released", DisplayName = "Released", Description = "Released to production", Phase = StatusPhase.Creation, IsActive = true },
                new GlobalStatus { StatusCode = "QueuedForPrinting", DisplayName = "Queued for Printing", Description = "In print queue", Phase = StatusPhase.Production, IsActive = true },
                new GlobalStatus { StatusCode = "Printed", DisplayName = "Printed", Description = "Document printed", Phase = StatusPhase.Production, IsActive = true },
                new GlobalStatus { StatusCode = "Inserted", DisplayName = "Inserted", Description = "Inserted into envelope", Phase = StatusPhase.Production, IsActive = true },
                new GlobalStatus { StatusCode = "Shipped", DisplayName = "Shipped", Description = "Shipped to member", Phase = StatusPhase.Logistics, IsActive = true },
                new GlobalStatus { StatusCode = "InTransit", DisplayName = "In Transit", Description = "In transit to member", Phase = StatusPhase.Logistics, IsActive = true },
                new GlobalStatus { StatusCode = "Delivered", DisplayName = "Delivered", Description = "Delivered to member", Phase = StatusPhase.Terminal, IsActive = true },
                new GlobalStatus { StatusCode = "Returned", DisplayName = "Returned", Description = "Returned to sender", Phase = StatusPhase.Terminal, IsActive = true },
                new GlobalStatus { StatusCode = "Failed", DisplayName = "Failed", Description = "Processing failed", Phase = StatusPhase.Terminal, IsActive = true }
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

            // Seed Status Transitions - EOB Workflow
            var eobTransitions = new StatusTransition[]
            {
                new StatusTransition { TypeCode = "EOB", FromStatusCode = null, ToStatusCode = "Created", SortOrder = 1, Description = "Initial EOB creation" },
                new StatusTransition { TypeCode = "EOB", FromStatusCode = "Created", ToStatusCode = "ReadyForRelease", SortOrder = 2, Description = "Ready for release" },
                new StatusTransition { TypeCode = "EOB", FromStatusCode = "ReadyForRelease", ToStatusCode = "Released", SortOrder = 3, Description = "Released to production" },
                new StatusTransition { TypeCode = "EOB", FromStatusCode = "Released", ToStatusCode = "QueuedForPrinting", SortOrder = 4, Description = "Queued for printing" },
                new StatusTransition { TypeCode = "EOB", FromStatusCode = "QueuedForPrinting", ToStatusCode = "Printed", SortOrder = 5, Description = "Document printed" },
                new StatusTransition { TypeCode = "EOB", FromStatusCode = "Printed", ToStatusCode = "Inserted", SortOrder = 6, Description = "Inserted into envelope" },
                new StatusTransition { TypeCode = "EOB", FromStatusCode = "Inserted", ToStatusCode = "Shipped", SortOrder = 7, Description = "Shipped to member" },
                new StatusTransition { TypeCode = "EOB", FromStatusCode = "Shipped", ToStatusCode = "InTransit", SortOrder = 8, Description = "In transit" },
                new StatusTransition { TypeCode = "EOB", FromStatusCode = "InTransit", ToStatusCode = "Delivered", SortOrder = 9, Description = "Delivered to member" }
            };

            // ID Card Workflow (simpler - skips some steps)
            var idCardTransitions = new StatusTransition[]
            {
                new StatusTransition { TypeCode = "ID_CARD", FromStatusCode = null, ToStatusCode = "Created", SortOrder = 1, Description = "Card requested" },
                new StatusTransition { TypeCode = "ID_CARD", FromStatusCode = "Created", ToStatusCode = "Printed", SortOrder = 2, Description = "Card printed" },
                new StatusTransition { TypeCode = "ID_CARD", FromStatusCode = "Printed", ToStatusCode = "Shipped", SortOrder = 3, Description = "Card shipped" },
                new StatusTransition { TypeCode = "ID_CARD", FromStatusCode = "Shipped", ToStatusCode = "Delivered", SortOrder = 4, Description = "Card delivered" }
            };

            var allTransitions = eobTransitions.Concat(idCardTransitions);
            foreach (var transition in allTransitions)
            {
                context.StatusTransitions.Add(transition);
            }
            context.SaveChanges();

            // Seed Sample Communications
            var communications = new Communication[]
            {
                new Communication { Title = "John Doe - EOB Q3 2024", TypeCode = "EOB", CurrentStatus = "Printed", CreatedUtc = DateTime.UtcNow.AddDays(-10), LastUpdatedUtc = DateTime.UtcNow.AddDays(-2), MemberInfo = "Member ID: 12345" },
                new Communication { Title = "Jane Smith - ID Card", TypeCode = "ID_CARD", CurrentStatus = "Shipped", CreatedUtc = DateTime.UtcNow.AddDays(-5), LastUpdatedUtc = DateTime.UtcNow.AddDays(-1), MemberInfo = "Member ID: 67890" }
            };

            foreach (var comm in communications)
            {
                context.Communications.Add(comm);
            }
            context.SaveChanges();
        }
    }
}