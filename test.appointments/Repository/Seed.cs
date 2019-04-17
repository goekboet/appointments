using System;
using System.Linq;
using System.Threading.Tasks;
using Appointments.Records;
using Db = Test.Records.DbLifeCycle;

namespace Test.Repository
{
    public static class Seed
    {
        public static Schedule[] ListSchedules(
            Guid principalId,
            string[] names) => names.Select(x => new Schedule
            {
                PrincipalId = principalId,
                Name = x,
                Appointments = Arbitrary
                        .Appointments(0, 7800, 2, x)
                        .Take(3)
                        .ToList()
            })
                .ToArray();

        public static Schedule[] GetSchedules(
            Guid owner,
            string schedule,
            Appointment[] appts)
        {
            return new[]
            {
                new Schedule
                {
                    PrincipalId = owner,
                    Name = schedule,
                    Appointments = appts.ToList()
                }
            };
        }

        public static Schedule[] PutSchedules(
            Guid owner,
            string schedule,
            Appointment[] appts)
        {
            return new[]
            {
                new Schedule
                {
                    PrincipalId = owner,
                    Name = schedule,
                    Appointments = appts.ToList()
                }
            };
        }
    }
}