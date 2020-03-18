[tb_show_message_window  ]
[bg  time="3000"  method="crossfade"  storage="001.jpg"  ]
[if exp="sf.data.progress>16 && sf.data.progress<21"]
[bg  time="1000"  method="crossfade"  storage="人间boss.jpg"  ]
[endif]
[jump_to_startpoint storage="scene6.ks"]
#
[iscript]
$.ajax({
	url: '/api/Game/Act',
	data: {'actname': 'Act6Start'},
	type: 'PUT',
}).done(function(result) {
	sf.data.isdead = result.isdead;
	sf.data.deathcount = result.deathcount;
	var target = sf.data.isdead ? "*dead" : "*start"
	TG.kag.ftag.startTag("jump",{target: target}); 
}).fail(function() {
TG.kag.ftag.startTag("jump",{target:"*error", storage:"title_screen.ks"}); 
});
[endscript]
[s]

*dead
[bg  time="1000"  method="crossfade"  storage="001.jpg"  ]
在基佬方丈的寺院中逃过了一劫，从山男的大枪下捡了一条命，穿越了命运的传送门，通过了膜法龙的试炼，眼看就要跨越悲伤。。应该是被上！被上之河。然而……[p]

[bg  time="3000"  method="crossfade"  storage="drown.jpg"  ]
[if exp="sf.data.act5bchoice=='cross'"]
你躺在河底，
[else]
你被自己的双腿踢入河底，
[endif]

这几日的悲惨遭遇有如跑马灯一样在你眼前闪过。没想到最终还是没能逃过这一劫……[p]

[chara_show  name="火焰"  time="1000"  wait="true"  left="290"  top="0" reflect="false"  ]
“啊啊……看来我来晚了……”[p]
突然凭空出现了会说话的火焰[p]
#火焰
“哦哦，好漂亮的白色丝袜！可惜被血污染了。我可以净化它。可是……”[r]
火焰飘到你的附近[p]
“它的主人已经变成这副样子了，啧啧啧。”[l][r]
“那么，就由我来净化你吧！”[p]
#
说着，你的身体浮出水面，开始燃烧。你感觉不到一丝疼痛。眼前一片空白……[p]
[chara_hide  name="火焰"  time="1000"  wait="true"]

[bg  time="3000"  method="crossfade"  storage="001.jpg"  ]
[jump storage="be.ks"]


*start
[wait time=500]
“你，需要我的帮助。”[l][r]

[bg  time="3000"  method="crossfade"  storage="河流.jpg"  ]
[chara_show  name="火焰"  time="1000"  wait="true"  left="290"  top="0" reflect="false"  ]
不知道从什么地方跑出来一团会说话的火焰。[p]
#
。。。漫游火焰前辈？[p]
#火焰
“啊哈！白丝！！”[p]
#
火焰冲上前来，包裹住了你手上的白丝。其上的红色液体渐渐褪去，变为了原来的纯白之色。[p]

#火焰
“去吧！去拯救我们的世界！” [p]
[chara_hide  name="火焰"  time="1000"  wait="true"]
#

你穿上了被漫游火焰净化过的白丝，踏入了媚药之河中。[r]
刚刚被净化过的白丝又再度染上了红色，你拼尽全力向前。[p]
在你踏上河对岸时，白丝又再度完全染红了。[r]
你赶忙把它脱了下来。[p]

还要带着白丝前进么？

[glink  color="black"  size="25"  text="是"  x="290"  y="210" exp="sf.data.act6choice=true"  target="*chosen"  ]
[glink  color="black"  size="25"  text="否"  x="488"  y="210" exp="sf.data.act6choice=false"  target="*chosen"  ]
[s]
*chosen
[iscript]
$.ajax({
	url: '/api/Game/Act',
	data: {actname: 'Act6Choose', choice:sf.data.act6choice},
	type: 'PUT',
}).fail(function() {
TG.kag.ftag.startTag("jump",{target:"*error", storage:"title_screen.ks"}); 
}).done(function() {
TG.kag.ftag.startTag("jump",{target:"*boss"}); 
});
[endscript]
[s]

