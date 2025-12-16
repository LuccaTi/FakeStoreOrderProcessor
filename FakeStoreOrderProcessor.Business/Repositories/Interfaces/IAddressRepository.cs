using FakeStoreOrderProcessor.Library.DTO.Address;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Business.Repositories.Interfaces
{
    public interface IAddressRepository : IApiRepository<AddressDto, CreateAddressDto, UpdateAddressDto>
    {
        public Task<AddressDto?> GetByStreetNumberAsync(StreetNumberDto streetNumberDto, CancellationToken cancellationToken);
    }
}
