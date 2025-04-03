namespace WebApp.Views.Home

type HomeViewModel =   
    { FormFieldName: string
      RequestToken: string }

[<RequireQualifiedAccess>]
module HomeView =
    open WebApp.Views.Html

    [<RequireQualifiedAccess>]
    type ElementId =
        | RandomNumberOutput
        | ChartJsContainer

        override this.ToString() =
            match this with
            | RandomNumberOutput -> "RandomNumberOutput"
            | ChartJsContainer -> "ChartJsContainer"

    let renderMain (vm: HomeViewModel) : string =
        $"""
            <div>
                <h3 class="title is-size-1">
                    Random Number Generator
                </h3>

                <sl-button size="small" class="block" type="button" 
                    hx-get="/Home/Random"
                    hx-target="#{ElementId.RandomNumberOutput}" 
                    hx-swap="innerHTML">
                    Get Random Number
                </sl-button>

                <h2 id="{ElementId.RandomNumberOutput}" class="title is-size-2">
                    N/A
                </h2>

                <div class="box">
                    <p>This is a test</p>
                </div>

                <div class="box">
                    <form hx-post="/Home/Save">
                        {Html.antiforgery vm.FormFieldName vm.RequestToken}
                        <sl-input size="small" placeholder="Enter some text" type="text"></sl-input>
                        <hr />
                        <sl-button size="small" type="submit">Save</sl-button>
                    </form>
                </div>

                <div class="box">
                    <sl-button size="small" class="block" type="button" 
                        hx-get="/Home/Chart" 
                        hx-swap="outerHTML"
                        hx-target="#{ElementId.ChartJsContainer}">
                        Load ChartJs
                    </sl-button>
                </div>

                <div class="box chartjs-container" id="ChartJsContainer"
                    hx-trigger="load"
                    hx-get="/Home/Chart" 
                    hx-swap="outerHTML"
                    hx-target="this">
                    <canvas id="myChart"></canvas>
                </div>   
            </div>
            """
