using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

using DevAuth = Appointments.Auth.DevelopmentAuth;

namespace Test.Integration
{
    public static class Helpers
    {
        public static Guid KnownPrincipal { get; } = Guid.NewGuid();
        public static Guid KnownSubject { get; } = Guid.NewGuid();
        public static HttpContent Json(object o) =>
            new StringContent(
                JsonConvert.SerializeObject(o),
                Encoding.UTF8,
                "application/json");

        public static string KnownPrincipalToken => DevAuth.Token(KnownPrincipal.ToString());
        public static string KnownSubjectToken => DevAuth.Token(KnownSubject.ToString());
    }
}