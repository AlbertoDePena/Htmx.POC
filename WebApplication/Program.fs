namespace WebApplication

#nowarn "20"

open System

open Microsoft.ApplicationInsights.Extensibility
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting

open Serilog
open Serilog.Events

open WebApplication.Infrastructure.Database
open WebApplication.Infrastructure.UserDatabase
open WebApplication.Infrastructure.HtmlTemplate
open WebApplication.Infrastructure.Telemetry

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

                builder
                    .Services
                    .AddApplicationInsightsTelemetry()
                    .AddSingleton<ITelemetryInitializer, CloudRoleNameInitializer>()
                    .AddSingleton<ITelemetryInitializer, ComponentVersionInitializer>()
                    .AddSingleton<ITelemetryInitializer, AuthenticatedUserInitializer>()
        
                builder.Host.UseSerilog(Action<HostBuilderContext,IServiceProvider,LoggerConfiguration>(
                    fun context provider loggerConfig ->
                        loggerConfig
                            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                            .WriteTo.Console()     
                            .WriteTo.ApplicationInsights(provider.GetRequiredService<TelemetryConfiguration>(), OperationTelemetryConverter())
                            .Enrich.With<OperationIdEnricher>()
                            .Enrich.WithMachineName()
                            .Enrich.FromLogContext()
                            .Enrich.WithProperty("Version", ApplicationVersion.Version)
                            .Enrich.WithProperty("Application", "HTMX-POC")
                        ()
                ))

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