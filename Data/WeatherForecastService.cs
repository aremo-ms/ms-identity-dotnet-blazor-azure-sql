using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using ms_identity_dotnet_blazor_azure_sql.AAD;
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
        private UserTokenService _userToken;

        public WeatherForecastService(IConfiguration configuration, UserTokenService userToken)
        {
            _configuration = configuration;
            _userToken = userToken;
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
            using (SqlConnection conn = GetSqlConnection())
            {
                conn.AccessToken = await _userToken.GetAccessToken(accountIdentifier);

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

            using (SqlConnection conn = GetSqlConnection())
            {
                var token = await _userToken.GetAccessToken(accountIdentifier);
                if (string.IsNullOrEmpty(token)) return loggedUser;

                conn.AccessToken = token;

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

        private SqlConnection GetSqlConnection(string connStringName = "SqlDbContext")
        {
            return new(_configuration.GetConnectionString(connStringName));
        }
    }
}
