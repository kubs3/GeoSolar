import { TestBed } from '@angular/core/testing';

import { SolarApiService } from './solar-api.service';

describe('SolarApiService', () => {
  let service: SolarApiService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SolarApiService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
