using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Library.DTO.Json
{
    public class NameDto
    {
        [Required(AllowEmptyStrings = false, ErrorMessage= "First name cannot be null or empty!")]
        public string? FirstName { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Last name cannot be null or empty!")]
        public string? LastName { get; set; }
    }
}
