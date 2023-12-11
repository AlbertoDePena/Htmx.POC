namespace WebApplication.Controllers

open System
open System.Threading.Tasks

open FsToolkit.ErrorHandling

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open WebApplication.Domain.User
open WebApplication.Domain.Shared
open WebApplication.Domain.Extensions
open WebApplication.Infrastructure.Constants
open WebApplication.Infrastructure.Database
open WebApplication.Infrastructure.UserDatabase
open WebApplication.Infrastructure.HtmlTemplate
open WebApplication.Views

type DemoController
    (logger: ILogger<DemoController>, htmlTemplate: IHtmlTemplate) =
    inherit Controller()

    let random = Random()

    member this.Random() =
        task {                        
            if this.Request.IsHtmx() then
                do! Task.Delay(3000)

                let randomNumber = random.NextDouble()

                return this.HtmlContent(randomNumber.ToString())
            else
                return! this.Index()
        }

    member this.Index() =
        task {            
            let page = DemoView.render ()

            let index =
                IndexView.render "Alberto De Pena" page

            return this.HtmlContent index
        }