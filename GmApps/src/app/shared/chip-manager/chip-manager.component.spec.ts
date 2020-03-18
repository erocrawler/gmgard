import { async, ComponentFixture, TestBed } from "@angular/core/testing";

import { ChipManagerComponent } from "./chip-manager.component";

describe("ChipManagerComponent", () => {
  let component: ChipManagerComponent;
  let fixture: ComponentFixture<ChipManagerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ChipManagerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ChipManagerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should be created", () => {
    expect(component).toBeTruthy();
  });
});
