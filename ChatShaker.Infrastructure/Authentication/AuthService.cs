using ChatShaker.Application.Users.Commands.Shared;
using ChatShaker.Core.Interfaces.Authentication;
using ChatShaker.Core.Models.Authentication;
using ChatShaker.Core.Models.Authorization;
using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Enums;
using ChatShaker.Domain.Exceptions;
using ChatShaker.Domain.Repositories;
using ChatShaker.Infrastructure.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.Infrastructure.Authentication
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenRepository _tokenRepository;
        private readonly JwtSettings _authSettings;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AuthService(IUserRepository userRepository, JwtSettings jwtSettings, IPasswordHasher<User> passwordHasher, ITokenRepository tokenRepository)
        {
            _userRepository = userRepository;
            _authSettings = jwtSettings;
            _passwordHasher = passwordHasher;
            _tokenRepository = tokenRepository;
        }

        //public async Task<AuthTokenModel> Login(LoginModel loginModel) 
        //{
        //    var user = await _userRepository.GetUserByEmail(loginModel.Email);

        //    if (user == null)
        //    {
        //        throw new BadAuthenticationException("Invalid user data.");
        //    }

        //    //if (user.Suspensions.Any(x=>x.Status == SuspensionStatus.Active) == true)
        //    //{
        //    //    throw new SuspendedUserException(user.IsSuspendedUntilDate.Value);
        //    //}
        //    if (user.IsEmailConfirmed == false)
        //    {
        //        throw new NotActiveUserException();
        //    }

        //    var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginModel.Password);
        //    if (result == PasswordVerificationResult.Failed)
        //    {
        //        throw new BadAuthenticationException("Invalid user data.");
        //    }

        //    var token = await GenerateJwtToken(user);
        //    return token;
        //}

        public async Task<AuthTokenModel> GenerateJwtToken(UserModel user, CancellationToken cancellationToken)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FirstName + user.LastName),
                new Claim(ClaimTypes.Role, user.RoleName),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authSettings.JwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expireDays = DateTime.Now.AddMinutes(_authSettings.JwtExpireMinutes);

            var token = new JwtSecurityToken(_authSettings.JwtIssuer, _authSettings.JwtIssuer, claims, expires: expireDays, signingCredentials: credentials);

            var tokenHandler = new JwtSecurityTokenHandler();

            var refreshToken = await CreateRefreshToken(user.Id, cancellationToken);

            var accessToken = tokenHandler.WriteToken(token);

            return new AuthTokenModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.TokenData,
                UserNick = user.PublicNick
            };
        }

        private async Task<Token> CreateRefreshToken(long userId, CancellationToken cancellationToken)
        {
            var token = new Token
            {
                TokenData = Guid.NewGuid().ToString(),
                ExpireDateTime = DateTime.UtcNow.AddDays(30),
                TokenType = TokenType.RefreshToken,
                UserId = userId
            };

            await _tokenRepository.CreateToken(token, cancellationToken);
            return token;

        }
    }
}
