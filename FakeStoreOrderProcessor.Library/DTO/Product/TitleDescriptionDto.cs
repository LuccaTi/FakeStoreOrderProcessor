using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Library.DTO.Product
{
    public class TitleDescriptionDto
    {
        [Required]
        public string? Title { get; set; }
        [Required]
        public string? Description { get; set; }
    }
}
