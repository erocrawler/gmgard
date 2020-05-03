interface WinWheel {
  new({ }): WinWheel;
  draw(clearTheCanvas: boolean): void;
  tween: gsap.core.Tween;
  animation: {
    type?: string;
    stopAngle?: number;
    duration?: number;
    spins?: number;
    yoyo?: boolean;
    repeat?: number;
    easing?: string;
    clearTheCanvas?: boolean;
    propertyName?: string;
    propertyValue?: number;
  }
  rotationAngle: number;
  canvas: HTMLCanvasElement;
  ctx: CanvasRenderingContext2D;
  computeAnimation(): void;
  getRandomForSegment(idx: number): number;
}
