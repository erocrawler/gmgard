function gmDetail(gmOptions) {
    if (this === window) { return new gmDetail(gmOptions); }
    var gm = this;
    var editor, ebutton, _content;

    function replaceeditor() {
        if (replyeditor) {
            _content = '';
            replyeditor.setData('');
            return replyeditor;
        }
        return document.getElementById('addreplycontent') && CKEDITOR.replace('addreplycontent', {
            customConfig: '/ckeditor/replyconfig.js'
        });
    }
    gm.editReply = function(id) {
        if (editor) {
            editor.setData(_content);
            _content = '';
            editor.destroy();
        }
        if (ebutton) {
            ebutton.hide();
            ebutton = null;
        }
        editor = CKEDITOR.replace($("#postcontent" + id + ">.reply")[0], {
            customConfig: '/ckeditor/replyconfig.js',
            uiColor: null,
            resize_enabled: false
        });
        ebutton = $("#btnarea_" + id);
        ebutton.show();
        //etitle = document.getElementById("blogtitle");

        _content = editor.getData();
    }

    gm.reply = function (id, data) {
        if (gmOptions.nocomment) {
            return;
        }
        var cnt = function () {
            var area = $('#postcontent' + id).find('textarea')[0];
            editor = CKEDITOR.replace(area, {
                customConfig: '/ckeditor/replyconfig.js',
                uiColor: null,
                resize_enabled: false
            });
            if (ebutton) {
                ebutton.hide();
            }
            ebutton = $('#postcontent' + id).find('.addReply');
            ebutton.show();
            if (data)
                editor.setData(data);
        }
        if (editor) {
            editor.setData(_content, function () { editor.destroy(); cnt(); });
            _content = '';
        } else {
            cnt();
        }
    }
    gm.replyReply = function (rid, author) {
        if (gmOptions.nocomment) {
            return;
        }
      var pid = $('#reply' + rid).parent().attr('data-postid');
      gm.reply(pid, '回复 <span>' + $(document.createElement('div')).text(author).html() + '</span>：');
    }
    gm.replyPost = function(id) {
        if (!editor.checkDirty())
            return false;
        ebutton.find('button').prop('disabled', true);
        $.post(gmOptions.replyposturl, { postid: id, addreplycontent: editor.getData() }, partial(replySuccess, id));
        return false;
    }

    gm.editClick = function(id) {
        if (!editor.checkDirty())
            return false;

        $.post(gmOptions.editreplyurl, { PostId: id, content: editor.getData() }, partial(editSuccess, id));
        return false;
    }

    gm.cancelClick = function() {
        if (_content)
            editor.setData(_content);
        _content = '';
        editor.destroy();
        editor = null;
        //etitle.innerHtml = _title;
        ebutton.hide().find('button').prop('disabled', false);
        return false;
    }

    gm.ratePost = function(id, val) {
        $.post(gmOptions.rateposturl, { postid: id, value: val }, function (result) {
            if (result && result.value) {
                $('#listpost' + id).find('.post-rating').text(result.value);
            }
        }).error(function () {
            $.globalMessenger().post('评分失败，请刷新重试。');
        })
    }

    function editSuccess(id, response) {
        if (response.errmsg) {
            $("#editerror_" + id).html(response.errmsg);
        }
        else {
            editor.destroy();
            editor = null;
            ebutton.hide();
        }
    }
    function replySuccess(id, response) {
        if (response.errmsg) {
            alert(response.errmsg);
            ebutton.find('button').prop('disabled', false);
        }
        else if (response.id) {
            window.location.hash = ('#reply' + response.id)
            var ul = $('#postcontent' + id).find('.postreply ul');
            if (ul.length == 0) {
                ul = $('<ul data-postid="' + id + '"></ul>');
                $('#postcontent' + id).find('.postreply').append(ul);
            }
            ul.append(response.view);
            $(location.hash).css("background-color", "lightGrey").delay(3000).queue(function () {
                $(this).addClass('trans').css("background-color", "inherit");
            });
            ul.scrollTop(ul[0].scrollHeight);
            gm.cancelClick();
            if (response.expmsg) {
                $.globalMessenger().post(response.expmsg);
            }
            //window.location.reload();
        }
    }

    function hidepost(id) {
        var e = document.getElementById("listpost" + id);
        e.style.display = 'none';
    }

    function hidereply(id) {
        var e = document.getElementById("reply" + id);
        var p = e.parentNode
        p.removeChild(e);
        if (!$.trim($(p).html())) {
            $(p).addClass("empty");
        }
    }

    function updaterate(rating) {
        var ratingText = rating.Total + "分（平均" + Math.round(rating.Average * 100) / 100 + "），（共" + rating.Count + "次评分）";
        $("#currentrating").text(ratingText);
    }

    function ratefail(error) {
        var e = document.getElementById("ratingmsg");
        e.innerText = error.get_message();
        e.style.display = null;
        e.className = "ratefail";
        rated = true;
    }

    function ratesuccess(value, result) {
        if (!result)
            result = value;
        var e = document.getElementById("ratingmsg");
        if (result.errmsg) {
            e.textContent = result.errmsg;
            $(e).show();
            e.className = "ratefail";
        }
        else {
            $('#ratecheck').trigger('click', { value: value });
            e.textContent = "评分成功！";
            if (result.msg) {
                $.globalMessenger().post(result.msg);
                if (chuncai)
                    chuncai.chuncaiSay("每天评分，造福社会！");
            }
            $(e).show();
            e.className = "ratesuccess";
            updaterate(result.rating);
        }
    }
    
    gm.rateClick = function(value) {
        $('.rating').addClass('rated').find('a').removeAttr('onclick');
        var blogid = gmOptions.blogid;
        $.post(gmOptions.rateurl, { blogid: blogid, rating: value }, partial(ratesuccess, value));
    }
    gm.gotoreply = function() {
        $('html, body').animate({ scrollTop: $('.replyeditor').offset().top }, 'slow');
        if (document.getElementById("ratecheck").getAttribute("data-check") != "checked") { $('#ratecheck').trigger('click'); }
    }

    //--- The following scripts must have gmOptions object initialized ---//
    var replyeditor;
    var flamecanvas;
    var flame;
    var spoiler = $('.spoiler');
    if (spoiler.length > 0) {
        var l = document.createElement('link');
        l.href = '/ckeditor/plugins/spoiler/css/spoiler.css';
        l.rel = 'stylesheet';
        $('head').append(l);
        $('div.spoiler-title').click(function () {
            $(this)
                .toggleClass('show-icon')
                .toggleClass('hide-icon');
            var l = $(this).next();
            l.hasClass('spoiler-content') && l.toggle();
        }).trigger('click');
    }

    $('#blogcontent .label, #dllist .label').hover(function () {
        selectText(this);
    });

    replyeditor = replaceeditor();
    var isreply = false;
    if (location.hash && (location.hash.indexOf("listpost") != -1 || (isreply = location.hash.indexOf("reply") != -1))) {
        var id = location.hash.replace(/^\D+/g, '');
        if (id && $(location.hash).length < 1) {
            $.post(isreply ? gmOptions.findreplyurl : gmOptions.findposturl, { id: id }, function (result) {
                if (result) {
                    $('#replydiv').html(result);
                    $(function () {
                        $('html, body').animate({ scrollTop: $(location.hash).offset().top - 100 }, 'slow');
                    });
                } else {
                    $('#replydiv').addClass('lazy-content');
                    $(window).on('scroll.lazy', handleLazyScroll);
                }
            });
            $('#replydiv').removeClass('lazy-content');
        }
    }
    $(".postreply>ul").each(function (i, e) { $(e).scrollTop(e.scrollHeight); });

    $('.favbtn').click(function () {
        var favbtn = $(this);
        if (favbtn.attr('title') == '加入收藏') { //未添加
            favbtn.attr('title', '已收藏');
            favbtn.addClass('btn-inverse');
            favbtn.html('<i class="icon-star icon-white"></i>');
            $.post(gmOptions.addfavurl);
            if (gmOptions.showflame == 'True') {
                showflame(this);
            }
        }
        else {
            favbtn.attr('title', '加入收藏');
            favbtn.removeClass('btn-inverse');
            favbtn.html('<i class="icon-star-empty"></i>');
            $.post(gmOptions.removefavurl);
            if (flamecanvas)
                flame.run = false;
        }
    });

    function showflame(elem) {
        if (!window.HTMLCanvasElement) {
            return;
        }
        if (flamecanvas) {
            flame.run = true;
            return;
        }
        var pos = $(elem).position();
        flamecanvas = document.createElement('canvas');
        flamecanvas.id = 'flamecanvas';
        $(flamecanvas).css({ top: (pos.top - 158), left: (pos.left - 151) }).insertBefore(elem);
        var startf = function () {
            flame = new FLAME(flamecanvas.id, 200, 200);
            flame.start();
        }
        window.FLAME ? startf() : $.getScript('/Scripts/canvas/min.js', startf);
    }
    var btnclicked = false;
    gm.submitReply = function () {
        var texts = replyeditor.getData();
        $('#addreplycontent').val(texts);
        var data = $('#AddReplyForm').serialize();
        if (btnclicked || texts == '')
            return false;
        btnclicked = true;
        $('#SubmitReply').button("loading");
        $("#loading").show();
        $('#addreplyerr').hide();
        $.post($('#AddReplyForm').attr('action'), data,
            function (result) {
                if (result.id) {
                    if ($('#ratecheck').attr('data-check') === 'checked') {
                        $('#score').prop('disabled', true).hide();
                        $('#hidden-rating').remove();
                        $('#ratecheck').hide().attr('data-check', 'unchecked');
                    }
                    replyeditor = replaceeditor();

                    if (!window.location.search) {
                        if (gmOptions.showreplyurl) {
                            $.post(gmOptions.showreplyurl, { pagenum: 1, hottest: false }, function (r) {
                                if (result.expmsg) {
                                    $.globalMessenger().post(result.expmsg);
                                }
                                r && $('html, body').animate({ scrollTop: $('#replydiv').html(r).offset().top - 100 }, 'slow');
                            });
                        }
                        else {
                            window.location.href = window.location.pathname + "#listpost" + result.id;
                            window.location.reload();
                        }
                    }
                    window.location.href = window.location.pathname + "#listpost" + result.id;
                }
                else if (result.err) {
                    $('#addreplyerr').html(result.err).show();
                }
            }).complete(function () {
                btnclicked = false;
                $('#SubmitReply').button("reset");
                $("#loading").hide();
            });
    }


    gm.rptReply = function(id) {
        $("#rptpostid").val(id);
        $("#pagenum").val($("#curpagenum").val());
    }
    gm.rptBlog = function() {
        $("#rptpostid").val(null);
    }

    gm.delReply = function(id) {
        if (confirm("确认删除？")) {
            $.post(gmOptions.deleteposturl, { PostId: id }, partial(hidepost, id));
        }
    }
    gm.delPostReply = function(id) {
        if (confirm("确认删除？")) {
            $.post(gmOptions.deletereplyurl, { ReplyId: id }, partial(hidereply, id));
        }
    }
    gm.redir = function() {
        window.location = gmOptions.listblogurl;
    }

    gm.editTag = function(elem) {
        $(elem).prop('disabled', true);
        $.get(gmOptions.edittagurl, { id: gmOptions.blogid }, function (result) {
            $('#tag-div').hide();
            $('#edit-tag').html(result);
        }).always(function () {
            $(elem).prop('disabled', false);
        });
    }
    return gm;
}