using FoodSprint.Models;

namespace FoodSprint.Services
{
    public class CulinaryService
    {
        public List<MenuItem> MenuItems { get; set; } = new();
        public List<KitchenTicket> ActiveTickets { get; set; } = new();
        public List<PantryItem> PantryStock { get; set; } = new();

        public CulinaryService()
        {
            SeedData();
        }

        private void SeedData()
        {
            // Seed Menu (12 dishes, Average margin 68.4%)
            MenuItems.AddRange(new[]
            {
                new MenuItem { Id = 1, Name = "Hamachi Crudo", Category = "Starters", MenuPrice = 18.00m, FoodCost = 6.30m, PrepLabor = 3.20m, PrepMinutes = 8, Allergens = "Fish", IsGlutenFree = true, IsAvailable = true },
                new MenuItem { Id = 2, Name = "Burrata & Persimmon", Category = "Starters", MenuPrice = 16.00m, FoodCost = 5.60m, PrepLabor = 2.80m, PrepMinutes = 10, Allergens = "Dairy", IsGlutenFree = true, IsAvailable = true },
                new MenuItem { Id = 3, Name = "Little Gem Salad", Category = "Starters", MenuPrice = 14.00m, FoodCost = 4.90m, PrepLabor = 2.50m, PrepMinutes = 7, Allergens = "None", IsGlutenFree = true, IsVegan = true, IsAvailable = true },
                new MenuItem { Id = 4, Name = "Crispy Calamari Fritti", Category = "Starters", MenuPrice = 16.50m, FoodCost = 5.80m, PrepLabor = 3.50m, PrepMinutes = 10, Allergens = "Shellfish", IsAvailable = true },
                new MenuItem { Id = 5, Name = "Roasted Atlantic Cod", Category = "Mains", MenuPrice = 36.00m, FoodCost = 12.60m, PrepLabor = 4.80m, PrepMinutes = 20, Allergens = "Fish", IsGlutenFree = true, IsAvailable = true },
                new MenuItem { Id = 6, Name = "Dry-Aged Duck", Category = "Mains", MenuPrice = 38.00m, FoodCost = 13.30m, PrepLabor = 5.20m, PrepMinutes = 22, Allergens = "None", IsGlutenFree = true, IsAvailable = true },
                new MenuItem { Id = 7, Name = "Wild Mushroom Risotto", Category = "Mains", MenuPrice = 28.00m, FoodCost = 9.80m, PrepLabor = 4.10m, PrepMinutes = 18, IsGlutenFree = true, IsVegan = true, Allergens = "Mushrooms", IsAvailable = true },
                new MenuItem { Id = 8, Name = "Pan-Seared Duck Breast", Category = "Mains", MenuPrice = 34.00m, FoodCost = 10.20m, PrepLabor = 4.50m, PrepMinutes = 18, Allergens = "None", IsAvailable = true },
                new MenuItem { Id = 9, Name = "Carnaroli Risotto", Category = "Mains", MenuPrice = 26.00m, FoodCost = 7.80m, PrepLabor = 3.80m, PrepMinutes = 16, IsGlutenFree = true, Allergens = "Dairy", IsAvailable = true },
                new MenuItem { Id = 10, Name = "Charred Broccolini", Category = "Mains", MenuPrice = 15.00m, FoodCost = 4.50m, PrepLabor = 2.20m, PrepMinutes = 9, IsGlutenFree = true, IsVegan = true, Allergens = "None", IsAvailable = true },
                new MenuItem { Id = 11, Name = "Warm Olive Cake", Category = "Desserts", MenuPrice = 13.00m, FoodCost = 3.90m, PrepLabor = 2.40m, PrepMinutes = 12, Allergens = "Gluten, Dairy", IsAvailable = true },
                new MenuItem { Id = 12, Name = "Valrhona Chocolate Soufflé", Category = "Desserts", MenuPrice = 14.00m, FoodCost = 3.50m, PrepLabor = 3.00m, PrepMinutes = 20, Allergens = "Dairy, Eggs", IsAvailable = false }
            });

            // Seed Active Tickets
            ActiveTickets.AddRange(new[]
            {
                new KitchenTicket
                {
                    TicketId = 101,
                    TableNumber = 4,
                    Guests = 2,
                    ActiveCourse = "Starters",
                    Station = "Garde Manger",
                    ElapsedMinutes = 6,
                    ElapsedTimeString = "06:15",
                    AllergyAlert = "None",
                    IsRush = false,
                    Status = "Cooking",
                    Items = new() { "2 Hamachi Crudo", "1 Little Gem Salad", "1 Burrata & Persimmon" },
                    ItemDetails = new()
                    {
                        new() { Name = "2 Hamachi Crudo" },
                        new() { Name = "1 Little Gem Salad" },
                        new() { Name = "1 Burrata & Persimmon" }
                    }
                },
                new KitchenTicket
                {
                    TicketId = 102,
                    TableNumber = 7,
                    Guests = 4,
                    ActiveCourse = "Mains",
                    Station = "Hot Line",
                    ElapsedMinutes = 14,
                    ElapsedTimeString = "14:18",
                    AllergyAlert = "Severe Peanut Anaphylaxis",
                    IsRush = true,
                    Status = "Cooking",
                    Items = new() { "1 Roasted Atlantic Cod (NO PEANUT)", "1 Charred Broccolini (ALLERGY)", "1 Carnaroli Risotto" },
                    ItemDetails = new()
                    {
                        new() { Name = "1 Roasted Atlantic Cod", Badge = "NO PEANUT" },
                        new() { Name = "1 Charred Broccolini", Badge = "ALLERGY", Note = "Clean pan · fresh gloves" },
                        new() { Name = "1 Carnaroli Risotto", Note = "No shared garnish" }
                    }
                },
                new KitchenTicket
                {
                    TicketId = 103,
                    TableNumber = 12,
                    Guests = 3,
                    ActiveCourse = "Mains",
                    Station = "Sauté",
                    ElapsedMinutes = 9,
                    ElapsedTimeString = "09:42",
                    AllergyAlert = "None",
                    IsRush = false,
                    Status = "Cooking",
                    Items = new() { "1 Dry-Aged Duck", "1 Wild Mushroom Risotto", "2 Warm Olive Cake" },
                    ItemDetails = new()
                    {
                        new() { Name = "1 Dry-Aged Duck" },
                        new() { Name = "1 Wild Mushroom Risotto" },
                        new() { Name = "2 Warm Olive Cake" }
                    }
                }
            });

            // Seed Pantry Inventory
            PantryStock.AddRange(new[]
            {
                new PantryItem { Id = 1, Name = "Atlantic Cod", Sku = "FSH-884", StorageZone = "Walk-in Cooler", CurrentStock = 36.0, MinThreshold = 50.0, Unit = "lb", DaysUntilExpiry = 1, FreshnessBadge = "1d 08h", FreshnessVariant = "warning", ParPercentage = 72, UnitCost = 14.20m },
                new PantryItem { Id = 2, Name = "Heirloom Tomatoes", Sku = "PRD-102", StorageZone = "Walk-in Cooler", CurrentStock = 23.0, MinThreshold = 50.0, Unit = "lb", DaysUntilExpiry = 0, FreshnessBadge = "18h", FreshnessVariant = "danger", ParPercentage = 46, UnitCost = 4.50m },
                new PantryItem { Id = 3, Name = "Carnaroli Rice", Sku = "DRY-330", StorageZone = "Dry Larder", CurrentStock = 44.0, MinThreshold = 50.0, Unit = "lb", DaysUntilExpiry = 42, FreshnessBadge = "42 days", FreshnessVariant = "success", ParPercentage = 88, UnitCost = 3.80m },
                new PantryItem { Id = 4, Name = "Microgreens", Sku = "PRD-041", StorageZone = "Walk-in Cooler", CurrentStock = 6.2, MinThreshold = 20.0, Unit = "lb", DaysUntilExpiry = 0, FreshnessBadge = "09h", FreshnessVariant = "danger", ParPercentage = 31, UnitCost = 18.00m },
                new PantryItem { Id = 5, Name = "Duck Breast (Magret)", Sku = "MT-205", StorageZone = "Walk-in Cooler", CurrentStock = 12.6, MinThreshold = 30.0, Unit = "lb", DaysUntilExpiry = 2, FreshnessBadge = "2 days", FreshnessVariant = "warning", ParPercentage = 42, UnitCost = 28.00m },
                new PantryItem { Id = 6, Name = "Valrhona Cocoa & Chocolate", Sku = "PST-019", StorageZone = "Dry Larder", CurrentStock = 28.5, MinThreshold = 30.0, Unit = "lb", DaysUntilExpiry = 90, FreshnessBadge = "90 days", FreshnessVariant = "success", ParPercentage = 95, UnitCost = 16.50m },
                new PantryItem { Id = 7, Name = "Heavy Whipping Cream 36%", Sku = "DRY-012", StorageZone = "Walk-in Cooler", CurrentStock = 11.0, MinThreshold = 30.0, Unit = "qt", DaysUntilExpiry = 1, FreshnessBadge = "1d 04h", FreshnessVariant = "warning", ParPercentage = 37, UnitCost = 6.20m },
                new PantryItem { Id = 8, Name = "Atlantic Squid Tubes", Sku = "FSH-412", StorageZone = "Freezer", CurrentStock = 32.5, MinThreshold = 50.0, Unit = "lb", DaysUntilExpiry = 30, FreshnessBadge = "30 days", FreshnessVariant = "success", ParPercentage = 65, UnitCost = 12.50m }
            });
        }
    }
}