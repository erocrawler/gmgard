import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AdvGameComponent } from './adv-game.component';

describe('AdvGameComponent', () => {
  let component: AdvGameComponent;
  let fixture: ComponentFixture<AdvGameComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AdvGameComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AdvGameComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
