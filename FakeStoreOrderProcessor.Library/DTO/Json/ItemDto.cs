using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Library.DTO.Json
{
    public class ItemDto
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Item title cannot be null or empty!")]
        public string? Title { get; set; }
        [Range(0.01, double.MaxValue, ErrorMessage = "Item price must be greater than zero!")]
        public decimal Price { get; set; }
        [Range(0.01, double.MaxValue, ErrorMessage = "Item total price must be greater than zero!")]
        public decimal TotalPrice { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Item description cannot be null or empty!")]
        public string? Description { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Item category cannot be null or empty!")]
        public string? Category { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Item image cannot be null or empty!")]
        public string? Image { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Item quantity must be at least 1!")]
        public int Quantity { get; set; }
    }
}
