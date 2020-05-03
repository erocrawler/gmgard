import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { SpinConfirmComponent } from './spin-confirm.component';

describe('SpinConfirmComponent', () => {
  let component: SpinConfirmComponent;
  let fixture: ComponentFixture<SpinConfirmComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ SpinConfirmComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SpinConfirmComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
