using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Library.Validation
{
    public class PastOrPresentDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not DateTime date)
                return new ValidationResult("A valid date must be provided!");

            if (date > DateTime.Now)
                return new ValidationResult("The date cannot be in the future!");

            return ValidationResult.Success;
        }
    }
}
