import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PunchInDetailComponent } from './punch-in-detail.component';

describe('PunchInDetailComponent', () => {
  let component: PunchInDetailComponent;
  let fixture: ComponentFixture<PunchInDetailComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PunchInDetailComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PunchInDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
