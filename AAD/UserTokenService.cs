using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ms_identity_dotnet_blazor_azure_sql.AAD
{
    public class UserTokenService
    {
        private IConfiguration _configuration;

        public UserTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> GetAccessToken(string accountIdentifier)
        {
            var azureSettings = _configuration.GetSection("AzureAd");

            IConfidentialClientApplication app =
                ConfidentialClientApplicationBuilder.Create(azureSettings["ClientId"])
                    .WithClientSecret(azureSettings["ClientSecret"])
                    .WithAuthority(AzureCloudInstance.AzurePublic, azureSettings["TenantId"])
                    .WithCacheOptions(CacheOptions.EnableSharedCacheOptions)
                    .Build();

            string accessToken = string.Empty;

            IAccount account = await app.GetAccountAsync(accountIdentifier);

            if (account != null)
            {
                try
                {
                    AuthenticationResult authResult = await app.AcquireTokenSilent(
                        new string[] { azureSettings["Scopes"] }, account).ExecuteAsync();
                    accessToken = authResult.AccessToken;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Authentication error: {ex.Message}");
                }

                Console.WriteLine($"Access token: {accessToken}\n");
            }
            else { 
                //try to re-authenticate user
            }

            return accessToken;
        }

        public string GetAccountIdentifier(AuthenticationState authState)
        {
            //return "<user object id>.<tenant id>" which is account identifier;
            return authState.User.Identities.First().Claims.Where(c => c.Type == "uid").First().Value + "." +
                authState.User.Identities.First().Claims.Where(c => c.Type == "utid").First().Value;
        }
    }
}
