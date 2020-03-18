[_tb_system_call storage=system/_scene3.ks]
[tb_show_message_window  ]
[jump_to_startpoint storage="scene3.ks"]

[iscript]
$.ajax({
	url: '/api/Game/Act',
	data: {'actname': 'Act3Start' },
	type: 'PUT',
}).done(function(result) {
	sf.data.isdead = result.isdead;
	sf.data.deathcount = result.deathcount;
	var target = "*start_" + sf.data.act2choice;
	if (result.isdead) {
		if (result.israndom) {
			target = (sf.data.act2choice == "yes") ? "*bed" : "*cliff"; 
		} else if (sf.data.profession == '扶她') {
			target = "*futa";
		} else if (sf.data.profession == '伪娘') {
			target = "*otoko_h";
		} else if (sf.data.profession == '萝莉') {
			target = "*loli_h";
		}
	}
	TG.kag.ftag.startTag("jump",{target: target}); 
}).fail(function() {
TG.kag.ftag.startTag("jump",{target:"*error", storage:"title_screen.ks"}); 
});
[endscript]
[s]

*bed
[wait time="500"]
深夜……[l][r]
梦里的h物突然全都化成了可怕的怪物，向你扑了过来。[p]
[bg  time="3000"  method="crossfade"  storage="tentacle.jpg"  ]
你从噩梦中惊醒了，寺庙的床竟然变成了巨大的触手苗床！[p]
你无力地被看着就在各种方面很糟糕的床怪吃掉了[l]←SEX的意味上。[p]
[bg  time="3000"  method="crossfade"  storage="001.jpg"  ]
[jump storage="scene3_dead.ks" target=""]

*cliff
[bg  time="3000"  method="crossfade"  storage="02.jpg"  ]
[wait time="500"]
已经是深夜了……[l][r]
眼前已经伸手不见五指，你在深山之中蹒跚前进。[p]
似乎已经迷路了，你只想走过这片峭壁，找一处可以歇脚的地方。[p]
突然，你脚下一滑，掉下了深渊之中...[p]
[bg  time="3000"  method="crossfade"  storage="001.jpg"  ]
[jump storage="scene3_dead.ks" target=""]

*futa
[wait time="500"]
深夜……[l][r]
梦里的h物突然全都化成了可怕的怪物，向你扑了过来。[p]
你从噩梦中惊醒，发现自己竟然动弹不得！是麻药！[p]
[bg  time="3000"  method="crossfade"  storage="寺院内部.jpg"  ]
[chara_show  name="方丈"  time="1000"  wait="true"  left="75"  top="35"  reflect="false"  ]
方丈拿着一把剪刀，向你缓缓靠近……[p]
#方丈
“老夫最喜欢可♂爱的男孩子了，但是！！！”[p]
#
方丈咆哮着，挥舞着手指的剪刀。[p]
#方丈
“老夫却最不能忍受扶她这种双性的异端！！！[r]
为什么女孩子会有大鸡鸡！！！为什么！！！”[p]
#
[quake  time="300"  count="3"  hmax="10" ]
只见刀光一闪，你失去了知觉。。。[p]

[chara_hide  name="方丈"  time="1000" ]
[bg  time="3000"  method="crossfade"  storage="扶她死亡.jpg"  ]
第二天，准备上山的绅士们发现本应住着扶她一行人的房间已经空无一人，只剩下一滩滩的血迹和无数大O……[p]
[bg  time="3000"  method="crossfade"  storage="001.jpg"  ]
[jump storage="scene3_dead.ks" target=""]

