import { TestBed, inject } from "@angular/core/testing";

import { GachaService } from "./gacha.service";

describe("GachaService", () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [GachaService]
    });
  });

  it("should be created", inject([GachaService], (service: GachaService) => {
    expect(service).toBeTruthy();
  }));
});
