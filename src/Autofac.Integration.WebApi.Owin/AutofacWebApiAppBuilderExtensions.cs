// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Web.Http;
using Autofac.Integration.WebApi.Owin;

namespace Owin;

/// <summary>
/// Extension methods for configuring the OWIN pipeline.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class AutofacWebApiAppBuilderExtensions
{
    /// <summary>
    /// Extends the Autofac lifetime scope added from the OWIN pipeline through to the Web API dependency scope.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="configuration">The HTTP server configuration.</param>
    /// <returns>The application builder for continued configuration.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="app" /> or <paramref name="configuration" /> is <see langword="null" />.
    /// </exception>
    [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "The handler created must exist for the entire application lifetime.")]
    public static IAppBuilder UseAutofacWebApi(this IAppBuilder app, HttpConfiguration configuration)
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        if (!configuration.MessageHandlers.OfType<DependencyScopeHandler>().Any())
        {
            configuration.MessageHandlers.Insert(0, new DependencyScopeHandler());
        }

        return app;
    }
}
