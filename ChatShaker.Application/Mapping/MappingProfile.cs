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
using ChatShaker.Application.Chats.Queries.GetUserRooms;
using ChatShaker.Application.Friendships.Query;
using ChatShaker.Application.Users.Commands.SaveIdentity;
using ChatShaker.Application.Users.Queries.GetFriends;

namespace ChatShaker.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            LoginMappings();
            RegisterMappings();
            Usermappings();
            ChatRoomMappings();
            KeysMappings();
            FriendRequestsMappings();
        }

        private void Usermappings()
        {
            CreateMap<User, UserModel>()
                .ForMember(d => d.RoleName, o => o.MapFrom(s => s.Role.RoleName));

            CreateMap<AuthTokenModel, AuthTokenDto>();

            CreateMap<User, UserInfoDto>()
                .ForMember(d => d.Name, o => o.MapFrom(s => s.FirstName))
                .ForMember(d => d.PublicId, o => o.MapFrom(s => s.PublicId));
        }
        private void LoginMappings()
        {
            CreateMap<AuthTokenDto, AuthTokenModel>();
        }
        private void RegisterMappings()
        {
        }

        private void ChatRoomMappings()
        {
            CreateMap<ChatRoom, ChatListItemDto>()
                .ForMember(d => d.RoomPublicId, o => o.MapFrom(s => s.PublicId))
                .ForMember(d => d.Name, o => o.MapFrom(s => s.Name));
        }

        private void KeysMappings()
        {
            CreateMap<UserPublicKey, UserKeyDataDto>()
                .ForMember(d => d.PublicUserId, o => o.MapFrom(s => s.User.PublicId));
        }

        private void FriendRequestsMappings()
        {
            CreateMap<FriendRequest, UserRequestsDto>()
                .ForMember(d => d.Username,o => o.MapFrom(s => s.Sender == null ? s.Recipient.FirstName : s.Sender.FirstName))
                .ForMember(d=>d.SentAtUtc, o=> o.MapFrom(s=>s.CreatedAtUtc));
        }
            
    }
}
