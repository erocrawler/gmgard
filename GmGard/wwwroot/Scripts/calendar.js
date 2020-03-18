$(function () {
    var $caldata = $('#calender-data');
    if ($caldata.length == 0) { return; }
    var curMonthData = JSON.parse($caldata.val()),
        today = new Date(), cTemplate = $('#calendar-template').html(),
        pTemplate = $('#popup-template').html(),
        parser = document.createElement('a'),
        curHost = extractHost($('#Logo').attr('src'));
    Mustache.parse(cTemplate); Mustache.parse(pTemplate);
    function createModel(obj) {
        obj.formatDate = makeFormatDate;
        return obj;
    }
    function extractHost(url) {
        parser.href = url;
        return parser.host;
    }
    function overrideHosts(arr) {
        for (var i = 0; i < arr.length; ++i) {
            arr[i] = overrideHost(arr[i]);
        }
        return arr;
    }
    function overrideHost(e) {
        parser.href = e.BlogThumb;
        if (parser.host != window.location.host && parser.host.indexOf('gmgard.us') > 0) {
            parser.host = curHost;
            e.BlogThumb = parser.href;
        }
        return e;
    }
    function parseDate(dateStr) {
        return new Date(dateStr);
    }
    function formatDate(date) {
        return date.getFullYear() + "年" +
            (date.getMonth() + 1) + "月" +
            date.getDate() + "日" +
            ("00" + date.getHours()).slice(-2) + ':' + ("00" + date.getMinutes()).slice(-2) + ':' + ("00" + date.getSeconds()).slice(-2);
    }
    function makeFormatDate() { return function (text, render) { return formatDate(parseDate(render(text))); } }
    function sortRating(a,b) { return b.Rating - a.Rating; }
    function rankToDict(rankings) {
        var dict = {};
        for (var i = 0; i < rankings.length; i++) {
            var rank = rankings[i],
                rankDate = parseDate(rank.RankDate),
                date = rankDate.getDate();
            if (dict[date]) {
                dict[date].push(rank);
            } else {
                dict[date] = [rank];
            }
        }
        return dict;
    }
    function parseData(currentDate, monthlyData) {
        var data = { caldata: {}, dailyDataDict: {}, weeklyDataDict: {}, monthlyData: [] };
        if (monthlyData) {
            if (monthlyData.DailyRankings) {
                data.dailyDataDict = rankToDict(overrideHosts(monthlyData.DailyRankings));
            }
            if (monthlyData.WeeklyRankings) {
                data.weeklyDataDict = rankToDict(overrideHosts(monthlyData.WeeklyRankings));
            }
            if (monthlyData.MonthlyRankings) {
                data.monthlyData = overrideHosts(monthlyData.MonthlyRankings);
            }
        }
        $.each(data.dailyDataDict, function (date, dateRankings) {
            var k = ('0' + (1 + currentDate.getMonth())).slice(-2) + '-' + ('0' + date).slice(-2) + '-' + currentDate.getFullYear();
            data.caldata[k] = Mustache.render(cTemplate, dateRankings.sort(sortRating));
        });
        return data;
    }
    function getDaysInMonth(m, y) {
        return (m === 2) ? (!((y % 4) || (!(y % 100) && (y % 400))) ? 29 : 28) : 30 + ((m + (m >> 3)) & 1);
    }

    var parsedData = parseData(today, curMonthData),
        weeklyDataDict = parsedData.weeklyDataDict,
        dailyDataDict = parsedData.dailyDataDict,
        weeks = ['周日', '周一', '周二', '周三', '周四', '周五', '周六'],
        months = '一月 二月 三月 四月 五月 六月 七月 八月 九月 十月 十一月 十二月'.split(' '),
        $wrapper = $('#custom-inner'),
        $calendar = $('#calendar'),
        cal = $calendar.calendario({
            weeks: weeks,
            weekabbrs: weeks,
            months: months,
            monthabbrs: months,
            onDayClick: function ($el, $contentEl, dateProperties, event) {

                if ($contentEl.length > 0) {
                    var y = parseInt(dateProperties.year), m = parseInt(dateProperties.month), d = parseInt(dateProperties.day),
                        model = createModel({
                        year: y, month: m, rankName: d + '日的日榜', Blog: dailyDataDict[dateProperties.day],
                    });
                    if (d > 1) {
                        model.prev = 'clickDay(' + (d - 1) + ')';
                    }
                    if (y == today.getFullYear() && m == today.getMonth() + 1) {
                        if (d < today.getDate()) {
                            model.next = 'clickDay(' + (d + 1) + ')';
                        }
                    } else if (d < getDaysInMonth(m, y)) {
                        model.next = 'clickDay(' + (d + 1) + ')';
                    }
                    showEvents(model, event);
                }

            },
            caldata: parsedData.caldata,
            displayWeekAbbr: true
        }),
        $month = $('#custom-month').html(cal.getMonthName()),
        $year = $('#custom-year').html(cal.getYear());

    function renderWeekly(weeklyData) {
        var weekCount = $calendar.find('.fc-row').length,
            rowClass = $calendar.find('.fc-calendar')[0].className.replace('fc-calendar', ''),
            $weeklyBody = $('.weekly-container').appendTo($calendar).find('.weekly-body').addClass(rowClass);
        for (var i = 0; i < weekCount; i++) {
            $weeklyBody.append('<div class="weekly-row fc-row"><div></div></div>');
        }
        var $weekRows = $weeklyBody.find('.weekly-row div');
        $.each(weeklyData, function (date, dateRankings) {
            var d = parseInt(date), i = parseInt((d-1) / 7);
            var model = createModel({
                year: cal.getYear(), month: cal.getMonth(), rankName: '第' + (i + 1) + '周的周榜', Blog: dateRankings,
            });
            if (i > 0) {
                model.prev = 'clickWeek(' + i + ')';
            }
            if (model.year == today.getFullYear() && model.month == today.getMonth() + 1) {
                if (d < today.getDate()) {
                    model.next = 'clickWeek(' + (i + 2) + ')';
                }
            } else if (d + 7 < getDaysInMonth(model.month, model.year)) {
                model.next = 'clickWeek(' + (i+2) + ')';
            }
            $weekRows.eq(i).empty().append(Mustache.render(cTemplate, dateRankings.sort(sortRating)))
                .before('<span class="weekly-weekday">第' + (i + 1) + '周</span>')
                .click(function (event) {
                    showEvents(model, event);
                });
        });
    }
    function renderMonthly(monthlyData) {
        $('.monthly-row').append(Mustache.render(cTemplate, monthlyData.sort(sortRating)))
              .click(function (event) {
                  showEvents(createModel({
                      year: cal.getYear(), month: cal.getMonth(), rankName: '的月榜', Blog: monthlyData,
                  }), event);
              });
    }
    renderWeekly(weeklyDataDict);
    renderMonthly(parsedData.monthlyData);

    $('#custom-next').on('click', function () {
        cal.gotoNextMonth(updateCalender);
    });
    $('#custom-prev').on('click', function () {
        cal.gotoPreviousMonth(updateCalender);
    });
    $('#custom-current').on('click', function () {
        cal.gotoNow(updateCalender);
    });

    function cacheKey(year, month) {
        return '~MonthRanking' + year + '/' + month;
    }

    function getCached(year, month) {
        if (year == today.getYear() && month == today.getMonth() + 1) {
            return null;
        }
        var val = sessionStorage[cacheKey(year, month)];
        if (val) {
            return JSON.parse(val);
        }
        return null;
    }

    var future;
    function updateCalender() {
        hideEvents();
        $month.html(cal.getMonthName());
        $year.html(cal.getYear());
        $('.monthly-row').empty().off('click');
        $('.weekly-body').empty().attr('class', 'weekly-body');
        $('#loading').show();
        if (future && !future.status) {
            future.abort();
        }
        var year = cal.getYear(), month = cal.getMonth(),
            monthData = getCached(year, month);
        if (monthData) {
            $('#loading').hide();
            return renderCalender(monthData);
        }
        future = $.post($calendar.data('update-url'), { year: year, month: month }, function (monthData) {
            sessionStorage[cacheKey(year, month)] = JSON.stringify(monthData);
            renderCalender(monthData);
        }).always(function () { $('#loading').hide() });
    }

    function renderCalender(monthData) {
        var parsed = parseData(new Date(cal.getYear(), cal.getMonth() - 1), monthData);
        dailyDataDict = parsed.dailyDataDict;
        cal.setData(parsed.caldata);
        renderWeekly(parsed.weeklyDataDict);
        renderMonthly(parsed.monthlyData);
    }

    function showEvents(model, event) {
        hideEvents();

        if (window.innerWidth <= 880) {
            if (event && event.target.href) {
                window.open(event.target.href);
            }
            return;
        }

        var $events = $(Mustache.render(pTemplate, model));
        $calendar.find('.fc-body, .weekly-body').addClass('blur');
        $events.find('.close').on('click', hideEvents).end().appendTo($wrapper.show());
    }
    function hideEvents() {
        $wrapper.hide();
        $calendar.find('.fc-body, .weekly-body').removeClass('blur');
        $('#custom-content-reveal').remove();
    }
});
function clickWeek(w) {
    $('.weekly-body .weekly-row').eq(w - 1).find('a:first').trigger('click');
}
function clickDay(d) {
    $('.fc-body .fc-content').eq(d - 1).find('a:first').trigger('click');
}