using System;
using System.Linq;
using System.Threading;
using System.Web.Http;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using Microsoft.Owin.Builder;
using Owin;
using Xunit;

namespace Autofac.Integration.WebApi.Owin.Test
{
    public class AutofacWebApiAppBuilderExtensionsFixture
    {
        [Fact]
        public void UseAutofacWebApiAddsDelegatingHandler()
        {
            var app = new AppBuilder();
            var configuration = new HttpConfiguration();

            app.UseAutofacWebApi(configuration);

            Assert.Equal(1, configuration.MessageHandlers.OfType<DependencyScopeHandler>().Count());
        }

        [Fact]
        public void UseAutofacWebApiWillOnlyAddDelegatingHandlerOnce()
        {
            var app = new AppBuilder();
            var configuration = new HttpConfiguration();

            app.UseAutofacWebApi(configuration);
            app.UseAutofacWebApi(configuration);

            Assert.Equal(1, configuration.MessageHandlers.OfType<DependencyScopeHandler>().Count());
        }

        [Fact]
        public void DisposeScopeOnAppDisposing()
        {
            var app = new AppBuilder();
            var tcs = new CancellationTokenSource();
            var scope = new TestableLifetimeScope();
            app.Properties.Add("host.OnAppDisposing", tcs.Token);

            app.DisposeScopeOnAppDisposing(scope);

            tcs.Cancel();

            Assert.True(scope.ScopeIsDisposed);
        }

        [Fact]
        public void DisposeScopeOnAppDisposingDoesNothingWhenNoTokenPresent()
        {
            var app = new AppBuilder();
            var scope = new TestableLifetimeScope();

            // XUnit doesn't have Assert.DoesNotThrow
            app.DisposeScopeOnAppDisposing(scope);
        }

        [Fact]
        public void DisposeScopeOnAppDisposingLifetimeScopeRequired()
        {
            var app = new AppBuilder();
            Assert.Throws<ArgumentNullException>(() => app.DisposeScopeOnAppDisposing(null));
        }

        [Fact]
        public void DisposeScopeOnAppDisposingAppBuildRequired()
        {
            var app = (IAppBuilder)null;
            Assert.Throws<ArgumentNullException>(() => app.DisposeScopeOnAppDisposing(new TestableLifetimeScope()));
        }

        class TestableLifetimeScope : LifetimeScope
        {
            public bool ScopeIsDisposed { get; set; }

            public TestableLifetimeScope()
                : base(new ComponentRegistry())
            {
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                this.ScopeIsDisposed = true;
            }
        }
    }
}