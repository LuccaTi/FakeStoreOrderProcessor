using AutoMapper;
using FakeStoreOrderProcessor.Library.DTO.Json;
using FakeStoreOrderProcessor.Library.DTO.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Business.Mappers
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<OrderCreatedDto, CreateOrderDto>();
            CreateMap<OrderPaymentConfirmedDto, UpdateOrderDto>();
            CreateMap<OrderShippedDto, UpdateOrderDto>();
            CreateMap<OrderDeliveredDto, UpdateOrderDto>();
        }
    }
}
