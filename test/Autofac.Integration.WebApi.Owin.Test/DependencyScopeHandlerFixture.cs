using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Hosting;
using Autofac.Integration.Owin;
using Microsoft.Owin;
using Xunit;

namespace Autofac.Integration.WebApi.Owin.Test
{
    public class DependencyScopeHandlerFixture
    {
        [Fact]
        public async void InvokeMethodThrowsExceptionIfRequestNull()
        {
            var handler = new DependencyScopeHandler();
            var invoker = new HttpMessageInvoker(handler);

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => invoker.SendAsync(null, new CancellationToken()));

            Assert.Equal("request", exception.ParamName);
        }

        [Fact]
        public async void AddsAutofacDependencyScopeToHttpRequestMessage()
        {
            var request = new HttpRequestMessage();
            var context = new OwinContext();
            request.Properties.Add("MS_OwinContext", context);

            var container = new ContainerBuilder().Build();
            context.Set(Constants.OwinLifetimeScopeKey, container);

            var fakeHandler = new FakeInnerHandler { Message = new HttpResponseMessage(HttpStatusCode.OK) };
            var handler = new DependencyScopeHandler { InnerHandler = fakeHandler };
            var invoker = new HttpMessageInvoker(handler);
            await invoker.SendAsync(request, new CancellationToken());

            var scope = (AutofacWebApiDependencyScope)request.Properties[HttpPropertyKeys.DependencyScope];

            Assert.Equal(container, scope.LifetimeScope);
        }
    }

    public class FakeInnerHandler : DelegatingHandler
    {
        public HttpResponseMessage Message { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Message == null ? base.SendAsync(request, cancellationToken) : Task.Run(() => Message, cancellationToken);
        }
    }
}