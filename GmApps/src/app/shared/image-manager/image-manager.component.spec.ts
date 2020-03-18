import { async, ComponentFixture, TestBed } from "@angular/core/testing";

import { ImageManagerComponent } from "./image-manager.component";

describe("ImageManagerComponent", () => {
  let component: ImageManagerComponent;
  let fixture: ComponentFixture<ImageManagerComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ImageManagerComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ImageManagerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should be created", () => {
    expect(component).toBeTruthy();
  });
});
