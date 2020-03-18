[_tb_system_call storage=system/_scene4.ks]
[tb_show_message_window  ]
[jump_to_startpoint storage="scene4.ks"]

[iscript]
$.ajax({
	url: '/api/Game/Act',
	data: {'actname': 'Act4Start' },
	type: 'PUT',
}).done(function(result) {
	sf.data.isdead = result.isdead;
	sf.data.deathcount = result.deathcount;
	var target = "*start_" + sf.data.act3choice;
	if (result.isdead && sf.data.act3choice != "attack") {
		target = "*start_dead";
	}
	TG.kag.ftag.startTag("jump",{target: target}); 

}).fail(function() {
TG.kag.ftag.startTag("jump",{target:"*error", storage:"title_screen.ks"}); 
});
[endscript]
[s]

*start_dead
[bg  time="3000"  method="crossfade"  storage="1933561923.jpg"  ]
[chara_show  name="谜之正太" face="smile" time="1000" height="600"  wait="true"  ]
#谜之正太
“我最喜欢破坏美丽而又脆弱的事物了。”[r]
小男孩笑着说。“小姐姐，我带你上天吧！”[p]
#
小男孩用蔑视的眼神看着你，同时身体也变得巨大而坚硬。是巨人！[p]
[chara_hide  name="谜之正太" ]
你被巨人牢牢按在地上，动弹不得。[p]
[if exp="sf.data.profession=='御姐'"]
[bg  time="3000"  method="crossfade"  storage="御姐.jpg"  ]
[else]
[bg  time="3000"  method="crossfade"  storage="aEV212D.jpg"  ]
[endif]
巨人把石柱般的肉棒对准了你的下体[p]
“不要啊！雅蠛蝶！”[r]
你绝望的挣扎着。[p]
巨人并不理会，肉棒缓缓的，缓缓的将你撑满 —— 直到你的胸腔。[p]
你四处张望希望能有人来帮助你，可是其他的绅士们不知道什么时候都不见了踪影。[p]
你在绝望中失去了知觉。。。[p]
[bg  time="3000"  method="crossfade"  storage="001.jpg"  ]
[jump storage="scene4_dead.ks" target=""]

*start_attack
[bg  time="3000"  method="crossfade"  storage="00971.jpg"  ]
你冲上前按倒了正太，并扒光了他的衣服。[p]
#谜之正太
“啊啊……小姐姐这么心急啊。。。”[r]
小男孩用蔑视的眼神看着你[p]
#
你开始玩弄正太的下体。[p]
#谜之正太
“嗯嗯……啊啊啊！”[r]
正太的下体越发坚硬，你变本加厉的开始玩弄正太的身体。[p]
#谜之正太
“呼呼……好久没这么舒服了……”[p]
[if exp="sf.data.profession=='伪娘' || sf.data.profession=='扶她'"]
#
“小混蛋，看我怎么收拾你！”[r]
说着，你掏出了大屌，插入正太的雏菊。[p]
[bg  time="3000"  method="crossfade"  storage="00972.jpg"  ]
#谜之正太
“嗯嗯……啊啊啊！！” [r]
在不断地抽插中，你和他同时喷出了白色液体[p]
#
“哈……呼……”[r]
你瞬间进入了贤者时间。刚才竟然推倒了一个男孩子……[p]
#谜之正太
[endif]
“那么，接下来换我来收拾你了！”[p]
#
[if exp="sf.data.profession=='御姐' || sf.data.profession=='扶她'"]
[bg  time="3000"  method="crossfade"  storage="御姐.jpg"  ]
[else]
[bg  time="3000"  method="crossfade"  storage="aEV212D.jpg"  ]
[endif]
说着，正太的身体也变得巨大而坚硬。是巨人！[r]
你被巨人牢牢按在地上，动弹不得。[p]
巨人把石柱般的肉棒对准了你的下体，你绝望的挣扎着。[p]
巨人并不理会，肉棒缓缓的，缓缓的将你撑满 —— 直到你的胸腔。[p]
你四处张望希望能有人来帮助你，可是其他的绅士们不知道什么时候都不见了踪影。[p]
你在绝望中失去了知觉。。。[p]
[bg  time="3000"  method="crossfade"  storage="001.jpg"  ]
[jump storage="scene4_dead.ks" target=""]

