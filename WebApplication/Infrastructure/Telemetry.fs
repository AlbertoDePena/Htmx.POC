namespace WebApplication.Infrastructure.Telemetry

open Microsoft.ApplicationInsights.Extensibility
open Microsoft
open Microsoft.AspNetCore.Http

open System.Diagnostics
open System.Reflection

open Serilog.Core
open Serilog.Events
open Serilog.Sinks.ApplicationInsights.TelemetryConverters

open WebApplication.Domain.Extensions

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

type OperationIdEnricher() =

    interface ILogEventEnricher with
        member this.Enrich(logEvent, propertyFactory) =
            let currentActivity = Activity.Current

            if currentActivity <> null then
                logEvent.AddPropertyIfAbsent(
                    new LogEventProperty("Operation Id", new ScalarValue(currentActivity.TraceId))
                )

                logEvent.AddPropertyIfAbsent(new LogEventProperty("Parent Id", new ScalarValue(currentActivity.SpanId)))

type OperationTelemetryConverter() =
    inherit TraceTelemetryConverter()

    let tryGetScalarProperty (logEvent: LogEvent) propertyName =
        let hasScalarValue, value = logEvent.Properties.TryGetValue(propertyName)

        if hasScalarValue && value :? ScalarValue then
            true, Some((value :?> ScalarValue).ToString())
        else
            false, None

    override _.Convert(logEvent, formatProvider) =

        ``base``.Convert(logEvent, formatProvider)
        |> Seq.map (fun telemetry ->
            let hasOperationId, operationId = tryGetScalarProperty logEvent "Operation Id"

            let hasParentOperationId, parentOperationId =
                tryGetScalarProperty logEvent "Parent Id"

            if hasOperationId then
                telemetry.Context.Operation.Id <- operationId |> Option.defaultValue String.defaultValue

            if hasParentOperationId then
                telemetry.Context.Operation.ParentId <- parentOperationId |> Option.defaultValue String.defaultValue

            telemetry)

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