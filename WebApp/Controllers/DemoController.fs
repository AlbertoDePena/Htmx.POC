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
            let antiforgeryToken = this.GetAntiforgeryToken()
            let props: DemoView.MainProps = { FormFieldName = antiforgeryToken.FormFieldName; RequestToken = antiforgeryToken.RequestToken }

            let mainContent = DemoView.renderMain props

            let pageProps: IndexView.PageProps = { PageName = "Demo"; UserName = "John Doe"; MainContent = mainContent }
            let htmlContent = IndexView.renderPage pageProps

            return this.HtmlContent htmlContent
        }

    [<HttpGet>]
    member this.Random() : Task<IActionResult> =
        task {
            if this.Request.IsHtmx() then
                do! Task.Delay 3000

                let randomNumber = random.NextDouble()

                return this.HtmlContent (randomNumber.ToString())
            else
                return! this.Index()
        }

    [<HttpPost>]
    member this.Save() : Task<IActionResult> =
        task {
            if this.Request.IsHtmx() then
                return this.HtmlContent "Saved!"
            else
                return! this.Index()
        }

    [<HttpGet>]
    member this.Chart() : Task<IActionResult> =
        task {
            if this.Request.IsHtmx() then
                do! Task.Delay 1000

                let randomNumbers = 
                    [1..3] 
                    |> List.map (fun _ -> random.Next(1, 10))
                    |> System.Text.Json.JsonSerializer.Serialize

                let htmlContent = 
                    $"""                    
                    <canvas id="myChart"></canvas>                
                    <script>                        
                        new Chart(document.getElementById('myChart'), {{
                            type: 'bar',
                            data: {{
                                labels: ['Number 1', 'Number 2', 'Number 3'], 
                                datasets: [{{
                                    label: 'Random Numbers',
                                    data: {randomNumbers},
                                    borderWidth: 1
                                }}]
                            }},
                            options: {{
                                maintainAspectRatio: false,
                                scales: {{
                                    y: {{
                                        beginAtZero: true
                                    }}
                                }}
                            }}
                        }});
                    </script>
                    """
                return this.HtmlContent htmlContent
            else
                return! this.Index()
        }