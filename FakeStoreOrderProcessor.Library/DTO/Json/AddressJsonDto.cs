using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Library.DTO.Json
{
    public class AddressJsonDto
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Address city cannot be null or empty!")]
        public string? City { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Address street cannot be null or empty!")]
        public string? Street { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Address number must be greater than zero!")]
        public int Number { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Address zipcode cannot be null or empty!")]
        public string? Zipcode { get; set; }
    }
}