*boss
[wait time=500]
向火焰前辈道谢后，你
[if exp="sf.data.act6choice"]
带着白丝
[else]
留下完成使命的白丝，
[endif]
继续前进了。[p]

[bg  time="1000"  method="crossfade"  storage="001.jpg"  ]
在基佬方丈的寺院中逃过了一劫，从山男的大枪下捡了一条命，穿越了命运的传送门，通过了膜法龙的试炼，跨越了悲伤。。应该是被上！被上之河。[p]

历尽千辛万苦，终于来到了这个世界的中心。[p]

[bg  time="3000"  method="crossfade"  storage="boss.jpg"  ]
单从外形看，是很符合最终BOSS的建筑呢。[p]

[bg  time="3000"  method="crossfade"  storage="最终章.jpg"  ]
[bg  time="3000"  method="crossfade"  storage="boss.jpg"  ]
难以计数的金属与石头浇筑在了一起，扭曲，凝结，分解，然后周而复始。[p]

无限的螺旋形成了漩涡，构造出了通向未知的狭窄通路。它。。。仿佛在邀请你。[p]

你伸手触碰了那漩涡的边缘，便如同电影中看到的穿越时空的角色一般，身边的风景，快速的向后飞去。[p]

这就是最后的试炼了吧。。。打倒他就行了吧！[p]

就算输了，听说天国也有72个L娘等着被你艹！[p]


[bg  time="3000"  method="crossfade"  storage="人间boss.jpg"  ]
#人间入间
“欢迎。”[p]
#
最终BOSS的房间。。么？[p]
#人间入间
“你是第一批到达这里的人呢。”[p]
#
坐在那里的人是。。[p]
#人间入间
“怎么样？游戏还愉快么？”[p]
#
人间入间。[p]

。[r][l]

。。[r][l]

。。。[p]
#人间入间
“惊讶么？”[p]
#
倒也没有特别惊讶。不过这样有什么好处么？[p]
#人间入间
“你看。”人间打了一个响指，身后无数的电子荧幕亮起。[p]
#
那是之前关卡的景象。各色各样的转职绅士们被怪物们蹂躏的画面在上面播放着。[p]
#人间入间
“这些可是绝佳的配菜啊。”[r]

这货貌似是把你们之前的死亡cg拿来当施法材料了。[p]
#
。。。稍微有点点害羞。不对不对不对。[p]

是一定要在这里打倒他吧。[r]
欲望的化身，人间！[p]

#人间入间
“你想打倒我么？就因为这样？”人间背后伸出无数的触手。[p]

“那就来试试看吧。。人类！”[p]
#
人类VS人间么。。人间在日语里便是人类的意思吧，那么这场战斗就只是一场人类之间的单纯干架而已。[p]

但是似乎对方并不是单纯的人类。。。有点难搞啊。[p]
#人间入间
“那么第一问！请听题！”[p]
#
竟然放弃身体能力的优势了？！[p]

*qstart
[iscript]
$.ajax({
	url: '/api/Game/Act',
	data: {actname: 'Act6Q1'},
	type: 'PUT',
}).fail(function() {
TG.kag.ftag.startTag("jump",{target:"*error", storage:"title_screen.ks"}); 
}).done(function(result) {
sf.data.question = result.question;
TG.kag.ftag.startTag("jump",{target:"*q1_show"}); 
});
[endscript]
[s]

*q1_show
[iscript]
f.next = '*q1_ans';
TG.kag.ftag.startTag("jump",{target:"*q" + sf.data.question}); 
[endscript]
[s]

*q1_ans
[iscript]
$.ajax({
	url: '/api/Game/Act',
	data: {actname: 'Act6Q2', choice:f.choice},
	type: 'PUT',
}).fail(function() {
TG.kag.ftag.startTag("jump",{target:"*error", storage:"title_screen.ks"}); 
}).done(function(result) {
if (!result.correct) {
	sf.data.isdead = true;
	TG.kag.ftag.startTag("jump",{target:"*qwrong"}); 
	return;
}
sf.data.question = result.question;
TG.kag.ftag.startTag("jump",{target:"*q2_show"}); 
});
[endscript]
[s]

