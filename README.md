# Autofac.WebApi.Owin

OWIN support for the ASP.NET Web API integration for [Autofac](https://autofac.org).

[![Build status](https://ci.appveyor.com/api/projects/status/sllnx8g95topf79l?svg=true)](https://ci.appveyor.com/project/Autofac/autofac-webapi-owin)

Please file issues and pull requests for this package [in this repository](https://github.com/autofac/Autofac.WebApi.Owin/issues) rather than in the Autofac core repo.

- [Documentation](https://autofac.readthedocs.io/en/latest/integration/webapi.html)
- [NuGet](https://www.nuget.org/packages/Autofac.WebApi2.Owin/)
- [Contributing](https://autofac.readthedocs.io/en/latest/contributors.html)
- [Open in Visual Studio Code](https://open.vscode.dev/autofac/Autofac.WebApi.Owin)

## Quick Start

If you are using Web API [as part of an OWIN application](https://autofac.readthedocs.io/en/latest/integration/owin.html), you need to:

- Do all the stuff for [standard Web API integration](https://autofac.readthedocs.io/en/latest/integration/webapi.html) - register controllers, set the dependency resolver, etc.
- Set up your app with the [base Autofac OWIN integration](https://autofac.readthedocs.io/en/latest/integration/owin.html).
- Add a reference to the `Autofac.WebApi2.Owin` NuGet package.
- In your application startup class, register the Autofac Web API middleware after registering the base Autofac middleware.

```c#
public class Startup
{
  public void Configuration(IAppBuilder app)
  {
    var builder = new ContainerBuilder();

    // STANDARD WEB API SETUP:

    // Get your HttpConfiguration. In OWIN, you'll create one
    // rather than using GlobalConfiguration.
    var config = new HttpConfiguration();

    // Register your Web API controllers.
    builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

    // Run other optional steps, like registering filters,
    // per-controller-type services, etc., then set the dependency resolver
    // to be Autofac.
    var container = builder.Build();
    config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

    // OWIN WEB API SETUP:

    // Register the Autofac middleware FIRST, then the Autofac Web API middleware,
    // and finally the standard Web API middleware.
    app.UseAutofacMiddleware(container);
    app.UseAutofacWebApi(config);
    app.UseWebApi(config);
  }
}
```

A common error in OWIN integration is use of the `GlobalConfiguration.Configuration`. **In OWIN you create the configuration from scratch.** You should not reference `GlobalConfiguration.Configuration` anywhere when using the OWIN integration.

[Check out the documentation](https://autofac.readthedocs.io/en/latest/integration/webapi.html) for more usage details.

## Get Help

**Need help with Autofac?** We have [a documentation site](https://autofac.readthedocs.io/) as well as [API documentation](https://autofac.org/apidoc/). We're ready to answer your questions on [Stack Overflow](https://stackoverflow.com/questions/tagged/autofac) or check out the [discussion forum](https://groups.google.com/forum/#forum/autofac).
