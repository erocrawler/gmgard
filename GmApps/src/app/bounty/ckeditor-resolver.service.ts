import { Injectable, Inject } from "@angular/core";
import {
    Router, Resolve, RouterStateSnapshot,
    ActivatedRouteSnapshot
} from "@angular/router";

import { DOCUMENT } from "@angular/platform-browser";

import { ENVIRONMENT } from "../../environments/environment_token";

@Injectable()
export class CkeditorResolverService implements Resolve<boolean> {

    constructor(@Inject(DOCUMENT) private document: any, @Inject(ENVIRONMENT) private env: any) { }

    resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Promise<boolean> {
        if (window["CKEDITOR"]) {
            return Promise.resolve(true);
        }
        const promise = new Promise<boolean>((resolve, reject) => {
            const script = this.document.createElement("script");
            script.type = "text/javascript";
            script.src = this.env.apiHost + "/ckeditor/ckeditor.js";
            script.onload = () => {
                resolve(true);
            };
            script.onerror = (error: any) => reject(error);
            this.document.head.appendChild(script);
        });
        return promise;
    }

    get config(): CKEDITOR.config {
        const config: CKEDITOR.config = {};
        config.extraAllowedContent = "a[rel]; span(*); embed[*]; table(*); ruby; iframe{*}";
        config.extraPlugins = "custom_smiley,colorbutton,font,mediaembed,flash,rubymarkup,spoiler";
        config.removeButtons = "Underline,Subscript,Superscript";
        config.format_tags = "p;h1;h2;h3;h4;h5;pre";
        config.removeDialogTabs = "image:advanced;link:advanced";
        config.removePlugins = "elementspath";
        config.customConfig = this.env.apiHost + "/ckeditor/smiley_config.js";
        config.skin = "moono-lisa";
        config.toolbar = [
            ["Undo", "Redo"],
            ["Link", "Unlink", "Anchor"],
            ["MediaEmbed", "Flash", "Image", "Custom_Smiley"],
            ["Table", "HorizontalRule", "Spoiler", "Mentions", "RubyMarkup"],
            ["Maximize"],
            ["Source"],
            ["TextColor", "BGColor"],
            "/",
            ["Bold", "Italic", "Strike", "-", "RemoveFormat"],
            ["NumberedList", "BulletedList", "-", "Outdent", "Indent", "-", "Blockquote"],
            ["Styles", "Format", "FontSize"]
        ];
        return config;
    };
}
