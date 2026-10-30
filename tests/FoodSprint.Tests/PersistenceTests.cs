using FoodSprint.Models;
using FoodSprint.Services;
using Xunit;

namespace FoodSprint.Tests
{
    public class PersistenceTests
    {
        [Fact]
        public void Persistence_SavesAndReloadsAcrossServiceInstances()
        {
            var tempFile = Path.Combine(Path.GetTempPath(), $"culinary_persist_test_{Guid.NewGuid():N}.json");

            try
            {
                // Instance 1: Create and add custom recipe, ticket, and ingredient
                var service1 = new CulinaryService(tempFile);

                service1.AddMenuItem(new MenuItem
                {
                    Name = "Peruvian Ceviche",
                    Category = "Starters",
                    MenuPrice = 21.00m,
                    FoodCost = 6.00m,
                    WeeklyVolume = 130
                });

                service1.AddKitchenTicket(new KitchenTicket
                {
                    TableNumber = 22,
                    Guests = 2,
                    ServerName = "Lucas B.",
                    Station = "Cold Bar",
                    Status = "Cooking"
                });

                service1.AddPantryItem(new PantryItem
                {
                    Name = "Leche de Tigre",
                    Sku = "SAU-440",
                    StorageZone = "Walk-in Cooler",
                    CurrentStock = 15.0,
                    MinThreshold = 20.0,
                    Unit = "qt",
                    UnitCost = 8.00m
                });

                // Instance 2: Reload from disk
                var service2 = new CulinaryService(tempFile);

                var reloadedDish = service2.MenuItems.FirstOrDefault(m => m.Name == "Peruvian Ceviche");
                Assert.NotNull(reloadedDish);
                Assert.Equal(21.00m, reloadedDish.MenuPrice);

                var reloadedTicket = service2.ActiveTickets.FirstOrDefault(t => t.TableNumber == 22);
                Assert.NotNull(reloadedTicket);
                Assert.Equal("Cold Bar", reloadedTicket.Station);

                var reloadedPantry = service2.PantryStock.FirstOrDefault(p => p.Sku == "SAU-440");
                Assert.NotNull(reloadedPantry);
                Assert.Equal(15.0, reloadedPantry.CurrentStock);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    try { File.Delete(tempFile); } catch { }
                }
            }
        }

        [Fact]
        public void Notifications_PreserveReadStateAcrossReloads()
        {
            var tempFile = Path.Combine(Path.GetTempPath(), $"culinary_notif_test_{Guid.NewGuid():N}.json");

            try
            {
                var service1 = new CulinaryService(tempFile);
                Assert.True(service1.Notifications.Any());

                var targetNotif = service1.Notifications.First();
                service1.MarkNotificationAsRead(targetNotif.Id);
                Assert.True(targetNotif.IsRead);

                // Reload in new instance
                var service2 = new CulinaryService(tempFile);
                var reloadedNotif = service2.Notifications.FirstOrDefault(n => n.Key == targetNotif.Key);
                Assert.NotNull(reloadedNotif);
                Assert.True(reloadedNotif.IsRead);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    try { File.Delete(tempFile); } catch { }
                }
            }
        }

        [Fact]
        public void ShiftToggle_UpdatesOperationalState()
        {
            var tempFile = Path.Combine(Path.GetTempPath(), $"culinary_shift_test_{Guid.NewGuid():N}.json");

            try
            {
                var service = new CulinaryService(tempFile);
                Assert.True(service.IsShiftActive);
                Assert.True(service.SeatedTableCount > 0);
                Assert.True(service.TableLoadPercentage > 0);

                // Toggle to Standby
                service.ToggleShift();
                Assert.False(service.IsShiftActive);
                Assert.Equal(0, service.SeatedTableCount);
                Assert.Equal(0, service.TableLoadPercentage);
                Assert.Equal("Service: Standby / Prep", service.ServicePaceStatus);

                // Toggle back to Active
                service.ToggleShift();
                Assert.True(service.IsShiftActive);
                Assert.True(service.TableLoadPercentage > 0);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    try { File.Delete(tempFile); } catch { }
                }
            }
        }

        [Fact]
        public void SearchAll_MatchesActualStateAndBuildsDeepLinks()
        {
            var tempFile = Path.Combine(Path.GetTempPath(), $"culinary_search_test_{Guid.NewGuid():N}.json");

            try
            {
                var service = new CulinaryService(tempFile);

                // Search dish
                var codResults = service.SearchAll("Cod");
                Assert.Contains(codResults, r => r.Category == "Menu Recipe" && r.Url.Contains("dish="));

                // Search table ticket
                var ticketResults = service.SearchAll("Table 7");
                Assert.Contains(ticketResults, r => r.Category == "Kitchen Rail" && r.Url.Contains("table=7"));

                // Search pantry ingredient
                var tomatoResults = service.SearchAll("Tomato");
                Assert.Contains(tomatoResults, r => r.Category == "Pantry Stock" && r.Url.Contains("sku="));
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    try { File.Delete(tempFile); } catch { }
                }
            }
        }
    }
}
