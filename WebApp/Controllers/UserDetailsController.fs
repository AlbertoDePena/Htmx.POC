namespace WebApp.Controllers

open System

open FsToolkit.ErrorHandling

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open WebApp.Domain.User
open WebApp.Domain.Shared
open WebApp.Infrastructure.Constants
open WebApp.Infrastructure.UserDatabase
open WebApp.Infrastructure.HtmlMarkup

type UserDetailsController(logger: ILogger<UserDetailsController>, htmlMarkup: HtmlMarkup, userDatabase: IUserDatabase) =
    inherit HtmxController(logger, htmlMarkup)

    member this.Index() =
        this.HtmlContent(userName = "Alberto De Pena", mainContent = "user/details-section.html")