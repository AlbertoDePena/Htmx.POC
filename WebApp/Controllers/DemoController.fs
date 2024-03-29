namespace WebApp.Controllers

open System
open System.Threading.Tasks

open Microsoft.AspNetCore.Antiforgery
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open WebApp.Views

type DemoController(logger: ILogger<DemoController>, antiforgery: IAntiforgery) =
    inherit HtmxController(antiforgery)

    let random = Random()

    [<HttpGet>]
    member this.Index() : Task<IActionResult> =
        task {
            let props: DemoView.MainProps = { Shared = this.GetSharedProps() }

            let htmlContent = DemoView.renderMain props

            return this.HtmlContent(htmlContent)
        }

    [<HttpGet>]
    member this.Random() : Task<IActionResult> =
        task {
            if this.Request.IsHtmx() then
                do! Task.Delay(3000)

                let randomNumber = random.NextDouble()

                return this.HtmlContent(randomNumber.ToString())
            else
                return! this.Index()
        }

    [<HttpPost>]
    member this.Save() : Task<IActionResult> =
        task {
            if this.Request.IsHtmx() then
                return this.HtmlContent("Saved!")
            else
                return! this.Index()
        }

    [<HttpGet>]
    member this.Chart() : Task<IActionResult> =
        task {
            //<script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
            if this.Request.IsHtmx() then
                let htmlContent = 
                    """
                    <div>
                      <canvas id="myChart"></canvas>
                    </div>
                    """
                return this.HtmlContent(htmlContent)
            else
                return! this.Index()
        }