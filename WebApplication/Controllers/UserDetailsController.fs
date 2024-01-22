namespace WebApplication.Controllers

open System

open FsToolkit.ErrorHandling

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open WebApplication.Domain.User
open WebApplication.Domain.Shared
open WebApplication.Domain.Extensions
open WebApplication.Infrastructure.Constants
open WebApplication.Infrastructure.UserDatabase
open WebApplication.Infrastructure.HtmlTemplate

type UserDetailsController(logger: ILogger<UserDetailsController>, htmlTemplate: IHtmlTemplate, userDatabase: IUserDatabase) =
    inherit HtmxController(logger, htmlTemplate)

    member this.Index() =
        task {
            return! this.HtmlContent(userName = "Alberto De Pena", mainContent = "user/details-section.html")
        }