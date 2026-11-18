using System;
using GmGard.Controllers;
using Xunit;

namespace GmGard.Tests
{
    public class TestAppSmokeTests : IDisposable
    {
        private readonly TestApp _app = new TestApp();
        public void Dispose() => _app.Dispose();

        [SkippableFact]
        public void Can_create_BlogController_with_real_services()
        {
            Skip.IfNot(_app.Available, "GM_TEST_PG not set");
            var c = _app.Controller<BlogController>("alice");
            Assert.NotNull(c);
        }
    }
}
