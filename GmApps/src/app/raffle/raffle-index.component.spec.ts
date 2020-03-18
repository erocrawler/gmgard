import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { RaffleIndexComponent } from './raffle-index.component';

describe('RaffleIndexComponent', () => {
  let component: RaffleIndexComponent;
  let fixture: ComponentFixture<RaffleIndexComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ RaffleIndexComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RaffleIndexComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
