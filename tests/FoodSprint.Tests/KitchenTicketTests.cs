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

        [Fact]
        public void AverageTicketCalculations_ReflectsActiveData()
        {
            var service = CreateIsolatedService();
            service.ActiveTickets.Clear();

            // When no tickets
            Assert.Equal(0, service.AverageTicketMinutes);
            Assert.Equal("--", service.FormattedAverageTicketTime);

            // Add tickets with known minutes
            service.AddKitchenTicket(new KitchenTicket { TableNumber = 1, ElapsedMinutes = 6 });
            service.AddKitchenTicket(new KitchenTicket { TableNumber = 2, ElapsedMinutes = 10 });

            // (6 + 10) / 2 = 8
            Assert.Equal(8.0, service.AverageTicketMinutes);
            Assert.Equal("8m 00s", service.FormattedAverageTicketTime);
            Assert.Equal("Service: On Pace", service.ServicePaceStatus);

            // Add a late ticket: (6 + 10 + 20) / 3 = 12
            service.AddKitchenTicket(new KitchenTicket { TableNumber = 3, ElapsedMinutes = 20 });
            Assert.Equal("Service: Exceeding Par", service.ServicePaceStatus);
            Assert.Equal("coral", service.ServicePaceCssClass);
        }

        [Fact]
        public void ServiceInsight_ProducesDynamicOperationalSummary()
        {
            var service = CreateIsolatedService();
            service.ActiveTickets.Clear();

            Assert.Contains("clear", service.ServiceInsight, StringComparison.OrdinalIgnoreCase);

            service.AddKitchenTicket(new KitchenTicket { TableNumber = 5, Station = "Sauté", ElapsedMinutes = 4, IsRush = true });
            Assert.Contains("expedited", service.ServiceInsight, StringComparison.OrdinalIgnoreCase);
        }
    }
}
