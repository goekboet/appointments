using System.Linq;
using System.Threading.Tasks;
using Appointments.Domain;
using Appointments.Records;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Db = Test.Records.DbLifeCycle;

namespace Test.Repository
{
    [TestClass]
    public class AppointmentsSuite
    {
        public IParticipantRepository SutFactory() => new ParticipantRepo(Db.dbConn());
        
        //[DataRow("HaveNoApp")]
        [DataRow("HaveManyAppointments")]
        [TestMethod]
        public async Task ListMyAppointments(string key)
        {
            var testCase = Cases.ListAppointment(key);

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
    }
}