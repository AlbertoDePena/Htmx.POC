namespace Database.ConsoleApp

open System
open System.Reflection
open Microsoft.Extensions.Configuration
open DbUp
open DbUp.Helpers

type Directory =
    | Seed
    | Migration
    | StoredProcedures
    | Views

type Application() = class end

[<RequireQualifiedAccess>]
module Program =

    let exitCode = 0

    let update dbConnectionString directory =
        let builder =
            DeployChanges.To
                .SqlDatabase(dbConnectionString)
                .WithScriptsEmbeddedInAssembly(
                    Assembly.GetExecutingAssembly(),
                    fun assemblyName -> assemblyName.Contains(directory.ToString())
                )
                .LogScriptOutput()
                .LogToConsole()

        let upgrader =
            match directory with
            | Seed -> builder.JournalTo(NullJournal()).Build()
            | Migration -> builder.Build()
            | StoredProcedures -> builder.JournalTo(NullJournal()).Build()
            | Views -> builder.JournalTo(NullJournal()).Build()

        let result = upgrader.PerformUpgrade()

        if not result.Successful then
            raise result.Error

    [<EntryPoint>]
    let main _ =        
        let configuration = ConfigurationBuilder().AddUserSecrets<Application>().Build()

        let dbConnectionString = configuration["Database:ConnectionString"]
        
        if String.IsNullOrWhiteSpace(dbConnectionString) then
            failwith "Database connection string is required"
        else
            // order matters!
            let directories = [ Migration; Seed; Views; StoredProcedures ]

            for directory in directories do
                update dbConnectionString directory

        exitCode