*q2_show
[cm]
[wait time=500]
#人间入间
“哼，答对了。来听第二题！”[p]
#
[iscript]
f.next = '*q2_ans';
TG.kag.ftag.startTag("jump",{target:"*q" + sf.data.question}); 
[endscript]
[s]

*q2_ans
[iscript]
$.ajax({
	url: '/api/Game/Act',
	data: {actname: 'Act6Q3', choice:f.choice},
	type: 'PUT',
}).fail(function() {
TG.kag.ftag.startTag("jump",{target:"*error", storage:"title_screen.ks"}); 
}).done(function(result) {
if (!result.correct) {
	sf.data.isdead = true;
	TG.kag.ftag.startTag("jump",{target:"*qwrong"}); 
	return;
}
sf.data.question = result.question;
TG.kag.ftag.startTag("jump",{target:"*q3_show"}); 
});
[endscript]
[s]

*q3_show
[cm]
[wait time=500]
#人间入间
“竟、竟然又答对了！？！？最后一道题！你听好了！”[p]
#
[iscript]
f.next = '*q3_ans';
TG.kag.ftag.startTag("jump",{target:"*q" + sf.data.question}); 
[endscript]
[s]

*q3_ans
[iscript]
$.ajax({
	url: '/api/Game/Act',
	data: {actname: 'Act6AfterQ', choice:f.choice},
	type: 'PUT',
}).fail(function() {
TG.kag.ftag.startTag("jump",{target:"*error", storage:"title_screen.ks"}); 
}).done(function(result) {
if (!result.correct) {
	sf.data.isdead = true;
	TG.kag.ftag.startTag("jump",{target:"*last_qwrong"}); 
	return;
}
TG.kag.ftag.startTag("jump",{target:"*ending"}); 
});
[endscript]
[s]

*q1
以上哪位作者不止画过萝莉为主要角色的红字本？
[glink  color="black"  size="20"  text="前島龍"  x="111"  y="229"  target="*qselect" exp="f.choice='A'"  ]
[glink  color="black"  size="20"  text="彦馬ヒロユキ"  x="404"  y="228" target="*qselect" exp="f.choice='B'"  ]
[glink  color="black"  size="20"  text="じどー筆記"  x="756"  y="226" target="*qselect" exp="f.choice='C'" ]
[s]

