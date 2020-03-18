(function ($) {
    var followFeeds = $('#follow-feeds');
    if (followFeeds.length == 0) return;
    var main = $('#body'),
        side = $('#sidebar'),
        mainContent = main.find('.main-content'),
        originalwidth = 380,
        mainwidth = main.width() - side.width(),
        feedSection = main.find('.feeds'),
        columns = $([]),
        datacol = main.find('.column').remove(),
        items = datacol.find('li'),
        count = 0;
    for (var i = 0; i < 3; i++) {
        columns = columns.add(document.createElement('ul'));
        columns.eq(i).addClass('column span5');
        //columns[i].data('id', i);
    }
    function populateColumns (count) {
        var currentcol = columns.detach().empty().slice(0, count);
        for (var i = 0; i < count; i++) {
            feedSection.append(columns[i]);
        }
        items.each(function (i, e) {
            currentcol.sort(function (a, b) {
                return a.clientHeight - b.clientHeight;
            }).eq(0).append(e);
        });
        (count > 1) ? mainContent.width(count * originalwidth).removeClass('full-width') : mainContent.css('width', '100%').addClass('full-width');
    }
    $(window).on('resize', function () {
        var currentCount = 1;
        mainwidth = main.width() - side.width();
        if (mainwidth >= (3 * originalwidth)) {
            currentCount = 3;
        }
        else if (mainwidth >= (2 * originalwidth)) {
            currentCount = 2;
        }
        if (count != currentCount) {
            populateColumns(count = currentCount);
        }
    }).trigger('resize');

    $(window).load(function () {
        populateColumns(count);
    });
    
    if (items.length < 10) {
        $.get('/Follow/Suggestions', null, function (view) {
            mainContent.append(view);
        });
    }
})(jQuery)