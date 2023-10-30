namespace WebApplication.Infrastructure.Options

[<CLIMutable>]
type Database = { ConnectionString: string }

[<CLIMutable>]
type AzureAd = { TenantId: string; ClientId: string }