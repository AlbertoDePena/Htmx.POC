# HTMX POC

## Environment Variables

* Use `:` in `dotnet user-secrets`
* Use `--` in Azure key vault

```
Database:ConnectionString ""

Application:DefaultLogLevel "Warning"
Application:InfrastructureLogLevel "Warning"

AzureAd:Instance "https://login.microsoftonline.com/"
AzureAd:Domain "<org>.onmicrosoft.com"
AzureAd:TenantId ""
AzureAd:ClientId ""
AzureAd:CallbackPath "/signin-oidc"
AzureAd:SignedOutCallbackPath "/signout-callback-oidc"
```