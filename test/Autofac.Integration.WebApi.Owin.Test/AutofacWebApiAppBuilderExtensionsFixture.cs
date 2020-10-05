using System.Linq;
using System.Web.Http;
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

            Assert.Single(configuration.MessageHandlers.OfType<DependencyScopeHandler>());
        }

        [Fact]
        public void UseAutofacWebApiWillOnlyAddDelegatingHandlerOnce()
        {
            var app = new AppBuilder();
            var configuration = new HttpConfiguration();

            app.UseAutofacWebApi(configuration);
            app.UseAutofacWebApi(configuration);

            Assert.Single(configuration.MessageHandlers.OfType<DependencyScopeHandler>());
        }
    }
}
