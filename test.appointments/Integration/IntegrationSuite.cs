using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Test.Records;

using Db = Test.Records.DbLifeCycle;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.TestHost;

namespace Test.Integration
{
    [TestClass]
    public class IntegrationSuite
    {
        static string Issuer { get; } = "test.appointments";
        static byte[] secret = Encoding.UTF8.GetBytes("a7d96014-dd1a-4228-8fa1-6db9c3987f71");
        static SecurityKey Key { get; } = new SymmetricSecurityKey(secret);
        static string Audience { get; } = "appointments";

        private static TokenValidationParameters Params = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Issuer,
                ValidAudience = Audience,
                IssuerSigningKey = Key
            };

        private readonly WebApplicationFactory<Appointments.Startup> _factory;

        public IntegrationSuite()
        {
            _factory = new WebApplicationFactory<Appointments.Startup>()
                .WithWebHostBuilder(host =>
            {
                host.ConfigureTestServices(services =>
                {
                    services
                        .PostConfigure<JwtBearerOptions>(
                            JwtBearerDefaults.AuthenticationScheme, 
                            opts =>
                            {
                                opts.TokenValidationParameters = Params;
                            });
                });

                host.ConfigureAppConfiguration((h, appcfg) =>
                {
                    appcfg.AddInMemoryCollection(
                        new Dictionary<string, string>
                        {
                            {"ConnectionStrings:Psql", Db.DBCreds},
                            {"Auth:Authority", null }
                        });
                });
            });
        }

        [DataRow("Known principal list Schedules")]
        [DataRow("Get Existing Schedule")]
        [DataRow("List my appointments")]
        [DataRow("Get my appointment")]
        [DataRow("PostSchedule")]
        [DataRow("PostAppointment")]
        [DataRow("DeleteMyAppointment")]
        [TestMethod]
        public async Task GetRoutesOk(string key)
        {
            var c = IntegrationCases.GetRequests(key);
            var client = _factory.CreateClient();

            using (var ctx = Db.dbConn())
            {
                ctx.AddRange(c.Given);

                await ctx.SaveChangesAsync();
            }

            var response = await client.SendAsync(c.Arguments);

            Assert.AreEqual(c.Expect, response.StatusCode);
        }
    }
}
