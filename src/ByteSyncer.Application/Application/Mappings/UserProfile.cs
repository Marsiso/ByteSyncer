using AutoMapper;
using ByteSyncer.Core.Application.Commands;
using ByteSyncer.Domain.Application.DataTransferObjects;
using ByteSyncer.Domain.Application.Models;

namespace ByteSyncer.Application.Application.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, User>();
            CreateMap<User, RegisterInput>();
            CreateMap<User, RegisterCommand>().ReverseMap();

            CreateMap<RegisterInput, RegisterCommand>().ReverseMap();
        }
    }
}
