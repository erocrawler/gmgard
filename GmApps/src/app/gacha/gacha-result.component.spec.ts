import { async, ComponentFixture, TestBed } from "@angular/core/testing";

import { GachaResultComponent } from "./gacha-result.component";

describe("GachaResultComponent", () => {
  let component: GachaResultComponent;
  let fixture: ComponentFixture<GachaResultComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ GachaResultComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(GachaResultComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should be created", () => {
    expect(component).toBeTruthy();
  });
});
