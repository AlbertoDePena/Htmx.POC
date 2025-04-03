namespace WebApp.Views.Layout

type LayoutViewModel =
    { PageName: string
      UserName: string
      MainContent: string }

[<RequireQualifiedAccess>]
module LayoutView =

    [<RequireQualifiedAccess>]
    type ElementId =
        | NavbarBurger
        | MainNavbar

        override this.ToString() =
            match this with
            | NavbarBurger -> "NavbarBurger"
            | MainNavbar -> "MainNavbar"

    let renderPage (vm: LayoutViewModel) : string =
        $"""
        <!DOCTYPE html>
        <html lang="en">

        <head>
            <meta charset="utf-8">
            <meta http-equiv="Cache-Control" content="no-cache, no-store, must-revalidate" />
            <meta http-equiv="Expires" content="0" />
            <meta http-equiv="Pragma" content="no-cache" />
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <title>{vm.PageName}</title>
            <link rel="stylesheet" href="https://fonts.googleapis.com/css?family=Poppins">
            <link rel="stylesheet" href="https://fonts.googleapis.com/icon?family=Material+Icons" />
            <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bulma@0.9.4/css/bulma.min.css" />
            <link
                rel="stylesheet"
                media="(prefers-color-scheme:light)"
                href="https://cdn.jsdelivr.net/npm/@shoelace-style/shoelace@2.20.1/cdn/themes/light.css" />
            <link
                rel="stylesheet"
                media="(prefers-color-scheme:dark)"
                href="https://cdn.jsdelivr.net/npm/@shoelace-style/shoelace@2.20.1/cdn/themes/dark.css"
                onload="document.documentElement.classList.add('sl-theme-dark');" />
            <link rel="stylesheet" href="/css/custom.css" />
            <link rel="icon shortcut" href="/favicon.ico" type="image/x-icon" />
        </head>

        <body class="has-navbar-fixed-top has-footer-fixed-bottom has-background-white-ter">
            <!--navbar-->
            <nav class="navbar is-fixed-top is-white is-size-7" role="navigation" aria-label="main navigation">
                <div class="navbar-brand">
                    <a class="navbar-item" href="/Home">
                        <img title="Bulma Logo" src="https://bulma.io/images/bulma-logo.png">
                    </a>            
                    <a id="{ElementId.NavbarBurger}" aria-expanded="false" aria-label="menu" class="navbar-burger" role="button" data-target="{ElementId.MainNavbar}">
                        <span aria-hidden="true"></span>
                        <span aria-hidden="true"></span>
                        <span aria-hidden="true"></span>
                    </a>
                </div>
                <div id="{ElementId.MainNavbar}" class="navbar-menu">
                    <div class="navbar-start">
                        <a class="navbar-item" href="/Home">
                            Home
                        </a>
                        <a class="navbar-item" href="/Users">
                            Users
                        </a>
                    </div>
                    <div class="navbar-end">
                        <div class="navbar-item has-dropdown is-hoverable">
                            <a class="navbar-link  has-text-link has-text-weight-semibold">{vm.UserName}</a>
                            <div class="navbar-dropdown">
                                <a class="navbar-item " href="#/logout">
                                    Log Out
                                </a>
                            </div>
                        </div>
                    </div>
                </div>
            </nav> 
            <!--main-->
            <main class="container-fluid p-5 is-size-7">
                {vm.MainContent}
            </main>
            <!--footer-->
            <footer class="footer is-fixed-bottom has-background-white is-size-7">
                <p class="has-text-centered">This is a test</p>
            </footer>
            <!--scripts-->
            <script type="module" src="https://cdn.jsdelivr.net/npm/@shoelace-style/shoelace@2.20.1/cdn/shoelace-autoloader.js"></script>
            <script src="https://unpkg.com/htmx.org@2.0.4"></script>
            <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
            <script src="/js/index.js"></script>
        </body>

        </html>
        """
