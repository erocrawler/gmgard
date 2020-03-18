import { async, ComponentFixture, TestBed } from "@angular/core/testing";

import { BountyListComponent } from "./bounty-list.component";

describe("BountyListComponent", () => {
  let component: BountyListComponent;
  let fixture: ComponentFixture<BountyListComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ BountyListComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(BountyListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
