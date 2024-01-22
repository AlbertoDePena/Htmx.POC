namespace WebApp.Infrastructure.Telemetry

open Microsoft.ApplicationInsights.Extensibility
open Microsoft
open Microsoft.AspNetCore.Http

open System.Reflection

type Application() =
    static member Name = "htmx-poc"
    static member Version =
        Assembly
            .GetAssembly(typeof<Application>)
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            .InformationalVersion

type CloudRoleNameInitializer() =

    interface ITelemetryInitializer with
        member this.Initialize(telemetry: ApplicationInsights.Channel.ITelemetry) =
            telemetry.Context.Cloud.RoleName <- Application.Name

type ComponentVersionInitializer() =

    interface ITelemetryInitializer with
        member this.Initialize(telemetry: ApplicationInsights.Channel.ITelemetry) =
            telemetry.Context.Component.Version <- Application.Version

type AuthenticatedUserInitializer(httpContextAccessor: IHttpContextAccessor) =

    interface ITelemetryInitializer with
        member this.Initialize(telemetry: ApplicationInsights.Channel.ITelemetry) =
            if
                httpContextAccessor.HttpContext <> null
                && httpContextAccessor.HttpContext.User <> null
                && httpContextAccessor.HttpContext.User.Identity.IsAuthenticated
            then
                telemetry.Context.User.AuthenticatedUserId <- httpContextAccessor.HttpContext.User.Identity.Name

[<AutoOpen>]
module ServiceCollectionExtensions =
    open Microsoft.Extensions.DependencyInjection

    type IServiceCollection with

        /// Adds custom telemetry initializers
        member this.AddCustomTelemetryInitializers() =
            this
                .AddSingleton<ITelemetryInitializer, CloudRoleNameInitializer>()
                .AddSingleton<ITelemetryInitializer, ComponentVersionInitializer>()
                .AddSingleton<ITelemetryInitializer, AuthenticatedUserInitializer>()