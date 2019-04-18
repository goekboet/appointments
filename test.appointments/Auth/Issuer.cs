using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Test.Auth
{
    public static class Issuer
    {
        static byte[] secret = Encoding.UTF8.GetBytes("a7d96014-dd1a-4228-8fa1-6db9c3987f71");
        public static SecurityKey Key { get; } = new SymmetricSecurityKey(secret);

        static SigningCredentials Creds => new SigningCredentials(
            key: Key, 
            algorithm: SecurityAlgorithms.HmacSha256);

        public static string Token(string subjectId)
        {
            var handler = new JwtSecurityTokenHandler();

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