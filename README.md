# GeoSolar (Portfolio Project)

GeoSolar is a comprehensive B2B lead-generation tool designed specifically for companies that install solar panels. This project serves as a showcase of my web development capabilities and represents a ready-to-use solution for capturing highly qualified customer leads.

It provides an intuitive map interface for prospective customers to click on their building and instantly receive an estimation of its roof area, orientation, and potential solar power capacity (kWp). Customers can then submit their contact details along with the calculated roof data directly to the solar installation company.

![GeoSolar App](https://raw.githubusercontent.com/username/GeoSolar/main/screenshot.png)

## Features
- **Interactive Map:** Powered by Leaflet and OpenStreetMap.
- **Address Search:** Integrated Nominatim Geocoding API to quickly fly to any location.
- **Instant Solar Calculation:** Automatically calculates roof area, primary axis orientation, and estimates kWp yield based on building geometry.
- **Lead Generation:** Integrated form to collect user contact details alongside the calculated roof data.

## Architecture
The project is structured as a monorepo containing two main components:
- **`frontend/`**: An Angular 17+ single-page application (SPA).
- **`backend/`**: An ASP.NET Core 8 API providing geometric calculations and email services.

---

## 🚀 Getting Started

### 1. Backend Setup (ASP.NET Core)
1. Ensure you have the [.NET 8.0 SDK](https://dotnet.microsoft.com/download) installed.
2. Navigate to the backend directory:
   ```bash
   cd backend
   ```
3. Run the API:
   ```bash
   dotnet run
   ```
   *The API will start on `http://localhost:5093`.*

### 2. Frontend Setup (Angular)
1. Ensure you have [Node.js](https://nodejs.org/) (v18+) and npm installed.
2. Navigate to the frontend directory:
   ```bash
   cd frontend
   ```
3. Install dependencies:
   ```bash
   npm install
   ```
4. Start the development server:
   ```bash
   npm run start
   ```
   *The application will be available at `http://localhost:4200`.*

---

## Technical Stack
* **Frontend:** Angular, TypeScript, SCSS, Leaflet
* **Backend:** C#, ASP.NET Core Minimal APIs, NetTopologySuite, ProjNet
* **Data Sources:** OpenStreetMap (Overpass API for building geometry, Nominatim for address search)

## License
This project is showcased as part of my professional freelance portfolio. It is **not** licensed for open-source redistribution or commercial use without permission. If you are a solar installation company interested in using or customizing this tool for your business, please contact me!
