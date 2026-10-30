using System.Text.Json;
using FoodSprint.Models;

namespace FoodSprint.Services
{
    public class CulinaryDataStore
    {
        public List<MenuItem> MenuItems { get; set; } = new();
        public List<KitchenTicket> ActiveTickets { get; set; } = new();
        public List<PantryItem> PantryStock { get; set; } = new();
        public List<SystemNotification> Notifications { get; set; } = new();
        public bool IsShiftActive { get; set; } = true;
        public int TableLoad { get; set; } = 18;
        public int TableCapacity { get; set; } = 24;
        public string ActiveRole { get; set; } = "Expo Lead";
        public string ActiveStation { get; set; } = "Station 02";
    }

    public class CulinaryService
    {
        private readonly object _lock = new();
        private readonly string _storageFilePath;

        public List<MenuItem> MenuItems { get; private set; } = new();
        public List<KitchenTicket> ActiveTickets { get; private set; } = new();
        public List<PantryItem> PantryStock { get; private set; } = new();
        public List<SystemNotification> Notifications { get; private set; } = new();

        public bool IsShiftActive { get; set; } = true;
        public int TableLoad { get; set; } = 18;
        public int TableCapacity { get; set; } = 24;
        public string ActiveRole { get; set; } = "Expo Lead";
        public string ActiveStation { get; set; } = "Station 02";

        public event Action? OnChange;

        public CulinaryService()
        {
            var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
            try
            {
                if (!Directory.Exists(dataDir))
                {
                    Directory.CreateDirectory(dataDir);
                }
            }
            catch
            {
                dataDir = AppContext.BaseDirectory;
            }

            _storageFilePath = Path.Combine(dataDir, "culinary_store.json");
            LoadFromDisk();
        }

