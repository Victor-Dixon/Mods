using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CitiesRegional.Tests.Tools;

/// <summary>
/// Analyzes game logs (Player.log and BepInEx logs) to validate mod behavior
/// </summary>
public class LogAnalyzer
{
    private readonly string _playerLogPath;
    private readonly string _bepInExLogPath;
    
    public LogAnalyzer(string? playerLogPath = null, string? bepInExLogPath = null)
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var localLow = Path.Combine(userProfile, "AppData", "LocalLow");
        
        _playerLogPath = playerLogPath ?? Path.Combine(
            localLow, 
            "Colossal Order", 
            "Cities Skylines II", 
            "Player.log"
        );
        
        _bepInExLogPath = bepInExLogPath ?? Path.Combine(
            localLow,
            "BepInEx",
            "LogOutput.log"
        );
    }
    
    /// <summary>
    /// Analyzes logs and extracts validation data
    /// </summary>
    public LogAnalysisResult Analyze()
    {
        var result = new LogAnalysisResult
        {
            PlayerLogExists = File.Exists(_playerLogPath),
            BepInExLogExists = File.Exists(_bepInExLogPath),
            AnalysisTimestamp = DateTime.Now
        };
        
        if (result.PlayerLogExists)
        {
            AnalyzePlayerLog(_playerLogPath, result);
        }
        
        if (result.BepInExLogExists)
        {
            AnalyzeBepInExLog(_bepInExLogPath, result);
        }
        
        return result;
    }
    
    private void AnalyzePlayerLog(string logPath, LogAnalysisResult result)
    {
        try
        {
            var lines = File.ReadAllLines(logPath);
            result.PlayerLogLineCount = lines.Length;
            
            // Look for CitiesRegional log entries
            var citiesRegionalLines = lines
                .Where(line => line.Contains("[CitiesRegional]"))
                .ToList();
            
            result.CitiesRegionalLogEntries = citiesRegionalLines.Count;
            
            // Extract key events
            ExtractFirstCall(citiesRegionalLines, result);
            ExtractHeartbeats(citiesRegionalLines, result);
            ExtractDataUpdates(citiesRegionalLines, result);
            ExtractTradeData(citiesRegionalLines, result);
            ExtractErrors(citiesRegionalLines, result);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error reading Player.log: {ex.Message}");
        }
    }
    
    private void AnalyzeBepInExLog(string logPath, LogAnalysisResult result)
    {
        try
        {
            var lines = File.ReadAllLines(logPath);
            result.BepInExLogLineCount = lines.Length;
            
            // Prefer CitiesRegional-tagged lines in BepInEx logs.
            // BepInEx commonly formats as: "[Info   :CitiesRegional] message"
            // Our Logging also includes "[CitiesRegional]" inside the message for easier grep.
            var citiesRegionalLines = lines
                .Where(line =>
                    line.Contains(":CitiesRegional]") ||
                    line.Contains("[CitiesRegional]"))
                .ToList();

            // Keep ECS count for diagnostics (some older logs may use "[ECS]" markers)
            var ecsLines = lines.Where(line => line.Contains("[ECS]")).ToList();
            result.EcsLogEntries = ecsLines.Count;

            // Count CitiesRegional entries found in BepInEx log
            result.CitiesRegionalLogEntries += citiesRegionalLines.Count;

            // Extract key events from CitiesRegional lines in BepInEx log
            ExtractFirstCall(citiesRegionalLines, result);
            ExtractHeartbeats(citiesRegionalLines, result);
            ExtractDataUpdates(citiesRegionalLines, result);
            ExtractTradeData(citiesRegionalLines, result);
            ExtractErrors(citiesRegionalLines, result);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error reading BepInEx log: {ex.Message}");
        }
    }
    
    private void ExtractFirstCall(List<string> lines, LogAnalysisResult result)
    {
        var firstCallPattern = new Regex(@"\*\*\*.*OnUpdate.*first.*time.*\*\*\*", RegexOptions.IgnoreCase);
        var firstCall = lines.FirstOrDefault(line => firstCallPattern.IsMatch(line));
        
        if (firstCall != null)
        {
            result.FirstCallDetected = true;
            result.FirstCallTimestamp = ExtractTimestamp(firstCall);
        }
    }
    
    private void ExtractHeartbeats(List<string> lines, LogAnalysisResult result)
    {
        var heartbeatPattern = new Regex(@"Heartbeat.*Frame=(\d+).*Enabled=(\w+).*Pop=(\d+)", RegexOptions.IgnoreCase);
        
        foreach (var line in lines)
        {
            var match = heartbeatPattern.Match(line);
            if (match.Success)
            {
                var heartbeat = new HeartbeatEntry
                {
                    Frame = int.Parse(match.Groups[1].Value),
                    Enabled = match.Groups[2].Value.Equals("True", StringComparison.OrdinalIgnoreCase),
                    Population = int.Parse(match.Groups[3].Value),
                    Timestamp = ExtractTimestamp(line)
                };
                
                result.Heartbeats.Add(heartbeat);
            }
        }
    }
    
    private void ExtractDataUpdates(List<string> lines, LogAnalysisResult result)
    {
        var dataUpdatePattern = new Regex(@"Data Update.*Pop=(\d+).*Treasury=.*?(\d+).*Frame=(\d+)", RegexOptions.IgnoreCase);
        
        foreach (var line in lines)
        {
            var match = dataUpdatePattern.Match(line);
            if (match.Success)
            {
                var dataUpdate = new DataUpdateEntry
                {
                    Population = int.Parse(match.Groups[1].Value),
                    Treasury = long.Parse(match.Groups[2].Value),
                    Frame = int.Parse(match.Groups[3].Value),
                    Timestamp = ExtractTimestamp(line)
                };
                
                result.DataUpdates.Add(dataUpdate);
            }
        }
    }
    
    private void ExtractTradeData(List<string> lines, LogAnalysisResult result)
    {
        var tradePattern = new Regex(@"Trade data.*Export=.*?(\d+).*Import=.*?(\d+)", RegexOptions.IgnoreCase);
        
        foreach (var line in lines)
        {
            var match = tradePattern.Match(line);
            if (match.Success)
            {
                var tradeData = new TradeDataEntry
                {
                    ExportValue = float.Parse(match.Groups[1].Value),
                    ImportValue = float.Parse(match.Groups[2].Value),
                    Timestamp = ExtractTimestamp(line)
                };
                
                result.TradeDataEntries.Add(tradeData);
            }
        }
    }
    
    private void ExtractErrors(List<string> lines, LogAnalysisResult result)
    {
        var errorPattern = new Regex(@"(Error|Exception|Failed|Failed to)", RegexOptions.IgnoreCase);
        
        foreach (var line in lines)
        {
            if (errorPattern.IsMatch(line) &&
                (line.Contains("[CitiesRegional]") || line.Contains(":CitiesRegional]")))
            {
                result.Errors.Add($"Error in log: {line.Trim()}");
            }
        }
    }
    
    private DateTime? ExtractTimestamp(string line)
    {
        // Try to extract timestamp from log line
        // Format varies, so we'll try common patterns
        var timestampPattern = new Regex(@"(\d{4}-\d{2}-\d{2}[\sT]\d{2}:\d{2}:\d{2})");
        var match = timestampPattern.Match(line);
        
        if (match.Success && DateTime.TryParse(match.Groups[1].Value, out var timestamp))
        {
            return timestamp;
        }
        
        return null;
    }
}

