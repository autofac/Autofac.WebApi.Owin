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
    }
}