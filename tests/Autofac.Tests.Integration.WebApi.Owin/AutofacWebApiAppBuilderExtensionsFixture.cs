using System.Linq;
using System.Threading;
using System.Web.Http;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using Autofac.Integration.WebApi.Owin;
using Microsoft.Owin.Builder;
using NUnit.Framework;
using Owin;

namespace Autofac.Tests.Integration.WebApi.Owin
{
    [TestFixture]
    public class AutofacWebApiAppBuilderExtensionsFixture
    {
        [Test]
        public void UseAutofacWebApiAddsDelegatingHandler()
        {
            var app = new AppBuilder();
            var configuration = new HttpConfiguration();

            app.UseAutofacWebApi(configuration);

            Assert.That(configuration.MessageHandlers.OfType<DependencyScopeHandler>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void UseAutofacWebApiWillOnlyAddDelegatingHandlerOnce()
        {
            var app = new AppBuilder();
            var configuration = new HttpConfiguration();

            app.UseAutofacWebApi(configuration);
            app.UseAutofacWebApi(configuration);

            Assert.That(configuration.MessageHandlers.OfType<DependencyScopeHandler>().Count(), Is.EqualTo(1));
        }

        [Test]
        public void DisposeScopeOnAppDisposing()
        {
            var app = new AppBuilder();
            var tcs = new CancellationTokenSource();
            var scope = new TestableLifetimeScope();
            app.Properties.Add("host.OnAppDisposing", tcs.Token);

            app.DisposeScopeOnAppDisposing(scope);

            tcs.Cancel();

            Assert.That(scope.IsDisposed, Is.True, "Should dispose scope on host.OnAppDisposing");
        }

        [Test]
        public void DisposeScopeOnAppDisposingDoesNothingWhenNoTokenPresent()
        {
            var app = new AppBuilder();
            var scope = new TestableLifetimeScope();

            Assert.That(() => app.DisposeScopeOnAppDisposing(scope), Throws.Nothing);
        }

        class TestableLifetimeScope : LifetimeScope
        {
            internal bool IsDisposed { get; set; }

            public TestableLifetimeScope() : base(new ComponentRegistry())
            {
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                IsDisposed = true;
            }
        }
    }
}