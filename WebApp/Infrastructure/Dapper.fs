namespace WebApp.Infrastructure.Dapper

[<RequireQualifiedAccess>]
module Dapper =

    open System
    open Dapper
    open WebApp.Domain.Shared
    open WebApp.Domain.User

    type private StringContainerHandler<'T>(ofString: string -> 'T option, getValue: 'T -> string) =
        inherit SqlMapper.TypeHandler<'T>()

        let typeName = typeof<'T>.Name

        override _.SetValue(param, value) = param.Value <- getValue value

        override _.Parse value =
            value :?> string
            |> ofString
            |> Option.defaultWith (fun () ->
                failwithf "The data structure %s does not support the value %O" typeName value)

    type private StringContainerOptionHandler<'T>(ofString: string -> 'T option, getValue: 'T -> string) =
        inherit SqlMapper.TypeHandler<option<'T>>()

        override _.SetValue(param, value) =
            param.Value <-
                (match value with
                 | Some t -> getValue t
                 | None -> String.defaultValue)

        override _.Parse value = value :?> string |> ofString

    type private OptionHandler<'T>() =
        inherit SqlMapper.TypeHandler<option<'T>>()

        override _.SetValue(param, value) =
            let valueOrNull =
                match value with
                | Some t -> box t
                | None -> null

            param.Value <- valueOrNull

        override _.Parse value =
            if isNull value || value = box DBNull.Value then
                None
            else
                Some(value :?> 'T)

    let private singleton =
        lazy
            (
             // primitive type wrapped in an option
             SqlMapper.AddTypeHandler(OptionHandler<Guid>())
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
             // string wrapped in a container
             SqlMapper.AddTypeHandler(StringContainerHandler(Text.OfString, (fun x -> x.Value)))
             SqlMapper.AddTypeHandler(StringContainerHandler(UserType.OfString, (fun x -> x.Value)))
             SqlMapper.AddTypeHandler(StringContainerHandler(UserPermission.OfString, (fun x -> x.Value)))
             SqlMapper.AddTypeHandler(StringContainerHandler(UserGroup.OfString, (fun x -> x.Value)))
             // string wrapped in an optional container
             SqlMapper.AddTypeHandler(StringContainerOptionHandler(Text.OfString, (fun x -> x.Value)))
             SqlMapper.AddTypeHandler(StringContainerOptionHandler(UserType.OfString, (fun x -> x.Value)))
             SqlMapper.AddTypeHandler(StringContainerOptionHandler(UserPermission.OfString, (fun x -> x.Value)))
             SqlMapper.AddTypeHandler(StringContainerOptionHandler(UserGroup.OfString, (fun x -> x.Value))))

    /// Register Dapper type handlers
    let registerTypeHandlers () = singleton.Force()
