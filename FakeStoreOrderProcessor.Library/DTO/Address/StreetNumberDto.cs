using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Library.DTO.Address
{
    public class StreetNumberDto
    {
        [Required]
        public string? Street { get; set; }
        [Required]
        public int Number { get; set; }
    }
}