*start_run
[bg  time="3000"  method="crossfade"  storage="1933561923.jpg"  ]
[chara_show  name="谜之正太" face="smile" time="1000" height="600"  wait="true"  ]
你把旁边的扶她推上前去。[p]
#谜之正太
“你真美丽。”[r]
小男孩笑着说。手在她的身上四处乱摸，“而且。。”[p]
小男孩脸色骤变。[r]
“这！这是！！！”[p]
#
他触碰到了她的那个。[p]

[chara_hide  name="谜之正太" time="1000"  wait="false"  ]
#谜之正太
“YOOOO！！！路西法！！！”[r]
小男孩大叫着跑了出去。[wait time="1000"][p]
[jump target="*common"]

*start_stay
[if exp="sf.data.profession=='御姐' || sf.data.profession=='萝莉'"]
[jump target="*start_run"]
[endif]
[bg  time="3000"  method="crossfade"  storage="1933561923.jpg"  ]
[chara_show  name="谜之正太" face="smile" time="1000" height="600"  wait="true"  ]
#谜之正太
“你真美丽。”[p]
小男孩笑着说。手在你的身上四处乱摸，“而且。。”[p]
小男孩脸色骤变。[r]
“这！这是！！！”[p]
#
他触碰到了你的那个。[p]
[chara_hide  name="谜之正太" time="1000"  wait="false"  ]
#谜之正太
“YOOOO！！！路西法！！！”[r]
小男孩大叫着跑了出去。[wait time="1000"][p]

*common
#
[bg  time="3000"  method="crossfade"  storage="01.jpg"  ]
你突然眼前一黑。再度睁开眼睛时，你发现山脉已经消失不见了。你所在的地方离寺庙仅有数十米之遥。[p]
原来是幻术！那么那个小男孩是什么人？[r]
就在你思考出答案之前，你便坠入了另一个世界中。[p]
耳边隐隐回响着谜样的低语：“我把我的灵魂献给了路西法！”[p]


[bg  time="3000"  method="crossfade"  storage="第三关标题.jpg"  ]
[bg  time="3000"  method="crossfade"  storage="001.jpg"  ]
时间的感觉渐渐模糊，你已不知坠落了多久。黑暗如同没有尽头，无限延伸到未知的彼方。[p]

[bg  time="3000"  method="crossfade"  storage="void.jpg"  ]
当你感觉下降已经停止而睁开双眼时，发现自己正独自一人置身于虚空之中。。。[p]
本以为是这样，但实际上却是虚空样式的背景。[r]
你伸出手的时候便碰到了结实的墙体。莫非最终BOSS他闲到这种地步？[p]


#???
“哼，又一个迷途的羔羊么？”[p]
#
循声望去，一个眼神空洞——不，是眼眶中没有眼睛才对。[r]
另一种意义上的空洞呢（爆笑）不对不对现在不是这么轻松的时候啊。[p]
#???
“你是[emb exp="sf.data.nickname"]。。。哼~这样啊，你也来到了这边的世界啊。感觉如何？你知道这样的话么？‘喜欢的东西变成职业的时候未必会有那么开心。’”[p]
#
虽然确实是喜欢的东西变成了职业也并不开心但是不是这样的，不是。[p]
不过你更在意的是另外一点。[p]
#???
“为什么知道你的名字？呼呼。。。这可不是你应该知道的哦。”[p]
#
“♪ ♫♬♫♪♩ ♬♬♫”[p]
哦呀哦呀，神秘人的口袋里响起了恶魔城的BGM呀。[p]
#???
神秘人看了一眼自己的手机屏幕。“。。请稍等。”[p]
“喂喂？啊，我知道啦克蕾儿酱，我现在正在工作啊。”[p]
#

[bg  time="3000" method="crossfade"  storage="BG_118.jpg"  ]
[chara_show  name="桃子淫"  time="1000"  wait="true"  left="336"  top="139"  width="245"  height="221"  reflect="false"  ]
神秘人桃子淫瞬间揭露了自己的身份。[p]
#桃子淫
“不是借口啊不是！我没有凶你啊！是我错了...啥？啊啊，我会在10分钟内回去的。。。你不要那么着急啊。。啊，恩，我也爱你。”[p]
#
桃子淫挂断了电话。[p]
#桃子淫
“额。。。麻烦刚刚的全都忘了吧。我还要去和其他掉进来的人交流呢。”[p]
桃子淫轻轻咳嗽了两声。[r]
“作为相应的回报，我会告诉你之后道路应该选择哪边的提示的。。呐？”[p]
#

