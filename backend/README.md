# ☀️ Solar Potential & Spatial Analysis API

![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)
![Architecture](https://img.shields.io/badge/Architecture-Clean%20DI-success.svg)
![Resiliency](https://img.shields.io/badge/Resiliency-Polly-orange.svg)
![Tests](https://img.shields.io/badge/Tests-xUnit-brightgreen.svg)

A robust, enterprise-ready **.NET 10 Web API** designed for the B2B Solar and Green Energy sector.
This microservice automates the assessment of building solar potential by orchestrating third-party geocoding APIs, fetching building footprints, and performing complex spatial transformations (WGS84 to local metric systems).

## 🎯 Business Value

Sales representatives in the solar industry spend hours manually estimating roof areas on maps. This API automates the process. Send an address anywhere in the world, and the service returns:

1. Exact building footprint (via OpenStreetMap).
2. **Real square meter area** using a Smart CRS Router.
3. **Roof orientation (Azimuth - True North)** calculated via vector analysis of the longest building wall.
4. Estimated solar capacity (kWp).

## 🌍 Smart CRS Router (Global Scale)

Calculating accurate square meters from spherical GPS coordinates requires local planar projections. This API implements a dynamic routing system using `ProjNET`:

- **CZ & SK Region:** Automatically routes to **S-JTSK (EPSG:5514)** for millimeter precision using the Krovak projection.
- **Rest of the World:** Dynamically calculates the correct **UTM Zone** based on the longitude and generates a custom metric coordinate system on the fly.

## 🏗️ Architecture & Extensibility

This project demonstrates the intersection of **GIS (Geographic Information Systems)** and solid **Software Engineering**:

- **Dependency Injection:** Core spatial logic is completely decoupled from the API endpoints.
- **Easily Swappable Data Sources:** Currently configured to use free OpenStreetMap (Nominatim/Overpass) for demonstration purposes. Due to the DI architecture, swapping this to premium data sources like **Google Solar API** or **Local Cadastral Data (e.g., RÚIAN)** for production use is trivial and requires zero changes to the core mathematical logic.
- **Fault Tolerance:** `Polly` is implemented to handle intermittent network failures from 3rd-party map providers (Wait-and-Retry policies).

## 🚀 Example Response

**GET** `/api/solar-potential?address=Brandenburger Tor, Berlin`

```json
{
  "queryAddress": "Brandenburger Tor, Berlin",
  "buildingData": {
    "totalRoofArea_m2": 4512.3,
    "usableRoofArea_m2": 2707.38,
    "estimatedSolarCapacity_kWp": 541.48,
    "mainAxisAzimuthDeg": 92.1,
    "orientationEstimate": "Roof planes likely face: East / West (Ideal for morning/evening distributed generation)"
  },
  "message": "Spatial data successfully fetched and analyzed."
}
```

## 🛡️ Reliability & QA

As a developer with a strong background in Quality Assurance, I prioritize system stability:

Corrupt Data Handling: Automatically detects and closes malformed polygons fetched from open data sources.

Network Resilience: External HTTP calls are wrapped in Polly policies to gracefully handle 504 Gateway Timeout or 502 Bad Gateway errors without crashing the service.

Automated Test Suite: Core spatial logic and the Smart CRS Router are fully covered by xUnit tests to ensure mathematical accuracy across different hemispheres.

Developed by Jakub Kohn - Freelance GIS & .NET Developer
