using FoodSprint.Models;
using FoodSprint.Services;
using Xunit;

namespace FoodSprint.Tests
{
    public class PantryInventoryTests
    {
        private CulinaryService CreateIsolatedService()
        {
            var tempFile = Path.Combine(Path.GetTempPath(), $"culinary_pantry_test_{Guid.NewGuid():N}.json");
            return new CulinaryService(tempFile);
        }

        [Fact]
        public void DynamicMetrics_CalculateAccurately()
        {
            var service = CreateIsolatedService();

            decimal expectedTotalVal = service.PantryStock.Sum(p => (decimal)p.CurrentStock * p.UnitCost);
            decimal expectedRisk = service.PantryStock.Where(p => p.DaysUntilExpiry <= 2).Sum(p => (decimal)p.CurrentStock * p.UnitCost);
            int expectedBelowPar = service.PantryStock.Count(p => p.IsLowStock);

            Assert.Equal(expectedTotalVal, service.TotalStockParValue);
            Assert.Equal(expectedRisk, service.SpoilageRisk48h);
            Assert.Equal(expectedBelowPar, service.BelowParCount);
        }

        [Fact]
        public void OrderStock_IncreasesQuantityAndPar()
        {
            var service = CreateIsolatedService();
            var item = service.PantryStock.First();
            double initialStock = item.CurrentStock;

            service.OrderStock(item.Id, 10.0);

            var updated = service.GetPantryItemById(item.Id);
            Assert.NotNull(updated);
            Assert.Equal(initialStock + 10.0, updated.CurrentStock);
        }

        [Fact]
        public void BatchOrderStock_RestocksMultipleSkus()
        {
            var service = CreateIsolatedService();
            var itemsToOrder = service.PantryStock.Take(2).ToList();
            var skus = itemsToOrder.Select(i => i.Sku).ToList();
            var initialStocks = itemsToOrder.ToDictionary(i => i.Sku, i => i.CurrentStock);

            service.BatchOrderStock(skus, 15.0);

            foreach (var sku in skus)
            {
                var item = service.PantryStock.First(p => p.Sku == sku);
                Assert.Equal(initialStocks[sku] + 15.0, item.CurrentStock);
            }
        }

        [Fact]
        public void AddAndDeletePantryItem_WorksCorrectly()
        {
            var service = CreateIsolatedService();
            var newItem = new PantryItem
            {
                Name = "Truffle Butter",
                Sku = "DAI-901",
                StorageZone = "Walk-in Cooler",
                CurrentStock = 12.0,
                MinThreshold = 15.0,
                Unit = "lb",
                UnitCost = 24.50m,
                DaysUntilExpiry = 14
            };

            service.AddPantryItem(newItem);
            Assert.NotNull(service.GetPantryItemById(newItem.Id));

            service.DeletePantryItem(newItem.Id);
            Assert.Null(service.GetPantryItemById(newItem.Id));
        }
    }
}

