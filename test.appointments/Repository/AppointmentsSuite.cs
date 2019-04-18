using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Appointments.Records.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Db = Test.Records.DbLifeCycle;
using P = Appointments.Domain.Participant;

namespace Test.Repository
{
    [TestClass]
    public class AppointmentsSuite
    {
        public P.IParticipantRepository SutFactory() => new ParticipantRepo(Db.dbConn());
        
        [DataRow("HaveNoApp")]
        [DataRow("HaveManyAppointments")]
        [TestMethod]
        public async Task ListMyAppointments(string key)
        {
            var testCase = AppointmentCases.ListAppointment(key);

            using (var db = Db.dbConn())
            {
                db.AddRange(testCase.Given);
                await db.SaveChangesAsync();
            }

            using (var sut = SutFactory())
            {
                var r = await sut.List(testCase.Arguments);

                Assert.IsTrue(testCase.Expect.SequenceEqual(r));
            }

        }

        static string Lines(IEnumerable<string> ss) => string.Join(
            Environment.NewLine,
            ss);
        [DataRow("NoSuchAppointment")]
        [DataRow("AppointmentOnRecord")]
        [TestMethod]
        public async Task GetMyAppointment(string key)
        {
            var testCase = AppointmentCases.GetAppointment(key);

            using (var db = Db.dbConn())
            {
                db.AddRange(testCase.Given);
                await db.SaveChangesAsync();
            }

            using (var sut = SutFactory())
            {
                var r = await sut.Get(testCase.Arguments);

                Assert.IsTrue(testCase.Expect.SequenceEqual(r),
                    Lines(new [] 
                    {
                        $"expect: {Lines(testCase.Expect.Select(x => x.ToString()))}",
                        "---",
                        $"actual: {Lines(r.Select(x => x.ToString()))}"
                    }));
            }
        }
    }
}