// ES module for 2FA QR rendering – Blazor JS isolation, no global <script> in index.html
// Loads legacy qrcode-generator (Kazuhiko Arase) via script tag injection (it's not an ES module)

let qrLibPromise = null;

function getQrLibUrl() {
    try {
        // Resolve relative to this module file (Blazor serves from /app/js/enable2fa.js)
        return new URL('./qrcode.min.js', import.meta.url).toString();
    } catch {
        // Fallback to base href
        return './js/qrcode.min.js';
    }
}

function loadQrLib() {
    const g = (typeof globalThis.qrcode !== 'undefined') ? globalThis.qrcode : (typeof window !== 'undefined' && typeof window.qrcode !== 'undefined' ? window.qrcode : undefined);
    if (typeof qrcode !== 'undefined' || g) return Promise.resolve();
    if (qrLibPromise) return qrLibPromise;

    qrLibPromise = new Promise((resolve, reject) => {
        const existing = document.querySelector('script[data-qr-lib]');
        if (existing) {
            existing.addEventListener('load', resolve);
            existing.addEventListener('error', reject);
            return;
        }
        const s = document.createElement('script');
        s.src = getQrLibUrl();
        s.async = true;
        s.dataset.qrLib = 'true';
        s.onload = () => resolve();
        s.onerror = (e) => reject(e);
        document.head.appendChild(s);
    });
    return qrLibPromise;
}

export async function renderQrCode(canvas, text) {
    try {
        await loadQrLib();
        const qrFactory = (typeof qrcode !== 'undefined') ? qrcode : (typeof window.qrcode !== 'undefined' ? window.qrcode : null);
        if (!qrFactory) {
            console.error('qrcode lib not loaded after injection');
            return false;
        }
        const qr = qrFactory(0, 'M');
        qr.addData(text);
        qr.make();
        const ctx = canvas.getContext('2d');
        if (!ctx) return false;
        const count = qr.getModuleCount();
        const size = canvas.width;
        const cell = size / count;
        ctx.fillStyle = 'white';
        ctx.fillRect(0, 0, size, size);
        ctx.fillStyle = 'black';
        for (let r = 0; r < count; r++) {
            for (let c = 0; c < count; c++) {
                if (qr.isDark(r, c)) {
                    ctx.fillRect(c * cell, r * cell, cell, cell);
                }
            }
        }
        return true;
    } catch (e) {
        console.error('QR render error', e);
        return false;
    }
}

export function copyText(text) {
    if (navigator.clipboard) {
        return navigator.clipboard.writeText(text);
    }
    return Promise.reject('clipboard not available');
}

