using System.Collections.Generic;

namespace selstam_radarr_sync
{
    public class MovieEntity
    {
        public int id { get; set; }

        public string title { get; set; }

        public string titleSlug { get; set; }

        public int tmdbId { get; set; }

        public string folderName { get; set; }

        public string path { get; set; }

        public bool monitored { get; set; }

        public int qualityProfileId { get; set; }

        public int profileId { get; set; }

        public int year { get; set; }

        public string minimumAvailability { get; set; }
        
        public List<MovieImage> images { get; set; }
    }

    public class MovieImage
    {
        public string coverType { get; set; }
        public string url { get; set; }
    }
}