        public CulinaryService(string storageFilePath)
        {
            _storageFilePath = storageFilePath;
            LoadFromDisk();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();

        #region Persistence

        public void LoadFromDisk()
        {
            lock (_lock)
            {
                if (File.Exists(_storageFilePath))
                {
                    try
                    {
                        var json = File.ReadAllText(_storageFilePath);
                        var store = JsonSerializer.Deserialize<CulinaryDataStore>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (store != null && store.MenuItems.Count > 0)
                        {
                            MenuItems = store.MenuItems;
                            ActiveTickets = store.ActiveTickets;
                            PantryStock = store.PantryStock;
                            Notifications = store.Notifications;
                            IsShiftActive = store.IsShiftActive;
                            TableLoad = store.TableLoad;
                            TableCapacity = store.TableCapacity;
                            ActiveRole = store.ActiveRole;
                            ActiveStation = store.ActiveStation;
                            SyncNotifications();
                            return;
                        }
                    }
                    catch
                    {
                        // Fall back to seed data on deserialization failure
                    }
                }

                SeedData();
                SaveToDisk();
            }
        }

        public void SaveToDisk()
        {
            lock (_lock)
            {
                try
                {
                    var store = new CulinaryDataStore
                    {
                        MenuItems = MenuItems,
                        ActiveTickets = ActiveTickets,
                        PantryStock = PantryStock,
                        Notifications = Notifications,
                        IsShiftActive = IsShiftActive,
                        TableLoad = TableLoad,
                        TableCapacity = TableCapacity,
                        ActiveRole = ActiveRole,
                        ActiveStation = ActiveStation
                    };

                    var dir = Path.GetDirectoryName(_storageFilePath);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    var json = JsonSerializer.Serialize(store, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(_storageFilePath, json);
                }
                catch
                {
                    // Fail silently or handle in production logging
                }
            }
            NotifyStateChanged();
        }

        #endregion

        #region Menu Engineering & Recipes

        public MenuItem? GetMenuItemById(int id) => MenuItems.FirstOrDefault(m => m.Id == id);

        public void AddMenuItem(MenuItem item)
        {
            lock (_lock)
            {
                if (item.Id <= 0)
                {
                    item.Id = (MenuItems.Any() ? MenuItems.Max(m => m.Id) : 0) + 1;
                }
                MenuItems.Add(item);
                SaveToDisk();
            }
        }

        public void UpdateMenuItem(MenuItem item)
        {
            lock (_lock)
            {
                var existing = MenuItems.FirstOrDefault(m => m.Id == item.Id);
                if (existing != null)
                {
                    existing.Name = item.Name;
                    existing.Category = item.Category;
                    existing.MenuPrice = item.MenuPrice;
                    existing.FoodCost = item.FoodCost;
                    existing.PrepLabor = item.PrepLabor;
                    existing.PrepMinutes = item.PrepMinutes;
                    existing.IsGlutenFree = item.IsGlutenFree;
                    existing.IsVegan = item.IsVegan;
                    existing.Allergens = item.Allergens;
                    existing.IsAvailable = item.IsAvailable;
                    existing.WeeklyVolume = item.WeeklyVolume;
                    existing.SubIngredients = new List<SubIngredient>(item.SubIngredients);
                    SaveToDisk();
                }
            }
        }

        public void DeleteMenuItem(int id)
        {
            lock (_lock)
            {
                var item = MenuItems.FirstOrDefault(m => m.Id == id);
                if (item != null)
                {
                    MenuItems.Remove(item);
                    SaveToDisk();
                }
            }
        }

        public void ToggleMenuItemAvailability(int id)
        {
            lock (_lock)
            {
                var item = MenuItems.FirstOrDefault(m => m.Id == id);
                if (item != null)
                {
                    item.IsAvailable = !item.IsAvailable;
                    SaveToDisk();
                }
            }
        }

        public string GetClassification(MenuItem dish)
        {
            var categoryDishes = MenuItems.Where(m => m.Category == dish.Category).ToList();
            if (!categoryDishes.Any()) categoryDishes = MenuItems;

            decimal avgMargin = categoryDishes.Average(m => m.ProfitMargin);
            double avgVolume = categoryDishes.Average(m => m.WeeklyVolume);

            bool highMargin = dish.ProfitMargin >= avgMargin;
            bool highVolume = dish.WeeklyVolume >= avgVolume;

            if (highMargin && highVolume) return "Star";
            if (highMargin && !highVolume) return "Puzzle";
            if (!highMargin && highVolume) return "Plowhorse";
            return "Dog";
        }

        #endregion

        #region Kitchen Tickets & Rail

        public KitchenTicket? GetTicketById(int ticketId) => ActiveTickets.FirstOrDefault(t => t.TicketId == ticketId);

        public void AddKitchenTicket(KitchenTicket ticket)
        {
            lock (_lock)
            {
                if (ticket.TicketId <= 0)
                {
                    ticket.TicketId = (ActiveTickets.Any() ? ActiveTickets.Max(t => t.TicketId) : 100) + 1;
                }
                if (string.IsNullOrWhiteSpace(ticket.OrderNumber))
                {
                    ticket.OrderNumber = (1080 + ticket.TicketId % 50).ToString();
                }
                ActiveTickets.Add(ticket);
                SyncNotifications();
                SaveToDisk();
            }
        }

        public void ToggleTicketRush(int ticketId)
        {
            lock (_lock)
            {
                var ticket = ActiveTickets.FirstOrDefault(t => t.TicketId == ticketId);
                if (ticket != null)
                {
                    ticket.IsRush = !ticket.IsRush;
                    SaveToDisk();
                }
            }
        }

        public void AdvanceTicketStatus(int ticketId)
        {
            lock (_lock)
            {
                var ticket = ActiveTickets.FirstOrDefault(t => t.TicketId == ticketId);
                if (ticket != null)
                {
                    if (ticket.Status == "Cooking")
                    {
                        ticket.Status = "Plated";
                    }
                    else if (ticket.Status == "Plated")
                    {
                        ticket.Status = "Served";
                    }
                    SaveToDisk();
                }
            }
        }

        public void BumpTicket(int ticketId)
        {
            lock (_lock)
            {
                var ticket = ActiveTickets.FirstOrDefault(t => t.TicketId == ticketId);
                if (ticket != null)
                {
                    ActiveTickets.Remove(ticket);
                    SyncNotifications();
                    SaveToDisk();
                }
            }
        }

        public void DeleteKitchenTicket(int ticketId) => BumpTicket(ticketId);

        #endregion

        #region Pantry & Inventory

        public decimal TotalStockParValue => PantryStock.Sum(p => (decimal)p.CurrentStock * p.UnitCost);

        public decimal SpoilageRisk48h => PantryStock
            .Where(p => p.DaysUntilExpiry <= 2)
            .Sum(p => (decimal)p.CurrentStock * p.UnitCost);

        public int BelowParCount => PantryStock.Count(p => p.IsLowStock);

        public int ExpiringSoonCount => PantryStock.Count(p => p.DaysUntilExpiry <= 2);

        public PantryItem? GetPantryItemById(int id) => PantryStock.FirstOrDefault(p => p.Id == id);

        public void AddPantryItem(PantryItem item)
        {
            lock (_lock)
            {
                if (item.Id <= 0)
                {
                    item.Id = (PantryStock.Any() ? PantryStock.Max(p => p.Id) : 0) + 1;
                }
                if (string.IsNullOrWhiteSpace(item.Sku))
                {
                    item.Sku = $"ITM-{item.Id:D3}";
                }
                if (item.MinThreshold > 0)
                {
                    item.ParPercentage = (int)Math.Round((item.CurrentStock / item.MinThreshold) * 100);
                }
                PantryStock.Add(item);
                SyncNotifications();
                SaveToDisk();
            }
        }

        public void UpdatePantryItem(PantryItem item)
        {
            lock (_lock)
            {
                var existing = PantryStock.FirstOrDefault(p => p.Id == item.Id);
                if (existing != null)
                {
                    existing.Name = item.Name;
                    existing.Sku = item.Sku;
                    existing.StorageZone = item.StorageZone;
                    existing.CurrentStock = item.CurrentStock;
                    existing.MinThreshold = item.MinThreshold;
                    existing.Unit = item.Unit;
                    existing.DaysUntilExpiry = item.DaysUntilExpiry;
                    existing.FreshnessBadge = item.FreshnessBadge;
                    existing.FreshnessVariant = item.FreshnessVariant;
                    existing.UnitCost = item.UnitCost;
                    existing.LotNumber = item.LotNumber;
                    existing.LocationRack = item.LocationRack;
                    existing.Supplier = item.Supplier;
                    if (existing.MinThreshold > 0)
                    {
                        existing.ParPercentage = (int)Math.Round((existing.CurrentStock / existing.MinThreshold) * 100);
                    }
                    SyncNotifications();
                    SaveToDisk();
                }
            }
        }

        public void DeletePantryItem(int id)
        {
            lock (_lock)
            {
                var item = PantryStock.FirstOrDefault(p => p.Id == id);
                if (item != null)
                {
                    PantryStock.Remove(item);
                    SyncNotifications();
                    SaveToDisk();
                }
            }
        }

        public void OrderStock(int id, double amount)
        {
            lock (_lock)
            {
                var item = PantryStock.FirstOrDefault(p => p.Id == id);
                if (item != null)
                {
                    item.CurrentStock += amount;
                    if (item.MinThreshold > 0)
                    {
                        item.ParPercentage = Math.Min(100, (int)Math.Round((item.CurrentStock / item.MinThreshold) * 100));
                    }
                    if (item.DaysUntilExpiry <= 1)
                    {
                        item.DaysUntilExpiry = 7;
                        item.FreshnessBadge = "7 days";
                        item.FreshnessVariant = "success";
                    }
                    SyncNotifications();
                    SaveToDisk();
                }
            }
        }

        public void BatchOrderStock(IEnumerable<string> skus, double amountPerItem = 10.0)
        {
            lock (_lock)
            {
                var skuSet = new HashSet<string>(skus, StringComparer.OrdinalIgnoreCase);
                foreach (var item in PantryStock.Where(p => skuSet.Contains(p.Sku)))
                {
                    item.CurrentStock += amountPerItem;
                    if (item.MinThreshold > 0)
                    {
                        item.ParPercentage = Math.Min(100, (int)Math.Round((item.CurrentStock / item.MinThreshold) * 100));
                    }
                    if (item.DaysUntilExpiry <= 1)
                    {
                        item.DaysUntilExpiry = 5;
                        item.FreshnessBadge = "5 days";
                        item.FreshnessVariant = "success";
                    }
                }
                SyncNotifications();
                SaveToDisk();
            }
        }

        #endregion

        #region Shift & Station

        public void ToggleShift()
        {
            lock (_lock)
            {
                IsShiftActive = !IsShiftActive;
                SaveToDisk();
            }
        }

        public void UpdateStationSettings(string station, string role)
        {
            lock (_lock)
            {
                ActiveStation = station;
                ActiveRole = role;
                SaveToDisk();
            }
        }

        #endregion

        #region Notifications & Search

        public bool HasUnreadNotifications => Notifications.Any(n => !n.IsRead);

        public void MarkNotificationAsRead(int id)
        {
            lock (_lock)
            {
                var notif = Notifications.FirstOrDefault(n => n.Id == id);
                if (notif != null)
                {
                    notif.IsRead = true;
                    SaveToDisk();
                }
            }
        }

        public void MarkAllNotificationsAsRead()
        {
            lock (_lock)
            {
                foreach (var notif in Notifications)
                {
                    notif.IsRead = true;
                }
                SaveToDisk();
            }
        }

        public void SyncNotifications()
        {
            var updated = new List<SystemNotification>();
            int nextId = 1;

            // 1. Critical allergens in active tickets
            foreach (var ticket in ActiveTickets.Where(t => t.IsCriticalAllergen && t.Status != "Served"))
            {
                updated.Add(new SystemNotification
                {
                    Id = nextId++,
                    Title = $"CRITICAL ALLERGEN: Table {ticket.TableNumber}",
                    Message = $"{ticket.AllergyAlert} alert on active order #{ticket.OrderNumber}.",
                    Severity = "danger",
                    LinkUrl = "/kitchen",
                    Timestamp = ticket.CreatedAt,
                    IsRead = false
                });
            }

            // 2. High spoilage risk ingredients (<= 24h)
            foreach (var item in PantryStock.Where(p => p.DaysUntilExpiry <= 1))
            {
                updated.Add(new SystemNotification
                {
                    Id = nextId++,
                    Title = $"Freshness Risk: {item.Name}",
                    Message = $"{item.CurrentStock} {item.Unit} expiring in {item.FreshnessBadge}. Action required.",
                    Severity = "warning",
                    LinkUrl = "/pantry",
                    Timestamp = DateTime.UtcNow.AddMinutes(-30),
                    IsRead = false
                });
            }

            // 3. Below Par inventory
            foreach (var item in PantryStock.Where(p => p.IsLowStock).Take(3))
            {
                updated.Add(new SystemNotification
                {
                    Id = nextId++,
                    Title = $"Stock Par Low: {item.Name}",
                    Message = $"Current stock ({item.CurrentStock} {item.Unit}) is below minimum threshold ({item.MinThreshold} {item.Unit}).",
                    Severity = "info",
                    LinkUrl = "/pantry",
                    Timestamp = DateTime.UtcNow.AddHours(-1),
                    IsRead = false
                });
            }

            // Preserve read status of existing notifications if matching title
            foreach (var n in updated)
            {
                if (Notifications.Any(existing => existing.Title == n.Title && existing.IsRead))
                {
                    n.IsRead = true;
                }
            }

            Notifications = updated;
        }

        public List<SearchResultItem> SearchAll(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return new();
            var q = query.Trim();
            var results = new List<SearchResultItem>();

            // Dishes
            foreach (var dish in MenuItems.Where(m => m.Name.Contains(q, StringComparison.OrdinalIgnoreCase) || m.Category.Contains(q, StringComparison.OrdinalIgnoreCase)))
            {
                results.Add(new SearchResultItem
                {
                    Title = dish.Name,
                    Subtitle = $"{dish.Category} · {dish.MenuPrice:C} · {dish.ProfitMargin:0.0}% Margin",
                    Category = "Menu Recipe",
                    Url = $"/?dish={Uri.EscapeDataString(dish.Name)}",
                    IconName = "recipe"
                });
            }

            // Tickets
            foreach (var ticket in ActiveTickets.Where(t => t.TableNumber.ToString().Contains(q) || t.Station.Contains(q, StringComparison.OrdinalIgnoreCase) || t.Items.Any(i => i.Contains(q, StringComparison.OrdinalIgnoreCase))))
            {
                results.Add(new SearchResultItem
                {
                    Title = $"Table {ticket.TableNumber} Ticket",
                    Subtitle = $"Station: {ticket.Station} · {ticket.Status} · {ticket.Guests} guests",
                    Category = "Kitchen Rail",
                    Url = "/kitchen",
                    IconName = "rail"
                });
            }

            // Pantry
            foreach (var item in PantryStock.Where(p => p.Name.Contains(q, StringComparison.OrdinalIgnoreCase) || p.Sku.Contains(q, StringComparison.OrdinalIgnoreCase) || p.StorageZone.Contains(q, StringComparison.OrdinalIgnoreCase)))
            {
                results.Add(new SearchResultItem
                {
                    Title = item.Name,
                    Subtitle = $"{item.Sku} · {item.StorageZone} · {item.CurrentStock} {item.Unit}",
                    Category = "Pantry Stock",
                    Url = "/pantry",
                    IconName = "pantry"
                });
            }

            return results;
        }

        #endregion

        #region Seed Data

        private void SeedData()
        {
            // Seed Menu
            MenuItems = new List<MenuItem>
            {
                new()
                {
                    Id = 1,
                    Name = "Hamachi Crudo",
                    Category = "Starters",
                    MenuPrice = 18.00m,
                    FoodCost = 6.30m,
                    PrepLabor = 3.20m,
                    PrepMinutes = 8,
                    Allergens = "Fish",
                    IsGlutenFree = true,
                    IsAvailable = true,
                    WeeklyVolume = 142,
                    SubIngredients = new()
                    {
                        new() { Name = "Yellowtail loin", Detail = "120 g · sashimi grade", Cost = 4.20m },
                        new() { Name = "Yuzu ponzu dressing", Detail = "30 ml · house crafted", Cost = 1.10m },
                        new() { Name = "Serrano chili & radish", Detail = "Garnish slice", Cost = 1.00m },
                        new() { Name = "Station prep labor", Detail = "8 min line prep", Cost = 3.20m }
                    }
                },
                new()
                {
                    Id = 2,
                    Name = "Burrata & Persimmon",
                    Category = "Starters",
                    MenuPrice = 16.00m,
                    FoodCost = 5.60m,
                    PrepLabor = 2.80m,
                    PrepMinutes = 10,
                    Allergens = "Dairy",
                    IsGlutenFree = true,
                    IsAvailable = true,
                    WeeklyVolume = 118,
                    SubIngredients = new()
                    {
                        new() { Name = "Pugliese Burrata", Detail = "125 g · fresh orb", Cost = 3.20m },
                        new() { Name = "Fuyu Persimmon", Detail = "80 g · thin shaved", Cost = 1.40m },
                        new() { Name = "Aged saba vinegar & EVOO", Detail = "20 ml emulsion", Cost = 1.00m },
                        new() { Name = "Garde manger labor", Detail = "10 min plating", Cost = 2.80m }
                    }
                },
                new()
                {
                    Id = 3,
                    Name = "Little Gem Salad",
                    Category = "Starters",
                    MenuPrice = 14.00m,
                    FoodCost = 4.90m,
                    PrepLabor = 2.50m,
                    PrepMinutes = 7,
                    Allergens = "None",
                    IsGlutenFree = true,
                    IsVegan = true,
                    IsAvailable = true,
                    WeeklyVolume = 156,
                    SubIngredients = new()
                    {
                        new() { Name = "Little Gem heads", Detail = "2 heads · washed", Cost = 2.40m },
                        new() { Name = "Green goddess dressing", Detail = "40 ml · vegan herb base", Cost = 1.30m },
                        new() { Name = "Toasted breadcrumbs & herbs", Detail = "Crunch topping", Cost = 1.20m },
                        new() { Name = "Salad station labor", Detail = "7 min prep", Cost = 2.50m }
                    }
                },
                new()
                {
                    Id = 4,
                    Name = "Crispy Calamari Fritti",
                    Category = "Starters",
                    MenuPrice = 16.50m,
                    FoodCost = 5.80m,
                    PrepLabor = 3.50m,
                    PrepMinutes = 10,
                    Allergens = "Shellfish",
                    IsAvailable = true,
                    WeeklyVolume = 110,
                    SubIngredients = new()
                    {
                        new() { Name = "Atlantic squid tubes", Detail = "180 g · dusted", Cost = 3.80m },
                        new() { Name = "Calabrian chili aioli", Detail = "45 ml · scratch", Cost = 1.00m },
                        new() { Name = "Fried lemon wheels", Detail = "Seasoned", Cost = 1.00m },
                        new() { Name = "Fryer station labor", Detail = "10 min prep", Cost = 3.50m }
                    }
                },
                new()
                {
                    Id = 5,
                    Name = "Roasted Atlantic Cod",
                    Category = "Mains",
                    MenuPrice = 36.00m,
                    FoodCost = 12.60m,
                    PrepLabor = 4.80m,
                    PrepMinutes = 20,
                    Allergens = "Fish",
                    IsGlutenFree = true,
                    IsAvailable = true,
                    WeeklyVolume = 148,
                    SubIngredients = new()
                    {
                        new() { Name = "Atlantic cod loin", Detail = "220 g · portion", Cost = 8.20m },
                        new() { Name = "Miso marinade", Detail = "45 ml · sub-recipe", Cost = 1.85m },
                        new() { Name = "Baby bok choy", Detail = "160 g · trimmed", Cost = 2.55m },
                        new() { Name = "Prep & line labor", Detail = "12 min · blended rate", Cost = 4.80m }
                    }
                },
                new()
                {
                    Id = 6,
                    Name = "Dry-Aged Duck",
                    Category = "Mains",
                    MenuPrice = 38.00m,
                    FoodCost = 13.30m,
                    PrepLabor = 5.20m,
                    PrepMinutes = 22,
                    Allergens = "None",
                    IsGlutenFree = true,
                    IsAvailable = true,
                    WeeklyVolume = 64,
                    SubIngredients = new()
                    {
                        new() { Name = "Hudson Valley duck breast", Detail = "240 g · aged 14d", Cost = 9.50m },
                        new() { Name = "Parsnip pureé", Detail = "100 g · brown butter", Cost = 2.00m },
                        new() { Name = "Spiced cherry jus", Detail = "40 ml reduction", Cost = 1.80m },
                        new() { Name = "Roast station labor", Detail = "22 min pan sear", Cost = 5.20m }
                    }
                },
                new()
                {
                    Id = 7,
                    Name = "Wild Mushroom Risotto",
                    Category = "Mains",
                    MenuPrice = 28.00m,
                    FoodCost = 9.80m,
                    PrepLabor = 4.10m,
                    PrepMinutes = 18,
                    IsGlutenFree = true,
                    IsVegan = true,
                    Allergens = "Mushrooms",
                    IsAvailable = true,
                    WeeklyVolume = 95,
                    SubIngredients = new()
                    {
                        new() { Name = "Carnaroli rice", Detail = "120 g · arborio", Cost = 2.20m },
                        new() { Name = "Chanterelle & cremini", Detail = "150 g · sautéed", Cost = 5.40m },
                        new() { Name = "Vegetable stock & herbs", Detail = "350 ml simmer", Cost = 2.20m },
                        new() { Name = "Sauté labor", Detail = "18 min stir", Cost = 4.10m }
                    }
                },
                new()
                {
                    Id = 8,
                    Name = "Pan-Seared Duck Breast",
                    Category = "Mains",
                    MenuPrice = 34.00m,
                    FoodCost = 10.20m,
                    PrepLabor = 4.50m,
                    PrepMinutes = 18,
                    Allergens = "None",
                    IsAvailable = true,
                    WeeklyVolume = 78,
                    SubIngredients = new()
                    {
                        new() { Name = "Duck breast magret", Detail = "210 g · scored", Cost = 7.20m },
                        new() { Name = "Roasted root vegetables", Detail = "120 g medley", Cost = 1.80m },
                        new() { Name = "Black currant glaze", Detail = "30 ml glaze", Cost = 1.20m },
                        new() { Name = "Pan station labor", Detail = "18 min cook", Cost = 4.50m }
                    }
                },
                new()
                {
                    Id = 9,
                    Name = "Carnaroli Risotto",
                    Category = "Mains",
                    MenuPrice = 26.00m,
                    FoodCost = 7.80m,
                    PrepLabor = 3.80m,
                    PrepMinutes = 16,
                    IsGlutenFree = true,
                    Allergens = "Dairy",
                    IsAvailable = true,
                    WeeklyVolume = 135,
                    SubIngredients = new()
                    {
                        new() { Name = "Aged Carnaroli rice", Detail = "130 g portion", Cost = 2.50m },
                        new() { Name = "Parmigiano-Reggiano 24m", Detail = "45 g grated", Cost = 3.10m },
                        new() { Name = "White wine & shallot", Detail = "Reduction base", Cost = 2.20m },
                        new() { Name = "Line cook labor", Detail = "16 min", Cost = 3.80m }
                    }
                },
                new()
                {
                    Id = 10,
                    Name = "Charred Broccolini",
                    Category = "Mains",
                    MenuPrice = 15.00m,
                    FoodCost = 4.50m,
                    PrepLabor = 2.20m,
                    PrepMinutes = 9,
                    IsGlutenFree = true,
                    IsVegan = true,
                    Allergens = "None",
                    IsAvailable = true,
                    WeeklyVolume = 175,
                    SubIngredients = new()
                    {
                        new() { Name = "Organic broccolini", Detail = "180 g florets", Cost = 2.80m },
                        new() { Name = "Garlic confit & chili flake", Detail = "House oil", Cost = 1.00m },
                        new() { Name = "Charred lemon juice", Detail = "Half lemon", Cost = 0.70m },
                        new() { Name = "Grill station labor", Detail = "9 min char", Cost = 2.20m }
                    }
                },
                new()
                {
                    Id = 11,
                    Name = "Warm Olive Cake",
                    Category = "Desserts",
                    MenuPrice = 13.00m,
                    FoodCost = 3.90m,
                    PrepLabor = 2.40m,
                    PrepMinutes = 12,
                    Allergens = "Gluten, Dairy",
                    IsAvailable = true,
                    WeeklyVolume = 48,
                    SubIngredients = new()
                    {
                        new() { Name = "Castelvetrano olive oil cake", Detail = "1 slice", Cost = 2.10m },
                        new() { Name = "Mascarpone whipped cream", Detail = "40 g dollop", Cost = 1.10m },
                        new() { Name = "Rosemary infused honey", Detail = "15 ml drizzle", Cost = 0.70m },
                        new() { Name = "Pastry labor", Detail = "12 min plating", Cost = 2.40m }
                    }
                },
                new()
                {
                    Id = 12,
                    Name = "Valrhona Chocolate Soufflé",
                    Category = "Desserts",
                    MenuPrice = 14.00m,
                    FoodCost = 3.50m,
                    PrepLabor = 3.00m,
                    PrepMinutes = 20,
                    Allergens = "Dairy, Eggs",
                    IsAvailable = false,
                    WeeklyVolume = 85,
                    SubIngredients = new()
                    {
                        new() { Name = "Valrhona 70% Guanaja", Detail = "60 g melted", Cost = 2.00m },
                        new() { Name = "Farm egg whites & sugar", Detail = "Whipped meringue", Cost = 0.80m },
                        new() { Name = "Crème anglaise sauce", Detail = "30 ml warm pour", Cost = 0.70m },
                        new() { Name = "Pastry bake labor", Detail = "20 min bake", Cost = 3.00m }
                    }
                }
            };

            // Seed Active Tickets
            ActiveTickets = new List<KitchenTicket>
            {
                new()
                {
                    TicketId = 101,
                    TableNumber = 4,
                    Guests = 2,
                    ServerName = "Chloe T.",
                    OrderNumber = "1084",
                    ActiveCourse = "Starters",
                    Station = "Garde Manger",
                    ElapsedMinutes = 6,
                    ElapsedTimeString = "06:15",
                    AllergyAlert = "None",
                    IsRush = false,
                    Status = "Cooking",
                    Items = new() { "2 Hamachi Crudo", "1 Little Gem Salad", "1 Burrata & Persimmon" },
                    Courses = new()
                    {
                        new()
                        {
                            CourseName = "Course 1 (Apps)",
                            Status = "Plated",
                            Items = new()
                            {
                                new() { Quantity = 2, Name = "Hamachi Crudo", Modifier = "Extra citrus" },
                                new() { Quantity = 1, Name = "Little Gem Salad", Modifier = "No scallions" }
                            }
                        },
                        new()
                        {
                            CourseName = "Course 2 (Mains)",
                            Status = "Fire",
                            Items = new()
                            {
                                new() { Quantity = 1, Name = "Roasted Atlantic Cod" }
                            }
                        }
                    }
                },
                new()
                {
                    TicketId = 102,
                    TableNumber = 7,
                    Guests = 3,
                    ServerName = "Marcus L.",
                    OrderNumber = "1091",
                    ActiveCourse = "Mains",
                    Station = "Hot Line",
                    ElapsedMinutes = 14,
                    ElapsedTimeString = "14:18",
                    AllergyAlert = "Severe Peanut Anaphylaxis",
                    IsRush = true,
                    Status = "Cooking",
                    Items = new() { "1 Roasted Atlantic Cod", "1 Charred Broccolini", "1 Carnaroli Risotto" },
                    Courses = new()
                    {
                        new()
                        {
                            CourseName = "Course 1 (Apps)",
                            Status = "Plated",
                            Items = new()
                            {
                                new() { Quantity = 1, Name = "Charred Broccolini", AllergyBadge = "ALLERGY", Modifier = "Clean pan · fresh gloves" }
                            }
                        },
                        new()
                        {
                            CourseName = "Course 2 (Mains)",
                            Status = "Fire",
                            Items = new()
                            {
                                new() { Quantity = 1, Name = "Roasted Atlantic Cod", AllergyBadge = "NO PEANUT" },
                                new() { Quantity = 1, Name = "Carnaroli Risotto", Modifier = "No shared garnish" }
                            }
                        }
                    }
                },
                new()
                {
                    TicketId = 103,
                    TableNumber = 12,
                    Guests = 2,
                    ServerName = "Ana R.",
                    OrderNumber = "1097",
                    ActiveCourse = "Mains",
                    Station = "Sauté",
                    ElapsedMinutes = 9,
                    ElapsedTimeString = "09:42",
                    AllergyAlert = "None",
                    IsRush = false,
                    Status = "Cooking",
                    Items = new() { "1 Dry-Aged Duck", "1 Wild Mushroom Risotto", "2 Warm Olive Cake" },
                    Courses = new()
                    {
                        new()
                        {
                            CourseName = "Course 1 (Apps)",
                            Status = "Plated",
                            Items = new()
                            {
                                new() { Quantity = 1, Name = "Burrata & Persimmon" }
                            }
                        },
                        new()
                        {
                            CourseName = "Course 2 (Mains)",
                            Status = "Fire",
                            Items = new()
                            {
                                new() { Quantity = 1, Name = "Dry-Aged Duck", Modifier = "Medium rare" },
                                new() { Quantity = 1, Name = "Wild Mushroom Risotto" }
                            }
                        }
                    }
                }
            };

            // Seed Pantry Inventory
            PantryStock = new List<PantryItem>
            {
                new() { Id = 1, Name = "Atlantic Cod", Sku = "SEA-1048", StorageZone = "Walk-in Cooler", CurrentStock = 36.0, MinThreshold = 50.0, Unit = "lb", DaysUntilExpiry = 1, FreshnessBadge = "1d 08h", FreshnessVariant = "warning", ParPercentage = 72, UnitCost = 14.20m, LotNumber = "Lot #9021 · Harbor Fish Co.", LocationRack = "Walk-in · Shelf B2", Supplier = "Harbor Fish Co." },
                new() { Id = 2, Name = "Heirloom Tomatoes", Sku = "PRO-2231", StorageZone = "Walk-in Cooler", CurrentStock = 23.0, MinThreshold = 50.0, Unit = "lb", DaysUntilExpiry = 0, FreshnessBadge = "18h", FreshnessVariant = "danger", ParPercentage = 46, UnitCost = 4.50m, LotNumber = "Lot #4418 · Field & Vine", LocationRack = "Produce · Rack A1", Supplier = "Field & Vine" },
                new() { Id = 3, Name = "Carnaroli Rice", Sku = "DRY-0812", StorageZone = "Dry Larder", CurrentStock = 44.0, MinThreshold = 50.0, Unit = "lb", DaysUntilExpiry = 42, FreshnessBadge = "42 days", FreshnessVariant = "success", ParPercentage = 88, UnitCost = 3.80m, LotNumber = "Lot #7714 · Urbani Foods", LocationRack = "Dry Store · Bin 14", Supplier = "Urbani Foods" },
                new() { Id = 4, Name = "Microgreens", Sku = "PRO-2904", StorageZone = "Walk-in Cooler", CurrentStock = 6.2, MinThreshold = 20.0, Unit = "lb", DaysUntilExpiry = 0, FreshnessBadge = "09h", FreshnessVariant = "danger", ParPercentage = 31, UnitCost = 18.00m, LotNumber = "Lot #1172 · City Sprout Farm", LocationRack = "Walk-in · Shelf A3", Supplier = "City Sprout Farm" },
                new() { Id = 5, Name = "Duck Breast (Magret)", Sku = "MT-205", StorageZone = "Walk-in Cooler", CurrentStock = 12.6, MinThreshold = 30.0, Unit = "lb", DaysUntilExpiry = 2, FreshnessBadge = "2 days", FreshnessVariant = "warning", ParPercentage = 42, UnitCost = 28.00m, LotNumber = "Lot #3391 · Hudson Valley", LocationRack = "Walk-in · Shelf C1", Supplier = "Hudson Valley" },
                new() { Id = 6, Name = "Valrhona Cocoa & Chocolate", Sku = "PST-019", StorageZone = "Dry Larder", CurrentStock = 28.5, MinThreshold = 30.0, Unit = "lb", DaysUntilExpiry = 90, FreshnessBadge = "90 days", FreshnessVariant = "success", ParPercentage = 95, UnitCost = 16.50m, LotNumber = "Lot #1204 · Valrhona USA", LocationRack = "Dry Store · Bin 08", Supplier = "Valrhona USA" },
                new() { Id = 7, Name = "Heavy Whipping Cream 36%", Sku = "DRY-012", StorageZone = "Walk-in Cooler", CurrentStock = 11.0, MinThreshold = 30.0, Unit = "qt", DaysUntilExpiry = 1, FreshnessBadge = "1d 04h", FreshnessVariant = "warning", ParPercentage = 37, UnitCost = 6.20m, LotNumber = "Lot #5512 · Ronnybrook", LocationRack = "Walk-in · Shelf C1", Supplier = "Ronnybrook" },
                new() { Id = 8, Name = "Atlantic Squid Tubes", Sku = "FSH-412", StorageZone = "Freezer", CurrentStock = 32.5, MinThreshold = 50.0, Unit = "lb", DaysUntilExpiry = 30, FreshnessBadge = "30 days", FreshnessVariant = "success", ParPercentage = 65, UnitCost = 12.50m, LotNumber = "Lot #8012 · Point Judith", LocationRack = "Freezer · Unit 02", Supplier = "Point Judith" }
            };

            SyncNotifications();
        }

        #endregion
    }
}
