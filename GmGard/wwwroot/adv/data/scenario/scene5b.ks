[_tb_system_call storage=system/_scene5b.ks]
[tb_show_message_window  ]
[jump_to_startpoint storage="scene5b.ks"]
#
[iscript]
$.ajax({
	url: '/api/Game/Act',
	data: {'actname': 'Act5bStart'},
	type: 'PUT',
}).done(function(result) {
	sf.data.isdead = result.isdead;
	sf.data.deathcount = result.deathcount;
	var target = "*start_" + sf.data.act5choice;
	TG.kag.ftag.startTag("jump",{target: target}); 
}).fail(function() {
TG.kag.ftag.startTag("jump",{target:"*error", storage:"title_screen.ks"}); 
});
[endscript]
[s]

*start_peace
[bg  time="3000"  method="crossfade"  storage="0e2442a7d933c895ffe911d7d11373f083020082.jpg"  ]
[chara_show  name="巨龙"  time="1000"  wait="true"  left="336"  top="134"  width="269"  height="234"  reflect="false"  ]
[wait time=500]
#巨龙
“愚蠢!”巨龙突然就发怒了。[r]

“在真理的守护者面前你胆敢说谎？！”[p]
[chara_hide  name="巨龙"  time="1000"  wait="false"]
#
[quake  time="300"  count="3"  hmax="10"  wait="true"  ]
[wait time=500]
巨龙振翅高飞，离开了。你被它带起的风吹倒在地。。。[p]

吃进去了一些红色物质，呃。。。[p]

虽然你及时吐了出来，但是下面还是瞬间就
[if exp="sf.data.profession=='伪娘' || sf.data.profession=='扶她'"]
完全站立
[else]
洪水泛滥
[endif]
了。[p]
你的双手无法停止去满足那里的动作。每一次触碰，都令理智飞的更远。[p]
你泻出的体液吸引了附近的触手怪向你靠近过来，虽然本能告诉你危险，但是身体。。。[l][r]
[bg  time="3000"  method="crossfade"  storage="触手.jpg"  ]
却是想要就这样变得乱七八糟。[p]
触手越积越多，你的身体完全陷进了触手堆[l][r]
最后一个触手盖住了你的脸。你眼前一片漆黑，失去了知觉……[p]
[bg  time="3000"  method="crossfade"  storage="001.jpg"  ]
[jump storage="scene5b_dead.ks" target=""]

*start_mo
[bg  time="3000"  method="crossfade"  storage="0e2442a7d933c895ffe911d7d11373f083020082.jpg"  ]
[chara_show  name="巨龙"  time="1000"  wait="true"  left="336"  top="134"  width="269"  height="234"  reflect="false"  ]
[wait time=500]
#巨龙
“吼啊！”巨龙戴着黑框眼镜化成了人型。。[r]
“你已经钦定被续了！”[p]
[chara_hide  name="巨龙"  time="1000"  wait="false"]
#
[if exp="sf.data.profession=='御姐' || sf.data.profession=='扶她'"]
[bg  time="3000"  method="crossfade"  storage="续命.jpg"  ]
[else]
[bg  time="3000"  method="crossfade"  storage="ev05_03.jpg"  ]
[endif]
不知从哪突然蹦出来一只大蛤蟆，一下抱住了你[p]
“啊啊啊！！！” 你吓得动弹不得，任凭这个绿油油黏糊糊的生物蹂躏[p]
#巨龙
“又有新的人来为我续命，exciting!” 巨龙用某种奇怪的口音说着。[p]
“让我来教你一些人生经验：要往好的方面想。虽然你可能会感到很惭愧，但还是为绅庭做了一些微小的贡献嘛。”[p]
“那么，谢谢大家！”巨龙掏出了一根法杖[p]
[tb_image_show  time="1000"  storage="default/-1s.jpg"  width="700"  height="338" x=125 y=7  ]
“阿瓦达索命！”[p]
#
临死前你看到自己头上蹦出了一大堆-1s。[p]
[tb_image_hide  time="1000"  ]
[bg  time="3000"  method="crossfade"  storage="001.jpg"  ]
[jump storage="scene5b_dead.ks" target=""]

*start_gan
[bg  time="3000"  method="crossfade"  storage="0e2442a7d933c895ffe911d7d11373f083020082.jpg"  ]
[chara_show  name="巨龙"  time="1000"  wait="true"  left="336"  top="134"  width="269"  height="234"  reflect="false"  ]
[wait time=500]
#巨龙
“哼。”巨龙轻蔑的一笑。[r][l]