/// <summary>
/// Result of log analysis
/// </summary>
public class LogAnalysisResult
{
    public bool PlayerLogExists { get; set; }
    public bool BepInExLogExists { get; set; }
    public int PlayerLogLineCount { get; set; }
    public int BepInExLogLineCount { get; set; }
    public int CitiesRegionalLogEntries { get; set; }
    public int EcsLogEntries { get; set; }
    public bool FirstCallDetected { get; set; }
    public DateTime? FirstCallTimestamp { get; set; }
    public List<HeartbeatEntry> Heartbeats { get; set; } = new();
    public List<DataUpdateEntry> DataUpdates { get; set; } = new();
    public List<TradeDataEntry> TradeDataEntries { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public DateTime AnalysisTimestamp { get; set; }
    
    public bool IsValid => FirstCallDetected && Heartbeats.Count > 0 && DataUpdates.Count > 0 && Errors.Count == 0;
}

/// <summary>
/// Heartbeat log entry
/// </summary>
public class HeartbeatEntry
{
    public int Frame { get; set; }
    public bool Enabled { get; set; }
    public int Population { get; set; }
    public DateTime? Timestamp { get; set; }
}

/// <summary>
/// Data update log entry
/// </summary>
public class DataUpdateEntry
{
    public int Population { get; set; }
    public long Treasury { get; set; }
    public int Frame { get; set; }
    public DateTime? Timestamp { get; set; }
}

/// <summary>
/// Trade data log entry
/// </summary>
public class TradeDataEntry
{
    public float ExportValue { get; set; }
    public float ImportValue { get; set; }
    public DateTime? Timestamp { get; set; }
}

