// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net;
using System.Web.Http.Hosting;
using Autofac.Integration.Owin;
using Microsoft.Owin;

namespace Autofac.Integration.WebApi.Owin.Test;

public class DependencyScopeHandlerFixture
{
    [Fact]
    public async void InvokeMethodThrowsExceptionIfRequestNull()
    {
        var handler = new DependencyScopeHandler();
        var invoker = new HttpMessageInvoker(handler);

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => invoker.SendAsync(null, CancellationToken.None));

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

        AutofacWebApiDependencyScope scope = null;
        var fakeHandler = new FakeInnerHandler(r =>
        {
            scope = (AutofacWebApiDependencyScope)r.Properties[HttpPropertyKeys.DependencyScope];
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        var handler = new DependencyScopeHandler { InnerHandler = fakeHandler };
        var invoker = new HttpMessageInvoker(handler);
        await invoker.SendAsync(request, CancellationToken.None);

        // checking if the handler was in fact called
        Assert.NotNull(scope);
        Assert.Equal(container, scope.LifetimeScope);
    }

    [Fact]
    public async void RemoveAutofacDependencyScopeFromHttpRequestMessageAfterLeaving()
    {
        var request = new HttpRequestMessage();
        var context = new OwinContext();
        request.Properties.Add("MS_OwinContext", context);

        var container = new ContainerBuilder().Build();
        context.Set(Constants.OwinLifetimeScopeKey, container);

        var fakeHandler = new FakeInnerHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var handler = new DependencyScopeHandler { InnerHandler = fakeHandler };
        var invoker = new HttpMessageInvoker(handler);
        await invoker.SendAsync(request, CancellationToken.None);

        Assert.DoesNotContain(HttpPropertyKeys.DependencyScope, request.Properties);
    }

    private class FakeInnerHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _delegate;

        public FakeInnerHandler(Func<HttpRequestMessage, HttpResponseMessage> @delegate)
            => _delegate = @delegate;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_delegate(request));
    }
}
