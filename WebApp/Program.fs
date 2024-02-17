namespace WebApp

open System
open System.Threading

open Microsoft.Identity.Web
open Microsoft.AspNetCore.Authentication.OpenIdConnect
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Antiforgery
open Microsoft.AspNetCore.Mvc.Authorization
open Microsoft.AspNetCore.CookiePolicy
open Microsoft.ApplicationInsights
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Options
open Microsoft.Extensions.Configuration

open Serilog

open WebApp.Infrastructure.DotEnv
open WebApp.Infrastructure.Database
open WebApp.Infrastructure.UserDatabase
open WebApp.Infrastructure.HtmlMarkup
open WebApp.Infrastructure.Telemetry
open WebApp.Infrastructure.Serilog
open WebApp.Infrastructure.Options
open WebApp.Infrastructure.ErrorHandlerMiddleware

#nowarn "20"

module Program =

    [<Literal>]
    let SuccessExitCode = 0

    [<Literal>]
    let FailureExitCode = -1

    [<EntryPoint>]
    let main args =
        try
            try
                DotEnv.init()

                let builder = WebApplication.CreateBuilder(args)

                builder.Services.AddSqlDatabase()
                builder.Services.AddUserDatabase()
                builder.Services.AddHtmlMarkup()

                builder.Services.Configure<CookiePolicyOptions>(
                    Action<CookiePolicyOptions>(fun options ->
                        options.Secure <- CookieSecurePolicy.Always
                        options.HttpOnly <- HttpOnlyPolicy.Always
                        options.MinimumSameSitePolicy <- SameSiteMode.Lax
                        options.CheckConsentNeeded <- (fun context -> true)                        
                        options.HandleSameSiteCookieCompatibility() |> ignore)
                )

                builder.Services
                    .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                    .AddMicrosoftIdentityWebApp(fun options -> builder.Configuration.Bind("AzureAd", options))

                builder.Services.AddAntiforgery()

                builder.Services.AddControllersWithViews(fun options ->
                    let policy = AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()
                    options.Filters.Add(AuthorizeFilter(policy))
                    options.Filters.Add(AutoValidateAntiforgeryTokenAttribute()))

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
                let applicationStopped = lifetime.ApplicationStopped

                applicationStopped.Register(fun () ->
                    telemetryClient.Flush()
                    Console.WriteLine("Flushing telemetry...")
                    Thread.Sleep(5000))

                if builder.Environment.IsDevelopment() then
                    app.UseDeveloperExceptionPage()
                else
                    app.UseCustomErrorHandler()
                    app.UseHsts()

                app.UseHttpsRedirection()
                app.UseStaticFiles()
                app.UseSerilogRequestLogging()
                app.UseCookiePolicy()
                app.UseRouting()
                app.UseAuthentication()
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
