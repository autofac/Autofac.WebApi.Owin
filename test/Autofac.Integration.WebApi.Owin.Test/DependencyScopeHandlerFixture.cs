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
    public void RemoveAutofacDependencyScopeAfterTaskExecutes_Task()
    {
        var request = new HttpRequestMessage();
        var context = new OwinContext();
        request.Properties.Add("MS_OwinContext", context);

        var container = new ContainerBuilder().Build();
        context.Set(Constants.OwinLifetimeScopeKey, container);

        var flag = new AutoResetEvent(false);
        var fakeHandler = new FakeInnerHandler(_ =>
        {
            return Task.Factory.StartNew(() =>
            {
                flag.WaitOne();
                Assert.Contains(HttpPropertyKeys.DependencyScope, request.Properties);
                return new HttpResponseMessage(HttpStatusCode.OK);
            });
        });
        var handler = new DependencyScopeHandler { InnerHandler = fakeHandler };
        var invoker = new HttpMessageInvoker(handler);
        var task = invoker.SendAsync(request, CancellationToken.None);

        Assert.Contains(HttpPropertyKeys.DependencyScope, request.Properties);
        flag.Set();
        task.GetAwaiter().GetResult();
        Assert.DoesNotContain(HttpPropertyKeys.DependencyScope, request.Properties);
    }

    [Fact]
    public async Task RemoveAutofacDependencyScopeAfterTaskExecutes_AsyncAwait()
    {
        var request = new HttpRequestMessage();
        var context = new OwinContext();
        request.Properties.Add("MS_OwinContext", context);

        var container = new ContainerBuilder().Build();
        context.Set(Constants.OwinLifetimeScopeKey, container);

        var flag = new AutoResetEvent(false);
        var fakeHandler = new FakeInnerHandler(async _ => await Task.Factory.StartNew(() =>
        {
            flag.WaitOne();
            Assert.Contains(HttpPropertyKeys.DependencyScope, request.Properties);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }));

        var handler = new DependencyScopeHandler { InnerHandler = fakeHandler };
        var invoker = new HttpMessageInvoker(handler);
        var task = invoker.SendAsync(request, CancellationToken.None);

        Assert.Contains(HttpPropertyKeys.DependencyScope, request.Properties);
        flag.Set();
        await task;
        Assert.DoesNotContain(HttpPropertyKeys.DependencyScope, request.Properties);
    }

    [Fact]
    public void RemoveAutofacDependencyScopeEvenIfTaskThrows_Task()
    {
        var request = new HttpRequestMessage();
        var context = new OwinContext();
        request.Properties.Add("MS_OwinContext", context);

        var container = new ContainerBuilder().Build();
        context.Set(Constants.OwinLifetimeScopeKey, container);

        var fakeHandler = new FakeInnerHandler(_ =>
        {
            return Task.Factory.StartNew<HttpResponseMessage>(() =>
            {
                throw new DivideByZeroException();
            });
        });
        var handler = new DependencyScopeHandler { InnerHandler = fakeHandler };
        var invoker = new HttpMessageInvoker(handler);
        var task = invoker.SendAsync(request, CancellationToken.None);

        Assert.Contains(HttpPropertyKeys.DependencyScope, request.Properties);
        Assert.Throws<AggregateException>(() => task.GetAwaiter().GetResult());
        Assert.DoesNotContain(HttpPropertyKeys.DependencyScope, request.Properties);
    }

    [Fact]
    public async Task RemoveAutofacDependencyScopeEvenIfTaskThrows_AsyncAwait()
    {
        var request = new HttpRequestMessage();
        var context = new OwinContext();
        request.Properties.Add("MS_OwinContext", context);

        var container = new ContainerBuilder().Build();
        context.Set(Constants.OwinLifetimeScopeKey, container);

        var fakeHandler = new FakeInnerHandler(async _ => await Task.Factory.StartNew<HttpResponseMessage>(() =>
        {
            throw new DivideByZeroException();
        }));
        var handler = new DependencyScopeHandler { InnerHandler = fakeHandler };
        var invoker = new HttpMessageInvoker(handler);
        var task = invoker.SendAsync(request, CancellationToken.None);

        Assert.Contains(HttpPropertyKeys.DependencyScope, request.Properties);
        await Assert.ThrowsAsync<AggregateException>(() => task);
        Assert.DoesNotContain(HttpPropertyKeys.DependencyScope, request.Properties);
    }

    private class FakeInnerHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _delegate;

        public FakeInnerHandler(Func<HttpRequestMessage, HttpResponseMessage> @delegate)
            : this(request => Task.FromResult(@delegate(request)))
        {
        }

        public FakeInnerHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> @delegate) => _delegate = @delegate;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => _delegate(request);
    }
}