*otoko_h
[wait time="500"]
深夜……[l][r]
梦里的h物突然全都化成了可怕的怪物，向你扑了过来。[p]
你从噩梦中惊醒，发现自己竟然动弹不得！是麻药！[p]
[bg  time="3000"  method="crossfade"  storage="寺院内部.jpg"  ]
[chara_show  name="方丈"  time="1000"  wait="true"  left="75"  top="35"  reflect="false"  ]
方丈带着如日间的笑容向你靠近[p]
#方丈
“老夫最喜欢可♂爱的男孩子了！呵呵呵呵！！！”[p]
“受欲望力量的影响，老夫已经不能忍受可爱的男孩子在自己眼前晃来晃去了！！！”[p]
#
方丈纵身向你扑来，你竭尽全力挣扎，却使方丈更加兴奋了。[p]
[chara_hide  name="方丈"  time="1000" ]
[bg  time="3000"  method="crossfade"  storage="ev01_on.jpg"  ]
#方丈
“噢噢噢噢哦哦哦！！！小骚货，尝尝我的厉害吧！！！哈哈哈啊哈哈哈！！！”[p]
“乖乖被我吃掉吧！没有人能逃过欲望魔王的掌控！”[r]
说着，方丈把大屌插进了你的体内。[p]
#
“嗯……啊嗯……啊啊……”[r]
你已全身无力，你本想要呼救，发出的声音却是淫荡的叫声……[p]
#方丈
“嘻嘻嘻！小骚货！看我不干死你！”[r]
方丈的巨根在你体内快速抽插。。。[p]
#
你被爆菊到失神。眼前一片漆黑……[p]
[bg  time="3000"  method="crossfade"  storage="001.jpg"  ]
[jump storage="scene3_dead.ks" target=""]

*loli_h
[bg  time="3000"  method="crossfade"  storage="02.jpg"  ]
[wait time="500"]
已经是深夜了……[l][r]
眼前已经伸手不见五指，你在深山之中蹒跚前进。[p]
“呜呜呜。。。”[r]
你不禁瑟瑟发抖，身体变成萝莉之后胆子也变小了么。。[p]
面前却突然跳出来一群大屌壮汉[r]
————是被欲望吞噬的徒弟们！[p]
“呀啊啊！！”[r]
你试图逃跑，却轻易地被他们抓住，尽管奋力挣扎，却因为萝莉娇小柔弱的体型而无法逃脱……[p]
[bg  time="3000"  method="crossfade"  storage="loli1.jpg"  ]
你被几个大汉扒光了衣服。小穴和嘴立刻就被塞上了大屌[p]
“乖乖被我吃掉吧！没有人能逃过欲望魔王的掌控！”[p]
“嗯……唔嗯……唔……”[r]
你已全身无力，你本想要呼救，发出的声音却是淫荡的叫声……[p]
“嘻嘻嘻！小骚货！看我不干死你！”[r]
听到声音的壮汉更加兴奋了。[p]

[bg  time="3000"  method="crossfade"  storage="loli2.jpg"  ]
第二天，上山的绅士们所见到的，是一只满身白浊，双目失神，嘴里呢喃着：“摩多，摩多……”的被玩坏的萝莉。[p]
[bg  time="3000"  method="crossfade"  storage="001.jpg"  ]
[jump storage="scene3_dead.ks" target=""]

*start_no
[bg  time="3000"  method="crossfade"  storage="02.jpg"  ]
[wait time="500"]
来到山上，已经是深夜了……[l][r]
眼前已经伸手不见五指，你在深山之中蹒跚前进。[p]
夜间突降暴雨，少数不幸的绅士从山涧坠落，被召回了庭院之中。[p]
刚才还在后面的萝莉绅士也都不见了踪影。[r]
仅仅第一关就损失了不少的同伴，你不禁担心 能不能到达最后了。。。[l][r]
如果全灭的话会怎么样呢？[p]
“全灭？啊，不会怎么样哦，只是大家会疯狂的互艹一周而已，以男性姿态。”[l][r]
不想以此种方式失贞的你暗下决心一定要完成任务。[p]
坚定地信念促使着你前进，你终于找到了一处山洞，决定躲进去过夜。[p]
呼……[p]
[jump storage="scene3.ks" target="*common"]

*start_yes
[bg  time="3000"  method="crossfade"  storage="寺院内部.jpg"  ]
[wait time="500"]
晚上睡得还不错，一觉醒来的你充满了精神。[r]
昨晚睡觉时好像隐约感到隔壁有什么骚动？[p]
你满怀好奇心的到其他房间看了看……[p]
[bg  time="3000"  method="crossfade"  storage="隔壁.jpg"  ]
……好可怕。[p]
有个幸存的绅士告诉你方丈在侵犯了所有的伪娘和扶她后从寺庙中消失了。[r]
看来就算是远离世俗之人也难逃欲望的污染。。还是说是被激发了压抑的欲望呢？[p]
事不宜迟，你决定立即出发上山，前往下一个目的地。[p]
[bg  time="3000"  method="crossfade"  storage="02.jpg"  ]
不知在山上走了多久，刚刚还日头高挂，这一小会便下起来了大雨。[p]
你走着走着，雨越下越大，你终于找到了一处山洞，决定进去躲雨。[p]
在山洞中，你们与先行一步的绅士们相遇了。[p]

