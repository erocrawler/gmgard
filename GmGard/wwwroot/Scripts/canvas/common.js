//set window.requestAnimationFrame
(function (w, r) {
    w['r' + r] = w['r' + r] || w['webkitR' + r] || w['mozR' + r] || w['msR' + r] || w['oR' + r] || function (c) { w.setTimeout(c, 1000 / 60); };
})(window, 'equestAnimationFrame');
