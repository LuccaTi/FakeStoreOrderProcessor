using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Library.Validation
{
    public static class DtoValidator
    {
        public static void Validate<T>(T dto) where T : class
        {
            var validationContext = new ValidationContext(dto, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(dto, validationContext, validationResults, validateAllProperties: true);

            if (!isValid)
            {
                string dtoTypeName = typeof(T).Name;
                var errorMessages = validationResults.Select(r => r.ErrorMessage);
                string fullErrorMessage = string.Join("\n", errorMessages);
                throw new ValidationException($"Invalid DTO: {dtoTypeName} - The following errors occurred:\n{fullErrorMessage}\n");
            }
        }
    }
}
