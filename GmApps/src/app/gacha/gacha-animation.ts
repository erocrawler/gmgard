export interface IGachaAnimation {
    playStart(): Promise<boolean>
    playGacha(card: HTMLElement): Promise<boolean>
    stop(): void
}
