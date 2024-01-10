namespace WebApplication

#nowarn "20"

open System
open System.Threading

open Microsoft.ApplicationInsights
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Options

open Serilog

open WebApplication.Infrastructure.Database
open WebApplication.Infrastructure.UserDatabase
open WebApplication.Infrastructure.HtmlTemplate
open WebApplication.Infrastructure.Telemetry
open WebApplication.Infrastructure.Serilog
open WebApplication.Infrastructure.Options

module Program =

    [<Literal>]
    let SuccessExitCode = 0

    [<Literal>]
    let FailureExitCode = -1

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
                            Serilog.configure context.Configuration services loggerConfig)
                )

                builder.Services
                    .AddHealthChecks()
                    .AddSqlServer(
                        connectionStringFactory =
                            (fun services -> services.GetRequiredService<IOptions<Database>>().Value.ConnectionString),
                        name = "HTMX POC Database"
                    )

                let app = builder.Build()

                let telemetryClient = app.Services.GetRequiredService<TelemetryClient>()
                let lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>()

                lifetime.ApplicationStopped.Register(fun () ->
                    telemetryClient.Flush()
                    Console.WriteLine("Flushing telemetry...")
                    Thread.Sleep(5000))

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
            Console.WriteLine("Flushing serilog...")
            Log.CloseAndFlush()
