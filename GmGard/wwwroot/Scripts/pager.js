function Pager(user_option) {
    if (this == window) { return new Pager(user_option); }
    var pager = this;
    var option = {
        'pager': '.pager',
        'page_init': null,
        'init_now': true,
        'destination': '.main-content',
        'source': null,
        'onchange': $.noop,
    };
    $.extend(option, user_option);
    var jumppage = function (url) {
        window.location.href = url;
    };
    function updateURLParameter(url, param, paramVal) {
        var newAdditionalURL = "";
        var tempArray = url.split("?");
        var baseURL = tempArray[0];
        var additionalURL = tempArray[1];
        var temp = "";
        if (additionalURL) {
            tempArray = additionalURL.split("&");
            for (i = 0; i < tempArray.length; i++) {
                if (tempArray[i].split('=')[0] != param) {
                    newAdditionalURL += temp + tempArray[i];
                    temp = "&";
                }
            }
        }
        var rows_txt = temp + "" + param + "=" + paramVal;
        return baseURL + "?" + newAdditionalURL + rows_txt;
    }
    var originalState = $(option.destination)[0].outerHTML;
    window.onpopstate = function (event) {
        if (event.state) {
            updatepage(event.state);
        } else if ($(option.destination).html() !== originalState) {
            updatepage(originalState);
        }
    }
    pager.onchange = option.onchange;
    var updatepage;
    $(updatepage = function (view) {
        if (history.pushState) {
            if (typeof view === 'string') {
                if (option.source) {
                    view = $('<div>' + view + '</div>').find(option.source).html();
                }
                $(option.destination).html(view);
            }
            jumppage = function (href) {
                $.get(href, null, function (view) {
                    history.pushState(view, document.title, href);
                    updatepage(view);
                    $('html, body').animate({ scrollTop: $(option.destination).offset().top - 200 }, 'fast');
                });
                $(option.pager).append('<img src="//static.gmgard.com/Images/loading2.gif"></img>');
                $('.pager a').addClass('disabled').removeAttr('href').off('click');
                $('.pager input').prop('disabled', true);
            }
            $(option.pager + ' .btn:not(.disabled)').click(function (event) {
                event.preventDefault();
                jumppage(this.href);
            });
        }
        var jumppagebox = $(option.pager + ' #jumppage').change(function () {
            var pagenum = parseInt(this.value);
            var pagecount = $(this).data('pagecount');
            if (pagenum > 0 && pagenum <= pagecount) {
                var url = document.URL;
                //url = url.split('?')[0];
                jumppage(updateURLParameter(url, 'page', pagenum));
                pager.pagenum = pagenum;
            }
        });
        pager.pagecount = jumppagebox.data('pagecount');
        if (option.pager_init && option.init_now) option.pager_init();
        option.init_now = true;
        pager.onchange();
    });
    pager.jumppage = function (pagenum) {
        var jumppagebox = $(option.pager + ' #jumppage');
        if (jumppagebox.length) {
            jumppagebox.val(pagenum);
            jumppagebox.trigger('change');
        } else {
            jumppage(updateURLParameter(document.URL, 'page', pagenum));
        }
    }
    pager.refreshPage = function () {
        jumppage(document.URL);
    }
    Pager.instances = Pager.instances || [];
    Pager.instances.push(pager);
    return pager;
}