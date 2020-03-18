import { IGachaAnimation } from "./gacha-animation";

const START_RANGE = [0, 2];
const GATCHA_RANGE = [2, 12];
const CARD_START_TIME = 5.44;
const CARD_AWAY_TIME = 10.9;

export class GachaVideoAnimation implements IGachaAnimation {
    constructor(
        private canvas: HTMLCanvasElement,
        private video: HTMLVideoElement
    ) {
        this.context = canvas.getContext("2d");
        this.video.muted = true;
        this.video.playsInline = true;
        this.video.playbackRate = 0.7;
    }

    private card: HTMLElement;
    private context: CanvasRenderingContext2D;
    private timeStart: number;
    private timeEnd: number;
    private currentAnimation = 0;
    private currentPromiseResolve: (value?: boolean) => void;

    adjustSize() {
        this.canvas.width = this.canvas.getBoundingClientRect().width;
        this.canvas.height = this.canvas.width / 16 * 9;
    }

    render() {
        this.adjustSize();
        this.canvas.getContext("2d").drawImage(this.video, 0, 0, this.canvas.width, this.canvas.height);

        const ct = this.video.currentTime;
        if (this.card) {
            if (ct >= CARD_START_TIME && ct < CARD_AWAY_TIME && !this.card.classList.contains("start")) {
                this.card.classList.add("start");
            } else if (ct >= CARD_AWAY_TIME && !this.card.classList.contains("away")) {
                this.card.classList.remove("start");
                this.card.classList.add("away");
            }
        }

        if (ct >= this.timeEnd || this.video.paused) {
            this.currentPromiseResolve(true);
        } else {
            this.currentAnimation = window.requestAnimationFrame(this.render.bind(this));
        }
    }

    playStart(): Promise<boolean> {
        if (this.currentAnimation) {
            window.cancelAnimationFrame(this.currentAnimation);
        }
        this.timeStart = START_RANGE[0];
        this.timeEnd = START_RANGE[1];
        const listener = () => {
            this.video.removeEventListener("playing", listener);
            this.currentAnimation = window.requestAnimationFrame(this.render.bind(this));
        };
        this.video.addEventListener("playing", listener);
        this.video.currentTime = this.timeStart;
        this.video.play();
        return new Promise<boolean>((resolve, reject) => {
            this.currentPromiseResolve = resolve;
        });
    }

    playGacha(card: HTMLElement): Promise<boolean> {
        if (this.currentAnimation) {
            window.cancelAnimationFrame(this.currentAnimation);
        }
        this.card = card;
        this.timeStart = GATCHA_RANGE[0];
        this.timeEnd = GATCHA_RANGE[1];
        this.currentAnimation = window.requestAnimationFrame(this.render.bind(this));
        const listener = () => {
            this.video.removeEventListener("playing", listener);
            this.currentAnimation = window.requestAnimationFrame(this.render.bind(this));
        };
        this.video.addEventListener("playing", listener);
        this.video.currentTime = this.timeStart;
        this.video.play();
        return new Promise<boolean>((resolve, reject) => {
            this.currentPromiseResolve = resolve;
        });
    }

    stop() {
        this.video.pause();
        if (this.currentAnimation) {
            window.cancelAnimationFrame(this.currentAnimation);
        }
    }
}
