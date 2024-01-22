namespace WebApplication.Infrastructure.Exceptions

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

type DatabaseException(ex: Exception) =
    inherit Exception(ex.Message, ex)
    new(message: string) = DatabaseException(Exception message)

    static member EventId = EventId(10002, "DatabaseError")

type DomainException(ex: Exception) =
    inherit Exception(ex.Message, ex)
    new(message: string) = DomainException(Exception message)

    static member EventId = EventId(10003, "DomainError")

type ServerException(ex: Exception) =
    inherit Exception(ex.Message, ex)
    new(message: string) = ServerException(Exception message)

    static member EventId = EventId(10004, "ServerError")