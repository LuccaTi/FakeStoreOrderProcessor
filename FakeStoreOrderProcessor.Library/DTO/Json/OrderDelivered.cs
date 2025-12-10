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
    public class OrderDelivered
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Order guid cannot be null or empty!")]
        public string? OrderGuid { get; set; }
        [EnumDataType(typeof(ShippingStatus), ErrorMessage = "Order invalid Shipping Status value!")]
        public ShippingStatus ShippingStatus { get; set; }
        [PastOrPresentDate]
        public DateTime DeliveredDate { get; set; }
    }
}
