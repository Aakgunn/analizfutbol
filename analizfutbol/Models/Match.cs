using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace analizfutbol.Models
{
    public class Match
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string RefereeId { get; set; }
        public string HomeTeamId { get; set; }
        public string AwayTeamId { get; set; }
        public DateTime MatchDate { get; set; }
        public int HomeTeamScore { get; set; }
        public int AwayTeamScore { get; set; }
        public int HomeTeamYellowCards { get; set; }
        public int HomeTeamRedCards { get; set; }
        public int AwayTeamYellowCards { get; set; }
        public int AwayTeamRedCards { get; set; }
    }
} 