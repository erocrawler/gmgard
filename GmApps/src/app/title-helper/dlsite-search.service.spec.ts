import { TestBed, inject } from "@angular/core/testing";

import { DlsiteSearchService } from "./dlsite-search.service";

describe("DlsiteSearchService", () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [DlsiteSearchService]
    });
  });

  it("should ...", inject([DlsiteSearchService], (service: DlsiteSearchService) => {
    expect(service).toBeTruthy();
  }));
});
