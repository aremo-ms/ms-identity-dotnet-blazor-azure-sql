## How the code was created

<details>
 <summary>Expand the section</summary>

 The application was generated out of standard Visual Studio template for **[Blazor Server App](https://docs.microsoft.com/en-us/aspnet/core/blazor/tooling?view=aspnetcore-6.0&pivots=windows)**
 After that SQL Server Database functionality and Authentication were added.

1. Create initial sample, follow the [instructions](https://docs.microsoft.com/en-us/aspnet/core/blazor/tooling?view=aspnetcore-6.0&pivots=windows)
2. Modify appsettings.json file
Replace contents of the configuration by the below lines:

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "[Enter the domain of your tenant, e.g. contoso.onmicrosoft.com]",
    "TenantId": "[Enter 'common', or 'organizations' or the Tenant Id (Obtained from the Azure portal. Select 'Endpoints' from the 'App registrations' blade and use the GUID in any of the URLs), e.g. da41245a5-11b3-996c-00a8-4d99re19f292]",
    "ClientId": "[Enter the Client Id (Application ID obtained from the Azure portal), e.g. ba74781c2-53c2-442a-97c2-3d60re42f403]",
    "SignedOutCallbackPath": "/signout-callback-oidc",
    "Scopes": "https://database.windows.net/.default",
    "OnSignOutRedirectPage": "https://localhost:44348",
    "ClientSecret": "[Copy the client secret added to the app from the Azure portal]",
    //"ClientCertificates": [
    //  {
    //    "SourceType": "KeyVault",
    //    "KeyVaultUrl": "[Enter URL for you Key Vault]",
    //    "KeyVaultCertificateName": "[Enter name of the certificate]"
    //  }
    //]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "SqlDbContext": "Server=<your server name>;database=<your database name>;Persist Security Info=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False"
  }
}
```

3. 

</details>