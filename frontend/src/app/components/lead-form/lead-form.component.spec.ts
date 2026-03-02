import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';

import { LeadFormComponent } from './lead-form.component';

describe('LeadFormComponent', () => {
  let component: LeadFormComponent;
  let fixture: ComponentFixture<LeadFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LeadFormComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()]
    })
      .compileComponents();

    fixture = TestBed.createComponent(LeadFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
