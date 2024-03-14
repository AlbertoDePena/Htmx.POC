namespace WebApp.Infrastructure.Dapper

[<RequireQualifiedAccess>]
module Dapper =

    open System
    open Dapper
    open WebApp.Domain.Shared
    open WebApp.Domain.User
    open FsToolkit.ErrorHandling

    type private UserTypeHandler() =
        inherit SqlMapper.TypeHandler<UserType>()

        override _.SetValue(param, value) = param.Value <- value.Value

        override _.Parse value =
            value :?> string
            |> UserType.OfString
            |> Option.defaultWith (fun () -> failwith "The UserType cannot be null/empty/white-space")

    type private UserTypeOptionHandler() =
        inherit SqlMapper.TypeHandler<option<UserType>>()

        override _.SetValue(param, value) =
            param.Value <- (value |> Option.either (fun x -> x.Value) (fun () -> String.defaultValue))

        override _.Parse value = value :?> string |> UserType.OfString

    type private TextHandler() =
        inherit SqlMapper.TypeHandler<Text>()

        override _.SetValue(param, value) = param.Value <- value.Value

        override _.Parse value =
            value :?> string
            |> Text.OfString
            |> Option.defaultWith (fun () -> failwith "The Text cannot be null/empty/white-space")

    type private TextOptionHandler() =
        inherit SqlMapper.TypeHandler<option<Text>>()

        override _.SetValue(param, value) =
            param.Value <- (value |> Option.either (fun x -> x.Value) (fun () -> String.defaultValue))

        override _.Parse value = value :?> string |> Text.OfString

    type private OptionHandler<'T>() =
        inherit SqlMapper.TypeHandler<option<'T>>()

        override _.SetValue(param, value) =
            let valueOrNull =
                match value with
                | Some x -> box x
                | None -> null

            param.Value <- valueOrNull

        override _.Parse value =
            if isNull value || value = box DBNull.Value then
                None
            else
                Some(value :?> 'T)

    let private singleton =
        lazy
            (SqlMapper.AddTypeHandler(OptionHandler<Guid>())
             SqlMapper.AddTypeHandler(OptionHandler<byte>())
             SqlMapper.AddTypeHandler(OptionHandler<int16>())
             SqlMapper.AddTypeHandler(OptionHandler<int>())
             SqlMapper.AddTypeHandler(OptionHandler<int64>())
             SqlMapper.AddTypeHandler(OptionHandler<uint16>())
             SqlMapper.AddTypeHandler(OptionHandler<uint>())
             SqlMapper.AddTypeHandler(OptionHandler<uint64>())
             SqlMapper.AddTypeHandler(OptionHandler<float>())
             SqlMapper.AddTypeHandler(OptionHandler<decimal>())
             SqlMapper.AddTypeHandler(OptionHandler<float32>())
             SqlMapper.AddTypeHandler(OptionHandler<string>())
             SqlMapper.AddTypeHandler(OptionHandler<char>())
             SqlMapper.AddTypeHandler(OptionHandler<DateTime>())
             SqlMapper.AddTypeHandler(OptionHandler<DateTimeOffset>())
             SqlMapper.AddTypeHandler(OptionHandler<bool>())
             SqlMapper.AddTypeHandler(OptionHandler<TimeSpan>())
             SqlMapper.AddTypeHandler(TextHandler())
             SqlMapper.AddTypeHandler(TextOptionHandler())
             SqlMapper.AddTypeHandler(UserTypeHandler())
             SqlMapper.AddTypeHandler(UserTypeOptionHandler()))

    /// Register Dapper type handlers
    let registerTypeHandlers () = singleton.Force()
