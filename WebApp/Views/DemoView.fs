namespace WebApp.Views

[<RequireQualifiedAccess>]
module DemoView =

    [<RequireQualifiedAccess>]
    type ElementId =
        | RandomNumberOutput
        | ChartJsContainer

        override this.ToString() =
            match this with
            | RandomNumberOutput -> "RandomNumberOutput"
            | ChartJsContainer -> "ChartJsContainer"

    type MainProps =
        { FormFieldName: string
          RequestToken: string }

    let renderMain (props: MainProps) : string =
        $"""
            <div>
                <h3 class="title is-size-1">
                    Random Number Generator
                </h3>

                <button class="button is-small is-primary block" type="button" 
                    hx-get="/Demo/Random"
                    hx-target="#{ElementId.RandomNumberOutput}" 
                    hx-swap="innerHTML">
                    Get Random Number
                </button>

                <h2 id="{ElementId.RandomNumberOutput}" class="title is-size-2">
                    N/A
                </h2>

                <div class="box">
                    <p>This is a test</p>
                </div>

                <div class="box">
                    <form hx-post="/Demo/Save">
                        {Html.antiforgery props.FormFieldName props.RequestToken}
                        <input class="input is-small" placeholder="Enter some text" type="text" />
                        <hr />
                        <button class="button is-small" type="submit">Save</button>
                    </form>
                </div>

                <div class="box">
                    <button class="button is-small is-primary block" type="button" 
                        hx-get="/Demo/Chart" 
                        hx-swap="innerHTML"
                        hx-target="#{ElementId.ChartJsContainer}">
                        Load ChartJs
                    </button>
                </div>

                <div class="box chartjs-container" id="{ElementId.ChartJsContainer}" 
                    hx-get="/Demo/Chart" 
                    hx-trigger="load" 
                    hx-swap="innerHTML"
                    hx-target="this"></div>
            </div>
            """
