using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Test.Records;

using Db = Test.Records.DbLifeCycle;

namespace Test.Integration
{
    [TestClass]
    public class RouteTests
    {
        private readonly WebApplicationFactory<Appointments.Startup> _factory;

        public RouteTests()
        {
            _factory = new WebApplicationFactory<Appointments.Startup>().WithWebHostBuilder(host =>
            {
                host.ConfigureAppConfiguration((h, appcfg) =>
                {
                    appcfg.AddInMemoryCollection(
                        new Dictionary<string, string>
                        {
                            {"ConnectionStrings:Psql", Db.DBCreds}
                        });
                });
            });
        }

        [ClassInitialize]
        public static async Task Up(TestContext testctx)
        {
            using (var ctx = Db.dbConn())
            {
                ctx.AddRange(Seed.Data);

                await ctx.SaveChangesAsync();
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
