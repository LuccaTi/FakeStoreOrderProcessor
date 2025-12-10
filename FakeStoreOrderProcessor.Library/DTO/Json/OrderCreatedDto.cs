using FakeStoreOrderProcessor.Library.Enums;
using FakeStoreOrderProcessor.Library.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Library.DTO.Json
{
    public class OrderCreatedDto
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Order guid cannot be null or empty")]
        public string? OrderGuid { get; set; }
        [Required]
        public CustomerJsonDto? Customer { get; set; }
        [Required(ErrorMessage = "The list of items cannot be null!")]
        [MinLength(1, ErrorMessage = "The order must contains at least one item.")]
        public List<ItemDto>? Items { get; set; }
        [Range(0.01, double.MaxValue, ErrorMessage = "Order total price must be greater than zero!")]
        public decimal TotalPrice { get; set; }
        [PastOrPresentDate]
        public DateTime OrderDate { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime ShippedDate { get; set; }
        public DateTime DeliveredDate { get; set; }
        [EnumDataType(typeof(PaymentStatus), ErrorMessage = "Order invalid payment status value!")]
        public PaymentStatus PaymentStatus { get; set; }
        [EnumDataType(typeof(ShippingStatus), ErrorMessage = "Order invalid shipping status value!")]
        public ShippingStatus ShippingStatus { get; set; }
    }
}
