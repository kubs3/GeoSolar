import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';

import { SolarMapComponent } from './solar-map.component';

describe('SolarMapComponent', () => {
  let component: SolarMapComponent;
  let fixture: ComponentFixture<SolarMapComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SolarMapComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()]
    })
      .compileComponents();

    fixture = TestBed.createComponent(SolarMapComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
