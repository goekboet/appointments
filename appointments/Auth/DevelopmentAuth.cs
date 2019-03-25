using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Appointments.Auth
{
    public static class DevelopmentAuth
    {
        static byte[] secret = Encoding.UTF8.GetBytes("a7d96014-dd1a-4228-8fa1-6db9c3987f71");

        static SecurityKey Key { get; } = new SymmetricSecurityKey(secret);
        static string Audience { get; } = "appointments";
        static string Issuer { get; } = "test.appointments";

        public static TokenValidationParameters Params = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Issuer,
                ValidAudience = Audience,
                IssuerSigningKey = Key
            };
        static SigningCredentials Creds => new SigningCredentials(
            key: Key, 
            algorithm: SecurityAlgorithms.HmacSha256);

        static JwtSecurityTokenHandler TokenHandler() => new JwtSecurityTokenHandler();

        public static string Token(string subjectId)
        {
            var handler = TokenHandler();

            var claims = new []
            {
                new Claim("sub", subjectId)
            };

            return handler.WriteToken(
                new JwtSecurityToken(
                    issuer: "test.appointments",
                    audience: "appointments",
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(30),
                    signingCredentials: Creds
            ));
        }
    }
}