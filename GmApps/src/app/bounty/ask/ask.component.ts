import { Observable, Observer } from "rxjs";
import { Component, OnInit } from "@angular/core";

import { CkeditorResolverService } from "../ckeditor-resolver.service";
import { AuthService } from "app/auth/auth.service";

@Component({
  selector: "app-ask",
  templateUrl: "./ask.component.html",
  styleUrls: ["./ask.component.css"]
})
export class AskComponent implements OnInit {

    constructor(private ckeditorService: CkeditorResolverService, private authService: AuthService) { }

    get config(): CKEDITOR.config {
        return this.ckeditorService.config;
    };

    ckeditorContent: string;

    ngOnInit() {
        this.authService.getUser().subscribe(u => this.maxPoint = u.points);
    }

    readonly maxSize = 10 * 1024 * 1024;

    content: string;
    title: string;
    tags: string[] = [];
    images: File[] = [];
    points = 0;

    maxPoint: number;
    shouldDisableImage = false;

    publish() {
        console.log(this.tags);
        console.log(this.images);
    }

    updateImages(images: File[]) {
        this.images = images;
        this.shouldDisableImage = this.imageSize > this.maxSize;
    }

    get imageSize() {
        return this.images.reduce((prev, cur) => prev + cur.size, 0);
    }
}

