import { IGachaAnimation } from "./gacha-animation";

const IMAGE_WIDTH4 = [23424, 23040, 23424, 23040];
const IMAGE_WIDTH = [11904, 11520, 11520, 11520, 11904, 11520, 11520, 11520];
const IMAGE_HEIGHT = 216;
const START_FRAMES = [0, 39];
const GATCHA_FRAMES = [39, 241];
const CARD_KEY_FRAME_MAP = {
    118: "start",
    228: "away",
};
const NUM_FRAMES4 = [61, 121, 182, 242];
const NUM_FRAMES = [31, 61, 91, 121, 152, 182, 212, 242];

export class GachaImageAnimation implements IGachaAnimation {
    constructor(
        private canvas: HTMLCanvasElement,
        private animationImages: HTMLImageElement[],
    ) {
        this.context = canvas.getContext("2d");
    }

    static getImages() {
        return IMAGE_WIDTH.map(w => new Image(w, IMAGE_HEIGHT));
    }

    static getSrcs() {
        return Array.from(Array(IMAGE_WIDTH.length).keys()).map(i => `//static.gmgard.com/Images/gacha/gacha-sprite-0${i}.jpg`);
    }

    context: CanvasRenderingContext2D;
    frameIndex = 0;
    tickCount = 0;
    ticksPerFrame = 3;

    frameEnd = 0;
    currentAnimation = 0;
    currentPromiseResolve: (value?: boolean) => void;
    currentCardClass = "";

    private card: HTMLElement;

    adjustSize() {
        this.canvas.width = this.canvas.getBoundingClientRect().width;
        this.canvas.height = this.canvas.width / 16 * 9;
    }

    render() {
        this.adjustSize();
        let image: HTMLImageElement, numFrames = 0, actualIndex = 0;
        for (let i = 0; i < NUM_FRAMES.length; ++i) {
            if (this.frameIndex < NUM_FRAMES[i]) {
                image = this.animationImages[i];
                numFrames = NUM_FRAMES[i] - actualIndex;
                actualIndex = this.frameIndex - actualIndex;
                break;
            }
            actualIndex = NUM_FRAMES[i];
        }
        this.context.drawImage(
            image,
            actualIndex * image.width / numFrames,
            0,
            image.width / numFrames,
            image.height,
            0,
            0,
            this.canvas.width,
            this.canvas.height
        );
        this.update();
        if (CARD_KEY_FRAME_MAP[this.frameIndex] && this.card) {
            if (this.currentCardClass) {
                this.card.classList.remove(this.currentCardClass);
            }
            this.currentCardClass = CARD_KEY_FRAME_MAP[this.frameIndex];
            this.card.classList.add(this.currentCardClass);
        }
        if (this.frameIndex > this.frameEnd) {
            this.currentPromiseResolve(true);
        } else {
            this.currentAnimation = window.requestAnimationFrame(this.render.bind(this));
        }
    }

    playStart(): Promise<boolean> {
        if (this.currentAnimation) {
            window.cancelAnimationFrame(this.currentAnimation);
        }
        this.frameIndex = START_FRAMES[0];
        this.frameEnd = START_FRAMES[1];
        this.currentAnimation = window.requestAnimationFrame(this.render.bind(this));
        return new Promise<boolean>((resolve, reject) => {
            this.currentPromiseResolve = resolve;
        });
    }

    playGacha(card: HTMLElement): Promise<boolean> {
        if (this.currentAnimation) {
            window.cancelAnimationFrame(this.currentAnimation);
        }
        this.card = card;
        this.currentCardClass = "";
        this.frameIndex = GATCHA_FRAMES[0];
        this.frameEnd = GATCHA_FRAMES[1];
        this.currentAnimation = window.requestAnimationFrame(this.render.bind(this));
        return new Promise<boolean>((resolve, reject) => {
            this.currentPromiseResolve = resolve;
        });
    }

    update() {
        this.tickCount += 1;

        if (this.tickCount > this.ticksPerFrame) {
            this.tickCount = 0;
            // Go to the next frame
            this.frameIndex += 1;
        }
    }

    stop() {
        if (this.currentAnimation) {
            window.cancelAnimationFrame(this.currentAnimation);
        }
    }
}
