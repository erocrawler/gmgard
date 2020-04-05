(function () {
  function bgpos($e, y) {
    var pcss = $e.css('background-position');
    var parts = pcss.split(',');
    var prefix = '';
    var pos = pcss;
    if (parts.length > 1) {
      pos = parts[parts.length - 1].trim();
      prefix = parts.slice(0, -1).join(',') + ',';
    }
    if (y) $e.css('background-position', prefix + pos.split(' ')[0] + ' ' + y);
    return pos.split(' ')[1];
  }
  if (window.screen.availWidth > 1024) {
    var $h = $('html'),
      $b = $('body'),
      resizeTimer,
      wrapper = $('.totop-wrapper'),
      hoffset = 0,
      boffset = 0,
      _adjustbg = function (pos) {
        bgpos($h, (pos + hoffset) + 'px');
        bgpos($b, (pos + boffset) + 'px');
      },
      adjustbg = $.noop;
    $(window).scroll(function () {
      var top = $(window).scrollTop();
      if (top > 400 && wrapper.hasClass('hidden')) {
        wrapper.removeClass('hidden');
        wrapper.addClass('active');
      }
      else if (top <= 400 && wrapper.hasClass('active')) {
        wrapper.addClass('hidden');
        wrapper.removeClass('active');
      }
      adjustbg(-top * 0.05);
    }).on('initoffset', function () {
      adjustbg = localStorage['parallax'] == 'off' ? $.noop : _adjustbg;
      hoffset = parseInt(bgpos($h.removeAttr('style')), 10);
      boffset = parseInt(bgpos($b.removeAttr('style')), 10);
      $(this).trigger('scroll');
    }).on('resize', function () {
      var $this = $(this);
      clearTimeout(resizeTimer);
      resizeTimer = setTimeout(function () {
        $b.removeAttr('style');
        $this.trigger('initoffset');
      }, 250);
    }).trigger('initoffset');
  }
})()