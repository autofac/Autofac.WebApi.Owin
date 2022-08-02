// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Web.Http;
using Microsoft.Owin.Builder;
using Owin;

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
