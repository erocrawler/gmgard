(function () {
  let applyGrey = function (el) {
    let s = this.getComputedStyle(el);
    let oriBG = s.backgroundImage;
    if (oriBG === "none") return;
    let oriPos = s.backgroundPosition;
    el.setAttribute('style',
      "background-image: radial-gradient(circle, transparent 50%, black 100%),linear-gradient(black,black)," + oriBG +
      "; background-blend-mode: normal, saturation; background-position: 0,0," + oriPos);
  }
  $(window).on('initoffset', function () {
    applyGrey(document.querySelector('html'));
    applyGrey(document.querySelector('body'));
  }).trigger('initoffset');
})()