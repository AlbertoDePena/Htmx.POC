﻿namespace WebApp.Views

[<RequireQualifiedAccess>]
module IndexView =

    type PageProps =
        { UserName: string
          MainContent: string }

    let renderPage (props: PageProps) : string =
        $"""
        <!DOCTYPE html>
        <html lang="en">

        <head>
            <meta charset="utf-8">
            <meta http-equiv="Cache-Control" content="no-cache, no-store, must-revalidate" />
            <meta http-equiv="Expires" content="0" />
            <meta http-equiv="Pragma" content="no-cache" />
            <meta name="viewport" content="width=device-width, initial-scale=1">
            <title>HTMX POC</title>
            <link rel="stylesheet" href="https://fonts.googleapis.com/css?family=Poppins">
            <link rel="stylesheet" href="https://fonts.googleapis.com/icon?family=Material+Icons" />
            <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bulma@0.9.4/css/bulma.min.css" />
            <link rel="stylesheet" href="/css/custom.css" />
            <link rel="icon shortcut" href="/favicon.ico" type="image/x-icon" />
        </head>

        <body class="has-navbar-fixed-top has-footer-fixed-bottom has-background-white-ter" 
              hx-boost="true"
              hx-target="#MainContent"
              hx-swap="innerHTML">
            <!--navbar-->
            <nav class="navbar is-fixed-top is-white is-size-7" role="navigation" aria-label="main navigation">
                <div class="navbar-brand">
                    <a class="navbar-item" href="/">
                        <img title="Bulma Logo" src="https://bulma.io/images/bulma-logo.png">
                    </a>            
                    <a id="NavbarBurger" aria-expanded="false" aria-label="menu" class="navbar-burger" role="button" data-target="MainNavbar">
                        <span aria-hidden="true"></span>
                        <span aria-hidden="true"></span>
                        <span aria-hidden="true"></span>
                    </a>
                </div>
                <div id="MainNavbar" class="navbar-menu">
                    <div class="navbar-start">
                        <a class="navbar-item" href="/Demo">
                            Demo
                        </a>
                        <a class="navbar-item" href="/Users">
                            Users
                        </a>
                    </div>
                    <div class="navbar-end">
                        <div class="navbar-item has-dropdown is-hoverable">
                            <a class="navbar-link  has-text-link has-text-weight-semibold">{props.UserName}</a>
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
            <main id="MainContent" class="container-fluid p-5 is-size-7">
                {props.MainContent}
            </main>
            <!--footer-->
            <footer class="footer is-fixed-bottom has-background-white is-size-7">
                <p class="has-text-centered">This is a test</p>
            </footer>
            <!--scripts-->
            <script src="https://unpkg.com/htmx.org@1.9.10"></script>
            <script src="/js/index.js"></script>
        </body>

        </html>
        """
