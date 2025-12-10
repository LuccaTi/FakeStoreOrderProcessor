using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Library.DTO.Json
{
    public class CustomerJsonDto
    {
        [Required]
        public AddressJsonDto? Address { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Customer email cannot be null or empty!")]
        public string? Email { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Customer username cannot be null or empty!")]
        public string? Username { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Customer password cannot be null or empty!")]
        public string? Password { get; set; }
        [Required]
        public NameDto? Name { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Customer phone cannot be null or empty!")]
        public string? Phone { get; set; }
    }
}
