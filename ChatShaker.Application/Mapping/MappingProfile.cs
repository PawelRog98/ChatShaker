using AutoMapper;
using ChatShaker.Application.Users.Commands.Login;
using ChatShaker.Application.Users.Commands.Register;
using ChatShaker.Application.Users.Commands.Shared;
using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Models.Authentication;
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
            CreateMap<AuthTokenDto, AuthTokenModel>();
        }
        private void RegisterMappings()
        {
        }
    }
}
