﻿using Iot.Database;

namespace BrickSchema.Net
{
    public partial class BrickSchemaManager
    {
        public async Task<List<(DateTime Timestamp, BsonValue Value)>> GetTimeSeries(string guid, DateTime from, DateTime to)
        {
            if (_database == null) return new();
            //var timeSeries = await _database.TimeSeries.GetAsync(new() { guid }, from, to);
            //if (timeSeries != null) { 
            //    if (timeSeries.ContainsKey(guid))
            //    {
            //        return timeSeries[guid].Select(x=>(x.Timestamp, x.Value)).ToList();
            //    }
            //}
            return new();
        }
    }
}
