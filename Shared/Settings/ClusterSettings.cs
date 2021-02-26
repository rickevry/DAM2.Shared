﻿namespace DAM2.Core.Shared.Settings
{
    public class ClusterSettings : IClusterSettings
    {
        public string ClusterName { get; set; }
        public string ClusterHost { get; set; }
        public int ClusterPort { get; set; }
        public string RedisPort { get; set; }
        public string PIDConnectionString { get; set; }
        public string PIDCollectionName { get; set; }
        public string PIDDatabaseName { get; set; }
        public string ConsulUri { get; set; }
    }
}
