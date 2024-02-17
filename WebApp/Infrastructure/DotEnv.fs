namespace WebApp.Infrastructure.DotEnv

[<RequireQualifiedAccess>]
module DotEnv =
    open System
    open System.IO

    let private parseLine (line: string) =
        let splitCount = 2

        match line.Split('=', splitCount, StringSplitOptions.RemoveEmptyEntries) with
        | args when args.Length = splitCount -> Environment.SetEnvironmentVariable(args.[0], args.[1])
        | _ -> ()

    let private load =
        lazy
            (let filePath = Path.Combine(Directory.GetCurrentDirectory(), ".env")

             filePath
             |> File.Exists
             |> function
                 | false -> ()
                 | true -> filePath |> File.ReadAllLines |> Seq.iter parseLine)

    let init () = load.Force()