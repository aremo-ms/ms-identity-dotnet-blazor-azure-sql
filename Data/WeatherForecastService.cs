using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ms_identity_dotnet_blazor_azure_sql.Data
{
    public class WeatherForecastService
    {
        private IConfiguration _configuration;

        public WeatherForecastService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<WeatherForecast[]> GetForecastAsync(DateTime startDate, string userName)
        {
            //database call
            var dbSummaries = await GetSummaries(userName);

            var rnd = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = startDate.AddDays(index),
                TemperatureC = rnd.Next(-20, 55),
                Summary = dbSummaries[rnd.Next(dbSummaries.Count)]
            }).ToArray();
        }

        private async Task<IList<string>> GetSummaries(string accountIdentifier)
        {
            var summaryList = new List<string>();
            using (SqlConnection conn = new(_configuration.GetConnectionString("SqlDbContext")))
            {
                conn.AccessToken = await GetAccessToken(accountIdentifier);

                if (conn.State == ConnectionState.Closed)
                    await conn.OpenAsync();
                try
                {
                    SqlCommand cmd = new(@"select * from Summary", conn);

                    var myReader = await cmd.ExecuteReaderAsync();

                    while (myReader.Read())
                    {
                        summaryList.Add(myReader["Summary"].ToString());
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                        await conn.CloseAsync();
                }
            }

            return summaryList;
        }

        public async Task<string> GetDatabaseLoggedUser(string accountIdentifier)
        {
            var loggedUser = "N/A";

            using (SqlConnection conn = new(_configuration.GetConnectionString("SqlDbContext")))
            {
                conn.AccessToken = await GetAccessToken(accountIdentifier);

                if (conn.State == ConnectionState.Closed)
                    await conn.OpenAsync();
                try
                {
                    SqlCommand cmd = new(@"SELECT [dbo].[UsernamePrintFn]()", conn);

                    loggedUser = (await cmd.ExecuteScalarAsync()).ToString();

                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                        await conn.CloseAsync();
                }
            }

            return loggedUser;
        }

        private async Task<string> GetAccessToken(string accountIdentifier)
        {
            var scopes = new string[] { "https://database.windows.net/.default" };
            var azureSettings = _configuration.GetSection("AzureAd");

            IConfidentialClientApplication app =
                ConfidentialClientApplicationBuilder.Create(azureSettings["ClientId"])
                    .WithClientSecret(azureSettings["ClientSecret"])
                    .WithAuthority(AzureCloudInstance.AzurePublic, azureSettings["TenantId"])
                    .WithCacheOptions(CacheOptions.EnableSharedCacheOptions)                    
                    .Build();

            string accessToken = string.Empty;
            var temp = app.UserTokenCache;
            AuthenticationResult authResult = null;
            IAccount account = await app.GetAccountAsync(accountIdentifier);

            try
            {
                authResult = await app.AcquireTokenSilent(scopes, account).ExecuteAsync();
                accessToken = authResult.AccessToken;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Authentication error: {ex.Message}");
            }

            Console.WriteLine($"Access token: {accessToken}\n");

            return accessToken;
        }

        public string GetAccountIdentifier(AuthenticationState authState)
        {
            //this is Alex's user account id for debugging purposes only
            //return "ae6bdf0d-ba13-4e00-88f8-0fe4f661c86d.979f4440-75dc-4664-b2e1-2cafa0ac67d1";
            return authState.User.Identities.First().Claims.Where(c => c.Type == "uid").First().Value + "." + 
                authState.User.Identities.First().Claims.Where(c => c.Type == "utid").First().Value;
        }
    }
}
