import { async, ComponentFixture, TestBed } from "@angular/core/testing";

import { GachaListComponent } from "./gacha-list.component";

describe("GachaListComponent", () => {
  let component: GachaListComponent;
  let fixture: ComponentFixture<GachaListComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ GachaListComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(GachaListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should be created", () => {
    expect(component).toBeTruthy();
  });
});
