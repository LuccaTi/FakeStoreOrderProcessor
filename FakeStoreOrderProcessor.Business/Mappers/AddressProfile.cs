using AutoMapper;
using FakeStoreOrderProcessor.Library.DTO.Address;
using FakeStoreOrderProcessor.Library.DTO.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Business.Mappers
{
    public class AddressProfile : Profile
    {
        public AddressProfile()
        {
            CreateMap<AddressJsonDto, CreateAddressDto>();
        }
    }
}
