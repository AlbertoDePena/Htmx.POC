namespace WebApp.Views

[<RequireQualifiedAccess>]
module DemoView =

    [<RequireQualifiedAccess>]
    type ElementId =
        | RandomNumberOutput

        override this.ToString() =
            match this with
            | RandomNumberOutput -> "RandomNumberOutput"

    [<NoEquality>]
    [<NoComparison>]
    type MainProps = { Shared: Html.SharedProps }

    let renderMain (props: MainProps) : string =
        let mainContent =
            $"""
            <div hx-target="#{ElementId.RandomNumberOutput}" hx-swap="innerHTML" hx-indicator=".loader-container">
                <h3 class="title is-size-1">
                    Random Number Generator
                </h3>

                <button class="button is-small is-primary block" type="button" hx-get="/Demo/Random">
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
                        {Html.antiforgery props.Shared.GetAntiforgeryToken}
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

        match props.Shared.IsHtmxBoosted with
        | true -> mainContent
        | false ->
            IndexView.renderPage
                { UserName = props.Shared.UserName
                  MainContent = mainContent }
