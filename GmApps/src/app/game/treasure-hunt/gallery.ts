import { Observable, fromEvent, forkJoin, BehaviorSubject } from "rxjs";
import { map, first } from "rxjs/operators";

const GALLARY_SIZE = {
    width: 2448,
    height: 965
}

const IMAGE_POSITION = [
    {
        x: 390,
        y: 410
    },
    {
        x: 955,
        y: 410
    },
    {
        x: 1505,
        y: 410
    },
    {
        x: 2058,
        y: 410
    },
]

export class Gallery {
    constructor(
        private canvas: HTMLCanvasElement,
        private images: HTMLImageElement[],
    ) {
        this.context = canvas.getContext("2d");
        this.background = new Image();
        this.background.src = "/assets/puzzle/gallery.jpg";
        let events: Observable<Event>[] = [];
        for (let img of images) {
            events.push(fromEvent(img, "load").pipe(first()));
        }
        events.push(fromEvent(this.background, "load").pipe(first()));
        let obs = forkJoin(...events).pipe(
            map(es => true),
        );
        obs.subscribe(_ => {
            this.loaded_.next(true);
        });
        this.canvas.addEventListener('click', this.listener);
    }

    unload() {
        this.canvas.removeEventListener('click', this.listener);
    }

    private loaded_: BehaviorSubject<boolean> = new BehaviorSubject(false);
    private context: CanvasRenderingContext2D;
    private background: HTMLImageElement;
    private ratio = 1;
    private listener = (ev: MouseEvent) => this.handleClick(ev);

    get loaded(): Observable<boolean> {
        return this.loaded_.asObservable();
    }

    adjustSize() {
        this.canvas.width = this.canvas.getBoundingClientRect().width;
        this.ratio = this.canvas.width / 2448;
        this.canvas.height = this.ratio * 965;
    }

    render() {
        this.adjustSize();
        this.context.drawImage(this.background, 0, 0, this.canvas.width, this.canvas.height);
        for (let i = 0; i < this.images.length; i++) {
            let img = this.images[i];
            let pos = IMAGE_POSITION[i];
            let imgW = Math.min(440, img.width),
                imgRatio = imgW / img.width,
                imgH = img.height * imgRatio;
            let x = pos.x - imgW / 2,
                y = pos.y - imgH / 2;
            this.context.drawImage(img, x * this.ratio, y * this.ratio, imgW * this.ratio, imgH * this.ratio);
        }
    }

    private handleClick(ev: MouseEvent) {
        this.render();
        let rect = this.canvas.getBoundingClientRect(), x = ev.clientX - rect.left, y = ev.clientY - rect.top;
        for (let i = 0; i < IMAGE_POSITION.length; i++) {
            let pos = IMAGE_POSITION[i];
            if (x > (pos.x - 220) * this.ratio && x < (pos.x + 220) * this.ratio
                && y > (pos.y - 220) * this.ratio && y < (pos.y + 220) * this.ratio) {
                this.images[i].click();
            }
        }
    }
}
