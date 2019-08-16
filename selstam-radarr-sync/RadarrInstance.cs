using System.Collections.Generic;

namespace selstam_radarr_sync
{
    public class Config
    {
        public bool DryRun { get; set; }

        public List<RadarrInstance> Instances { get; set; }
    }

    public class RadarrInstance
    {
        public string Name { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string ApiKey { get; set; }
        public int ProfileId { get; set; }

        public List<MovieEntity> Movies { get; set; }

        public List<ReplacePattern> ReplacePatterns { get; set; }
    }

    public class ReplacePattern
    {
        public string From { get; set; }
        public string To { get; set; }
    }
}