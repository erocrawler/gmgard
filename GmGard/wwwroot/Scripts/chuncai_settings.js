window.chuncai = $.extend(window.chuncai || {}, {
    ques: ["你好", "再见", "吃了么"],
    ans: ["泥嚎蛤蛤蛤蛤", "祝你身体健康，再贱", "快喂我"],
    foods: ["金坷垃", "节操", "香蕉", "棒棒糖"],
    eatsay: ["没有金坷垃，怎么种庄稼！", "节操真好吃！", "吧唧吧唧", "撇喽撇喽~"],
    talkself_arr: [
	["阅尽a片无数，心中自然无码", "1"],
	["一起来投稿吧...", "3"],
	["我是不是很厉害呀～～？", "2"],
	["祝天下所有的情侣都是失散多年的兄妹", "3"],
	["小撸怡情，大撸伤身！", "2"],
    ["来调教我吧！", "2"]
    ],
    imgs: [
       '//static.gmgard.com/Images/Chuncai/0.gif',
       '//static.gmgard.com/Images/Chuncai/1.gif',
       '//static.gmgard.com/Images/Chuncai/2.gif',
       '//static.gmgard.com/Images/Chuncai/3.gif',
       '//static.gmgard.com/Images/Chuncai/4.png'
       ]
})
if (window.location.host.indexOf("hggard.com") >= 0) {
    chuncai.imgs = chuncai.imgs.map(function (e) { return e.replace('gmgard.com', 'hggard.com') });
}