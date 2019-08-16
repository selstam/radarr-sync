using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace selstam_radarr_sync
{
    public interface IRadarrService
    {
        Task<IEnumerable<MovieEntity>> GetMoviesAsync(string server, string apikey);
        Task PostMovieAsync(string server, string apiKey, MovieEntity entity);
    }

    public class RadarrService : IRadarrService
    {
        public async Task<IEnumerable<MovieEntity>> GetMoviesAsync(string server, string apiKey)
        {
            Log.Information("Getting movie information from {server}.", server);
            using (var client = CreateClient())
            {
                var url = $"http://{server}/api/movie?apikey={apiKey}";

                var data = await client.GetStreamAsync(url);

                var serializer = new DataContractJsonSerializer(typeof(MovieEntity[]));

                if(!(serializer.ReadObject(data) is MovieEntity[] items))
                    throw new Exception("Error getting items from server.");

                Log.Debug("{count} movies materialized.", items.Length);

                return items;
            }
        }

        public async Task PostMovieAsync(string server, string apiKey, MovieEntity entity)
        {
            Log.Information("Adding movie {movieName} ({movieYear}) to server.", entity.title, entity.year);

            using (var client = CreateClient())
            {
                var url = $"http://{server}/api/movie?apikey={apiKey}";

                var json = SerializeJson(entity);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var result = await client.PostAsync(url, content);
                if(!result.IsSuccessStatusCode)
                    throw new Exception("Error posting to Radarr: " + result.Content.ReadAsStringAsync().Result);
            }
        }

        private static string SerializeJson(MovieEntity entity)
        {
            string json;
            var js = new DataContractJsonSerializer(typeof(MovieEntity));
            using (var msObj = new MemoryStream())
            {
                js.WriteObject(msObj, entity);
                msObj.Position = 0;
                var sr = new StreamReader(msObj);

                json = sr.ReadToEnd();
            }

            return json;
        }

        private static HttpClient CreateClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", "selstam-radarr-sync");
            return client;
        }
    }
}