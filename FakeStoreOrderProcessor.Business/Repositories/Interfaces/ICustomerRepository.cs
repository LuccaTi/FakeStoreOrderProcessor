using FakeStoreOrderProcessor.Library.DTO.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Business.Repositories.Interfaces
{
    public interface ICustomerRepository : IApiRepository<CustomerDto, CreateCustomerDto, UpdateCustomerDto>
    {
        public Task<CustomerDto?> GetByLoginRequestAsync(LoginRequestDto loginRequest, CancellationToken cancellationToken);
    }
}
