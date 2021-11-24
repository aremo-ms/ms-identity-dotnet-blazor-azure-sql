using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
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
        //private ITokenAcquisition _tokenAquisition;

        public WeatherForecastService(IConfiguration configuration)//, ITokenAcquisition tokenAcquisition)
        {
            _configuration = configuration;
            //_tokenAquisition = tokenAcquisition;
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

        private async Task<IList<string>> GetSummaries(string userName)
        {
            var summaryList = new List<string>();
            using (SqlConnection conn = new(_configuration.GetConnectionString("SqlDbContext")))
            {
                conn.AccessToken = await GetAccessToken(userName);

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

        public async Task<string> GetLoggedUser()
        {
            var loggedUser = "N/A";

            using (SqlConnection conn = new(_configuration.GetConnectionString("SqlDbContext")))
            {
                // conn.AccessToken = await GetAccessToken();

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

        private async Task<string> GetAccessToken(string userName)
        {
            var scopes = new string[] { "https://database.windows.net/.default" };
            var azureSettings = _configuration.GetSection("AzureAd");

            IConfidentialClientApplication app =
                ConfidentialClientApplicationBuilder.Create(azureSettings["ClientId"])
                    .WithRedirectUri("https://localhost:44348/")
                    .WithClientSecret(azureSettings["ClientSecret"])
                    .WithAuthority(AzureCloudInstance.AzurePublic, azureSettings["TenantId"])
                    //.WithClientCapabilities(new[] { "cp1" }) // Declare this app to be able to receive CAE events
                    .Build();
            //IPublicClientApplication app =
            //    PublicClientApplicationBuilder
            //.Create(azureSettings["ClientId"])
            //.WithAuthority(AzureCloudInstance.AzurePublic, azureSettings["TenantId"])
            //.WithRedirectUri("http://localhost")
            //.Build();

            string accessToken = string.Empty;
            var temp = app.UserTokenCache;
            AuthenticationResult authResult = null;
            IAccount account = (await app.GetAccountsAsync()).FirstOrDefault();//ClaimsPrincipal.Current.GetMsalAccountId());

            try
            {
                //accessToken = await tokenAcquisition.GetAccessTokenForUserAsync(scopes);

                authResult = await app.AcquireTokenSilent(scopes, account).ExecuteAsync();
                accessToken = authResult.AccessToken;
            }
            catch (MsalUiRequiredException)
            {
                //authResult = await app.AcquireTokenInteractive(scopes).ExecuteAsync();
                //accessToken = authResult.AccessToken;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Authentication error: {ex.Message}");
            }

            Console.WriteLine($"Access token: {accessToken}\n");

            return accessToken;
        }
    }
}
