[_tb_system_call storage=system/_scene5.ks]
[tb_show_message_window  ]
[jump_to_startpoint storage="scene5.ks"]
#
[iscript]
$.ajax({
	url: '/api/Game/Act',
	data: {'actname': 'Act5Start'},
	type: 'PUT',
}).done(function(result) {
	sf.data.isdead = result.isdead;
	sf.data.deathcount = result.deathcount;
	var target = "*start_" + sf.data.act4choice;
	if (result.isdead) {
		target = target + "_dead";
	}
	TG.kag.ftag.startTag("jump",{target: target}); 
}).fail(function() {
TG.kag.ftag.startTag("jump",{target:"*error", storage:"title_screen.ks"}); 
});
[endscript]
[s]


*start_unknow
[jump target="*start_before_chapter"]

*start_unknow_dead
[bg  time="1000" wait="false"  method="crossfade"  storage="void.jpg"  ]
[tb_image_show  time="1000"  storage="default/portal.jpg"  width="700"  height="365" x=125 y=7  ]
你抛出的硬币落地了，发出清脆的声响。[r]
几乎同时，你听到了一声凄厉而不详的叫声。[p]
你不知道那是什么，只知道那并非是在宣告友好。[p]
你猛的回头，在下一个瞬间就无比后悔。[p]
无数口器上长着千奇百怪形状的牙齿，外形恶心的虫子，向你涌来。[p]
你试图逃跑，然而飞扑过来的昆虫一下就把你按倒在地。。。[p]
[tb_image_hide  time="1000" wait=false ]
[if exp="sf.data.profession=='御姐' || sf.data.profession=='扶她'"]
[bg  time="3000"  method="crossfade"  storage="fly01.jpg"  ]
[else]
[bg  time="3000"  method="crossfade"  storage="ffly02.jpg"  ]
[endif]
这些污秽的造物一边咬噬你的肉体和衣物，一边将那肮脏的东西插入你的体内。[p]
无数的虫子在你身上蠕动，你却动弹不得。[r]
粘稠湿滑的液体似乎有麻醉效果，你瘫软的身体被淫虫尽情蹂躏。[p]
“苗床”[p]
在你想到这个词时，那些口器将你所有的思考停止了。[p]
不过，你残存的部分仍会孕育出新的生命。[p]
[bg  time="3000"  method="crossfade"  storage="001.jpg"  ]
[jump storage="scene5_dead.ks" target=""]


*start_right
[bg  time="1000" wait="false"  method="crossfade"  storage="void.jpg"  ]
[tb_image_show  time="1000"  storage="default/portal.jpg"  width="700"  height="365" x=125 y=7  ]


[if exp="sf.data.profession=='扶她'"]
“吼吼。。。真是罕见啊。”[p]
你踏入右侧的传送门时，听到了略微熟悉的声音。[p]


[chara_show  name="F酱"  time="1000"  wait="true"  left="376"  top="91"  width="200"  height="203"  reflect="false"  ]
#F酱
“欢迎，吾辈的虔诚信徒啊！这里是，扶她才能进入的领域。”[p]
#
F酱，古明地觉，冴月麟，lolichushou等诸多扶她UP在此聚集。[p]
你踏入的地方，正是扶她圣域。[p]
#F酱
“虽然我们不能直接干涉这个游戏。”F学姐说道。[p]
“不过可以极尽我们所能，让你免受其他的弱小魔物伤害。”[p]
#
系统消息：[emb exp="sf.data.nickname"]获得了 扶她之守护！[r]
之后的游戏中，[emb exp="sf.data.nickname"]将不会被比最终BOSS弱的任何怪物杀死！[p]

[chara_hide  name="F酱"  time="1000"  wait="true"  ]
感觉通关在即了呢。[p]
橙色的光芒将你包裹。[p]
May the futa be with you.[p]
[tb_image_hide  time="1000" wait=false ]
[jump storage="scene5.ks" target="*start_before_chapter"]
[endif]


