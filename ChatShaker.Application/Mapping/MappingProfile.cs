using AutoMapper;
using ChatShaker.Application.Users.Commands.Login;
using ChatShaker.Application.Users.Commands.Register;
using ChatShaker.Application.Users.Commands.Shared;
using ChatShaker.Core.Models.Authentication;
using ChatShaker.Core.Models.Authorization;
using ChatShaker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            LoginMappings();
            RegisterMappings();
            Usermappings();
        }

        private void Usermappings()
        {
            CreateMap<User, UserModel>()
                .ForMember(d => d.RoleName, o => o.MapFrom(s => s.Role.RoleName));

            CreateMap<AuthTokenModel, AuthTokenDto>();
        }
        private void LoginMappings()
        {
            CreateMap<LoginDto, LoginModel>();
            CreateMap<AuthTokenDto, AuthTokenModel>();
        }
        private void RegisterMappings()
        {
            CreateMap<RegisterDto, RegisterModel>();
        }
    }
}
