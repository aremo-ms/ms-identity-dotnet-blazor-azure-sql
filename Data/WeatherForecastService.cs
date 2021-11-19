using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
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

        public async Task<WeatherForecast[]> GetForecastAsync(DateTime startDate)
        {
            //database call
            var dbSummaries = await GetSummaries();

            var rnd = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = startDate.AddDays(index),
                TemperatureC = rnd.Next(-20, 55),
                Summary = dbSummaries[rnd.Next(dbSummaries.Count)]
            }).ToArray();
        }

        private async Task<IList<string>> GetSummaries()
        {
            var summaryList = new List<string>();
            using (SqlConnection conn = new(_configuration.GetConnectionString("SqlDbContext")))
            {
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
    }
}
