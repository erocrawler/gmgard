import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PunchInComponent } from './punch-in.component';

describe('PunchInComponent', () => {
  let component: PunchInComponent;
  let fixture: ComponentFixture<PunchInComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PunchInComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PunchInComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
