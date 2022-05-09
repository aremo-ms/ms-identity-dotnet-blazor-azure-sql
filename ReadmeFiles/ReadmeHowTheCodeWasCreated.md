## How the code was created

<details>
 <summary>Expand the section</summary>

 The application was generated out of standard Visual Studio template for **[Blazor Server App](https://docs.microsoft.com/en-us/aspnet/core/blazor/tooling?view=aspnetcore-6.0&pivots=windows)**
 After that SQL Server Database functionality and Authentication were configured.

1. Create initial sample, follow the [instructions](https://docs.microsoft.com/en-us/aspnet/core/blazor/tooling?view=aspnetcore-6.0&pivots=windows).  During the setup choose to use Microsoft Identity PLatform for Authentication.

1. Modify appsettings.json file
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

1. Open Program.cs.
   - Replace

    ```csharp
     builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
     .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
    ```

    by

    ```csharp
     builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
                .EnableTokenAcquisitionToCallDownstreamApi(new string[] { "https://sql.azuresynapse.usgovcloudapi.net/user_impersonation" })
                .AddInMemoryTokenCaches();

    ```

   - Comment the below code. If you leave it uncommented, the application will try to login immediately after start and you won't have a chance to see main page while user is not logged-in

     ```csharp
      builder.Services.AddAuthorization(options =>
      {
       // By default, all incoming requests will be authorized according to the default policy
       options.FallbackPolicy = options.DefaultPolicy;
      });
     ```

   - Replace

     ```csharp
       builder.Services.AddSingleton<WeatherForecastService>();
      ```

      by

      ```csharp
       builder.Services
                .AddScoped<WeatherForecastService>()
                .AddScoped<UserAADService>()
                .AddSingleton<SqlDatabase>();
      ```

1. Open Data/WeatherForecastService.cs and replace the entire class by below code:

    ```csharp
     public class WeatherForecastService
     {
        private readonly UserAADService _userAAD;
        private readonly SqlDatabase _database;

        public WeatherForecastService(UserAADService userAAD, SqlDatabase database)
        {
            _userAAD = userAAD;
            _database = database;
        }

        public async Task<WeatherForecast[]> GetForecastAsync(DateTime startDate, AuthenticationState authState)
        {
            //database call
            var dbSummaries = await GetSummaries(authState);

            var rnd = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = startDate.AddDays(index),
                TemperatureC = rnd.Next(-20, 55),
                Summary = dbSummaries[rnd.Next(dbSummaries.Count)]
            }).ToArray();
        }

        private async Task<IList<string>> GetSummaries(AuthenticationState authState)
        {
            var summaryList = new List<string>();
            using (SqlConnection conn = _database.GetSqlConnection())
            {
                conn.AccessToken = await _userAAD.GetAccessToken(authState);
                if (conn.AccessToken.IsNullOrEmpty()) return summaryList;

                try
                {
                    if (conn.State == ConnectionState.Closed)
                    await conn.OpenAsync();
                    
                    SqlCommand cmd = new(@"select * from Summary", conn);

                    var myReader = await cmd.ExecuteReaderAsync();

                    while (myReader.Read())
                    {
                        summaryList.Add(myReader["Summary"].ToString());
                    }
                }
                catch (Exception)
                {
                    return summaryList;
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                        await conn.CloseAsync();
                }
            }

            return summaryList;
        }
     }
    ```

1. Create SqlDatabase class

    ```csharp
     public class SqlDatabase
     {
        IConfiguration _configuration;

        public SqlDatabase(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public SqlConnection GetSqlConnection(string connStringName = "SqlDbContext")
        {
            return new(_configuration.GetConnectionString(connStringName));
        }
     }
    ```

1. Create UserAADService class

    ```csharp
     public class UserAADService
     {
        private readonly IConfiguration _configuration;
        private readonly IConfidentialClientApplication _app;
        private readonly IConfigurationSection _azureSettings;
        private readonly SqlDatabase _database;
        readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentHandler;

        public UserAADService(IConfiguration configuration, SqlDatabase database, MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler)
        {
            _consentHandler = consentHandler;

            _database = database;

            _configuration = configuration;

            _azureSettings = _configuration.GetSection("AzureAd");

            _app =
                ConfidentialClientApplicationBuilder.Create(_azureSettings["ClientId"])
                    .WithClientSecret(_azureSettings["ClientSecret"])
                    .WithAuthority(AzureCloudInstance.AzurePublic, _azureSettings["TenantId"])
                    .WithCacheOptions(CacheOptions.EnableSharedCacheOptions)
                    .Build();
        }

        public async Task<string> GetAccessToken(AuthenticationState authState)
        {
            string accessToken = string.Empty;
            var scopes = new string[] { _azureSettings["Scopes"] };

            try
            {
                var accountIdentifier = GetAccountIdentifier(authState);

                IAccount account = await _app.GetAccountAsync(accountIdentifier);

                AuthenticationResult authResult = await _app.AcquireTokenSilent(scopes, account).ExecuteAsync();
                accessToken = authResult.AccessToken;
            }
            catch (Exception)
            {
                _consentHandler.ChallengeUser(scopes);
                return accessToken;
            }

            return accessToken;
        }

        public async Task<string> GetDatabaseLoggedUser(AuthenticationState authState)
        {
            var loggedUser = "N/A";

            using (SqlConnection conn = _database.GetSqlConnection())
            {
                try
                {
                    var token = await GetAccessToken(authState);

                    if (string.IsNullOrEmpty(token)) return loggedUser;

                    conn.AccessToken = token;

                    if (conn.State == ConnectionState.Closed)
                        await conn.OpenAsync();

                    SqlCommand cmd = new(@"SELECT [dbo].[UsernamePrintFn]()", conn);

                    loggedUser = (await cmd.ExecuteScalarAsync()).ToString();

                }
                catch (Exception)
                {
                    return loggedUser;
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                        await conn.CloseAsync();
                }
            }

            return loggedUser;
        }

        private string GetAccountIdentifier(AuthenticationState authState)
        {
            if (authState.User.Identities.First().Claims.Where(c => c.Type == "uid").Count() == 0 ||
                authState.User.Identities.First().Claims.Where(c => c.Type == "utid").Count() == 0)
            {
                return null;
            }
            //return "<user object id>.<tenant id>" which is account identifier;
            return authState.User.Identities.First().Claims.Where(c => c.Type == "uid").First().Value + "." +
                authState.User.Identities.First().Claims.Where(c => c.Type == "utid").First().Value;
        }
     }
    ```

1. Open Pages/FetchData.razor and replace the entire code with this:

    ```csharp
        @page "/fetchdata"

        @using ms_identity_dotnet_blazor_azure_sql.AAD
        @using ms_identity_dotnet_blazor_azure_sql.Data
        @inject WeatherForecastService ForecastService
        @inject UserAADService UserAADService
        @inject AuthenticationStateProvider GetAuthenticationStateAsync

        <h1>Weather forecast</h1>
        <h4><strong>@_greetingsMessage</strong></h4>

        <p>This component demonstrates fetching data from a service that is connected to SQL database.</p>

        @if (forecasts == null)
        {
            <p><em>Loading...</em></p>
        }
        else
        {
            <table class="table">
                <thead>
                    <tr>
                        <th>Date</th>
                        <th>Temp. (C)</th>
                        <th>Temp. (F)</th>
                        <th>Summary</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var forecast in forecasts)
                    {
                        <tr>
                            <td>@forecast.Date.ToShortDateString()</td>
                            <td>@forecast.TemperatureC</td>
                            <td>@forecast.TemperatureF</td>
                            <td>@forecast.Summary</td>
                        </tr>
                    }
                </tbody>
            </table>
        }

        @code {
            private WeatherForecast[] forecasts;
            private string _loggedUser;
            private string _greetingsMessage;

            protected override async Task OnInitializedAsync()
            {
                var authstate = await GetAuthenticationStateAsync.GetAuthenticationStateAsync();

                _loggedUser = await UserAADService.GetDatabaseLoggedUser(authstate);

                if (_loggedUser == "N/A")
                    _greetingsMessage = "Please Log Out of the current user and re-login.";
                else
                {
                    _greetingsMessage = $"The user logged into SQL Database is {_loggedUser}";

                    forecasts = await ForecastService.GetForecastAsync(DateTime.Now, authstate);
                }
            }
        }
    ```

1. Delete **counter** link from Shared/NavMenu.razor

1.  
</details>
