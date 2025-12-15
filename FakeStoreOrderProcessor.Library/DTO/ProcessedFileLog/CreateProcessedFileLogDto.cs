using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Library.DTO.ProcessedFileLog
{
    public class CreateProcessedFileLogDto
    {
        [Required]
        public string? FileName { get; set; }
    }
}
