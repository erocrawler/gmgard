import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { GachaIntroComponent } from './gacha-intro.component';

describe('GachaIntroComponent', () => {
  let component: GachaIntroComponent;
  let fixture: ComponentFixture<GachaIntroComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ GachaIntroComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(GachaIntroComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
