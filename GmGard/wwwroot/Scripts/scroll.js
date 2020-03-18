(function () {
    function bgpos($e, x, y) {
        if (x && y) $e.css('background-position', x + ' ' + y);
        return $e.css('background-position').split(' ');
    }
    if (window.screen.availWidth > 1024) {
        var $h = $('html'),
            $b = $('body'),
            resizeTimer,
            wrapper = $('.totop-wrapper'),
            hoffset = 0,
            boffset = 0,
            _adjustbg = function (pos) {
                bgpos($h, bgpos($h)[0], (pos + hoffset) + 'px');
                bgpos($b, bgpos($b)[0], (pos + boffset) + 'px');
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
            hoffset = parseInt(bgpos($h.removeAttr('style'))[1], 10);
            boffset = parseInt(bgpos($b.removeAttr('style'))[1], 10);
            $(this).trigger('scroll');
        }).on('resize', function () {
            var $this = $(this);
            clearTimeout(resizeTimer);
            resizeTimer = setTimeout(function () {
                $b.removeAttr('style');
                $this.trigger('scroll');
            }, 250);
        }).trigger('initoffset');
    }
})()