import { Component, EventEmitter, Output, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { SolarApiService } from '../../services/solar-api.service';
import { RoofData, LeadData, LeadPayload } from '../../models/solar.models';

@Component({
  selector: 'app-lead-form',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './lead-form.component.html',
  styleUrl: './lead-form.component.scss'
})
export class LeadFormComponent {
  // Roof data coming from the map map (area, orientation, power estimate)
  @Input() roofData: RoofData | null = null;

  // Event to close the modal (sets isLeadFormVisible = false in map-component)
  @Output() close = new EventEmitter<void>();

  // Form data model
  leadData: LeadData = {
    name: '',
    email: '',
    phone: ''
  };

  // Submission state
  submitted = false;
  isSending = false;

  constructor(private solarApi: SolarApiService) { }

  onSubmit() {
    // If we don't have roof data, something went wrong
    if (!this.roofData) {
      alert('Error: Roof data not found.');
      return;
    }

    this.isSending = true;

    // Build the final payload for the backend (combine contact + technical data)
    const finalPayload: LeadPayload = {
      name: this.leadData.name,
      email: this.leadData.email,
      phone: this.leadData.phone,
      roofArea: this.roofData.area,
      orientation: this.roofData.orientationText,
      estimatedKWp: this.roofData.estimated_kWP
    };

    // Call the backend to send the email
    this.solarApi.submitLead(finalPayload).subscribe({
      next: (response) => {
        console.log('Lead successfully sent:', response);
        this.isSending = false;
        this.submitted = true;

        // Close form and reset after 3 seconds
        setTimeout(() => {
          this.close.emit();
          this.submitted = false;
        }, 3000);
      },
      error: (err) => {
        console.error('Error sending lead:', err);
        this.isSending = false;
        alert('Failed to submit request. Check if the backend is running.');
      }
    });
  }
}