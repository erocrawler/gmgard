String.prototype.ReplaceAll = function (stringToFind, stringToReplace) {
    var temp = this;
    var index = temp.indexOf(stringToFind);
    while (index !== -1) {
        temp = temp.replace(stringToFind, stringToReplace);
        index = temp.indexOf(stringToFind);
    }
    return temp;
};

function partial(func /*, 0..n args */) {
    var args = Array.prototype.slice.call(arguments, 1);
    return function () {
        var allArguments = args.concat(Array.prototype.slice.call(arguments));
        return func.apply(this, allArguments);
    };
}

function selectText(element) {
    var doc = document
        , text = element
        , range, selection
    ;
    if (doc.body.createTextRange) { //ms
        range = doc.body.createTextRange();
        range.moveToElementText(text);
        range.select();
    } else if (window.getSelection) { //all others
        selection = window.getSelection();
        range = doc.createRange();
        range.selectNodeContents(text);
        selection.removeAllRanges();
        selection.addRange(range);
    }
}

function setCookie(c_name, value, ex) {
    if (isNaN(ex))
        ex = 31536000000;
    if (document.cookie.indexOf(c_name + '=') >= 0) {
        var zero = new Date(0);
        document.cookie = c_name + '=;path=/; expires=' + zero.toUTCString();
    }
    var d = new Date();
    d.setTime(d.getTime() + ex);
    document.cookie = c_name + '=' + value + ';path=/; expires=' + d.toUTCString();
}
function rmCookie(c_name) {
    var zero = new Date(0);
    document.cookie = c_name + '=;path=/; expires=' + zero.toUTCString();
}
function getCookie(c_name) {
    var cookie = document.cookie;
    var spos = cookie.indexOf(c_name + '=');
    if (spos < 0)
        return null;
    else {
        spos = cookie.indexOf('=', spos) + 1;
        var epos = cookie.indexOf(";", spos);
        if (epos < 0)
            epos = cookie.length;
        return cookie.substring(spos, epos);
    }
}

function handleLazyScroll() {
    var $lazy = $('.lazy-content'), $w = $(window);
    if ($lazy.length > 0) {
        var wt = $w.scrollTop(), wb = wt + $w.height(), th = 200;
        $lazy.each(function () {
            var $this = $(this), et = $this.offset().top, eb = et + $this.height();
            if (eb >= wt - th && et <= wb + th) {
                $this.removeClass('lazy-content').addClass('lazy-content-loading');
                $.get($this.data('url'), null, function (r) {
                    if ($this.children().length < 1) {
                        $this.removeClass('lazy-content-loading').html(r);
                    }
                }).fail(function (r) { $this.removeClass('lazy-content-loading') });
            }
        });
    } else {
        $w.off('scroll.lazy');
    }
}
if ($('.lazy-content').length > 0) {
    $(window).on('scroll.lazy', handleLazyScroll);
    $(document).ready(handleLazyScroll);
}