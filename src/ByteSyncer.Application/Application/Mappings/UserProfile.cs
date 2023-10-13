using AutoMapper;
using ByteSyncer.Core.CQRS.Application.Commands;
using ByteSyncer.Core.CQRS.Application.Queries;
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
            CreateMap<User, ValidateCredentialsCommand>().ReverseMap();
            CreateMap<User, RegisterInput>().ReverseMap();
            CreateMap<User, RegisterUserCommand>().ReverseMap();

            CreateMap<LoginInput, ValidateCredentialsCommand>().ReverseMap();
            CreateMap<RegisterInput, RegisterUserCommand>().ReverseMap();

            CreateMap<ValidateCredentialsCommand, FindUserUsingEmailQuery>().ReverseMap();
        }
    }
}
