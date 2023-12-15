namespace WebApplication.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open Microsoft.Extensions.Diagnostics.HealthChecks
open Microsoft.AspNetCore.Http
open System.Text.Json
open System.Text.Json.Serialization

type HealthController(logger: ILogger<HealthController>, healthCheck: HealthCheckService) =
    inherit Controller()

    member this.Index() =
        task {
            let! healthReport = healthCheck.CheckHealthAsync()

            if
                healthReport.Status = HealthStatus.Healthy
                || healthReport.Status = HealthStatus.Degraded
            then
                return this.StatusCode(StatusCodes.Status200OK, "Healthy")
            else
                return this.StatusCode(StatusCodes.Status503ServiceUnavailable, healthReport.Entries)
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
