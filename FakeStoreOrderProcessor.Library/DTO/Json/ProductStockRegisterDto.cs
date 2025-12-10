using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Library.DTO.Json
{
    public class ProductStockRegisterDto
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Product title cannot be null or empty!")]
        public string? Title { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Product price must be greater than zero!")]
        public decimal Price { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Product description cannot be null or empty!")]
        public string? Description { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Product category cannot be null or empty!")]
        public string? Category { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Product image URL cannot be null or empty!")]
        public string? Image { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Product quantity must be at least 1!")]
        public int QuantityToRegister { get; set; }
    }
}
