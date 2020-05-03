import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PrizeConfirmComponent } from './prize-confirm.component';

describe('PrizeConfirmComponent', () => {
  let component: PrizeConfirmComponent;
  let fixture: ComponentFixture<PrizeConfirmComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PrizeConfirmComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PrizeConfirmComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
