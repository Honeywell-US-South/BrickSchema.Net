using Iot.Database;

namespace BrickSchema.Net.DB
{
    internal class PropertyTable
    {
        public long Id { get; set; } = 0;
        public string Guid { get; set; } = System.Guid.NewGuid().ToString();
        public string ParentGuid { get; set; }
        public string Name { get; set; }
        public BsonValue Value { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
