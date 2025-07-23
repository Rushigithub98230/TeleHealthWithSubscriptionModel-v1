import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MarketingHomeComponent } from './marketing-home.component';

describe('MarketingHomeComponent', () => {
  let component: MarketingHomeComponent;
  let fixture: ComponentFixture<MarketingHomeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MarketingHomeComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MarketingHomeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
