using ChatShaker.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.Domain.Entities
{
    public class User : ICommonData
    {
        public User()
        {
            Tokens = new HashSet<Token>();
            Suspensions = new HashSet<Suspension>();
            ReceivedFriendRequests = new HashSet<FriendRequest>();
            SentFriendRequests = new HashSet<FriendRequest>();
        }
        public long Id { get; set; }
        public Guid PublicId { get; set; }
        public string PublicNick {  get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public DateTime? LastActivityDateTime { get; set; }
        public long? RoleId { get; set; }
        public virtual Role Role { get; set; }
        public string? AccountInfo { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? ModifiedAtUtc { get; set; }
        public string UserInvitationCode { get; set; }

        public ICollection<Token> Tokens { get; set; }
        public ICollection<Suspension> Suspensions { get; set;}
        public ICollection<FriendRequest> SentFriendRequests { get; set; }
        public ICollection<FriendRequest> ReceivedFriendRequests { get; set; }
        //public ICollection<Friendship>  Friendships { get; set; }
    }
}