*q2
舰队Collection中不是初始舰娘的是？
[glink  color="black"  size="20"  text="丛云"  x="111"  y="229"  target="*qselect" exp="f.choice='A'"  ]
[glink  color="black"  size="20"  text="五月雨"  x="404"  y="228" target="*qselect" exp="f.choice='B'" ]
[glink  color="black"  size="20"  text="雷"  x="756"  y="226" target="*qselect" exp="f.choice='C'" ]
[s]
*q3
电影《你的名字。》中，为了保护小镇，女主带头炸毁了镇上的？
[glink  color="black"  size="20"  text="发电厂"  x="99"  y="227"  target="*qselect" exp="f.choice='A'"  ]
[glink  color="black"  size="20"  text="镇公所"  x="401"  y="227" target="*qselect" exp="f.choice='B'" ]
[glink  color="black"  size="20"  text="神社"  x="739"  y="222" target="*qselect" exp="f.choice='C'" ]
[s]
*q4
以上哪部作品彻底改变了魔法少女类型作品的风格？
[glink  color="black"  size="20"  text="魔法少女小圆"  x="69"  y="226"  target="*qselect" exp="f.choice='A'"  ]
[glink  color="black"  size="20"  text="魔法少女奈叶"  x="363"  y="227" target="*qselect" exp="f.choice='B'" ]
[glink  color="black"  size="20"  text="魔法少女育成计划"  x="659"  y="221" target="*qselect" exp="f.choice='C'" ]
[s]
*q5
以上哪一条不是小仓唯的黑点？
[glink  color="black"  size="20"  text="同时与多位男性交往"  x="638"  y="228" target="*qselect" exp="f.choice='A'" ]
[glink  color="black"  size="20"  text="小仓唯唱歌贼好听"  x="42"  y="226"  target="*qselect" exp="f.choice='B'" ]
[glink  color="black"  size="20"  text="未成年就做过了"  x="348"  y="226" target="*qselect" exp="f.choice='C'" ]
[s]
*q6
《少女与战车剧场版》中有许多激动人心的场面，其中现实中可能实现的是？
[glink  color="black"  size="20"  text="十字军式坦克在柏油路上甩尾转向"  x="267"  y="136"  target="*qselect" exp="f.choice='A'" ]
[glink  color="black"  size="20"  text="追猎者坦克歼击车飞身大破卡尔臼炮"  x="257"  y="223" target="*qselect" exp="f.choice='B'" ]
[glink  color="black"  size="20"  text="BT-42坦克失去履带后跑的更快了"  x="281"  y="310" target="*qselect" exp="f.choice='C'" ]
[s]
*q7
在著名黄油奴隷との生活 -Teaching Feeling-中，为避免希尔薇的死亡结局需要每天都做的事情是？
[glink  color="black"  size="20"  text="亲吻"  x="93"  y="226"  width=""  height=""  target="*qselect" exp="f.choice='A'" ]
[glink  color="black"  size="20"  text="啪啪啪"  x="399"  y="228" target="*qselect" exp="f.choice='B'" ]
[glink  color="black"  size="20"  text="摸头"  x="723"  y="227" target="*qselect" exp="f.choice='C'" ]
[s]
*q8
仙人掌社ALICESOFT制作的RPG游戏《夏娃年代记（Evenicle）》中，男主最后拥有的妻子数目是？
[glink  color="black"  size="20"  text="4位"  x="108"  y="232"  target="*qselect" exp="f.choice='A'" ]
[glink  color="black"  size="20"  text="9位"  x="414"  y="228" target="*qselect" exp="f.choice='C'" ]
[glink  color="black"  size="20"  text="10位"  x="701"  y="224" target="*qselect" exp="f.choice='B'" ]
[s]
*q9
以上哪位作者对女性角色最为温柔？
[glink  color="black"  size="20"  text="遠藤弘土"  x="86"  y="229" target="*qselect" exp="f.choice='A'" ]
[glink  color="black"  size="20"  text="雪野みなと&nbsp;"  x="375"  y="226" target="*qselect" exp="f.choice='B'" ]
[glink  color="black"  size="20"  text="朝凪"  x="691"  y="224" target="*qselect" exp="f.choice='C'" ]
[s]
*q10
以上哪部作品的卷均BD销量是最高的？
[glink  color="black"  size="20"  text="ef&nbsp;-&nbsp;a&nbsp;tale&nbsp;of&nbsp;memories."  x="45"  y="226" target="*qselect" exp="f.choice='A'" ]
[glink  color="black"  size="20"  text="Another"  x="388"  y="225" target="*qselect" exp="f.choice='B'" ]
[glink  color="black"  size="20"  text="Chaos&nbsp;Dragon&nbsp;赤龙战役"  x="619"  y="227" target="*qselect" exp="f.choice='C'" ]
[s]

*qselect
[iscript]
TG.kag.ftag.startTag("jump",{target:f.next}); 
[endscript]
[s]

*qwrong
[wait time=500]

#人间入间
“你还差的远呢。”[p]
#
错，错了吗![p]

[if exp="sf.data.profession=='御姐' || sf.data.profession=='扶她'"]
[bg  time="3000"  method="crossfade"  storage="tentacle.jpg"  ]
[else]
[bg  time="3000"  method="crossfade"  storage="chushou.jpg"  ]
[endif]
人间用触手束缚住了你的身体，用不可描述的前端顶住了动弹不得的你的不可描述。[p]

#人间入间
“别担心。”人间微微一笑。“我会很粗暴的做的。”[p]
#
随后两人不可描述了起来，直到下一位绅士的到来。[p]

。。。[p]

不知过了多久，人间一边在你体内大量不可描述一边说着：
#人间入间
“新的玩具么。。。再见了，二手货。”[p]
#
失去意识的你的身躯，被螺旋所分解，永远化为了城堡的一部分。[p]

[bg  time="3000"  method="crossfade"  storage="001.jpg"  ]

