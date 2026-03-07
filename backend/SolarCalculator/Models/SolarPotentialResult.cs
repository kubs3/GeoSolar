namespace SolarCalculator.Models;
public record SolarPotentialResult(double Area, double BuildingAxis, double RoofPitch1, double RoofPitch2, string OrientationText, double Estimated_kWP, List<double[]> Geometry = null);
