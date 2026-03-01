using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using System.Globalization;

namespace SolarCalculator.Services;

public class SolarCalculationService : ISolarCalculationService
{
    public (double Area, double Azimuth) CalculateAreaAndOrientation(List<double[]> wgs84Coordinates)
    {
        // 1. Determine the approximate center of the building
        double centerLon = wgs84Coordinates[0][0];
        double centerLat = wgs84Coordinates[0][1];

        // 2. Get the best projected (metric) coordinate system for the given location
        var localCoordinateSystem = GetBestCoordinateSystem(centerLon, centerLat);
        
        var ctFactory = new CoordinateTransformationFactory();
        var transformation = ctFactory.CreateFromCoordinateSystems(GeographicCoordinateSystem.WGS84, localCoordinateSystem);
        
        var projectedCoords = new List<Coordinate>();

        foreach (var wgsPoint in wgs84Coordinates)
        {
            double[] projectedPoint = transformation.MathTransform.Transform(new double[] { wgsPoint[0], wgsPoint[1] });
            projectedCoords.Add(new Coordinate(projectedPoint[0], projectedPoint[1]));
        }

        // Ensure the polygon is closed
        if (!projectedCoords[0].Equals2D(projectedCoords[^1])) projectedCoords.Add(projectedCoords[0]);

        var geometryFactory = new GeometryFactory();
        var roofPolygon = geometryFactory.CreatePolygon(projectedCoords.ToArray());
        double area = roofPolygon.Area;

        // 3. Calculate Azimuth directly from WGS84 (True North) - universally applicable
        double maxWallLength = 0;
        double azimuthDeg = 0;
        
        // Convert latitude to radians to calculate the scaling factor (meridian convergence)
        double centerLatRad = centerLat * Math.PI / 180.0;
        double cosLat = Math.Cos(centerLatRad);

        for (int i = 0; i < wgs84Coordinates.Count - 1; i++)
        {
            // Multiply the longitude difference (dx) by cos(lat) to get the real aspect ratio
            double dx = (wgs84Coordinates[i+1][0] - wgs84Coordinates[i][0]) * cosLat;
            double dy = wgs84Coordinates[i+1][1] - wgs84Coordinates[i][1];
            double length = Math.Sqrt(dx * dx + dy * dy);

            if (length > maxWallLength)
            {
                maxWallLength = length;
                
                // Math.Atan2 returns mathematical angle (0° is East, counter-clockwise)
                double mathAngleDeg = Math.Atan2(dy, dx) * 180 / Math.PI;
                
                // Convert to navigational azimuth (0° is North, clockwise)
                azimuthDeg = (450 - mathAngleDeg) % 360;
            }
        }

        return (area, azimuthDeg);
    }

    // ====================================================================
    // Smart CRS Router: Selects the best Coordinate Reference System
    // ====================================================================
    private CoordinateSystem GetBestCoordinateSystem(double lon, double lat)
    {
        var csFactory = new CoordinateSystemFactory();

        // Check if location is within Czechia or Slovakia (Approximate bounding box)
        if (lon >= 12.0 && lon <= 23.0 && lat >= 47.0 && lat <= 51.5)
        {
            // Use S-JTSK Krovak East North for high precision in CZ/SK
            string sjtskWkt = @"PROJCS[""S-JTSK / Krovak East North"",GEOGCS[""S-JTSK"",DATUM[""System_Jednotne_Trigonometricke_Site_Katastralni"",SPHEROID[""Bessel 1841"",6377397.155,299.1528128,AUTHORITY[""EPSG"",""7004""]],TOWGS84[589,76,480,0,0,0,0],AUTHORITY[""EPSG"",""6156""]],PRIMEM[""Greenwich"",0,AUTHORITY[""EPSG"",""8901""]],UNIT[""degree"",0.0174532925199433,AUTHORITY[""EPSG"",""9122""]],AUTHORITY[""EPSG"",""4156""]],PROJECTION[""Krovak""],PARAMETER[""latitude_of_center"",49.5],PARAMETER[""longitude_of_center"",24.83333333333333],PARAMETER[""azimuth"",30.28813975277778],PARAMETER[""pseudo_standard_parallel_1"",78.5],PARAMETER[""scale_factor"",0.9999],PARAMETER[""false_easting"",0],PARAMETER[""false_northing"",0],UNIT[""metre"",1,AUTHORITY[""EPSG"",""9001""]],AUTHORITY[""EPSG"",""5514""]]";
            return csFactory.CreateFromWkt(sjtskWkt);
        }

        // Rest of the world: Dynamically calculate UTM zone
        int utmZone = (int)Math.Floor((lon + 180.0) / 6.0) + 1;
        double centralMeridian = (utmZone * 6) - 183;
        double falseNorthing = lat >= 0 ? 0 : 10000000; // Offset for Southern Hemisphere
        string hemisphere = lat >= 0 ? "N" : "S";

        // Generate UTM definition for the specific location
        string utmWkt = $@"PROJCS[""WGS 84 / UTM zone {utmZone}{hemisphere}"",GEOGCS[""WGS 84"",DATUM[""WGS_1984"",SPHEROID[""WGS 84"",6378137,298.257223563,AUTHORITY[""EPSG"",""7030""]],AUTHORITY[""EPSG"",""6326""]],PRIMEM[""Greenwich"",0,AUTHORITY[""EPSG"",""8901""]],UNIT[""degree"",0.0174532925199433,AUTHORITY[""EPSG"",""9122""]],AUTHORITY[""EPSG"",""4326""]],PROJECTION[""Transverse_Mercator""],PARAMETER[""latitude_of_origin"",0],PARAMETER[""central_meridian"",{centralMeridian.ToString(CultureInfo.InvariantCulture)}],PARAMETER[""scale_factor"",0.9996],PARAMETER[""false_easting"",500000],PARAMETER[""false_northing"",{falseNorthing.ToString(CultureInfo.InvariantCulture)}],UNIT[""metre"",1,AUTHORITY[""EPSG"",""9001""]]]";
        
        return csFactory.CreateFromWkt(utmWkt);
    }
}