using System.ComponentModel.DataAnnotations;

namespace FoodSprint.Models
{
    public class MenuItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Dish name is required.")]
        [StringLength(40, MinimumLength = 2, ErrorMessage = "Name must be 2–40 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required.")]
        public string Category { get; set; } = "Mains";

        [Required]
        [Range(1.00, 500.00, ErrorMessage = "Price must be between $1.00 and $500.00.")]
        public decimal MenuPrice { get; set; } = 18.00m;

        [Required]
        [Range(0.10, 200.00, ErrorMessage = "Cost must be between $0.10 and $200.00.")]
        public decimal FoodCost { get; set; } = 5.50m;

        public decimal PrepLabor { get; set; } = 4.80m;
        public int PrepMinutes { get; set; } = 15;
        public bool IsGlutenFree { get; set; }
        public bool IsVegan { get; set; }
        public string Allergens { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true;

        public decimal ProfitMargin => MenuPrice > 0 ? ((MenuPrice - FoodCost) / MenuPrice) * 100 : 0;
    }

    public class KitchenTicketItemDetail
    {
        public string Name { get; set; } = string.Empty;
        public string? Badge { get; set; }
        public string? Note { get; set; }
    }

    public class KitchenTicket
    {
        public int TicketId { get; set; }
        public int TableNumber { get; set; }
        public int Guests { get; set; }
        public string ActiveCourse { get; set; } = "Mains"; // Starters, Mains, Dessert
        public List<string> Items { get; set; } = new();
        public List<KitchenTicketItemDetail> ItemDetails { get; set; } = new();
        public string Station { get; set; } = "Grill"; // Sauté, Grill, Pastry, Cold Bar, Garde Manger, Hot Line
        public string AllergyAlert { get; set; } = string.Empty;
        public int ElapsedMinutes { get; set; } = 8;
        public string ElapsedTimeString { get; set; } = string.Empty;
        public bool IsRush { get; set; }
        public string Status { get; set; } = "Cooking"; // Cooking, Plated, Served
        public bool ExpoCalled { get; set; }
    }

    public class PantryItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Item name is required.")]
        public string Name { get; set; } = string.Empty;

        public string Sku { get; set; } = string.Empty;
        public string StorageZone { get; set; } = "Walk-in Cooler"; // Walk-in Cooler, Dry Larder, Freezer
        public double CurrentStock { get; set; } = 5.0;
        public double MinThreshold { get; set; } = 10.0;
        public string Unit { get; set; } = "lb";
        public int DaysUntilExpiry { get; set; } = 4;
        public string FreshnessBadge { get; set; } = string.Empty; // e.g. "1d 08h", "18h", "42 days", "09h"
        public string FreshnessVariant { get; set; } = "success"; // success, warning, danger
        public int ParPercentage { get; set; } = 72; // e.g. 72, 46, 88, 31
        public decimal UnitCost { get; set; } = 12.00m;

        public bool IsLowStock => CurrentStock <= MinThreshold || ParPercentage < 50;
    }
}