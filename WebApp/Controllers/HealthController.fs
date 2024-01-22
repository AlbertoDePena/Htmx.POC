namespace WebApp.Controllers

open System.Text.Json
open System.Text.Json.Serialization

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Diagnostics.HealthChecks
open Microsoft.AspNetCore.Http

type HealthController(logger: ILogger<HealthController>, healthCheck: HealthCheckService) =
    inherit Controller()

    member this.Index() =
        task {
            let! healthReport = healthCheck.CheckHealthAsync()

            match healthReport.Status with
            | HealthStatus.Healthy
            | HealthStatus.Degraded ->
                return this.StatusCode(StatusCodes.Status200OK, "Healthy")
            | HealthStatus.Unhealthy ->
                return this.StatusCode(StatusCodes.Status503ServiceUnavailable, healthReport.Entries)
            | _ ->
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error")
        }

    member this.Report() =
        task {
            let! healthReport = healthCheck.CheckHealthAsync()

            let options = JsonSerializerOptions(
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            )

            options.Converters.Add(JsonStringEnumConverter())

            return this.Json(healthReport, options)
        }
