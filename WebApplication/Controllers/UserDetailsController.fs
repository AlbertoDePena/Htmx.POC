namespace WebApp.Controllers

open System

open FsToolkit.ErrorHandling

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open WebApp.Domain.User
open WebApp.Domain.Shared
open WebApp.Infrastructure.Constants
open WebApp.Infrastructure.UserDatabase
open WebApp.Infrastructure.HtmlTemplate

type UserDetailsController(logger: ILogger<UserDetailsController>, htmlTemplate: IHtmlTemplate, userDatabase: IUserDatabase) =
    inherit HtmxController(logger, htmlTemplate)

    member this.Index() =
        task {
            return! this.HtmlContent(userName = "Alberto De Pena", mainContent = "user/details-section.html")
        }