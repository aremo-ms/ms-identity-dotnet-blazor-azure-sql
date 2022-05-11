using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using ms_identity_dotnet_blazor_azure_sql.Database;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ms_identity_dotnet_blazor_azure_sql.AAD
{
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

            //https://database.windows.net/.default
            var scopes = new string[] { _azureSettings["Scopes"] };

            try
            {
                var accountIdentifier = GetAccountIdentifier(authState);

                IAccount account = await _app.GetAccountAsync(accountIdentifier);

                AuthenticationResult authResult = await _app.AcquireTokenSilent(scopes, account).ExecuteAsync();
                accessToken = authResult.AccessToken;
            }
            catch (MsalUiRequiredException)
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
}
