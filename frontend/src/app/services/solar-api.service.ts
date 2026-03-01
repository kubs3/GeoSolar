import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { RoofData, LeadPayload } from '../models/solar.models';

@Injectable({
  providedIn: 'root'
})
export class SolarApiService {
  // Adjust the URL depending on where your backend is running
  private apiUrl = 'http://localhost:5093/api/solar/calculate';

  constructor(private http: HttpClient) { }

  calculateSolarPotential(lat: number, lng: number): Observable<RoofData> {
    return this.http.post<RoofData>(this.apiUrl, { latitude: lat, longitude: lng });
  }
  submitLead(leadData: LeadPayload): Observable<any> {
    return this.http.post<any>('http://localhost:5093/api/solar/submit-lead', leadData);
  }
}