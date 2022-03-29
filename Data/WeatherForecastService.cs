using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ms_identity_dotnet_blazor_azure_sql.AAD;
using ms_identity_dotnet_blazor_azure_sql.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ms_identity_dotnet_blazor_azure_sql.Data
{
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
    }
}
