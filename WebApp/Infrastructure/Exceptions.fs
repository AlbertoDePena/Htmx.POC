namespace WebApp.Infrastructure.Exceptions

open System
open Microsoft.Extensions.Logging

type AuthenticationException(ex: Exception) =
    inherit Exception(ex.Message, ex)
    new(message: string) = AuthenticationException(Exception message)

    static member EventId = EventId(10000, "AuthenticationError")

type AuthorizationException(ex: Exception) =
    inherit Exception(ex.Message, ex)
    new(message: string) = AuthorizationException(Exception message)

    static member EventId = EventId(10001, "AuthorizationError")

type DataAccessException(ex: Exception) =
    inherit Exception(ex.Message, ex)
    new(message: string) = DataAccessException(Exception message)

    static member EventId = EventId(10002, "DataAccessError")

type ServerException(ex: Exception) =
    inherit Exception(ex.Message, ex)
    new(message: string) = ServerException(Exception message)

    static member EventId = EventId(10003, "ServerError")