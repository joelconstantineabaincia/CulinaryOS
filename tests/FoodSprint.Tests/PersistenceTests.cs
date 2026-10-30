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
    }
}