*common
[bg  time="3000"  method="crossfade" wait="true"  storage="第二关.jpg"  ]
[bg  time="3000"  method="crossfade"  storage="1933561923.jpg"  ]
此山不知为何，天气几乎是瞬息万变。[p]
现在已经是中午了，烈日，冰雹，狂风，暴雨在山间永无止息的肆虐着。[r]
如果不能解决这个问题的话，怕是不能翻过这山了。[p]

#???
“你们是什么人？”[p]
#
身后突然响起了人的声音。[p]

[chara_show  name="谜之正太"  time="1000" height="600"  wait="true"  ]
回过身去，那里站着一个外貌如孩童般的人。[p]
#谜之正太
“你们来这里做什么？”[p]
#
你表明了你的身份和来意，他思考了一会，说道：[p]
#谜之正太
“嗯嗯……是来讨伐欲望的魔王的啊。[l][r]
他就在这个山里，这山上瞬息万变的天气也都是受到魔力的影响导致的。”[p]
#谜之正太:smile
正太思考了一会儿，嘴角微微一笑[r]
“我可以帮助你们越过山脉，不过，你要留下来服侍我一晚。”[p]
#
你们面面相觑，似乎唯有屈服一途可选。[r]
可面前的这位正太是否有这个能耐呢？[r]
还是只是虚张声势想揩你点油水的小色狼呢？[p]

[glink  color="black"  storage="scene3.ks"  size="20"  text="选择留下"  x="22"  y="202"  width=""  height=""  exp="sf.data.act3choice='stay'" target="*chosen" ]
[glink  color="black"  storage="scene3.ks"  size="20"  text="叫其他人代替你"  x="343"  y="202"  width=""  height=""  exp="sf.data.act3choice='run'" target="*chosen" ]
[glink  color="black"  storage="scene3.ks"  size="20"  text="先发制人控制住这个小色鬼"  x="626"  y="202"  exp="sf.data.act3choice='attack'"  target="*chosen"  ]
[s]

*chosen
[iscript]
$.ajax({
	url: '/api/Game/Act',
	data: {actname:'Act3Choose', choice:sf.data.act3choice},
	type: 'PUT',
}).fail(function() {
TG.kag.ftag.startTag("jump",{target:"*error", storage:"title_screen.ks"}); 
}).done(function() {
TG.kag.ftag.startTag("jump",{target:"*"+sf.data.act3choice}); 
});
[endscript]
[s]

*afterchoose
[bg  time="3000"  method="crossfade"  storage="1933561923.jpg"  ]
[chara_show  name="谜之正太" face="smile" time="1000" height="600"  wait="true"  ]
[iscript]
TG.kag.ftag.startTag("jump",{target:"*"+sf.data.act3choice}); 
[endscript]

*stay
[wait time="500"]
“真…真没办法呢……”[r]
你自告奋勇的站了出来……[p]
#谜之正太
“你真美丽。”[r]
小男孩笑着说。“而且散发着柔弱的气息。”[p]
#
不知道是期待还是紧张，你的身体不自觉的瑟瑟发抖起来。[p]
小男孩缓缓向你靠近……[p]
[jump  storage="scene3.ks"  target="*end"  ]

*run
[wait time="500"]
“这里的绅士又不止我一个……”[r]
你心中暗想，同时偷偷地走到了人群的后面。[p]
小男孩似乎没有注意到你，缓缓向另一位绅士靠近……[p]
[jump  storage="scene3.ks"  target="*end"  ]

*attack
[wait time="500"]
前面还有欲望魔王要讨伐，怎么能在一个小鬼头面前示弱？[r]
想想过去一天的遭遇，你心中充满了怒火。[p]
“可恶的小色鬼，看老娘不给你点颜色看看！”[r]
你冲了上去，试图按住小正太……[p]

*end
[chara_hide_all]
[bg  time="3000"  method="crossfade"  storage="001.jpg"  ]

[jump  storage="scene4.ks"  target=""  ]
[s]