using System.Linq;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace selstam_radarr_sync
{
    public class RadarrSync
    {
        private readonly IRadarrService _radarrService;
        private readonly IConfiguration _configuration;

        public RadarrSync(IRadarrService radarrService, IConfiguration configuration)
        {
            _radarrService = radarrService;
            _configuration = configuration;
        }

        public void Run()
        {
            Log.Information("Starting.");

            var config = _configuration.GetSection("RadarrSync").Get<Config>();

            if(config.DryRun)
                Log.Information("Dry run - no actual changes will be made.");

            foreach (var instance in config.Instances)
            {
                var data = _radarrService.GetMoviesAsync($"{instance.Host}:{instance.Port}", instance.ApiKey).Result;
                instance.Movies = data.ToList();
            }

            var instanceMovies = config.Instances.SelectMany(i => i.Movies).ToList();
            var allMovies = instanceMovies.DistinctBy(c => c.tmdbId).ToList();

            Log.Information("A total of {totalCount} movies discovered whereas {totalDistinct} distinct.", instanceMovies.Count, allMovies.Count);

            var moviesLookup = allMovies.ToLookup(i => i.tmdbId, i => i);

            foreach (var instance in config.Instances)
            {
                Log.Information("Managing Radarr installation {name}", instance.Name);

                var missing = allMovies.Where(m => instance.Movies.All(m2 => m2.tmdbId != m.tmdbId)).ToList();

                Log.Debug("There are {missingTitleCount} missing titles.", missing.Count);

                foreach (var movieId in missing)
                {
                    var missingMovie = moviesLookup[movieId.tmdbId].Single();
                    Log.Debug("Movie {movieName} ({movieYear}) is missing in {radarr}.", missingMovie.title, missingMovie.year, instance.Name);

                    missingMovie.id = 0;
                    missingMovie.profileId = instance.ProfileId;
                    foreach (var pattern in instance.ReplacePatterns)
                    {
                        if (missingMovie.folderName.Contains(pattern.From))
                        {
                            Log.Debug("Replacing {source} in movie folder name to {destination}", pattern.From, pattern.To);
                            missingMovie.folderName = missingMovie.folderName.Replace(pattern.From, pattern.To);
                        }

                        if (missingMovie.path.Contains(pattern.From))
                        {
                            Log.Debug("Replacing {source} in movie path to {destination}", pattern.From, pattern.To);
                            missingMovie.path = missingMovie.path.Replace(pattern.From, pattern.To);
                        }
                    }

                    if(!config.DryRun)
                        _radarrService.PostMovieAsync($"{instance.Host}:{instance.Port}", instance.ApiKey, missingMovie).Wait();
                }
            }

            Log.Information("Done.");
        }
    }
}