namespace WebApp.Controllers

open System
open System.Threading.Tasks

open Microsoft.AspNetCore.Antiforgery
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open WebApp.Views.Layout
open WebApp.Views.Home

type HomeController(logger: ILogger<HomeController>, antiforgery: IAntiforgery) =
    inherit HtmxController(antiforgery)

    let random = Random()

    [<HttpGet>]
    member this.Index() : Task<IActionResult> =
        task {
            let antiforgeryToken = this.GetAntiforgeryToken()

            let homeViewModel: HomeViewModel =
                { FormFieldName = antiforgeryToken.FormFieldName
                  RequestToken = antiforgeryToken.RequestToken }

            let layoutViewModel: LayoutViewModel =
                { PageName = "Demo"
                  UserName = this.GetUserName()
                  MainContent = HomeView.renderMain homeViewModel }

            let htmlContent = LayoutView.renderPage layoutViewModel

            return this.HtmlContent htmlContent
        }

    [<HttpGet>]
    member this.Random() : Task<IActionResult> =
        task {
            if this.Request.IsHtmx() then
                do! Task.Delay 3000

                let randomNumber = random.NextDouble()

                return this.HtmlContent(randomNumber.ToString())
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
                    [ 1..3 ]
                    |> List.map (fun _ -> random.Next(1, 10))
                    |> System.Text.Json.JsonSerializer.Serialize

                let htmlContent =
                    $"""     
                    <div class="box chartjs-container" id="ChartJsContainer">
                        <canvas id="myChart"></canvas>
                    </div>               
                                  
                    <script>      
                        (function () {{
                            setTimeout(function () {{                                                            
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
                            }}, 0);
                        }})();                                          
                    </script>
                    """

                return this.HtmlContent htmlContent
            else
                return! this.Index()
        }
