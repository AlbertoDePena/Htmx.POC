namespace WebApp.Views

[<RequireQualifiedAccess>]
module DemoView =

    [<NoEquality>]
    [<NoComparison>]
    type MainProps =
        { HtmxRequest: Htmx.Request
          GetAntiforgeryToken: unit -> Html.AntiforgeryToken }

    let renderMain (props: MainProps) : string =
        let mainContent =
            $"""
            <div hx-target="#RandomNumberOutput" hx-swap="innerHTML" hx-indicator=".loader-container">
                <h3 class="title is-size-1">
                    Random Number Generator
                </h3>

                <button class="button is-small is-primary block" type="button" hx-get="/Demo/Random">
                    Get Random Number
                </button>

                <h2 id="RandomNumberOutput" class="title is-size-2">
                    N/A
                </h2>

                <div class="box">
                    <p>This is a test</p>
                </div>

                <div class="box">
                    <form hx-post="/Demo/Save">
                        {Html.antiforgery props.GetAntiforgeryToken}
                        <input class="input is-small" placeholder="Enter some text" type="text" />
                        <hr />
                        <button class="button is-small" type="submit">Save</button>
                    </form>
                </div>

                <div class="loader-container">
                    <div class="loader"></div>
                </div>
            </div>
            """

        match props.HtmxRequest with
        | Htmx.Request.FullPage userName -> IndexView.renderPage userName mainContent
        | Htmx.Request.MainContent -> mainContent
