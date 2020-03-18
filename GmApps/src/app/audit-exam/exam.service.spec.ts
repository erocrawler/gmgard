import { TestBed, inject } from "@angular/core/testing";

import { ExamService } from "./exam.service";

describe("ExamService", () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [ExamService]
    });
  });

  it("should ...", inject([ExamService], (service: ExamService) => {
    expect(service).toBeTruthy();
  }));
});