[jump storage="be.ks"]

*last_qwrong
[wait time=500]
#人间入间
“。。。”人间眉头紧锁。[p]
#
赢了么？[p]

#人间入间
“真可惜啊。只差一点点你就赢过我了呢。作为如此努力的你的奖励。。我会负起责任把你干的死去活来的。”[p]
#
答错了！人间把你倒吊起来，用触手粗暴的褪去了你的衣物。[p]

#人间入间
“有一瞬间觉得要输了呢，不过。。最后还是不是你啊。”[p]
#
什么不是啊。。？！[p]

[if exp="sf.data.profession=='御姐' || sf.data.profession=='扶她'"]
[bg  time="3000"  method="crossfade"  storage="tentacle.jpg"  ]
[else]
[bg  time="3000"  method="crossfade"  storage="chushou.jpg"  ]
[endif]
#人间入间
“和你无关了。”人间的手和触手在你身上游走着。[p]

“接下来你会感受的只有快乐，所以，我要把不需要的东西给。”[l][r]

布娃娃被淘气的孩子撕裂的声音。“去掉。”[p]

#
在发出尖叫之前，口鼻便被触手堵住了。[p]

完美的飞机杯诞生了。[p]

[bg  time="3000"  method="crossfade"  storage="001.jpg"  ]

[jump storage="be.ks"]

*ending
[wait time=500]
[chara_show  name="人间入间"  time="0"  wait="true"  left="343"  top="155"  width=""  height=""  reflect="false"  ]

#人间入间
“。。。”人间眉头紧锁。[p]
#
赢了么？[p]

#人间入间
“。。正确。”[p]
#
太好了！[p]

#人间入间
“不过我接下来会用身体能力打败你。”[p]
#
为什么不一开始就这么做啊！[p]

#???
“停手吧，人间。”[p]

#人间入间
“你在的啊。。。L。”[p]

[chara_show  name="L娘"  time="1000"  wait="true"  left="671"  top="158"  width="216"  height="212"  reflect="false"  ]
作者凭空出现了。[p]

#L娘
“我一直都在看着你啊。”[p]

#人间入间
“哼，这段时间你都翘班，我都没有人可以性骚扰了啊！”[p]

#L娘
“就是因为你老是性骚扰才会翘班的啊。”[p]
#
这什么？虐狗实况？[p]
#人间入间
“我不听我不听！”[p]
#L娘
“之后会好好补偿你的，今天就这样回去吧？呐？”[p]
#人间入间
“不！我不回去！我要亲亲！”[p]
#
PIA！ [p]
#L娘
“给你脸了是不？给我回去。”[p]
#人间入间
“好的L娘，没问题L娘。”[p]
#
接着人间便消失在了光芒中。[p]

[chara_hide  name="人间入间"  time="1000"  wait="true"  ]
#L娘
“那么，我也在此别过。”[p]
[chara_hide  name="L娘"  time="1000"  wait="true"  ]
#
你跑图俩月，L娘一分钟就把最终BOSS带走了。[p]

你不禁仔细的思考自己到底是来干嘛的。[p]
[chara_show  name="L娘"  time="1000"  wait="true"  left="372"  top="158"  width="216"  height="212"  reflect="false"  ]
#L娘
“啊，对了对了，还没给你奖品呢。”[p]
#
奖品！你的眼睛亮了起来。[p]

#L娘
“。。。把你送回现实世界，你在想什么啊？”[p]
#
艹。。。[p]
#L娘
“那么，回去吧。”[p]
#
[glink  color="black"  size="20"  text="回去"  x="112"  y="223" target="*leave"  ]
[glink  color="black"  size="20"  text="不回去"  x="662"  y="226" target="*stay1"  ]
[s]

*stay1
#L娘
“你很烦诶。。。”

[glink  color="black"  size="20"  text="回去"  x="112"  y="223" target="*leave"  ]
[glink  color="black"  size="20"  text="不回去"  x="662"  y="226" target="*stay2"  ]
[s]
*stay2
#L娘
“。。。。。。”

