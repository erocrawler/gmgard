import { Injectable } from '@angular/core';

interface Scripts {
  name: string
  src: string
}

export interface ScriptState {
  loaded: boolean
  src: string
}

const ScriptStore: Scripts[] = [
  { name: 'qrcode', src: '/assets/js/qrcode.min.js' }
]

@Injectable({
  providedIn: 'root'
})
export class ScriptLoaderService {

  private scripts = new Map<string, ScriptState>();

  constructor() {
    ScriptStore.forEach((script: Scripts) => {
      this.scripts.set(script.name, {
        loaded: false,
        src: script.src
      });
    });
  }

  load(...scripts: string[]) {
    const promises: Promise<ScriptState>[] = [];
    scripts.forEach((script) => promises.push(this.loadScript(script)));
    return Promise.all(promises);
  }

  loadScript(name: string) {
    return new Promise<ScriptState>((resolve, reject) => {
      if (!this.scripts.has(name)) {
        reject(name + " not found");
        return;
      }
      let s = this.scripts.get(name);
      if (!s.loaded) {
        //load script
        let script = document.createElement('script');
        script.type = 'text/javascript';
        script.src = s.src;
        script.onload = () => {
            s.loaded = true;
            resolve(s);
        };
        script.onerror = (error) => reject(error);
        document.getElementsByTagName('head')[0].appendChild(script);
      } else {
        resolve(s);
      }
    });
  }

}
