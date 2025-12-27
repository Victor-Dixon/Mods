/*
 * Cities Regional - Simple Server Prototype
 * 
 * This is a minimal ASP.NET Core server for regional data exchange.
 * Run this separately from the game mod.
 * 
 * To run:
 *   1. Create a new ASP.NET Core Web API project
 *   2. Copy this code
 *   3. dotnet run
 * 
 * Or use the provided docker-compose.yml
 */

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace CitiesRegional.Server;

#region Models (Copy from mod)

public class Region
{
    public string RegionId { get; set; } = Guid.NewGuid().ToString();
    public string RegionName { get; set; } = "";
    public string RegionCode { get; set; } = "";
    public int MaxCities { get; set; } = 4;
    public List<RegionalCityData> Cities { get; set; } = new();
    public List<RegionalConnection> Connections { get; set; } = new();
    public List<RegionalEvent> Events { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
}

public class RegionalCityData
{
    public string CityId { get; set; } = "";
    public string CityName { get; set; } = "";
    public string PlayerName { get; set; } = "";
    public string PlayerId { get; set; } = "";
    public bool IsOnline { get; set; }
    public DateTime LastSync { get; set; }
    public int Population { get; set; }
    public float Happiness { get; set; }
    // ... other fields as needed
}

public class RegionalConnection
{
    public string ConnectionId { get; set; } = "";
    public string FromCityId { get; set; } = "";
    public string ToCityId { get; set; } = "";
    public string Type { get; set; } = "";
    public int Capacity { get; set; }
}

public class RegionalEvent
{
    public string EventId { get; set; } = "";
    public string Type { get; set; } = "";
    public string Title { get; set; } = "";
    public DateTime Timestamp { get; set; }
}

public class SharedServiceInfo
{
    public string ServiceId { get; set; } = "";
    public string Type { get; set; } = "";
    public int Capacity { get; set; }
}

#endregion

#region Storage (In-Memory for prototype)

public interface IRegionStore
{
    Region? GetById(string regionId);
    Region? GetByCode(string regionCode);
    void Save(Region region);
    void Delete(string regionId);
    IEnumerable<Region> GetAll();
}

public class InMemoryRegionStore : IRegionStore
{
    private readonly ConcurrentDictionary<string, Region> _regions = new();
    
    public Region? GetById(string regionId) => 
        _regions.TryGetValue(regionId, out var r) ? r : null;
    
    public Region? GetByCode(string regionCode) => 
        _regions.Values.FirstOrDefault(r => r.RegionCode == regionCode);
    
    public void Save(Region region)
    {
        region.LastActivity = DateTime.UtcNow;
        _regions[region.RegionId] = region;
    }
    
    public void Delete(string regionId) => 
        _regions.TryRemove(regionId, out _);
    
    public IEnumerable<Region> GetAll() => _regions.Values;
}

#endregion

#region API Controllers

[ApiController]
[Route("api/regions")]
public class RegionsController : ControllerBase
{
    private readonly IRegionStore _store;
    
    public RegionsController(IRegionStore store)
    {
        _store = store;
    }
    
    /// <summary>
    /// Create a new region
    /// </summary>
    [HttpPost]
    public ActionResult<Region> CreateRegion([FromBody] CreateRegionRequest request)
    {
        var region = new Region
        {
            RegionId = Guid.NewGuid().ToString(),
            RegionCode = GenerateCode(),
            RegionName = request.Name,
            MaxCities = request.MaxCities
        };
        
        _store.Save(region);
        return Ok(region);
    }
    
    /// <summary>
    /// Get region by code
    /// </summary>
    [HttpGet("code/{code}")]
    public ActionResult<Region> GetByCode(string code)
    {
        var region = _store.GetByCode(code);
        if (region == null) return NotFound();
        return Ok(region);
    }
    
    /// <summary>
    /// Get region by ID
    /// </summary>
    [HttpGet("{regionId}")]
    public ActionResult<Region> GetById(string regionId)
    {
        var region = _store.GetById(regionId);
        if (region == null) return NotFound();
        return Ok(region);
    }
    
