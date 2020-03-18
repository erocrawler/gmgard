import { TestBed, inject } from "@angular/core/testing";

import { BountyService } from "./bounty.service";

describe("BountyService", () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [BountyService]
    });
  });

  it("should ...", inject([BountyService], (service: BountyService) => {
    expect(service).toBeTruthy();
  }));
});
