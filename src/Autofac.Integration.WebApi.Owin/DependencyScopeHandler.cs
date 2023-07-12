// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Security;
using System.Web.Http.Hosting;
using Autofac.Integration.Owin;

namespace Autofac.Integration.WebApi.Owin;

/// <summary>
/// Delegating handler that manages coordinating the OWIN request lifetime with the Web API request lifetime.
/// </summary>
[SecurityCritical]
internal class DependencyScopeHandler : DelegatingHandler
{
    /// <summary>
    /// Assigns the OWIN request lifetime scope to the Web API request lifetime scope.
    /// </summary>
    /// <param name="request">The HTTP request message to send to the server.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    [SecuritySafeCritical]
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var owinContext = request.GetOwinContext();
        if (owinContext == null)
        {
            return base.SendAsync(request, cancellationToken);
        }

        var lifetimeScope = owinContext.GetAutofacLifetimeScope();
        if (lifetimeScope == null)
        {
            return base.SendAsync(request, cancellationToken);
        }

        var dependencyScope = new AutofacWebApiDependencyScope(lifetimeScope);
        request.Properties[HttpPropertyKeys.DependencyScope] = dependencyScope;

        // Using .ContinueWith instead of try/finally because you can't
        // have async/await in [SecurityCritical] or [SecuritySafeCritical]
        // marked code.
        return base
            .SendAsync(request, cancellationToken)
            .ContinueWith(task =>
            {
                request.Properties.Remove(HttpPropertyKeys.DependencyScope);
                return task.Result;
            });
    }
}
