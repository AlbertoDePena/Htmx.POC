namespace WebApplication.Infrastructure.Database

[<RequireQualifiedAccess>]
module UniqueId =

    let create () =
        RT.Comb.Provider.Sql.Create()

[<AutoOpen>]
module SqlDataReaderExtensions =
    open System.Threading.Tasks
    open Microsoft.Data.SqlClient

    type SqlDataReader with

        /// Map all records available in the result set
        member this.ReadAllAsync<'T>(mapper: SqlDataReader -> 'T) : Task<'T seq> =
            task {
                let items = ResizeArray<'T>()

                let mutable keepGoing = false
                let! hasNextRecord = this.ReadAsync()
                keepGoing <- hasNextRecord

                while keepGoing do
                    items.Add(mapper this)
                    let! hasMoreItems = this.ReadAsync()
                    keepGoing <- hasMoreItems

                return items.ToArray() |> Seq.ofArray
            }

        /// Map the first record available in the result set
        member this.ReadFirstOrAsync<'T>(mapper: SqlDataReader -> 'T, defaultValue: 'T) : Task<'T> =
            task {
                let! hasMoreItems = this.ReadAsync()

                let item = if hasMoreItems then mapper this else defaultValue

                return item
            }

