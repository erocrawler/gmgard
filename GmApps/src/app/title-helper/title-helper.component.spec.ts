import { async, ComponentFixture, TestBed } from "@angular/core/testing";

import { TitleHelperComponent } from "./title-helper.component";

describe("TitleHelperComponent", () => {
  let component: TitleHelperComponent;
  let fixture: ComponentFixture<TitleHelperComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TitleHelperComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TitleHelperComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
