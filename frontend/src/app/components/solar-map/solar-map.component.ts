import { Component, AfterViewInit } from '@angular/core';
import * as L from 'leaflet';
import { SolarApiService } from '../../services/solar-api.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LeadFormComponent } from '../lead-form/lead-form.component';
import { RoofData } from '../../models/solar.models';

const iconRetinaUrl = 'assets/leaflet/marker-icon-2x.png';
const iconUrl = 'assets/leaflet/marker-icon.png';
const shadowUrl = 'assets/leaflet/marker-shadow.png';
const iconDefault = L.icon({
  iconRetinaUrl,
  iconUrl,
  shadowUrl,
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  tooltipAnchor: [16, -28],
  shadowSize: [41, 41]
});
L.Marker.prototype.options.icon = iconDefault;

@Component({
  selector: 'app-solar-map',
  standalone: true,
  // Need to import CommonModule for @if and LeadFormComponent for our form
  imports: [CommonModule, FormsModule, LeadFormComponent],
  templateUrl: './solar-map.component.html',
  styleUrl: './solar-map.component.scss'
})
export class SolarMapComponent implements AfterViewInit {
  private map!: L.Map;
  private marker?: L.Marker;
  public result: RoofData | null = null;
  public isLoading = false;

  public isLeadFormVisible = false;

  public searchQuery = '';
  public isSearching = false;

  constructor(private solarApi: SolarApiService) { }

  ngAfterViewInit(): void {
    this.initMap();
  }

  private initMap(): void {
    this.map = L.map('map').setView([49.8, 15.5], 7);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© OpenStreetMap contributors'
    }).addTo(this.map);

    this.map.on('click', (e: L.LeafletMouseEvent) => {
      this.onMapClick(e.latlng.lat, e.latlng.lng);
    });
  }

  private onMapClick(lat: number, lng: number): void {
    if (this.marker) this.map.removeLayer(this.marker);
    this.marker = L.marker([lat, lng]).addTo(this.map);

    this.isLoading = true;
    this.result = null;

    this.solarApi.calculateSolarPotential(lat, lng).subscribe({
      next: (data: RoofData) => {
        this.result = data;
        this.isLoading = false;
        console.log('Data from backend:', data);
      },
      error: (err: any) => {
        console.error('Error communicating with backend:', err);
        this.isLoading = false;
        alert('Error communicating with the backend (possibly failed to find a building).');
      }
    });
  }

  public searchAddress(): void {
    if (!this.searchQuery.trim()) return;

    this.isSearching = true;

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
          this.map.flyTo([lat, lon], 18);
        } else {
          alert('Adresa nebyla nalezena. Zkuste upřesnit zadání (např. včetně města).');
        }
      })
      .catch(error => {
        console.error('Chyba při vyhledávání adresy:', error);
        this.isSearching = false;
        alert('Došlo k chybě při vyhledávání. Zkuste to prosím znovu.');
      });
  }
}