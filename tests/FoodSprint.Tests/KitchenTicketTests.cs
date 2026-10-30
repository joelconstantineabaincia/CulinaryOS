using FoodSprint.Models;
using FoodSprint.Services;
using Xunit;

namespace FoodSprint.Tests
{
    public class KitchenTicketTests
    {
        private CulinaryService CreateIsolatedService()
        {
            var tempFile = Path.Combine(Path.GetTempPath(), $"culinary_kds_test_{Guid.NewGuid():N}.json");
            return new CulinaryService(tempFile);
        }

        [Fact]
        public void AddKitchenTicket_EntersActiveRail()
        {
            var service = CreateIsolatedService();
            int initialCount = service.ActiveTickets.Count;

            var ticket = new KitchenTicket
            {
                TableNumber = 16,
                Guests = 4,
                ServerName = "Marcus L.",
                Station = "Hot Line",
                Status = "Cooking",
                Courses = new()
                {
                    new()
                    {
                        CourseName = "Course 1 (Apps)",
                        Status = "Cooking",
                        Items = new()
                        {
                            new() { Quantity = 2, Name = "Hamachi Crudo" }
                        }
                    }
                }
            };

            service.AddKitchenTicket(ticket);

            Assert.Equal(initialCount + 1, service.ActiveTickets.Count);
            var saved = service.GetTicketById(ticket.TicketId);
            Assert.NotNull(saved);
            Assert.Equal(16, saved.TableNumber);
            Assert.Equal("Hot Line", saved.Station);
        }

        [Fact]
        public void CriticalAllergen_TriggersAlert()
        {
            var ticketWithAllergy = new KitchenTicket
            {
                TableNumber = 9,
                AllergyAlert = "Severe Shellfish Anaphylaxis"
            };

            var ticketWithoutAllergy = new KitchenTicket
            {
                TableNumber = 10,
                AllergyAlert = "None"
            };

            Assert.True(ticketWithAllergy.IsCriticalAllergen);
            Assert.False(ticketWithoutAllergy.IsCriticalAllergen);
        }

        [Fact]
        public void ToggleRush_TogglesIsRushFlag()
        {
            var service = CreateIsolatedService();
            var ticket = service.ActiveTickets.First();
            bool initialRush = ticket.IsRush;

            service.ToggleTicketRush(ticket.TicketId);

            Assert.Equal(!initialRush, ticket.IsRush);
        }

        [Fact]
        public void AdvanceAndBumpTicket_WorkflowCompletesOrder()
        {
            var service = CreateIsolatedService();
            var ticket = service.ActiveTickets.First(t => t.Status == "Cooking");
            int ticketId = ticket.TicketId;

            // 1. Advance to Plated
            service.AdvanceTicketStatus(ticketId);
            var platedTicket = service.GetTicketById(ticketId);
            Assert.NotNull(platedTicket);
            Assert.Equal("Plated", platedTicket.Status);

            // 2. Bump ticket off rail
            service.BumpTicket(ticketId);
            Assert.Null(service.GetTicketById(ticketId));
        }
    }
}

