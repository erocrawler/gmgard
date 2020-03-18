import { TestBed, inject } from "@angular/core/testing";

import { CkeditorResolverService } from "./ckeditor-resolver.service";

describe("CkeditorResolverService", () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [CkeditorResolverService]
    });
  });

  it("should be created", inject([CkeditorResolverService], (service: CkeditorResolverService) => {
    expect(service).toBeTruthy();
  }));
});
