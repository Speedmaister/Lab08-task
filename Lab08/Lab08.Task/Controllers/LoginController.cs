using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Lab08.Task.Models.Configuration;
using Lab08.Task.Models.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Lab08.Task.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly JwtSettings jwtSettings;
        private List<UserLoginModel> appUsers = new List<UserLoginModel>
        {
            new UserLoginModel { FullName = "Radoslav Sholev", UserName = "admin", Password = "1234", UserRole = "Admin" },
            new UserLoginModel { FullName = "Test User", UserName = "user", Password = "1234", UserRole = "User" }
        };

        public LoginController(JwtSettings jwtSettings)
        {
            this.jwtSettings = jwtSettings;
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Login([FromBody] UserLoginModel login)
        {
            UserLoginModel user = AuthenticateUser(login);
            if (user != null)
            {
                var token = GenerateJWTToken(user);
                return Ok(new
                {
                    token = token,
                    userDetails = user,
                });
            }

            return Unauthorized();
        }

        private UserLoginModel AuthenticateUser(UserLoginModel loginCredentials)
        {
            UserLoginModel user = appUsers.SingleOrDefault(x => x.UserName == loginCredentials.UserName && x.Password == loginCredentials.Password);
            return user;
        }

        private string GenerateJWTToken(UserLoginModel userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userInfo.UserName),
                new Claim("fullName", userInfo.FullName.ToString()),
                new Claim("role", userInfo.UserRole),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(issuer: jwtSettings.Issuer,
                                             audience: jwtSettings.Audience,
                                             claims: claims,
                                             expires: DateTime.Now.AddMinutes(30),
                                             signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}