“站住！”[p]
一个声音将你喝止。[p]
“这前面只有真正的强者才能通行！回去吧！”[p]
是否继续前进？


[glink  color="black"  storage="scene5.ks"  size="20"  text="是"  x="168"  y="190"  exp="f.choice='yes'" target="*right_choose"  ]
[glink  color="black"  storage="scene5.ks"  size="20"  text="否"  x="631"  y="201" exp="f.choice='no'"  target="*right_choose" ]
[s]

*right_choose
[iscript]
$.ajax({
	url: '/api/Game/Act',
	data: {actname:'Act5Extra', choice: f.choice},
	type: 'PUT',
}).done(function(result) {
	sf.data.isdead = result.isdead;
	sf.data.deathcount = result.deathcount;
	var target = "*right_" + f.choice;
	if (f.choice == "yes" && result.isdead) {
		target = "*right_dead";
	}
	TG.kag.ftag.startTag("jump",{target: target}); 
}).fail(function() {
TG.kag.ftag.startTag("jump",{target:"*error", storage:"title_screen.ks"}); 
});
[endscript]
[s]

*right_yes
[wait time=500]
你犹豫了一下，还是向前行进了。[p]
“勇气可嘉但是缺乏实力啊。”[p]

[chara_show  name="F酱"  time="1000"  wait="true"  left="364"  top="110"  width="182"  height="185"  reflect="false"  ]
F学姐突然出现在你面前。[p]
“但是相信自己是好事，所以我给予你奖励。”[p]
系统消息：[emb exp="sf.data.nickname"]获得了 扶她之守护！[r]
之后的游戏中，[emb exp="sf.data.nickname"]将不会被比最终BOSS弱的任何怪物杀死！[p]

[chara_hide  name="F酱"  time="1000"  wait="true"  ]
感觉通关在即了呢。[p]
橙色的光芒将你包裹。[p]
“May the futa be with you.”[p]
[tb_image_hide  time="1000" wait=false ]
[jump storage="scene5.ks" target="*start_before_chapter"]


*right_no
[tb_image_hide  time="1000" wait=false ]
[wait time=500]
那么。。[p]
接下来将去往何方呢。[p]
你转身折返。[p]
“吼。。。把背后暴露给别人啊。”[p]
不知从哪里突然出现了一个巨大的亚人生物，脸上是西瓜般的纹路，还长着两只角。同时有着奶子和雄伟的大JJ。。。[p]
“虽然我只是个看门的，不过今天我要教给你一些人生的经验。”[p]
[bg  time="3000"  method="crossfade"  storage="ev_16b.jpg"  ]
你还没来的及反应过来就被从后庭贯穿到了嘴。[p]
“不要背对不是你伙伴的人。”[p]
你切实的记住了。[p]
[bg  time="3000"  method="crossfade"  storage="001.jpg"  ]
[jump storage="scene5_dead.ks" target=""]


*right_dead
[wait time=500]
你犹豫了一下，还是向前行进了。[p]
[tb_image_hide  time="1000" wait=false ]
“。。。不自量力。”[p]
不知从哪里突然出现了一个巨大的亚人生物，脸上是西瓜般的纹路，还长着两只角。同时有着奶子和雄伟的大JJ。。。[p]
[bg  time="3000"  method="crossfade"  storage="ev_16b.jpg"  ]
你还没来的及反应过来就被从后庭贯穿到了嘴。[p]
“啊啊啊啊啊啊啊啊啊”[r]
你在绝望中失去了知觉……[p]
[bg  time="3000"  method="crossfade"  storage="001.jpg"  ]
[jump storage="scene5_dead.ks" target=""]


