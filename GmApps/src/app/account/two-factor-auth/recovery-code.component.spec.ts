import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RecoveryCodeComponent } from './recovery-code.component';

describe('RecoveryCodeComponent', () => {
  let component: RecoveryCodeComponent;
  let fixture: ComponentFixture<RecoveryCodeComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RecoveryCodeComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RecoveryCodeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
