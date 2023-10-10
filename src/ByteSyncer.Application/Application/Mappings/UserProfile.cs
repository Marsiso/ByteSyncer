using AutoMapper;
using ByteSyncer.Core.Application.Commands;
using ByteSyncer.Core.Application.Queries;
using ByteSyncer.Domain.Application.DataTransferObjects;
using ByteSyncer.Domain.Application.Models;

namespace ByteSyncer.Application.Application.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, User>();
            CreateMap<User, LoginInput>().ReverseMap();
            CreateMap<User, LoginCommand>().ReverseMap();
            CreateMap<User, RegisterInput>().ReverseMap();
            CreateMap<User, RegisterCommand>().ReverseMap();

            CreateMap<LoginInput, LoginCommand>().ReverseMap();
            CreateMap<RegisterInput, RegisterCommand>().ReverseMap();

            CreateMap<LoginCommand, FindUserByEmailQuery>().ReverseMap();
        }
    }
}
