using System;
using System.Linq;
using System.Threading.Tasks;
using Appointments.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Integration;

namespace Test.Records
{
    [TestClass]
    public class DbLifeCycle
    {
        public static string DBCreds { get; } = TestContextFactory.GetFresh();
        public static Pgres dbConn() =>
            new TestContextFactory().CreateDbContext(new[] { DBCreds });

        static Schedule[] Noice() => Arbitrary.Schedules(
                    DateTimeOffset.Now.ToUnixTimeSeconds(),
                    7200,
                    2,
                    3
                )
                .Take(10)
                .ToArray();

        [AssemblyInitialize]
        public static async Task AssemblyInitialize(TestContext context)
        {
            using (var ctx = dbConn())
            {
                await ctx.Database.MigrateAsync();
                ctx.AddRange(Noice());
                await ctx.SaveChangesAsync();
            }
        }

        [TestMethod]
        public async Task EnsureConnection()
        {
            using (var c = dbConn())
            {
                var connected = await c.Database.CanConnectAsync();
                Assert.IsTrue(connected);
            }
        }

        [AssemblyCleanup]
        public static async Task AssemblyCleanup()
        {
            using (var ctx = dbConn())
            {
                await ctx.Database.EnsureDeletedAsync();
            }
        }

    }
}