“虽然很低劣，但还是很忠于自己的内心的嘛。那么。。”[p]
#
巨龙扔下了什么。[p]
#巨龙
“这是给予你的诚实的奖励。”[p]
#
[chara_hide  name="巨龙"  time="1000"  wait="true"  ]

[tb_image_show  time="1000"  storage="default/白丝.png"  width="362"  height="193"  x="290"  y="192"  _clickable_img=""  ]
你仔细一看，是一双白色的丝袜。不过掉到地上已经侵染成了红色。。。[p]
[tb_image_hide  time="1000"  ]
#巨龙
“再见了，卑微的尘埃之子！”[p]
#
巨龙向上冲入了云层中。。。又掉了什么下来。[r]
似乎。。是一本书？[p]

你捡起来一看。。[r][l]

《死亡○翼无惨~乱交篇~(R1800限定)》[p]

呜哇，亏这牲口刚才敢那么说话。。[p]

不知是否是这变态龙的动静太大，附近的触手怪开始往这边靠拢了。你一边在心中咒骂着那个畜生，一边向河边跑去。[p]


[bg  time="3000"  method="crossfade"  storage="河流.jpg"  ]
来到河边，远远的可以望见某个品味很渣的城堡，大概那就是下一个目的地了吧。[r]
虽然不用走传送门了是很好，不过需要徒步走这么远的距离还是感觉太心累了啊。[r][l]
不过，在此之前。。。[p]

河道中奔流着的红色液体，散发着恶魔的芳香。仅仅是靠近，身体便燥热了起来。[p]

身前是媚药之河，身后是触手怪大军，手上拿的是脏兮兮黏糊糊的丝袜。应该，如何应对。。。

[glink  color="black"  storage="scene5b.ks"  size="20"  text="直接过河"  x="117"  y="238"  exp="sf.data.act5bchoice='cross'" target="*chosen" ]
[glink  color="black"  storage="scene5b.ks"  size="20"  text="穿上弄脏的白丝袜过河"  x="349"  y="238"  exp="sf.data.act5bchoice='wear'" target="*chosen" ]
[glink  color="black"  storage="scene5b.ks"  size="20"  text="先思考一下人生"  x="700"  y="238" exp="sf.data.act5bchoice='think'" target="*chosen" ]
[s]

*chosen
[iscript]
$.ajax({
	url: '/api/Game/Act',
	data: {actname: 'Act5bChoose', choice:sf.data.act5bchoice},
	type: 'PUT',
}).fail(function() {
TG.kag.ftag.startTag("jump",{target:"*error", storage:"title_screen.ks"}); 
}).done(function() {
TG.kag.ftag.startTag("jump",{target:"*"+sf.data.act5bchoice}); 
});
[endscript]
[s]

*afterchoose
[bg  time="3000"  method="crossfade"  storage="河流.jpg"  ]
[iscript]
TG.kag.ftag.startTag("jump",{target:"*"+sf.data.act5bchoice}); 
[endscript]

*cross
[wait time=500]
触手怪步步逼近，已经容不得犹豫了。你直接踏入了媚药之河中。[p]

在这之后你深刻的学到了皮肤吸收的知识，以及触手怪的粘液是脂溶性的冷知识。[p]

走到河的一半你的双腿便无力支撑身体了。你跪倒在媚药之河的中央，作为【人】的你，已经死去了。[p]

你将作为【肉块】重生。[p]

或许，那白丝有什么用吧。。。[p]

你的思考停滞了。。。[p]
[jump target="*end"]

*wear
[wait time=500]
或许这白丝有什么用？这样想着，你把它穿在了双腿。[p]

啊，自己身材真好，穿上白丝更能凸显你柔妙的曲线。[p]

无论从前面，侧面，还是从下往上看去，都—— [r][l]

从下。。往上？[p]

你的头与身体已经分离开来，双腿在你的头周围欢快的跳着舞。[p]

你的双腿踏着优美的舞步，向曾经的顶头上司走来。[p]

原来，我的腿的力道这么强啊。。。[p]
[jump target="*end"]

*think
[wait time=500]
面对这样的危机时刻，你依然决定停下来先思考一下人生。[p]

你突然想到之前人间给你的提示：跨越障碍之时，必得以洁净——[r][l]

那是不是在说要穿上这个白丝呢？不过这玩意沾满了莫名的红色液体，怕不是穿上也要死啊。。。[p]

有办法净化一下么。。？[p]

你四处张望，周围除了河流和触手怪，就没有别的东西了。[r]

到此为止了么……你叹了一口气，闭上了双眼……[p]

*end
[bg  time="3000"  method="crossfade"  storage="001.jpg"  ]

[jump  storage="scene6.ks"  target=""  ]
[s]