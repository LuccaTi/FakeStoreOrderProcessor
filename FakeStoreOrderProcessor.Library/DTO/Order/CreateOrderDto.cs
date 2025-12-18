using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Library.DTO.Order
{
    public class CreateOrderDto
    {
        [Required]
        public string? OrderGuid { get; set; }
        [Required]
        public long CustomerId { get; set; }
        [Required]
        public DateTime OrderDate { get; set; }
        public DateTime PaymentDate { get; set; }
        [Required]
        public decimal TotalPrice { get; set; }
        public DateTime ShippedDate { get; set; }
        public DateTime DeliveredDate { get; set; }
        [Required]
        public string? OrderStatus { get; set; }
        [Required]
        public string? PaymentStatus { get; set; }
        public string? ShippingStatus { get; set; }
        [Required]
        public List<CreateOrderItemDto> OrderItems { get; set; } = new List<CreateOrderItemDto>();
    }
}
