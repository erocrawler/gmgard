import { TestBed } from '@angular/core/testing';

import { PunchInService } from './punch-in.service';

describe('PunchInService', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  it('should be created', () => {
    const service: PunchInService = TestBed.get(PunchInService);
    expect(service).toBeTruthy();
  });
});
