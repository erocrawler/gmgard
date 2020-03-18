import { async, ComponentFixture, TestBed } from "@angular/core/testing";

import { GachaIndexComponent } from "./gacha-index.component";

describe("GachaIndexComponent", () => {
  let component: GachaIndexComponent;
  let fixture: ComponentFixture<GachaIndexComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ GachaIndexComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(GachaIndexComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should be created", () => {
    expect(component).toBeTruthy();
  });
});
