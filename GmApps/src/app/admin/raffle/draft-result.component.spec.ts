import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DraftResultComponent } from './draft-result.component';

describe('DraftResultComponent', () => {
  let component: DraftResultComponent;
  let fixture: ComponentFixture<DraftResultComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DraftResultComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DraftResultComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
