import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PuzzleDetailComponent } from './puzzle-detail.component';

describe('PuzzleDetailComponent', () => {
  let component: PuzzleDetailComponent;
  let fixture: ComponentFixture<PuzzleDetailComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PuzzleDetailComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PuzzleDetailComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
