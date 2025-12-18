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
    public class OrderPaymentConfirmedDto
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Order guid cannot be null or empty!")]
        public string? OrderGuid { get; set; }
        [EnumDataType(typeof(OrderStatus), ErrorMessage = "Order invalid order status value!")]
        public OrderStatus OrderStatus { get; set; }
        [EnumDataType(typeof(PaymentStatus), ErrorMessage = "Order invalid Payment Status value!")]
        public PaymentStatus PaymentStatus { get; set; }
        [PastOrPresentDate]
        public DateTime PaymentDate { get; set; }
        [Range(0.01, double.MaxValue, ErrorMessage = "Order price must be greater than zero!")]
        public decimal TotalPrice { get; set; }

    }
}
