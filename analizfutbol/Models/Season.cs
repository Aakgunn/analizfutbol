using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

public class Season
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string Name { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class SeasonStatistics
{
    public string SeasonId { get; set; }
    public string SeasonName { get; set; }
    public int TotalMatches { get; set; }
    public int TotalYellowCards { get; set; }
    public int TotalRedCards { get; set; }
    public double AverageYellowCardsPerMatch { get; set; }
    public double AverageRedCardsPerMatch { get; set; }
    public List<RefereeSeasonSummary> TopReferees { get; set; }
}

public class RefereeSeasonSummary
{
    public string RefereeId { get; set; } = string.Empty;
    public string RefereeName { get; set; } = string.Empty;
    public string SeasonId { get; set; } = string.Empty;
    public string SeasonName { get; set; } = string.Empty;
    public int TotalMatches { get; set; }
    public int TotalYellowCards { get; set; }
    public int TotalRedCards { get; set; }
    public double AverageCardsPerMatch { get; set; }
} 