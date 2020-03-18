import { GameModule } from './game.module';

describe('GameModule', () => {
  let gameModule: GameModule;

  beforeEach(() => {
    gameModule = new GameModule();
  });

  it('should create an instance', () => {
    expect(gameModule).toBeTruthy();
  });
});
