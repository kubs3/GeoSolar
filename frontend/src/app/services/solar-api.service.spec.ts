import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';

import { SolarApiService } from './solar-api.service';

describe('SolarApiService', () => {
  let service: SolarApiService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(SolarApiService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
