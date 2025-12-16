using AutoMapper;
using FakeStoreOrderProcessor.Library.DTO.Customer;
using FakeStoreOrderProcessor.Library.DTO.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Business.Mappers
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            CreateMap<CustomerJsonDto, CreateCustomerDto>();
            CreateMap<CustomerJsonDto, UpdateCustomerDto>();
        }
    }
}
