namespace WebApplication

#nowarn "20"

open System

open Microsoft.ApplicationInsights.Extensibility
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Options

open Serilog
open Serilog.Events

open FsToolkit.ErrorHandling

open WebApplication.Infrastructure.Database
open WebApplication.Infrastructure.UserDatabase
open WebApplication.Infrastructure.HtmlTemplate
open WebApplication.Infrastructure.Telemetry
open WebApplication.Infrastructure.Options

module Program =

    [<Literal>]
    let SuccessExitCode = 0

    [<Literal>]
    let FailureExitCode = -1

    let getLogLevel (configuration: IConfiguration) (key: string) =
        configuration.GetValue<string>(key)
        |> Enum.TryParse<LogEventLevel>
        |> Option.ofPair
        |> Option.defaultValue LogEventLevel.Warning

    [<EntryPoint>]
    let main args =
        try
            try
                let builder = WebApplication.CreateBuilder(args)

                builder.Services.AddSqlDatabase()
                builder.Services.AddUserDatabase()
                builder.Services.AddHtmlTemplate()
                builder.Services.AddControllers()

                builder.Services
                    .AddApplicationInsightsTelemetry()
                    .AddCustomTelemetryInitializers()

                builder.Host.UseSerilog(
                    Action<HostBuilderContext, IServiceProvider, LoggerConfiguration>
                        (fun context services loggerConfig ->
                            let defaultLogLevel =
                                getLogLevel context.Configuration "Application:DefaultLogLevel"

                            let infrastructureLogLevel =
                                getLogLevel context.Configuration "Application:InfrastructureLogLevel"

                            loggerConfig.MinimumLevel
                                .Is(defaultLogLevel)
                                .MinimumLevel.Override("Microsoft.AspNetCore", infrastructureLogLevel)
                                .MinimumLevel.Override("System.Net.Http.HttpClient", infrastructureLogLevel)
                                .Enrich.WithMachineName()
                                .Enrich.FromLogContext()
                                .Enrich.WithProperty("Application", Application.Name)
                                .Enrich.WithProperty("Version", Application.Version)
                                .Enrich.With<OperationIdEnricher>()
                                .WriteTo.Console()
                                .WriteTo.ApplicationInsights(
                                    services.GetRequiredService<TelemetryConfiguration>(),
                                    OperationTelemetryConverter()
                                )

                            ())
                )

                builder.Services
                    .AddHealthChecks()
                    .AddSqlServer(
                        connectionStringFactory =
                            (fun services -> services.GetRequiredService<IOptions<Database>>().Value.ConnectionString),
                        name = "HTMX POC Database"
                    )

                let app = builder.Build()

                if builder.Environment.IsDevelopment() then
                    app.UseDeveloperExceptionPage()
                else
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts()

                app.UseSerilogRequestLogging()
                app.UseHttpsRedirection()
                app.UseStaticFiles()
                app.UseRouting()
                app.UseAuthorization()

                app.MapControllerRoute(name = "default", pattern = "{controller=Home}/{action=Index}/{id?}")

                app.Run()

                SuccessExitCode
            with ex ->
                Console.Error.WriteLine(ex)

                FailureExitCode
        finally
            Log.CloseAndFlush()
