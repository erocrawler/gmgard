import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RedeemCouponComponent } from './redeem-coupon.component';

describe('RedeemCouponComponent', () => {
  let component: RedeemCouponComponent;
  let fixture: ComponentFixture<RedeemCouponComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RedeemCouponComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RedeemCouponComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
