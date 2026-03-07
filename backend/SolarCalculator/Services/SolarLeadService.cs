using NetTopologySuite.Geometries;
using SolarCalculator.Models;
using System.Text.Json.Nodes;

namespace SolarCalculator.Services;

public interface ISolarLeadService
{
    Task<SolarPotentialResult?> GetPotentialAsync(double lat, double lon);
}

public class SolarLeadService : ISolarLeadService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly ISolarCalculationService _geoService;

    public SolarLeadService(IHttpClientFactory httpFactory, ISolarCalculationService geoService)
    {
        _httpFactory = httpFactory;
        _geoService = geoService;
    }

    public async Task<SolarPotentialResult?> GetPotentialAsync(double lat, double lon)
    {
        var http = _httpFactory.CreateClient("OsmClient");
        string latStr = lat.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string lonStr = lon.ToString(System.Globalization.CultureInfo.InvariantCulture);

        string query = $"[out:json];way[\"building\"](around:40,{latStr},{lonStr});out geom;";
        var response = await http.GetFromJsonAsync<JsonObject>($"https://overpass-api.de/api/interpreter?data={Uri.EscapeDataString(query)}");

        var elements = response?["elements"] as JsonArray;
        if (elements == null || elements.Count == 0) return null;

        JsonArray? bestGeometry = null;
        double minDistance = double.MaxValue;
        var geometryFactory = new GeometryFactory();
        var clickPoint = geometryFactory.CreatePoint(new Coordinate(lon, lat));

        foreach (var element in elements)
        {
            var geometry = element["geometry"] as JsonArray;
            if (geometry == null || geometry.Count < 3) continue;

            var coordsList = geometry.Select(n => new Coordinate((double)n["lon"]!, (double)n["lat"]!)).ToList();
            if (!coordsList[0].Equals2D(coordsList[^1])) coordsList.Add(coordsList[0]);

            var poly = geometryFactory.CreatePolygon(coordsList.ToArray());

            // Exact match if the clicked point falls within the building polygon
            if (poly.Contains(clickPoint))
            {
                bestGeometry = geometry;
                break;
            }

            // Fallback: finding the closest building
            double dist = poly.Distance(clickPoint);
            if (dist < minDistance)
            {
                minDistance = dist;
                bestGeometry = geometry;
            }
        }

        if (bestGeometry == null) return null;

        var coords = bestGeometry.Select(n => new double[] { (double)n["lon"]!, (double)n["lat"]! }).ToList();

        var geoResult = _geoService.CalculateAreaAndOrientation(coords);

        return FormatResult(geoResult.Area, geoResult.Azimuth, coords);
    }

    private SolarPotentialResult FormatResult(double area, double azimuth, List<double[]> geometry)
    {
        double p1 = (azimuth + 90) % 360;
        double p2 = (azimuth + 270) % 360;

        return new SolarPotentialResult(
            Math.Round(area, 2),
            Math.Round(azimuth, 1),
            Math.Round(p1, 1),
            Math.Round(p2, 1),
            $"{GetDir(p1)} / {GetDir(p2)}",
            Math.Round((area * 0.6) / 5.0, 2),
            geometry
        );
    }

    private string GetDir(double deg)
    {
        if (deg >= 337.5 || deg < 22.5) return "Sever";
        if (deg >= 22.5 && deg < 67.5) return "Severovýchod";
        if (deg >= 67.5 && deg < 112.5) return "Východ";
        if (deg >= 112.5 && deg < 157.5) return "Jihovýchod";
        if (deg >= 157.5 && deg < 202.5) return "Jih";
        if (deg >= 202.5 && deg < 247.5) return "Jihozápad";
        if (deg >= 247.5 && deg < 292.5) return "Západ";
        return "Severozápad";
    }
}