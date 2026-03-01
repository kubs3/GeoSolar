import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SolarMapComponent } from './solar-map.component';

describe('SolarMapComponent', () => {
  let component: SolarMapComponent;
  let fixture: ComponentFixture<SolarMapComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SolarMapComponent]
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
