(function ($) {
    window.chuncai = window.chuncai || {};
    window.chuncai.imagewidth = window.chuncai.imageheight = 0;
    window.chuncai.data = {};
    var timer = null;
    window.chuncai.loadChuncai = function() {
        if (chuncai) {
            talkself_arr = chuncai.talkself_arr;
            $.each(chuncai.ques, function (i) {
                $('#talk')
                    .append($("<option></option>")
                    .attr("value", chuncai.ques[i])
                    .text(chuncai.ques[i]));
            });
        }
        var width = localStorage["cc_width"];
        var height = localStorage["cc_height"];
        var cwidth = document.documentElement.clientWidth - 30;
        var cheight = document.documentElement.clientHeight - 100;
        if (width==null || height == null || width <=0 || width > cwidth || height<=0 || height>cheight){
            width = document.documentElement.clientWidth - 30 - chuncai.imagewidth;
            height = document.documentElement.clientHeight - 100 - chuncai.imageheight;
            if (parseInt(height) > 2000) {
                var height = -180 - chuncai.imageheight;
            }
        }

        //判断春菜是否处于隐藏状态
        var is_closechuncai = localStorage["cc_isclose"];
        if (is_closechuncai == 'close') {
            closechuncai_init();
        }
        else {
            $("#smchuncai").show();
        }
        //设置初始状态
        getdata("defaultccs");
    
        $("#smchuncai").css({ 'left': width + 'px', 'top': height + 'px', 'width': chuncai.imagewidth + 'px', 'height': chuncai.imageheight + 'px' })
            .mousedown(function (ent) {
                var moveX = ent.clientX,
                    moveY = ent.clientY,
                    obj = this,
                    curX = parseInt(obj.style.left),
                    curY = parseInt(obj.style.top);
                $(document).on('mousemove.chuncai', function (e) {
                    obj.style.left = (curX + e.clientX - moveX) + "px";
                    obj.style.top = (curY + e.clientY - moveY) + "px";
                }).one('mouseup', function () {
                    $(document).off('mousemove.chuncai');
                    window.localStorage.setItem("cc_width", parseInt(obj.style.left));
                    window.localStorage.setItem("cc_height", parseInt(obj.style.top));
                });
            }).mouseover(function() {
                if (talkobj) {
                    clearTimeout(talkobj);
                }
                talktime = 0;
                talkSelf(talktime);
            }).click(resetTalkSelf);

        $("#getmenu").click(function () {
            chuncaiMenu();
            setFace(1);
        });
        $("#shownotice").click(function () {
            getdata("getnotice");
            setFace(1);
        });
        $("#closechuncai").click(function () {
            setFace(3);
            closechuncai();
        });
        $("#callchuncai").click(function () {
            setFace(2);
            callchuncai();
            window.localStorage.setItem("cc_isclose", '');
        });
        $("#shownotice").click(function () {
            setFace(1);
            closeChuncaiMenu();
        });
        $("#lifetimechuncai").click(function () {
            closeChuncaiMenu();
            closeNotice();
            setFace(2);
            getdata('showlifetime');
        });
        $("#chatTochuncai").click(function () {
            showInput();
        });
        $("#inp_r").click(function () {
            closeInput();
            chuncaiSay('不聊天了吗？(→_→)');
            setFace(3);
        });
        $("#switchbackground").click(function () {
            switchbg();
            chuncaiSay('哪个更好看？');
            setFace(2);
        });
        $("#showranking").click(function () {
            jumpPage('/Rank', "马上就跳转到排行页面去啦～～～");
        });
        $("#talk").change(function () {
            getdata("talking");
        });
        $("#blogmanage").click(function () {
            jumpPage( '/Account/Manage',"马上就跳转到个人设置页面去啦～～～");
        });
        $("#randomgmchuncai").click(function () {
          jumpPage('/Blog/Random', '打开一个随机资源的传送门喽~');
        });
        $('#gachachuncai').click(function () {
          jumpPage('/Home/App?path=/gacha', '课金~抽卡~');
        });
        $("#foods").click(function () {
            closeChuncaiMenu();
            closeNotice();
            getdata("foods");
        });
        document.onmousemove = function () {
            stoptime();
            setTime();
        }
        talkSelf(talktime);
    }
    function jumpPage(href, saying) {
        closeChuncaiMenu();
        closeNotice();
        $("#getmenu").hide();
        chuncaiSay(saying);
        setFace(2);
        setTimeout(function () {
            window.location.href = href;
        }, 1500);
    }

    var eattimes = 0;
    window.chuncai.eatfood = function eatfood(obj){
        var gettimes = parseInt(window.sessionStorage['cc_eat'], 10);
        if(parseInt(gettimes) > 9){
            chuncaiSay("撑死我了……");
            setFace(3);
            closechuncai_evil();
        }else if(gettimes > 7){
            chuncaiSay("哎呦快要撑死了");
            setFace(3);
        }else if(gettimes == 5){
            chuncaiSay("我已经吃饱啦，不要再吃啦");
            setFace(3);
        }else if(gettimes == 3){
            chuncaiSay("多谢款待，我吃饱啦～");
            setFace(2);
        }else{
            var id = obj.replace("f",'');
            getdata('eatsay', id);
        }
        eattimes++;
        window.sessionStorage['cc_eat'] = eattimes;
    }
    function chuncaiMenu(){
        clearChuncaiSay();
        closeInput();
        chuncaiSay("准备做什么呢？");
        $("#showchuncaimenu").show();
        $("#getmenu").hide();
        $("#chuncaisaying").hide();
    }
    function closeChuncaiMenu(){
        clearChuncaiSay();
        $("#showchuncaimenu").hide();
        showNotice();
        $("#getmenu").show();
    }
    function showNotice(){
        $("#chuncaisaying").show();
    }
    function closechuncai(){
        stopTalkSelf();
        chuncaiSay("记得再叫我出来哦...");
        $("#showchuncaimenu").hide();
        setTimeout(function(){
            $("#smchuncai").fadeOut(1200);
            $("#callchuncai").show();}, 2000);
        localStorage['cc_isclose'] = 'close';
    }
    function closechuncai_evil(){
        stopTalkSelf();
        $("#showchuncaimenu").hide();
        setTimeout(function(){
            $("#smchuncai").fadeOut(1200);
            $("#callchuncai").hide();
        }, 2000);
    }
    function closechuncai_init(){
        stopTalkSelf();
        $("#showchuncaimenu").hide();
        setTimeout(function(){
            $("#smchuncai").hide();
            $("#callchuncai").show();}, 30);
    }
    function callchuncai(){
        talkSelf(talktime);
        $("#smchuncai").fadeIn('normal');
        $("#callchuncai").hide();
        closeChuncaiMenu();
        closeNotice();
        chuncaiSay("我回来啦～");
        localStorage['cc_isclose'] = '';
    }

    function chuncaiSay(s){
        clearChuncaiSay();
        $("#tempsaying").append(s).show();
        typeWords(s, 0);
    }
    window.chuncai.chuncaiSay = chuncaiSay;
    function clearChuncaiSay(){
        document.getElementById("tempsaying").innerHTML = '';
    }
    function closeNotice(){
        $("#chuncaisaying").hide();
    }
    function showInput(){
        closeChuncaiMenu();
        closeNotice();
        chuncaiSay("............?");
        $("#addinput").show().css('top', (chuncai.imageheight + 20) + 'px');
    }
    function closeInput(){
        setFace(3);
        $("#addinput").hide();
    }
    function clearInput(){
        document.getElementById("talk").value = '';
    }
    function setFace(num){
        var obj = chuncai.imgs[num];
        var img = new Image();
        img.src = obj;
        img.onload = function () {
            chuncai.imagewidth = img.width;
            chuncai.imageheight = img.height;
            $("#chuncaiface").attr("style", "background:url(" + obj + ") no-repeat scroll 50% 0% transparent; width:" + chuncai.imagewidth + "px;height:" + chuncai.imageheight + "px;");
        }
    }

    function getdata(el, id) {
        if (chuncai.data) {
            usedata(el, id, chuncai.data);
        }
        else {
            chuncaiSay('好像出错了，是什么错误呢...请联系管理猿');
        }
    }
    function usedata(el, id, data) {
        $("#dialog_chat_loading").hide();
        $("#tempsaying").css('display', "");
        var dat = data;
        if(el == 'defaultccs'){
            chuncaiSay(dat.defaultccs);
            setFace(dat.defaultface);
        }else if(el == 'getnotice'){
            chuncaiSay(dat.notice);
            setFace(1);
        }else if(el == 'showlifetime'){
            chuncaiSay(dat.showlifetime);
        }else if(el == 'talking'){
            var talkcon = $("#talk").val();
            var i = $.inArray(talkcon, chuncai.ques);
            if(i >= 0){
                chuncaiSay(chuncai.ans[i]);
                setFace(2);
            }else{
                chuncaiSay('.......................啥？');
                setFace(3);
            }
            clearInput();
        }else if(el == 'foods'){
            var str='<ul>';
            var arr = chuncai.foods;
            var preg = /function/;	
            for(var i in arr){
                if(arr[i] != '' && !preg.test(arr[i]) ){
                    str +='<li id="f'+i+'" class="eatfood" onclick="chuncai.eatfood(this.id)">'+arr[i]+'</li>';
                }
            }
            str+="</ul>";
            chuncaiSay(str);
        }else if(el = "eatsay"){
            var str = chuncai.eatsay[id];
            chuncaiSay(str);
            setFace(0);
        }
    }

    var timenum;
    //10分钟后页面没有响应就停止活动
    function setTime(){
        timenum = window.setTimeout(function () {
            stopTalkSelf();
            closeChuncaiMenu();
            closeNotice();
            closeInput();
            chuncaiSay("主人跑到哪里去了呢....");
            setFace(3);
            stoptime();
            $(document).one('mousemove', function () {
                chuncaiSay("野生的主人出现啦！");
                setFace(1);
                setTime();
            });
        }, 10*60*1000);
    }
    function stoptime(){
        if(timenum){
            clearTimeout(timenum);
        }
    }
    var talktime = 0;
    //设置自言自语频率（单位：秒）
    var talkself = 60;
    var talkobj;
    var talkself_arr = [
	    ["白日依山尽，黄河入海流，欲穷千里目，更上.....一层楼？", "1"],
	    ["我看见主人熊猫眼又加重了！", "3"],
	    ["我是不是很厉害呀～～？", "2"],
	    ["5555...昨天有个小孩子跟我抢棒棒糖吃.....", "3"],
	    ["昨天我好像看见主人又在众人之前卖萌了哦～", "2"]
    ];

    function talkSelf(talktime) {
        if (++talktime % talkself == 9) {
            closeChuncaiMenu();
            closeNotice();
            closeInput();
            var tsi = Math.floor(Math.random() * talkself_arr.length + 1)-1;
            chuncaiSay(talkself_arr[tsi][0]);
            setFace(talkself_arr[tsi][1]);
        }
        talkobj = window.setTimeout(function(){talkSelf(talktime)}, 1000);
    }
    function resetTalkSelf() {
        if (talkobj) {
            clearTimeout(talkobj);
            talkobj = window.setTimeout(function () { talkSelf(0) }, 1000);
        }
    }
    function stopTalkSelf(){
        if(talkobj){
            clearTimeout(talkobj);
        }
    }
    function arrayShuffle(arr){
        var result = [],
	    len = arr.length;
        while(len--){
            result[result.length] = arr.splice(Math.floor(Math.random()*(len+1)),1);
        }
        return result;
    }


    function typeWords(weichuncai_text, _typei) {
        var p = 1;
        var str = weichuncai_text.substr(0,_typei);
        var w = weichuncai_text.substr(_typei,1);
        if(w=="<"){
            str += weichuncai_text.substr(_typei,weichuncai_text.substr(_typei).indexOf(">")+1);
            p= weichuncai_text.substr(_typei).indexOf(">")+1;
        }
        _typei+=p;
        document.getElementById("tempsaying").innerHTML = str;
        if (_typei > weichuncai_text.length) {
            _typei = 0;
        } else {
            setTimeout(function () { typeWords(weichuncai_text, _typei); }, 20);
        }
    }
})(jQuery);
