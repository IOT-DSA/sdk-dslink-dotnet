using DSLink.Connection;
using NUnit.Framework;

namespace DSLink.Test
{
    [TestFixture]
    public class TokenTests
    {
        [Test]
        public void TokenHash()
        {
            var token = "RMtO6mEJmUlJfoWfofiLgjguUEpuIzWP3sXeoBNSbLIVumlw";
            var dsId = "test-wjN6iQTk7TOXZbHHkQDH1T2zfrPcphTxchiPvTgzbww";
            var tokenHash = Token.CreateToken(token, dsId);

            Assert.AreEqual("RMtO6mEJmUlJfoWfegkDI-jCG-4J2Ke1L26hX_63vHlq9zsRJbFUWWIgE8U", tokenHash);
        }
    }
}
