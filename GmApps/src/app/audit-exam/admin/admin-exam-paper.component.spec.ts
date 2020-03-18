import { async, ComponentFixture, TestBed } from "@angular/core/testing";

import { AdminExamPaperComponent } from "./admin-exam-paper.component";

describe("AdminExamPaperComponent", () => {
  let component: AdminExamPaperComponent;
  let fixture: ComponentFixture<AdminExamPaperComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AdminExamPaperComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AdminExamPaperComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
