using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Team
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string Name { get; set; }
    public int TotalYellowCards { get; set; }
    public int TotalRedCards { get; set; }
} 