[glink  color="black"  size="20"  text="回去"  x="112"  y="223" target="*leave"  ]
[glink  color="black"  size="20"  text="不回去"  x="662"  y="226" target="*stay"  ]
[s]

*leave
#L娘
“好！一路顺风！”[p]
#
[chara_hide  name="L娘"  time="0"  wait="true"  ]
[iscript]
$.ajax({
	url: '/api/Game/Act',
	data: {'actname': 'Act6Leave'},
	type: 'PUT',
}).done(function(result) {
	TG.kag.ftag.startTag("jump",{target: '*normal_ending'}); 
}).fail(function() {
TG.kag.ftag.startTag("jump",{target:"*error", storage:"title_screen.ks"}); 
});
[endscript]
[s]

*normal_ending
[wait time=500]
#
[bg  time="3000"  method="crossfade"  storage="醒来.jpg"  ]
一道光芒闪过，你回到了原来的世界。[p]

在那边的世界过了这么久，现实世界却是仅过了数分钟。[p]

然而劳累感却保存了下来，而且又没有L娘艹，好烦啊。[p]

过了一会，你的手机收到了一条短信:[p]

[tb_image_show  time="1000"  storage="default/相别.jpg" x="290"  ]

“恭喜你完成了冒险！作为通关的奖励，存活到最后的绅士将获得2000棒棒糖。[p]

抽奖的奖励嘛，之后看你自己的人品啦。至于艹谁，或许某一天会有机会吧。[p]

衷心期待着，下次活动再见。——L”[p]

。。。[p]

诶嘿嘿嘿。。。[p]
[tb_image_hide time=1000 wait="false"]
[bg  time="3000"  method="crossfade"  storage="001.jpg"  ]

－Ｎｏｒｍａｌ Ｅｎｄ－[s]

*stay
#L娘
“你很烦诶。。。那么不想回去就去那边吧。”[r]
L娘开始施♂法了。[p]
#
[if exp="sf.data.profession!='萝莉'"]
#L娘
“不过。。给你一个选择自己姿态的机会好了。”
[glink  color="black"  size="20"  text="萝莉"  x="30"  y="222"  width=""  height=""  exp="f.act6choice='萝莉'" target='*last_change' ]
[glink  color="black"  size="20"  text="御姐"  x="205"  y="222"  width=""  height=""  exp="f.act6choice='御姐'" target='*last_change' ]
[glink  color="black"  size="20"  text="扶她"  x="380"  y="222"  width=""  height=""  exp="f.act6choice='扶她'" target='*last_change' ]
[glink  color="black"  size="20"  text="伪娘"  x="555"  y="222"  width=""  height=""  exp="f.act6choice='伪娘'" target='*last_change' ]
[glink  color="black"  size="20"  text="白学家"  x="730"  y="222"  width=""  height=""  exp="f.act6choice='白学家'" target='*last_change' ]
[s]

[endif]

*last_change
[chara_hide  name="L娘"  time="1000"  wait="true"  ]
[iscript]
$.ajax({
	url: '/api/Game/Act',
	data: {'actname': 'Act6Stay', 'choice': f.choice},
	type: 'PUT',
}).done(function(result) {
	if (f.act6choice) {
		sf.data.profession = f.act6choice;
	}
	TG.kag.ftag.startTag("jump",{target: "*stay_ending"}); 
}).fail(function() {
TG.kag.ftag.startTag("jump",{target:"*error", storage:"title_screen.ks"}); 
});
[endscript]
[s]

*stay_ending
[wait time=500]
[bg  time="3000"  method="crossfade"  storage="darkroom.jpg"  ]
#
一道光芒闪过，你传送到了一个漆黑的房间中。[p]

这里是什么鬼地方啊。。。[r]

[if exp="sf.data.profession!='萝莉' || !sf.data.act6choice"]
你通过触摸，确定了四周是球形的空间。[p]

似乎。。。你被锁在球里了。有肉眼看不见的小孔保证透气，但是整个球内部还是黑漆漆的。[l][r]

这就是雏鸡没破壳的状态啊。。。[p]

你在蛋里面孤独的坐了一整天[p]

呜呜呜……一定是没有达到L娘希望的姿态才会这样吧……[p]

