import { TestBed, inject } from "@angular/core/testing";

import { BlogSearchService } from "./blog-search.service";

describe("BlogSearchService", () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [BlogSearchService]
    });
  });

  it("should ...", inject([BlogSearchService], (service: BlogSearchService) => {
    expect(service).toBeTruthy();
  }));
});
