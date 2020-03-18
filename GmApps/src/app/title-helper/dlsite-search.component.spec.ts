import { async, ComponentFixture, TestBed } from "@angular/core/testing";

import { DlsiteSearchComponent } from "./dlsite-search.component";

describe("DlsiteSearchComponent", () => {
  let component: DlsiteSearchComponent;
  let fixture: ComponentFixture<DlsiteSearchComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ DlsiteSearchComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(DlsiteSearchComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
