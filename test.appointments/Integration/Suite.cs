using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Test.Records;

namespace Test.Integration
{
    [TestClass]
    public class RouteTests
    {
        private readonly WebApplicationFactory<Appointments.Startup> _factory;

        public static readonly Dictionary<string, string> _dict =
        new Dictionary<string, string>
        {
            {"ConnectionStrings:Psql", TestContextFactory.Cs},
        };

        public RouteTests()
        {
            _factory = new WebApplicationFactory<Appointments.Startup>().WithWebHostBuilder(host =>
            {
                host.ConfigureAppConfiguration((h, appcfg) =>
                {
                    appcfg.AddInMemoryCollection(_dict);
                });
            });
        }

        [ClassInitialize]
        public static async Task Up(TestContext testctx)
        {
            using (var ctx = new TestContextFactory().CreateDbContext(new string[0]))
            {
                await ctx.Database.MigrateAsync();
                ctx.AddRange(Seed.Data);
                await ctx.SaveChangesAsync();
            }
        }

        [ClassCleanup]
        public static async Task Down()
        {
            using (var ctx = new TestContextFactory().CreateDbContext(new string[0]))
            {
                await ctx.Database.EnsureDeletedAsync();
            }
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
            var c = TestCases.Repository[key];
            var client = _factory.CreateClient();

            var response = await client.SendAsync(c.Request);

            var expectedContent = await c.Expect.Content.ReadAsStringAsync();
            var actualContent = await response.Content.ReadAsStringAsync();
            
            Assert.AreEqual(c.Expect.StatusCode, response.StatusCode);
            Assert.AreEqual(
                c.Expect.Headers.Location,
                response.Headers.Location);
            Assert.AreEqual(expectedContent, actualContent);
        }
    }
}
