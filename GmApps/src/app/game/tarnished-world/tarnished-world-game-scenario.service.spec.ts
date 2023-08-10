import { TestBed } from '@angular/core/testing';

import { TarnishedWorldGameScenarioService } from './tarnished-world-game-scenario.service';

describe('TarnishedWorldGameScenarioService', () => {
  let service: TarnishedWorldGameScenarioService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TarnishedWorldGameScenarioService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
