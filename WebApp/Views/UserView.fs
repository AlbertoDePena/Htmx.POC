namespace WebApp.Views

[<RequireQualifiedAccess>]
module UserView =
    open WebApp.Domain.Shared
    open WebApp.Domain.User

    [<NoEquality>]
    [<NoComparison>]
    type MainProps =
        { HtmxRequest: Htmx.Request
          GetAntiforgeryToken: unit -> Html.AntiforgeryToken }

    let renderMain (props: MainProps) : string =
        let mainContent =
            $"""
            <section hx-target="#UserSearchTable" hx-swap="outerHTML" hx-indicator=".loader-container">
                <div class="box">
                    <nav class="breadcrumb is-small" aria-label="breadcrumbs">
                        <ul>
                            <li><a href="#">Dashboard</a></li>
                            <li class="is-active"><a href="#" aria-current="page">Users</a></li>
                        </ul>
                    </nav>
                </div>
                <div class="box">
                    <div class="columns">
                        <div class="column">
                            <form id="UserSearchForm" class="columns"
                                  hx-trigger="load"
                                  hx-post="/Users/Search">
                                {Html.antiforgery props.GetAntiforgeryToken}
                                <div class="column">
                                    <div class="control">
                                        <input class="input is-small" name="search" type="text"
                                               hx-trigger="keyup changed delay:500ms, search"
                                               hx-post="/Users/Search"
                                               hx-include="#UserSearchForm"
                                               hx-vals='{{"page": "1"}}'
                                               placeholder="Begin typing to search users...">
                                    </div>
                                </div>
                                <div class="column is-narrow">
                                    <div class="is-flex is-align-items-center">
                                        <input id="ActiveOnly" name="active-only" type="checkbox" value="true"
                                               hx-post="/Users/Search"
                                               hx-include="#UserSearchForm"
                                               hx-vals='{{"page": "1"}}'
                                               class="is-clickable ml-2 is-small">
                                        <label class="is-clickable mx-2 has-text-weight-semibold"
                                               for="ActiveOnly">Active Only</label>
                                    </div>
                                </div>
                            </form>
                        </div>
                    </div>
                </div>
                <div class="box">
                    <div class="table-container table-has-fixed-header">
                        <table id="UserSearchTable" class="table is-narrow is-hoverable is-fullwidth">
                            <thead>
                                <tr>
                                    <th class="p-2" scope="col">Display Name</th>
                                    <th class="p-2" scope="col">Email Address</th>
                                    <th class="p-2" scope="col">Type</th>
                                    <th class="p-2" scope="col">Active</th>
                                </tr>
                            </thead>
                        </table>
                    </div>
                </div>
                <div class="loader-container">
                    <div class="loader"></div>
                </div>
            </section>
            """

        match props.HtmxRequest with
        | Htmx.Request.FullPage userName ->
            IndexView.renderPage
                { UserName = userName
                  MainContent = mainContent }
        | Htmx.Request.MainContent -> mainContent

    [<NoEquality>]
    [<NoComparison>]
    type TableProps = { PagedData: PagedData<User> }

    let renderTable (props: TableProps) : string =
        let searchResultSummary =
            sprintf
                "%i users found | showing page %i of %i"
                props.PagedData.TotalCount
                props.PagedData.Page
                props.PagedData.TotalPages

        let userTableRow (user: User) =
            let typeNameClass =
                match user.UserTypeName with
                | UserType.Customer -> "tag is-light is-info"
                | UserType.Employee -> "tag is-light is-success"

            let isActiveClass = if user.IsActive then "tag is-success" else "tag"

            let isActiveText = if user.IsActive then "Yes" else "No"

            $"""
            <tr class="is-clickable">
                <td class="p-2">{Html.encodeText user.DisplayName}</td>
                <td class="p-2">{Html.encodeText user.EmailAddress}</td>
                <td class="p-2">
                    <span class="{typeNameClass}">{user.UserTypeName}</span>
                </td>
                <td class="p-2">
                    <span class="{isActiveClass}">{isActiveText}</span>
                </td>
            </tr>
            """

        $"""
        <table id="UserSearchTable" class="table is-narrow is-hoverable is-fullwidth">
            <thead>
                <tr>
                    <th colspan="4" class="pb-6">
                        <div class="is-flex is-justify-content-space-between is-align-items-center">
                            <div>{searchResultSummary}</div>

                            <div class="is-flex is-justify-content-right">
                                <button class="button is-small" type="button" {HtmlAttribute.disabled (not props.PagedData.HasPreviousPage)}
                                        hx-post="/Users/Search"
                                        hx-include="#UserSearchForm"
                                        hx-vals='{{"page": "{props.PagedData.Page - 1}"}}'>
                                    <span class="material-icons ">navigate_before</span>
                                    <span class="is-sr-only">Previous</span>
                                </button>
                                <button class="button is-small mx-1" type="button" {HtmlAttribute.disabled (not props.PagedData.HasNextPage)}
                                        hx-post="/Users/Search"
                                        hx-include="#UserSearchForm"
                                        hx-vals='{{"page": "{props.PagedData.Page + 1}"}}'>
                                    <span class="material-icons ">navigate_next</span>
                                    <span class="is-sr-only">Next</span>
                                </button>
                                <button class="button is-small"
                                        type="button">
                                    <span class="material-icons ">add</span>
                                    <span class="is-sr-only">New User</span>
                                </button>
                            </div>
                        </div>
                    </th>
                </tr>
                <tr>
                    <th class="p-2" scope="col">Display Name</th>
                    <th class="p-2" scope="col">Email Address</th>
                    <th class="p-2" scope="col">Type</th>
                    <th class="p-2" scope="col">Active</th>
                </tr>
            </thead>
            <tbody>
                {Html.forEach props.PagedData.Data userTableRow ""}
            </tbody>
        </table>
        """
