namespace WebApplication.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open Microsoft.Extensions.Diagnostics.HealthChecks
open Microsoft.AspNetCore.Http

type HealthController(logger: ILogger<HealthController>, healthCheck: HealthCheckService) =
    inherit Controller()

    member this.Index() =
        task {
            let! healthReport = healthCheck.CheckHealthAsync()

            if
                healthReport.Status = HealthStatus.Healthy
                || healthReport.Status = HealthStatus.Degraded
            then
                return this.StatusCode(StatusCodes.Status200OK, "OK")
            else
                return this.StatusCode(StatusCodes.Status503ServiceUnavailable, healthReport.Entries)
        }

    member this.Report() =
        task {
            let! healthReport = healthCheck.CheckHealthAsync()

            return this.StatusCode(StatusCodes.Status200OK, healthReport)
        }
