using NetTopologySuite.Geometries;

namespace SolarCalculator.Services;

public interface ISolarCalculationService
{
    // Accepts WGS84 coordinates and returns the area in m2 and the azimuth of the longest wall
    (double Area, double Azimuth) CalculateAreaAndOrientation(List<double[]> wgs84Coordinates);
}