import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RedeemCeilingComponent } from './redeem-ceiling.component';

describe('RedeemCeilingComponent', () => {
  let component: RedeemCeilingComponent;
  let fixture: ComponentFixture<RedeemCeilingComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RedeemCeilingComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RedeemCeilingComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
