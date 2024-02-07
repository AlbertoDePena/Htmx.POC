namespace WebApp.Infrastructure.Serilog

open System

open Microsoft.ApplicationInsights.Extensibility
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration

open Serilog
open Serilog.Events

open FsToolkit.ErrorHandling

open WebApp.Infrastructure.Telemetry

[<RequireQualifiedAccess>]
module Serilog =

    let private getLogLevel (configuration: IConfiguration) (key: string) =
        configuration.GetValue<string>(key)
        |> Enum.TryParse<LogEventLevel>
        |> Option.ofPair
        |> Option.defaultValue LogEventLevel.Warning

    let configure (configuration: IConfiguration) (services: IServiceProvider) (loggerConfig: LoggerConfiguration) =
        let defaultLogLevel =
            getLogLevel configuration "Application:DefaultLogLevel"

        let infrastructureLogLevel =
            getLogLevel configuration "Application:InfrastructureLogLevel"

        loggerConfig.MinimumLevel
            .Is(defaultLogLevel)
            .MinimumLevel.Override("Azure.Messaging.ServiceBus", infrastructureLogLevel)
            .MinimumLevel.Override("Microsoft", infrastructureLogLevel)
            .MinimumLevel.Override("Microsoft.AspNetCore", infrastructureLogLevel)
            .MinimumLevel.Override("System.Net.Http.HttpClient", infrastructureLogLevel)
            .Enrich.WithMachineName()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", Application.Name)
            .Enrich.WithProperty("Version", Application.Version)
            .WriteTo.Console()
            .WriteTo.ApplicationInsights(
                services.GetRequiredService<TelemetryConfiguration>(),
                TelemetryConverter.Traces
            ) |> ignore