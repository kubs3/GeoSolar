import { Component, AfterViewInit, OnDestroy } from '@angular/core';
import * as maptilersdk from '@maptiler/sdk';
import { SolarApiService } from '../../services/solar-api.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LeadFormComponent } from '../lead-form/lead-form.component';
import { RoofData } from '../../models/solar.models';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-solar-map',
  standalone: true,
  imports: [CommonModule, FormsModule, LeadFormComponent],
  templateUrl: './solar-map.component.html',
  styleUrl: './solar-map.component.scss'
})
export class SolarMapComponent implements AfterViewInit, OnDestroy {
  private map?: maptilersdk.Map;
  private marker?: maptilersdk.Marker;
  public result: RoofData | null = null;
  public isLoading = false;

  public isLeadFormVisible = false;

  public searchQuery = '';
  public isSearching = false;

  public errorMessage: string | null = null;
  private errorTimeout: any;

  // New Map UX States
  public currentStyle: 'HYBRID' | 'STREETS' = 'STREETS';
  public userManuallyChangedStyle = false;

  constructor(private solarApi: SolarApiService) { }

  ngAfterViewInit(): void {
    // Initialize MapTiler with the API Key from environment
    maptilersdk.config.apiKey = environment.maptilerApiKey;
    this.initMap();
  }

  ngOnDestroy(): void {
    if (this.map) {
      this.map.remove();
    }
  }

  private initMap(): void {
    // Start with basic map for speed and clarity
    this.currentStyle = 'STREETS';
    this.map = new maptilersdk.Map({
      container: 'map',
      style: maptilersdk.MapStyle.STREETS,
      center: [15.5, 49.8],
      zoom: 7
    });

    this.map.on('click', (e) => {
      // e.lngLat contains the precise coordinates
      this.onMapClick(e.lngLat.lat, e.lngLat.lng);
    });

    // UX Feature: Dynamic zoom-based map style switching
    this.map.on('zoomend', () => {
      if (this.userManuallyChangedStyle || !this.map) return;

      const currentZoom = this.map.getZoom();

      // Auto-switch to Hybrid when zoomed in close (street/roof level)
      if (currentZoom >= 17 && this.currentStyle !== 'HYBRID') {
        this.currentStyle = 'HYBRID';
        this.map.setStyle(maptilersdk.MapStyle.HYBRID);
      }
      // Auto-switch back to Streets when zoomed out (city/country level)
      else if (currentZoom < 17 && this.currentStyle !== 'HYBRID' /* Wait, let's auto-switch TO streets only if they were auto-switched to hybrid previously */) {
        // Actually, if they zoom out, always go back to clean streets to reduce visual noise
        if (this.currentStyle !== 'STREETS') {
          this.currentStyle = 'STREETS';
          this.map.setStyle(maptilersdk.MapStyle.STREETS);
        }
      }
    });
  }

  private onMapClick(lat: number, lng: number): void {
    if (this.marker) {
      this.marker.remove();
    }

    const sourceId = 'roof-source';
    const layerId = 'roof-layer';

    if (this.map) {
      if (this.map.getLayer(layerId)) {
        this.map.removeLayer(layerId);
      }
      if (this.map.getSource(sourceId)) {
        this.map.removeSource(sourceId);
      }
    }

    // MapTiler Marker takes [lng, lat]
    if (this.map) {
      this.marker = new maptilersdk.Marker({ color: "#FF0000" })
        .setLngLat([lng, lat])
        .addTo(this.map);
    }

    this.isLoading = true;
    this.result = null;
    this.clearError();

    // Send the latitude and longitude explicitly to maintain backward compatibility with .NET backend
    this.solarApi.calculateSolarPotential(lat, lng).subscribe({
      next: (data: RoofData) => {
        this.result = data;
        this.isLoading = false;
        console.log('Data from backend:', data);

        // Draw roof polygon if geometry is returned
        if (data.geometry && data.geometry.length > 0 && this.map) {
          const geoJson: any = {
            type: 'Feature',
            geometry: {
              type: 'Polygon',
              coordinates: [data.geometry] // Data from backend is already [[lon, lat], [lon, lat]...] list
            }
          };

          this.map.addSource('roof-source', {
            type: 'geojson',
            data: geoJson
          });

          this.map.addLayer({
            id: 'roof-layer',
            type: 'fill',
            source: 'roof-source',
            paint: {
              'fill-color': '#40E0D0', // Turquoise
              'fill-opacity': 0.6
            }
          });
        }
      },
      error: (err: any) => {
        console.error('Error communicating with backend:', err);
        this.isLoading = false;
        this.showError('Nepodařilo se najít budovu na vybrané pozici. Zkuste kliknout přesněji na střechu.');
      }
    });
  }

  public searchAddress(): void {
    if (!this.searchQuery.trim()) return;

    this.isSearching = true;
    this.clearError();

    // Call Nominatim API for geocoding
    const url = `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(this.searchQuery)}`;

    fetch(url)
      .then(response => response.json())
      .then(data => {
        this.isSearching = false;
        if (data && data.length > 0) {
          const firstResult = data[0];
          const lat = parseFloat(firstResult.lat);
          const lon = parseFloat(firstResult.lon);

          // Move and zoom map to the searched location (zoom level 18 is close up)
          if (this.map) {
            this.map.flyTo({ center: [lon, lat], zoom: 18 });
          }
        } else {
          this.showError('Adresa nebyla nalezena. Zkuste upřesnit zadání (např. včetně města).');
        }
      })
      .catch(error => {
        console.error('Chyba při vyhledávání adresy:', error);
        this.isSearching = false;
        this.showError('Došlo k chybě při vyhledávání. Zkuste to prosím znovu.');
      });
  }

  private showError(message: string): void {
    this.errorMessage = message;
    if (this.errorTimeout) clearTimeout(this.errorTimeout);
    this.errorTimeout = setTimeout(() => {
      this.errorMessage = null;
    }, 6000);
  }

  private clearError(): void {
    this.errorMessage = null;
    if (this.errorTimeout) clearTimeout(this.errorTimeout);
  }

  public setMapStyle(styleType: 'HYBRID' | 'STREETS'): void {
    if (!this.map) return;

    this.userManuallyChangedStyle = true; // Prevents auto-zoom switching from fighting the user
    this.currentStyle = styleType;
    if (styleType === 'HYBRID') {
      this.map.setStyle(maptilersdk.MapStyle.HYBRID);
    } else {
      this.map.setStyle(maptilersdk.MapStyle.STREETS);
    }
  }
}