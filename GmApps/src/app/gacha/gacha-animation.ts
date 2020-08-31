export interface IGachaAnimation {
  play(cards: IGachaCard[]): Promise<boolean>
  stop(): void
}

export interface IGachaCard {
  setState(state: string): void
  getState(): string
}
