import { Observable, Observer, from } from "rxjs";
import { Component, OnInit, EventEmitter, Output, Input } from "@angular/core";
import { concatMap, toArray, tap } from "rxjs/operators";

@Component({
  selector: "image-manager",
  templateUrl: "./image-manager.component.html",
  styleUrls: ["./image-manager.component.css"]
})
export class ImageManagerComponent implements OnInit {

    constructor() { }

    ngOnInit() {
    }

    images: FileWithContent[] = [];

    @Input()
    disabled = false;

    @Output()
    update = new EventEmitter<File[]>();

    private loadFile(f: File): Observable<FileWithContent> {
        return Observable.create((observer: Observer<FileWithContent>) => {
            const reader = new FileReader();
            reader.onload = function (e) {
                observer.next({
                    content: reader.result,
                    file: f,
                });
                observer.complete();
            }
            reader.onerror = function (e) {
                observer.error(e);
            }
            reader.readAsDataURL(f);
        });
    }

    addFiles(fileElement: HTMLInputElement) {
        console.log("addFiles!")
        from(fileElement.files)
            .pipe(
                concatMap(this.loadFile),
                toArray(),
                tap(() => {
                    fileElement.value = "";
                })
            ).subscribe((files) => {
                this.images = this.images.concat(files);
                this.update.emit(this.images.map(f => f.file));
            });
    }

    removeFile(file: FileWithContent) {
        this.images = this.images.filter(i => i != file);
        this.update.emit(this.images.map(f => f.file));
    }
}


interface FileWithContent {
    file: File
    content: any
}
