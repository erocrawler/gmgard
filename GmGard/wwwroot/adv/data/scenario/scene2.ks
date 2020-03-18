[_tb_system_call storage=system/_scene2.ks]
[tb_show_message_window  ]
[jump_to_startpoint storage="scene2.ks"]

[iscript]
$.ajax({
	url: '/api/Game/Act',
	data: {'actname': 'Act2Start' },
	type: 'PUT',
}).done(function(result) {
	TG.kag.ftag.startTag("jump",{target:"*start"}); 
}).fail(function() {
TG.kag.ftag.startTag("jump",{target:"*error", storage:"title_screen.ks"}); 
});
[endscript]
[s]

*start
[bg  time="3000"  method="crossfade"  storage="bg_005.png"  ]
[tb_show_message_window  ]
[chara_show  name="L娘"  time="1000"  wait="true"  left="340"  top="148"  width="233"  height="228"  reflect="false"  ]
#L娘
“那么，选择白学家的人直接死亡！打死白学家！”Ｌ娘掏出了大Ｘ。[p]
#
白学家绅士们：“这是为什么呢？[r]
第一次,有了喜欢的职业 [r]
还得到了一生的挚友 [p]
两份喜悦相互重叠 [r]
这双重的喜悦又带来了更多更多的喜悦 [r]
本应已经得到了梦幻一般的幸福时光 [p]
然而,为什么,会变成这样？”[p]
#L娘
“妈的白学家！还在白！”[p]
[chara_hide  name="L娘"  time="1000"  wait="true"  ]
#
白学家绅士们被Ｌ娘带走了，他们将被迫在活动结束之前持续的推车轮，真是残酷啊。[p]

[if exp="sf.data.profession=='白学家'"]
[jump storage="scene2_dead.ks" target=""]
[endif]


[bg  storage="bg_005.png"  time="1000"  method="crossfade"  ]
[chara_show  name="人间入间"  time="1000"  wait="true"  left="355"  top="138"  width=""  height=""  reflect="false"  ]
#人间入间
“所有人都决定好自己的职业了吗，那么，出发吧!打开传送门！”[p]
#
人间使用某种膜法的力量打开了一道巨大的传送门。[p]

[chara_hide  name="人间入间"  time="1000" ]
[bg  time="3000"  method="crossfade"  storage="1C4203207E097627ED0AA1E50D4A642E.jpg"  ]
#
众萌妹，有大JJ的萌妹，有小JJ的萌妹一同踏入了传送门中。[p]


[bg  time="3000"  method="crossfade"  storage="01.jpg"  ]
[chara_show  name="人间入间"  time="1000"  wait="true"  left="363"  top="128"  width=""  height=""  reflect="false"  ]
#人间入间
“诸位，”人间在逐渐关闭的传送门中说道。“接下来的旅途，需要你们依靠自己的力量了，去吧！[r]
去消除欲望的魔王吧！[l]
用你们的身体。”[p]

[chara_hide  name="人间入间"  time="1000"  wait="true"  ]
#
传送门关闭了。[p]


随着灵魂形态人间的消失，在场的全员都意识到自己被坑了。[r]
有的绅士因为自己将不得不被啪啪啪而失落不已，有些人反而因此而兴奋了起来。[r]
不论如何，只要解决了异变，便能恢复原状了对吧！[p]

众人踏入了第一道关卡的大门。[p]

[bg  time="3000"  method="crossfade"  storage="无标题.jpg"  ]
[bg  time="3000"  method="crossfade"  storage="01.jpg"  ]
眼前是一个庞大的山间寺庙，天色已是黄昏。[p]
寺院的门口走出一个人影[p]
[chara_show  name="方丈"  time="1000"  wait="true"  left="75"  top="35"   reflect="false"  ]
#方丈
“欢迎，老夫是这里的方丈。”[p]
“进来吧，我们边走边说。”[p]
[chara_hide  name="方丈"  time="500"  wait="true"  ]
#
庞大的寺庙雅雀无声，只能听到潺潺的流水声。似乎除了方丈外一个人影也没有了。[p]

[chara_show  name="方丈"  time="1000"  wait="true"  left="75"  top="35"   reflect="false"  ]
#方丈
“老夫曾经有上百名弟子。如今老夫的弟子们全都被欲望的魔王所吞噬，只剩下了老夫一个人。。。”[p]
“天色不早了，老夫可以收留你们住一晚。但老夫希望你们能够救出我的弟子们。”[p]
[chara_hide  name="方丈"  time="500"  wait="true"  ]
#
下一站要走山路到达，似乎睡一晚养好精神是正确的选择。[p]

[glink  color="black"  storage="scene2.ks" target="*chosen" exp="sf.data.act2choice='yes'" size="20"  text="是"  x="278"  y="242"  width=""  height=""  _clickable_img=""  ]
[glink  color="black"  storage="scene2.ks" target="*chosen" exp="sf.data.act2choice='no'" size="20"  text="否"  x="500"  y="243"  width=""  height=""  _clickable_img=""  ]
要住下么？
[s]
*chosen
[iscript]
$.ajax({
	url: '/api/Game/Act',
	data: {'actname': 'Act2Choose', choice:sf.data.act2choice},
	type: 'PUT',
}).fail(function() {
TG.kag.ftag.startTag("jump",{target:"*error", storage:"title_screen.ks"}); 
}).done(function() {
TG.kag.ftag.startTag("jump",{target:"*"+sf.data.act2choice}); 
});
[endscript]
[s]

*afterchoose
[bg  time="3000"  method="crossfade"  storage="01.jpg"  ]
[iscript]
TG.kag.ftag.startTag("jump",{target:"*"+sf.data.act2choice}); 
[endscript]

*yes
[chara_show  name="方丈"  time="1000"  wait="true"  left="75"  top="35"   reflect="false"  ]
[wait time=1000]
#方丈
“很好，进来吧。”
[bg  time="3000"  method="crossfade"  storage="寺院内部.jpg"  ]
[p]

“没什么可以招待你们的，只有一些粗茶淡饭。”[p]
[chara_hide  name="方丈"  time="500"  wait="true"  ]
#
果然是“粗茶淡饭”，桌子上只有米饭和茶。。。[p]
吃过之后，你感到困意十足。[p]
[chara_show  name="方丈"  time="1000"  wait="true"  left="75"  top="35"  reflect="false"  ]
#方丈
“由于弟子都不见了，你们各自随便挑屋子休息吧。”[p]
[chara_hide  name="方丈"  time="500"  wait="true"  ]
#
屋子里的床似乎还很干净，可能是弟子们刚消失不久。[r]
你钻进了被窝，想着今天发生的事情。[p]
人间和方丈都有提及什么“欲望的魔王”，那是什么东西呢？[r]
脑子里浮想联翩闪过很多H物，你伴随着YY进入了梦乡。。。[p]
[jump  storage="scene2.ks"  target="*end"  ]

*no
[chara_show  name="方丈"  time="1000"  wait="true"  left="75"  top="35"  reflect="false"  ]
[wait time=1000]
#方丈
“哼，随你的便吧。” [p]

[chara_hide  name="方丈"  time="500"  wait="true"  ]
#
方丈把你推出了寺庙，并关上了大门。[p]
面对眼前无尽的崎岖山路，和渐暗的天色。你多少有点后悔没有选择进屋休息。[p]
但既然已经决定了，就赶夜路吧。或许可以捷足先登也说不定。[r]
你打了打寒碜，加快脚步前往下一个目的地。[p]

*end
[bg  time="3000"  method="crossfade"  storage="001.jpg"  ]

[jump  storage="scene3.ks"  target=""  ]
[s]