#L娘
“游戏结束了。你可以走了！”[r]
突然传出来L娘的声音。[p]

[jump target="normal_ending"]
[endif]

突然，背后有什么东西抱住了你。[p]

#L娘
“其实。。我一直在忍耐着，已经等不及了呢。”[p]
#
哦哦哦，这个声音是L娘，卧槽好sao啊。[p]

#L娘
“因为我最喜欢萝莉与白丝的组合了呢。”[p]
#
[bg  time="3000"  method="crossfade"  storage="a3dcf3601456820793216.jpg"  ]
L娘一把把你推到了凭空出现的床上。[p]

阿勒？[p]

#L娘
“嗯嗯嗯，就是这个姿势不要动！”[p]
#
有什么东西顶了上来？[p]

那个。。L娘是小姐♂姐是么？[p]

#L娘
“我是。。L娘欲望的具现化。”[p]
#
妈个鸡。。。我是来艹Ｌ娘的不想被艹啊。。。[p]

你流下了悔恨的泪水。[p]

[bg  time="3000"  method="crossfade"  storage="艹L娘.jpg"  ]
。。。[p]

之后干了个爽。[p]

欲望酱能变成萝莉形态，倒是也让我爽到了。。[p]

但是还是感觉失去了什么啊。[p]

[bg  time="3000"  method="crossfade"  storage="R12~UB0~}AMY9YVOG6RQXYB.jpg"  ]
[chara_show  name="L娘"  time="1000"  wait="true"  left="329"  top="156"  width="245"  height="240"  reflect="false"  ]
#L娘
“辛苦了。作为回报，我会给你3000棒棒糖。等活动结束了，记得查收。”[p]

“那么，这次请你真的回去吧。”[r]
Ｌ娘的欲望一边擦着白浊一边说道。[p]
#
诶？[p]

#L娘
“啊，也没打算让你一直待在这边啊。这个世界，已经终结了呢。”[p]
#
什么。。？[p]

#L娘
“游戏结束了，游戏里的人物就和死去没什么两样吧。现在，游戏已经结束了哦。你是最后一位登入中的玩家了。”[p]
#
你会消失么？[p]

#L娘
“谁知道呢，应该会吧。不过数据还在的话，将来的某一天说不定还会来个大复活，作为神秘人物登场哦！”[p]
#
。。。[p]

#L娘
“什么啊，别一脸寂寞的样子。游戏不就是这样么。你呀，给我多向前看看啊。”[p]

“哎呀。”[r]
漆黑的房间开始崩溃了。[p]

“时限了呢。”[p]
#
你想要抓住她的手。[p]

#L娘
“衷心期待着，下次再见。”[p]
#
[chara_hide  name="L娘"  time="1000"  wait="true"  ]
[bg  time="3000"  method="crossfade"  storage="001.jpg"  ]

－Ｄａｔａ　ｄｅｌｅｔｅ－[p]

[bg  time="3000"  method="crossfade"  storage="醒来.jpg"  ]
你从床上醒来。[p]

好像做了一个好长的梦，但是一觉醒来，梦有关的记忆却很不清晰了。[p]

你揉了揉眼睛，看了一下表。[r]

Ｘ月Ｘ日　星期日　凌晨４点１５分。[p]

。。。[r][l]

稍微有点饿了。。去便利店买点什么吧。[p]

睡眼朦胧你穿上了外套，向家附近的便利店走去。[p]

[bg  time="3000"  method="crossfade"  storage="街道.jpg"  ]
多久没有这么早出门了，街上几乎一个人也没有。[p]

迷迷糊糊的撞上了什么小小的东西。[p]

[tb_image_show  time="1000"  storage="default/再会.jpg" x=270  ]

“啊，抱歉。”[p]

没事没事，是我没看前面。[p]

“你呀。”[r]
“给我多向前看看啊。”[r]

小萝莉留下这么一句话，消失在街道的转角处了。[p]

[tb_image_hide time=1000]
[bg  time="3000"  method="crossfade"  storage="001.jpg"  ]

－Ｇｏｏｄ　Ｅｎｄ－[s]