    /// <summary>
    /// Get all cities in a region
    /// </summary>
    [HttpGet("{regionId}/cities")]
    public ActionResult<List<RegionalCityData>> GetCities(string regionId)
    {
        var region = _store.GetById(regionId);
        if (region == null) return NotFound();
        return Ok(region.Cities);
    }
    
    /// <summary>
    /// Update a city's data
    /// </summary>
    [HttpPut("{regionId}/cities/{cityId}")]
    public ActionResult UpdateCity(string regionId, string cityId, 
                                    [FromBody] RegionalCityData cityData)
    {
        var region = _store.GetById(regionId);
        if (region == null) return NotFound();
        
        if (region.Cities.Count >= region.MaxCities && 
            !region.Cities.Any(c => c.CityId == cityId))
        {
            return BadRequest("Region is full");
        }
        
        // Remove existing and add updated
        region.Cities.RemoveAll(c => c.CityId == cityId);
        cityData.LastSync = DateTime.UtcNow;
        region.Cities.Add(cityData);
        
        _store.Save(region);
        return Ok();
    }
    
    /// <summary>
    /// Remove a city from the region
    /// </summary>
    [HttpDelete("{regionId}/cities/{cityId}")]
    public ActionResult RemoveCity(string regionId, string cityId)
    {
        var region = _store.GetById(regionId);
        if (region == null) return NotFound();
        
        region.Cities.RemoveAll(c => c.CityId == cityId);
        _store.Save(region);
        return Ok();
    }
    
    /// <summary>
    /// Add a connection between cities
    /// </summary>
    [HttpPost("{regionId}/connections")]
    public ActionResult AddConnection(string regionId, 
                                       [FromBody] RegionalConnection connection)
    {
        var region = _store.GetById(regionId);
        if (region == null) return NotFound();
        
        connection.ConnectionId = Guid.NewGuid().ToString();
        region.Connections.Add(connection);
        
        _store.Save(region);
        return Ok(connection);
    }
    
    /// <summary>
    /// Get recent events
    /// </summary>
    [HttpGet("{regionId}/events")]
    public ActionResult<List<RegionalEvent>> GetEvents(string regionId, 
                                                        [FromQuery] DateTime? since = null)
    {
        var region = _store.GetById(regionId);
        if (region == null) return NotFound();
        
        var events = since.HasValue
            ? region.Events.Where(e => e.Timestamp > since.Value).ToList()
            : region.Events.Take(50).ToList();
            
        return Ok(events);
    }
    
    /// <summary>
    /// Post an event
    /// </summary>
    [HttpPost("{regionId}/events")]
    public ActionResult PostEvent(string regionId, [FromBody] RegionalEvent evt)
    {
        var region = _store.GetById(regionId);
        if (region == null) return NotFound();
        
        evt.EventId = Guid.NewGuid().ToString();
        evt.Timestamp = DateTime.UtcNow;
        region.Events.Insert(0, evt);
        
        // Keep only last 100 events
        if (region.Events.Count > 100)
            region.Events = region.Events.Take(100).ToList();
        
        _store.Save(region);
        return Ok(evt);
    }
    
    /// <summary>
    /// Add/update a shared service
    /// </summary>
    [HttpPost("{regionId}/services")]
    public ActionResult AddService(string regionId, [FromBody] SharedServiceInfo service)
    {
        // TODO: Implement shared services storage
        return Ok(service);
    }
    
    private static string GenerateCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var part1 = new string(Enumerable.Range(0, 4).Select(_ => chars[random.Next(chars.Length)]).ToArray());
        var part2 = new string(Enumerable.Range(0, 4).Select(_ => chars[random.Next(chars.Length)]).ToArray());
        return $"{part1}-{part2}";
    }
}

public class CreateRegionRequest
{
    public string Name { get; set; } = "";
    public int MaxCities { get; set; } = 4;
}

#endregion

#region Program Entry Point

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Add services
        builder.Services.AddControllers();
        builder.Services.AddSingleton<IRegionStore, InMemoryRegionStore>();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        // CORS for local development
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });
        
        var app = builder.Build();
        
        // Configure pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        
        app.UseCors("AllowAll");
        app.MapControllers();
        
        Console.WriteLine("Cities Regional Server starting on http://localhost:5000");
        app.Run("http://localhost:5000");
    }
}

#endregion

