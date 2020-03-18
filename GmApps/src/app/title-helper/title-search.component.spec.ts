import { async, ComponentFixture, TestBed } from "@angular/core/testing";

import { TitleSearchComponent } from "./title-search.component";

describe("TitleSearchComponent", () => {
  let component: TitleSearchComponent;
  let fixture: ComponentFixture<TitleSearchComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ TitleSearchComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TitleSearchComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
