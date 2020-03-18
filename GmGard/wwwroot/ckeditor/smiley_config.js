CKEDITOR.editorConfig = function (config) {
    config.skin = 'moono';
    config.smiley_path = '//static.gmgard.com/smiley/';
    var cookie;
    if (window.getCookie && (cookie = window.getCookie('imgbackup'))) {
      config.smiley_path = cookie === "tu2.gmgard.com" ? '//tu2.gmgard.com/smiley/' : (cookie === "cnmobchishi.hggard.com" ? '//cnmobchishi.hggard.com/smiley/' : config.smiley_path);
    }

    config.smiley_images = {
        '真诗酱': ['真诗酱-无表情.jpg', '真诗酱-笑颜.jpg', '真诗酱-愤怒.jpg', '真诗酱-悲伤.jpg', '真诗酱-哭泣.jpg', '真诗酱-傲娇.jpg', '真诗酱-焦急.jpg', '真诗酱-吃惊.jpg', '真诗酱-受.jpg', '真诗酱-攻.jpg', '真诗酱-喜欢.jpg', '真诗酱-嫌弃.jpg'],
        '绅士': ['绅士.jpg', '蛤绅.jpg', '蛤绅2.jpg', '非洲绅.jpg', '斯巴达绅.jpg', '哭绅.jpg', '明绅.jpg', '狗绅.jpg', 'doge绅.jpg', '好绅士.jpg', '好绅士2.jpg',
            '二爷绅.jpg', '兵库北绅.jpg', '逸峰绅.jpg', '海绅.jpg', '食屎绅.jpg'],
        '馆长': ['蛤蛤嘲讽.gif', '蛤蛤抽风.gif', '蛤蛤抽烟.gif', '蛤蛤捶桌.gif', '蛤蛤扶墙.jpg', '蛤蛤哭.jpg', '蛤蛤拍桌.gif', '蛤蛤喝茶.gif',
            '蛤蛤问号.gif', '蛤蛤撞墙.gif', '呵呵.jpg', '金坷垃.jpg', '流泪捂脸.jpg', '啪啪啪.gif', '皮卡蛤.gif', '数钱.gif', '摊手.gif', '捂脸.jpg', '被水淹没.gif',
            '鼻青脸肿呵呵.jpg', '蛤蛤抽脸.jpg', '一起流泪.jpg', '蛤蛤拍.gif', '持珠捂脸.jpg', '蛤蛤摔.jpg', '蛤蛤木鱼.gif', 'naive.jpg', 'PSV.jpg', 'TM瞎了.jpg', 'woshenmedoubuzhidao.jpg',
            '下手重.jpg', '不服.jpg', '不科学.jpg', '不科学啊.jpg', '丧心病狂.jpg', '为何这么叼.jpg', '兄弟捂脸.jpg', '制造战争.jpg', '剧本不对.jpg', '只要loli.jpg',
            '奇迹.jpg', '奠.jpg', '奠2.jpg', '好棒好棒的.jpg', '好评.jpg', '尝尝这个.jpg', '就是这个人.jpg', '屌打Q.gif', '差评.jpg', '抠脚.jpg', '捂脸吐血.jpg', '死刑.jpg', '气死偶咧.jpg', '渣渣.jpg',
            '电柱.jpg', '羞涩.jpg', '膝盖中箭.gif', '蛤吐血.jpg', '蛤想天生.gif', '蛤蛤赞.gif', '蛤锤地.gif', '蜡烛捂脸.gif', '读艹.jpg', '跟我走一趟.jpg', '跪蛤.jpg', '被打了.jpg', '噫.jpg'],
        '阿鲁': ['噗.jpg', '抽烟.jpg', '吐血.jpg', '赞.jpg', 'boy♂next♂door.jpg', 'no.jpg', 'NTR.jpg', '♂赞♂.jpg', '击掌.jpg', '友♂達.jpg', '双重击掌.jpg', '屠龙宝刀.gif', '快吃药.jpg', '捂一脸血.jpg',
            '放弃治疗.jpg', '枪决.jpg', '石像脸.jpg', '绿帽.jpg', '自爆头.jpg', '艹字头.jpg', '追逐.gif', '阿鲁捂脸.jpg', '阿鲁赞.jpg', '-_-阿鲁.gif', '3嘴阿鲁.gif', '3重击掌阿鲁.jpg', 'A嘴阿鲁.jpg',
            'XD阿鲁.jpg', 'yoooo阿鲁.jpg', 'ω嘴阿鲁.jpg', '中指阿鲁.jpg', '侧目流汗阿鲁.jpg', '侧目阿鲁.jpg', '侧目阿鲁2.jpg', '侧目阿鲁3.jpg', '傻笑阿鲁.jpg', '内牛满面阿鲁.jpg', '内牛满面阿鲁2.jpg',
            '内牛满面阿鲁3.jpg', '内裤天线阿鲁.gif', '刷牙阿鲁.gif', '半阿鲁.jpg', '发情阿鲁.gif', '发芽阿鲁.jpg', '口水阿鲁.jpg', '叼烟阿鲁.jpg', '吃冰棍阿鲁.jpg', '吐血阿鲁.jpg', '团子阿鲁.jpg',
            '囧阿鲁.jpg', '基元首.jpg', '墨镜阿鲁.jpg', '大量血泪阿鲁.jpg', '天线阿鲁.gif', '奔跑阿鲁.gif', '奸笑阿鲁.jpg', '奸笑阿鲁2.jpg', '奸笑阿鲁3.jpg', '对眼阿鲁.jpg', '平嘴阿鲁.jpg',
            '平嘴阿鲁2.gif', '微笑阿鲁.jpg', '怒阿鲁.gif', '恶魔阿鲁.jpg', '戳眼阿鲁.jpg', '托腮阿鲁.jpg', '拍手阿鲁.gif', '拍桌阿鲁2.gif', '招手阿鲁.gif', '拿花阿鲁.jpg', '挖鼻阿鲁.gif',
            '捂嘴哭阿鲁.jpg', '排桌阿鲁.gif', '敲阿鲁.gif', '斜眼哭阿鲁.jpg', '斜眼阿鲁.gif', '方嘴阿鲁.jpg', '无眼阿鲁.jpg', '无脸阿鲁.jpg', '无表情阿鲁.jpg', '普通阿鲁.jpg', '普通阿鲁2.jpg',
            '横线阿鲁.jpg', '横线阿鲁2.jpg', '横线阿鲁3.jpg', '歪眼阿鲁.gif', '汗阿鲁.jpg', '泪扇阿鲁.jpg', '流汗阿鲁.gif', '深井冰阿鲁.jpg', '深井冰阿鲁×2.jpg', '阿鲁渣渣.jpg', '点头阿鲁.gif',
            '独眼阿鲁.jpg', '猫眼阿鲁.jpg', '猫阿鲁.jpg', '猫阿鲁2.gif', '目隐阿鲁.jpg', '目隐阿鲁哭.jpg', '眼镜阿鲁.jpg', '竖线阿鲁.gif', '竖线阿鲁2.jpg', '竖线阿鲁3.jpg', '脸红阿鲁.jpg',
            '脸红阿鲁2.jpg', '脸红阿鲁3.jpg', '草丛阿鲁.jpg', '血泪阿鲁.jpg', '血泪阿鲁2.jpg', '起包阿鲁.jpg', '蹲哭阿鲁.gif', '阿鲁kira.jpg', '阿鲁papapa.jpg', '阿鲁ye.jpg', '阿鲁扭.gif',
            '阿鲁内牛赞.jpg', '阿鲁鄙视.jpg', '阿鲁闪避.jpg', '阿鲁闹.gif', '顶翔阿鲁.jpg', '高兴阿鲁.jpg', '高兴阿鲁3.gif', '黑脸阿鲁.jpg', '鼓嘴阿鲁.jpg', '龅牙阿鲁.jpg'],
        'xsk': ['xsk.gif', 'padxsk.gif', 'xsk吐血.jpg', 'xsk柴刀.jpg', 'xsk梦想天生.gif', '旋转xsk.gif', '量子运动XSK.gif', '面码.jpg', '食翔操眼.gif', '鸟踩xsk.gif', '泪飘.gif', '矿工三人组.gif', '血泪.gif'],
        '杂': ['可达鸭.gif', '垂头可达鸭.gif', '学姐鸭.jpg', '抬头可达鸭.jpg', '红毛鸭.jpg', '蓝猫鸭.jpg',
            '好吃.gif', '射了.gif', '好男人.jpg', '喷鼻血.gif', '瞎了.jpg', '好男人2.jpg', '初音瞎了.jpg', 'SB.jpg', '药在手.jpg', '狗头.jpg', 'boy♂next♂door♂.jpg',
            'come come.gif', 'IOT.jpg', 'nimabi.jpg', 'not bad.jpg', 'sparta.jpg', 'spartaaaaaaa.jpg', 'sparta羊驼.jpg', 'T右.jpg', 'T左.jpg', 'van様.gif',
            '不知所谓.jpg', '二爷脸.jpg', '企士奇.jpg', '刀右.jpg', '刀左.jpg', '切丝五梱烟.jpg', '切丝烟.jpg',
            '到碗里来.jpg', '吃我精灵球啦.jpg', '吃翔.jpg', '呸！.jpg', '喵膜拜.jpg', '国宝火焰拳.jpg', '墨镜瞎.gif', '多重膜拜.gif', '大林同志.jpg',
            '好男人以募集.jpg', '好男人脸红.jpg', '妈妈会伤心.jpg', '屌捂脸.jpg', '屌捂脸3D.jpg', '开心的gates.jpg', '开心的jobs.jpg', '微笑微笑.jpg', '怪我喽.jpg', '恋妹.jpg',
            '我刀废了.jpg', '我来啦.jpg', '战五渣.gif', '打我啊.jpg', '扔出窗外.jpg', '抽脸3D.jpg', '拜拜.gif', '无节操锤地.gif', '暴走yaoming.jpg', '来回抽.gif',
            '枪右.jpg', '枪左.jpg', '核弹炮.jpg', '残念miku.jpg', '残念pad.jpg', '爸爸都没打过我.jpg', '猛磕.gif', '王♂凝视.jpg', '真是太好了.jpg', '碎蛋.jpg',
            '禽兽.jpg', '穷人感受.jpg', '管不住手.jpg', '红白喷茶.jpg', '红白捂脸.jpg', '红白捂脸3D.jpg', '红白神拳.jpg', '膜拜闹.gif', '舰队狗.jpg', '蔑视.jpg', '蜡烛取暖.gif',
            '触右.gif', '触左.gif', '逗我？.jpg', '醒醒逗比.jpg', '门缝猫.jpg', '闭嘴基佬.jpg', '队长手刀.gif', '队长手刀3D.gif', '陷入沉思.jpg',
            '鼻血.jpg', '曾蜡烛.gif', 'doge.jpg', 'dogelion.jpg', '跪穿地板.jpg', '扑通改二.jpg', '北方酱报警.jpg', '我在报警.jpg', '稳如poi.jpg', '摔成poi.jpg', '绅士の干杯.gif',
            'interesting.jpg', '二狗失望.jpg', '嘿嘿嘿.jpg', '欧拉？.jpg', '欧洲人的嘴脸.jpg', '看个宝贝.jpg', '真好啊.jpg', '邓摇.gif', "diao.gif", "Oh!.jpg", "一脸懵逼.jpg",
            "低头.jpg", "原来如此.jpg", "可了不得.jpg", "向大佬低头.jpg", "咕哒兴奋.jpg", "哈哈哈.jpg", "噗嗤.jpg", "墙角发抖.gif", "我晓得.jpg", "智障.jpg", "汪汪大哭.jpg",
            "突然兴奋.jpg", "突然变态.jpg", "突然消沉.jpg", "金刚锤子.jpg"],
        'FFF': ['kuma审判.jpg', 'kuma审判2.jpg', '喷射器.jpg', '处以火刑.jpg', '异端审判1.jpg', '异端审判2.jpg', '异端审判3.jpg', '异端审判4.jpg',
            '异端审判5.jpg', '异端审判6.jpg', '异端审判7.jpg', '异端审判8.jpg', '异端审判翠星篇.jpg', '引燃FFF.jpg', '引燃右.jpg', '引燃左.jpg',
            '情侶死ね.jpg', '无敌了.jpg', '正义审判.jpg', '烧死并围观.gif', '烧死.jpg', '烧！烧！烧！.jpg', '高举火把.jpg', 'FFF审判.jpg'],
        'doge': ['doge赞1.png', 'doge赞2.jpg', '上眺doge.png', '下眺doge.png', '兴奋doge.gif', '兴奋滑稽doge.gif', '冷汗doge.jpg', '冷汗阴影doge.jpg', '卷被doge.png',
            '双手合十doge.png', '吐舌doge.jpg', '喝饮料doge.jpg', '嫌弃doge.png', '害羞doge1.png', '害羞doge2.jpg', '思考doge.png', '惊讶doge1.png', '惊讶doge2.jpg',
            '愤慨doge.jpg', '持剑doge.jpg', '捂嘴doge.png', '斜眼doge1.png', '斜眼doge2.png', '斜眼doge3.png', '无感doge1.jpg', '无感doge2.gif', '无表情doge.jpg', '无语doge.png',
            '流泪doge.jpg', '滑稽doge.png', '瞪眼doge.jpg', '连续斜眼doge.gif', '鄙视doge.png', '鼓掌doge.gif']
    };
}