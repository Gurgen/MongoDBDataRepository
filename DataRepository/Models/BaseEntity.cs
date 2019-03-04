using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartArmenia.DataRepository.Models
{
    public class ObjectIdBaseEntity : BaseEntity
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public new string Id { get; set; }
    }

    public class StringIdBaseEntity : BaseEntity
    {
        [BsonRepresentation(BsonType.String)]
        public new string Id { get; set; }
    }

    [BsonIgnoreExtraElements(true)]
    public abstract class BaseEntity
    {
        [BsonIgnore] 
        public string Id { get; set; }
        public long LastUpdate { get; set; }
        public bool Active { get; set; } = true;
    }
}