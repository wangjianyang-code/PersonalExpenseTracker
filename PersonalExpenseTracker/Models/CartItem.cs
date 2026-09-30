using System.ComponentModel.DataAnnotations;

namespace PersonalExpenseTracker.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Description { get; set; } = string.Empty;

        [Range(1, 100)]
        public int Quantity { get; set; } = 1;

        [Range(0.01, 999999999)]
        public decimal UnitPrice { get; set; }

        public decimal Total
        {
            get
            {
                return Quantity * UnitPrice;
            }
        }
    }
}