using NetTopologySuite.Geometries;

namespace SolarCalculator.Services;

public class MockSolarCalculationService : ISolarCalculationService
{
    public (double Area, double Azimuth) CalculateAreaAndOrientation(List<double[]> wgs84Coordinates)
    {
        // --------------------------------------------------------------------------------
        // MOCKED IMPLEMENTATION FOR PUBLIC REPOSITORY
        // --------------------------------------------------------------------------------
        // The actual implementation (AdvancedGisAnalysisService) uses ProjNet and 
        // EPSG:5514 / EPSG:32633 for precise square meter conversions. 
        // For the purpose of this public showcase, we return a randomized mock result.
        
        // Deterministic mock based on coordinates
        double centerLon = wgs84Coordinates[0][0];
        double centerLat = wgs84Coordinates[0][1];
        
        int seed = (int)(Math.Abs(centerLon * centerLat) * 100000);
        var rand = new Random(seed);
        
        double area = rand.Next(40, 160) + Math.Round(rand.NextDouble(), 2);
        double azimuth = rand.Next(90, 270) + Math.Round(rand.NextDouble(), 1);
        
        return (area, azimuth);
    }
}
