using Xunit;
using SolarCalculator.Services;

namespace SolarCalculator.Tests;

public class SolarCalculationServiceTests
{
    private readonly ISolarCalculationService _service;

    public SolarCalculationServiceTests()
    {
        // Setup: Initialize the service before each test
        _service = new SolarCalculationService();
    }

    [Fact]
    public void CalculateArea_ValidSquare_ReturnsExpectedAreaMeters()
    {
        // Arrange: Approx 10x10 meters square in Olomouc (WGS84)
        // 1 degree of latitude is approx 111 km. 10 meters is roughly 0.00009 degrees.
        var coords = new List<double[]>
        {
            new double[] { 17.25100, 49.59300 },
            new double[] { 17.25100, 49.59309 },
            new double[] { 17.25114, 49.59309 },
            new double[] { 17.25114, 49.59300 },
            new double[] { 17.25100, 49.59300 } // Closed polygon
        };

        // Act
        var result = _service.CalculateAreaAndOrientation(coords);

        // Assert
        // We verify the area falls within a reasonable margin due to spherical WGS84 conversion
        Assert.True(result.Area > 50 && result.Area < 150, $"Area should be around 100m2, but was {result.Area}");
    }

    [Fact]
    public void CalculateOrientation_LongestWallEastWest_ReturnsAzimuthAround90or270()
    {
        // Arrange: Rectangle with the longest wall oriented East-West
        var coords = new List<double[]>
        {
            new double[] { 17.25100, 49.59300 }, // SW corner
            new double[] { 17.25100, 49.59302 }, // NW corner (short wall)
            new double[] { 17.25300, 49.59302 }, // NE corner (long wall)
            new double[] { 17.25300, 49.59300 }, // SE corner (short wall)
            new double[] { 17.25100, 49.59300 }  // Closed polygon
        };

        // Act
        var result = _service.CalculateAreaAndOrientation(coords);

        // Assert
        // The math should identify the wall with an azimuth around 90° (East) or 270° (West)
        bool isEastWest = (result.Azimuth >= 70 && result.Azimuth <= 110) || (result.Azimuth >= 250 && result.Azimuth <= 290);
        Assert.True(isEastWest, $"Azimuth should be East/West, but calculated as {result.Azimuth}°");
    }

    [Fact]
    public void CalculateArea_UnclosedPolygonFromOSM_AutoClosesWithoutException()
    {
        // Arrange: Common data issue - missing closing point
        var unclosedCoords = new List<double[]>
        {
            new double[] { 17.25100, 49.59300 },
            new double[] { 17.25100, 49.59309 },
            new double[] { 17.25114, 49.59309 },
            new double[] { 17.25114, 49.59300 }
        };

        // Act
        var exception = Record.Exception(() => _service.CalculateAreaAndOrientation(unclosedCoords));
        var result = _service.CalculateAreaAndOrientation(unclosedCoords);

        // Assert
        Assert.Null(exception); // The service must not crash
        Assert.True(result.Area > 0, "Area must be calculated even for initially unclosed polygons.");
    }

    [Fact]
    public void CalculateArea_LocationOutsideCZ_UsesUTMAndReturnsExpectedArea()
    {
        // Arrange: Approx 10x10 meters square in Berlin (outside S-JTSK bounding box)
        // CZ bounding box ends at 51.5°N. Berlin is at 52.5°N.
        var berlinCoords = new List<double[]>
        {
            new double[] { 13.40500, 52.52000 },
            new double[] { 13.40500, 52.52009 },
            new double[] { 13.40514, 52.52009 },
            new double[] { 13.40514, 52.52000 },
            new double[] { 13.40500, 52.52000 } // Closed polygon
        };

        // Act
        // The service should detect it's outside CZ, calculate UTM zone 33N and use it
        var result = _service.CalculateAreaAndOrientation(berlinCoords);

        // Assert
        // In UTM, the area must be calculated correctly around 100m2.
        Assert.True(result.Area > 50 && result.Area < 150, 
            $"Expected area in Berlin was ~100m2, but system returned {result.Area} m2. CRS Router likely failed.");
    }
}