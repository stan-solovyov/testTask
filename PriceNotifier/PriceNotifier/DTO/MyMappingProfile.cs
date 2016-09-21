﻿using AutoMapper;
using BLL;
using Domain.Entities;

namespace PriceNotifier.DTO
{
    public class MyMappingProfile:Profile
    {
        public MyMappingProfile()
        {
            CreateMap<Product, ProductDto>().ForMember("Id",opt=>opt.MapFrom(c=>c.ProductId));
            CreateMap<ProductDto, Product>();
            CreateMap<Product, Product>();
            CreateMap<User, UserDto>();
            CreateMap<UserDto, User>();
            CreateMap<UserFromDbWithCount, UserDtoWithCount>();
        }
    }
}