using FoodSprint.Models;
using FoodSprint.Services;
using Xunit;

namespace FoodSprint.Tests
{
    public class MenuEngineeringTests
    {
        private CulinaryService CreateIsolatedService()
        {
            var tempFile = Path.Combine(Path.GetTempPath(), $"culinary_test_{Guid.NewGuid():N}.json");
            return new CulinaryService(tempFile);
        }

        [Fact]
        public void ProfitMargin_CalculatesAccurately()
        {
            var item = new MenuItem
            {
                Name = "Pan-Seared Halibut",
                MenuPrice = 40.00m,
                FoodCost = 10.00m,
                PrepLabor = 5.00m
            };

            // (40 - 10) / 40 = 75%
            Assert.Equal(75.00m, item.ProfitMargin);
            // 40 - 10 - 5 = $25.00
            Assert.Equal(25.00m, item.ContributionMargin);
        }

        [Fact]
        public void AddMenuItem_PersistsInCatalog()
        {
            var service = CreateIsolatedService();
            int initialCount = service.MenuItems.Count;

            var newDish = new MenuItem
            {
                Name = "Crispy Soft-Shell Crab",
                Category = "Starters",
                MenuPrice = 22.00m,
                FoodCost = 6.60m,
                PrepLabor = 3.50m,
                WeeklyVolume = 90
            };

            service.AddMenuItem(newDish);

            Assert.Equal(initialCount + 1, service.MenuItems.Count);
            var saved = service.GetMenuItemById(newDish.Id);
            Assert.NotNull(saved);
            Assert.Equal("Crispy Soft-Shell Crab", saved.Name);
            Assert.Equal(70.00m, saved.ProfitMargin);
        }

        [Fact]
        public void UpdateMenuItem_ModifiesDetailsCorrectly()
        {
            var service = CreateIsolatedService();
            var dish = service.MenuItems.First();
            decimal originalPrice = dish.MenuPrice;

            dish.MenuPrice = originalPrice + 10.00m;
            service.UpdateMenuItem(dish);

            var updated = service.GetMenuItemById(dish.Id);
            Assert.NotNull(updated);
            Assert.Equal(originalPrice + 10.00m, updated.MenuPrice);
        }

        [Fact]
        public void DeleteMenuItem_RemovesFromList()
        {
            var service = CreateIsolatedService();
            var dish = service.MenuItems.First();
            int id = dish.Id;

            service.DeleteMenuItem(id);

            Assert.Null(service.GetMenuItemById(id));
        }

        [Fact]
        public void ToggleAvailability_FlipsIsAvailable()
        {
            var service = CreateIsolatedService();
            var dish = service.MenuItems.First();
            bool initialAvailability = dish.IsAvailable;

            service.ToggleMenuItemAvailability(dish.Id);

            Assert.Equal(!initialAvailability, dish.IsAvailable);
        }

        [Fact]
        public void Classification_CalculatesStarsPuzzlesPlowhorsesAndDogs()
        {
            var service = CreateIsolatedService();

            // Clear to set up controlled category test
            service.MenuItems.Clear();

            // Avg margin will be (80 + 80 + 40 + 40) / 4 = 60%
            // Avg volume will be (200 + 50 + 200 + 50) / 4 = 125
            var star = new MenuItem { Id = 1, Name = "Star Dish", Category = "Mains", MenuPrice = 100, FoodCost = 20, WeeklyVolume = 200 }; // 80% margin, 200 vol
            var puzzle = new MenuItem { Id = 2, Name = "Puzzle Dish", Category = "Mains", MenuPrice = 100, FoodCost = 20, WeeklyVolume = 50 }; // 80% margin, 50 vol
            var plowhorse = new MenuItem { Id = 3, Name = "Plowhorse Dish", Category = "Mains", MenuPrice = 100, FoodCost = 60, WeeklyVolume = 200 }; // 40% margin, 200 vol
            var dog = new MenuItem { Id = 4, Name = "Dog Dish", Category = "Mains", MenuPrice = 100, FoodCost = 60, WeeklyVolume = 50 }; // 40% margin, 50 vol

            service.AddMenuItem(star);
            service.AddMenuItem(puzzle);
            service.AddMenuItem(plowhorse);
            service.AddMenuItem(dog);

            Assert.Equal("Star", service.GetClassification(star));
            Assert.Equal("Puzzle", service.GetClassification(puzzle));
            Assert.Equal("Plowhorse", service.GetClassification(plowhorse));
            Assert.Equal("Dog", service.GetClassification(dog));
        }
    }
}

