using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace analizfutbol.Models
{
    public class MatchPrediction
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string HomeTeamId { get; set; }
        public string AwayTeamId { get; set; }
        public string RefereeId { get; set; }
        public DateTime PredictionDate { get; set; }
        public double PredictedHomeTeamCards { get; set; }
        public double PredictedAwayTeamCards { get; set; }
        public double WinProbabilityHome { get; set; }
        public double WinProbabilityAway { get; set; }
        public double DrawProbability { get; set; }
        
        // Tahmin güvenilirliği (0-100 arası)
        public double ConfidenceScore { get; set; }
        
        // Tahmin için kullanılan geçmiş maç sayısı
        public int HistoricalMatchesUsed { get; set; }
    }

    public class PredictionFactors
    {
        public double HomeTeamFormWeight { get; set; }
        public double AwayTeamFormWeight { get; set; }
        public double RefereeHistoryWeight { get; set; }
        public double HeadToHeadWeight { get; set; }
        public int FormMatchCount { get; set; } // Son kaç maçın dikkate alınacağı
    }
} 