*start_left_dead
[bg  time="1000" wait="false"  method="crossfade"  storage="void.jpg"  ]
[tb_image_show  time="1000"  storage="default/portal.jpg"  width="700"  height="365" x=125 y=7  ]
“那个。。。我传送不了太厚的东西，如果出现什么状况。。我只能保证你不会死掉哦？”[p]
谜之少女音在你耳边响起。[p]
“愿慈爱的大地母亲守护你们!”[p]
传送门散发出柔和的光芒，你感觉意识渐渐远去。[p]
[tb_image_hide  time="1000" wait=false ]
你睁开了双眼。[p]
[if exp="sf.data.profession=='御姐']
你发现自己傲人的事业线变成了可悲的峭壁。[p]
[else]
你发现自己本该是雄伟大雕的地方变成了可悲的峭壁。[p]
[endif]
原来。。强大是指的凸度么。[p]
[bg  time="3000"  method="crossfade"  storage="阿卡林.jpg"  ]
虽然没有作为生物死亡，但是你已经失去了继续下去的动力。[p]
你找了一个安静的山洞，静静的躺在了里面。[p]
[bg  time="3000"  method="crossfade"  storage="001.jpg"  ]
[jump storage="scene5_dead.ks" target=""]

*start_left
[bg  time="1000" wait="false"  method="crossfade"  storage="void.jpg"  ]
[tb_image_show  time="1000"  storage="default/portal.jpg"  width="700"  height="365" x=125 y=7  ]
传送门散发出柔和的光芒，你感觉意识渐渐远去。[p]
“温室中绽放的花朵们啊。”[p]
你听见令人怀念的话语在你耳边回响。[p]
“愿慈爱的大地母亲守护你们!”[p]
似乎哥○林杀手那个女主常常这么说。[p]
那么。。[p]
接下来将去往何方呢。[p]
[tb_image_hide  time="1000" wait=false ]

*start_before_chapter
[iscript]
$.ajax({
	url: '/api/Game/Act',
	data: {'actname': 'Act5BeforeChoose'},
	type: 'PUT',
}).done(function() {
TG.kag.ftag.startTag("jump",{target:"*start_chapter"}); 
});
[endscript]
[s]

*start_chapter
[bg  time="3000"  method="crossfade"  storage="001.jpg"  ]
又一次穿过传送门，展现在你眼前的景色愈发得混沌与狂乱。[p]

[bg  time="3000"  method="crossfade"  storage="0e2442a7d933c895ffe911d7d11373f083020082.jpg"  ]
大地，天空，河流，触手怪。[r]
尽是耀眼的鲜红。[p]
等等，触手怪？[p]
[bg  time="3000"  method="crossfade"  storage="第4关.jpg"  ]
[bg  time="3000"  method="crossfade"  storage="0e2442a7d933c895ffe911d7d11373f083020082.jpg"  ]
一片血红的世界中遍布着数目惊人的触手怪。。可以看到有些先行到达的绅士们已经成为了触手怪的饵食了。。。[p]
诶诶。。。那边是那样的么。。。呕。。。竟然还能那样。。。[p]
看不下去啊。[p]
总之要避免与那些东西正面相遇，慎重的前进吧。啊。。不过。。。[p]
感觉身体好像有点热热的。。。累了？不对，是这土地和水有问题么？！[p]
你向人间求助。[p]

[chara_show  name="人间入间" left="356"  top="140" time="1000"  wait="true"  ]
#人间入间
“恩，是的。那边的红色物质是那些触手怪的分泌物，含有强效媚药成分哦，请小心不要沾染上。”[p]
#
喂，这种事是不是应该提前说啊！[p]
#人间入间
“诶☆嘿”[p]
#
卖萌也完全没有卵用的好么。。。[p]
#人间入间
“那么这次提前告诉你一下两个重要的提示吧。”[p]
“其一，面对守护者要直面自己的内心。”[p]
“其二，跨越障碍之时，必得以洁净。。”通讯突然中断了。[p]

#
[chara_hide  name="人间入间"  time="1000"  wait="true"  ]
妈的，亭子又上不去了？[p]
“是你自己的网渣啊。”[p]
不知道还有多少家伙的声音要从上面传来。[p]
你扬起了头，那是与这鲜红世界形成极大反差的，深蓝色的巨龙。[p]

[chara_show  name="巨龙"  time="1000"  wait="true"  left="287"  top="75"  width="346"  height="301"  reflect="false"  ]
#巨龙
“我是玛里○斯·奈法○安，曾是在这片土地上的国家——‘真理’的象征。时代变迁，生死无常，以我一己之力也难以力挽狂澜。”[p]
#
卧槽，这货到底是要搞笑还是要膜，他不会想念点什么吧[p]
#巨龙
“那么。”巨龙用灼热的目光打量着你。“小家伙，你来到这里是想要得到些什么？”[p]
#
你要回答。。。


[glink  color="black"  storage="scene5.ks"  size="20"  text="为了守护绅庭的和平"  x="65"  y="252"  exp="sf.data.act5choice='peace'" target="*chosen"   ]
[glink  color="black"  storage="scene5.ks"  size="20"  text="苟"  x="416"  y="253"  width=""  exp="sf.data.act5choice='mo'" target="*chosen"   ]
[glink  color="black"  storage="scene5.ks"  size="20"  text="为了最后能艹到L娘"  x="618"  y="254"  exp="sf.data.act5choice='gan'" target="*chosen"   ]
[s]

*chosen
[iscript]
$.ajax({
	url: '/api/Game/Act',
	data: {actname: 'Act5Choose', choice:sf.data.act5choice},
	type: 'PUT',
}).fail(function() {
TG.kag.ftag.startTag("jump",{target:"*error", storage:"title_screen.ks"}); 
}).done(function() {
TG.kag.ftag.startTag("jump",{target:"*"+sf.data.act5choice}); 
});
[endscript]
[s]

*afterchoose
[bg  time="3000"  method="crossfade"  storage="0e2442a7d933c895ffe911d7d11373f083020082.jpg"  ]
[chara_show  name="巨龙"  time="1000"  wait="true"  left="287"  top="75"  width="346"  height="301"  reflect="false"  ]

[iscript]
TG.kag.ftag.startTag("jump",{target:"*"+sf.data.act5choice}); 
[endscript]

*peace
[wait time=500]
既然你诚心诚意的问了，我就大发慈悲的告诉你！[r]
为了防止绅庭被破坏，为了守护绅庭的和平。。。[p]
#巨龙
“够了！”巨龙打断了你，并轻蔑的看着你。。。[p]
[jump target="*end"]


*mo
[wait time=500]
庭子和绅士们把我放到这个位置上，我一定鞠躬尽瘁、死而后已，一定做到苟……[p]
#巨龙
“哼。”巨龙戴上了一副黑框眼镜，并轻蔑的看着你。。。[p]
[jump target="*end"]


*gan
[wait time=500]
从一开始我的目标就没有动摇过！[r]
我清楚地记得当初的目标：[p]

[chara_hide  name="巨龙"  time="1000"  wait="false"]
[bg  time="2000" wait="true" method="crossfade"  storage="bg_005.jpg"  ]
#人间入间
“绅士之庭的勇士们！你们听到了么！” [p]
#L娘
“？？？？？”Ｌ娘头上冒出一堆问号。[p]
#

[quake  time="300"  count="3"  hmax="10"  wait="true"  ]
“听到了！”门口爆发出了巨大的欢呼声。[p]
#人间入间
“我们的目标是？”人间戴上了Ｌ娘的胖次、[p]
#
“干爆Ｌ娘！”人群高声回应。[p]

[bg  time="3000"  method="crossfade"  storage="0e2442a7d933c895ffe911d7d11373f083020082.jpg"  ]
[chara_show  name="巨龙"  time="1000"  wait="true"  left="287"  top="75"  width="346"  height="301"  reflect="false"  ]
是的，为了最后能艹到L娘，我死再多次，花再多的棒棒糖也在所不辞！[p]
#巨龙
“哼。”巨龙轻蔑的一笑，死死地盯着你。。。[p]


*end
#
[chara_hide  name="巨龙"  time="1000"  wait="false"]
[bg  time="3000"  method="crossfade"  storage="001.jpg"  ]

[jump  storage="scene5b.ks"  target=""  ]
[s]