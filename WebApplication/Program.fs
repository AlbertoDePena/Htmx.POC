namespace WebApplication

#nowarn "20"

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting

open WebApplication.Infrastructure.Database
open WebApplication.Infrastructure.UserDatabase
open WebApplication.Infrastructure.HtmlTemplate

module Program =

    let exitCode = 0

    [<EntryPoint>]
    let main args =
        let builder = WebApplication.CreateBuilder(args)

        builder.Services.AddDbConnectionFactory()
        builder.Services.AddUserDatabase()
        builder.Services.AddHtmlTemplate()
        builder.Services.AddControllers()
        
        let app = builder.Build()

        if builder.Environment.IsDevelopment() then
            app.UseDeveloperExceptionPage()
        else
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts()

        app.UseHttpsRedirection()
        app.UseStaticFiles()
        app.UseRouting()
        app.UseAuthorization()

        app.MapControllerRoute(name = "default", pattern = "{controller=Home}/{action=Index}/{id?}")

        app.Run()

        exitCode
