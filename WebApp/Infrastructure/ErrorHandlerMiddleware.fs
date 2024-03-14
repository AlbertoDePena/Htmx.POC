namespace WebApp.Infrastructure.ErrorHandlerMiddleware

open System.Net
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging

open WebApp.Infrastructure.Exceptions

type ErrorHandlerMiddleware(next: RequestDelegate, logger: ILogger<ErrorHandlerMiddleware>) =

    [<Literal>]
    let AuthenticationErrorMessage = "The request is not authenticated."

    [<Literal>]
    let AuthorizationErrorMessage = "The request is not allowed."

    [<Literal>]
    let ServerErrorMessage =
        "Something really bad happened. Please contact the system administrator."

    member this.Invoke(context: HttpContext) =
        task {
            try
                do! next.Invoke(context)
            with
            | :? AuthenticationException as ex ->
                logger.LogDebug(AuthenticationException.EventId, ex, ex.Message)

                context.Response.StatusCode <- HttpStatusCode.Unauthorized |> int

                return! context.Response.WriteAsJsonAsync({| message = AuthenticationErrorMessage |})

            | :? AuthorizationException as ex ->
                logger.LogDebug(AuthorizationException.EventId, ex, ex.Message)

                context.Response.StatusCode <- HttpStatusCode.Forbidden |> int

                return! context.Response.WriteAsJsonAsync({| message = AuthorizationErrorMessage |})

            | :? DataAccessException as ex ->
                logger.LogError(DataAccessException.EventId, ex, ex.Message)

                context.Response.StatusCode <- HttpStatusCode.InternalServerError |> int

                return! context.Response.WriteAsJsonAsync({| message = ServerErrorMessage |})

            | ex ->
                logger.LogError(ServerException.EventId, ex, ex.Message)

                context.Response.StatusCode <- HttpStatusCode.InternalServerError |> int

                return! context.Response.WriteAsJsonAsync({| message = ServerErrorMessage |})
        }

[<AutoOpen>]
module ServiceCollectionExtensions =
    open Microsoft.AspNetCore.Builder

    type IApplicationBuilder with

        /// Adds custom error handler middleware.
        member this.UseCustomErrorHandler() =
            this.UseMiddleware<ErrorHandlerMiddleware>()