[glink  color="black"  storage="scene4.ks"  size="20"  text="要听"  x="261"  y="266"  width=""  height=""  target="*listen"  ]
[glink  color="black"  storage="scene4.ks"  size="20"  text="不要听"  x="488"  y="265"  width=""  height=""  target="*nolisten"  ]
[s]

*nolisten
#桃子淫
“哼，不管你说什么我都会告诉你的。”[p]
#
啊，傲娇好烦。[p]
*listen
#桃子淫
“那么听好了啊。。。”[p]
“1.弱者应选择向右而行。”[l][r]
“2.强者才选择向右前进。”[l][r]
“3.2是假的。”[l][r]
“4.我所说的所有话都是假的。”[p]
#
你只觉得听完还不如不听，可能还不如扔硬币决定。。。[p]


[chara_hide  name="桃子淫"  time="1000"  wait="true"  ]
还没来得及吐槽，桃子淫便没了踪影，估计是回家找老婆去了吧。[p]
炸裂吧现充。[p]

[bg  time="1000" wait="false"  method="crossfade"  storage="void.jpg"  ]
[tb_image_show  time="1000"  storage="default/portal.jpg"  width="700"  height="365" x=125 y=7  ]
向前走了一小段，便看到了桃子淫所说的岔路。两条道路分别连接着不同的传送门，左侧的传送门闪耀着蓝色的光芒，而右侧的则是温暖却又令人畏惧的橙色。[p]

你选择：
[glink  color="black"  storage="scene4.ks"  size="20"  text="向左"  x="149"  y="213"  width=""  height=""   exp="sf.data.act4choice='left'" target="*chosen"  ]
[glink  color="black"  storage="scene4.ks"  size="20"  text="向右"  x="398"  y="213"  width=""  height=""   exp="sf.data.act4choice='right'" target="*chosen"  ]
[glink  color="black"  storage="scene4.ks"  size="20"  text="抛硬币决定"  x="643"  y="211"  width=""  height=""   exp="sf.data.act4choice='unknow'" target="*chosen"  ]
[s]

*chosen
[iscript]
$.ajax({
	url: '/api/Game/Act',
	data: {actname: 'Act4Choose', choice:sf.data.act4choice},
	type: 'PUT',
}).fail(function() {
TG.kag.ftag.startTag("jump",{target:"*error", storage:"title_screen.ks"}); 
}).done(function() {
TG.kag.ftag.startTag("jump",{target:"*"+sf.data.act4choice}); 
});
[endscript]
[s]

*afterchoose
[bg  time="1000" wait="false"  method="crossfade"  storage="void.jpg"  ]
[tb_image_show  time="1000"  storage="default/portal.jpg"  width="700"  height="365" x=125 y=7  ]
[iscript]
TG.kag.ftag.startTag("jump",{target:"*"+sf.data.act4choice}); 
[endscript]

*left
[wait time="500"]
虽然不是很明白，但是还是去左边的门吧。[p]
你走近传送门，传送门散发出柔和的光芒，你感觉意识渐渐远去。。。[p]
[jump  storage="scene4.ks"  target="*end"  ]

*right
[wait time="500"]
虽然不是很明白，但是还是去右边的门吧。[p]
你走近传送门，传送门依然散发着温暖却又令人畏惧的光芒，你感觉意识渐渐远去。。。[p]
[jump  storage="scene4.ks"  target="*end"  ]

*unknow
[wait time="500"]
完全不明所以，还是抛硬币决定吧。[p]
你从口袋中拿出一枚硬币，将它高高抛起。[p]
世界简直定格了一般，在空中盘旋硬币被传送门的光芒映射的闪闪发光。。。[p]

*end
[tb_image_hide  time="1000"  ]
[bg  time="3000"  method="crossfade"  storage="001.jpg"  ]
[if exp="sf.data.stage<=4"]
第四章结束了。敬请期待后续章节！
[s]
[endif]
[jump  storage="scene5.ks"  target=""  ]
[s]