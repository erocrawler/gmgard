[_tb_system_call storage=system/_checkrevive.ks]

[if exp="sf.data.deathcount == 3"]
[jump target="*choose"]
[endif]

[bg  storage="bg00.jpg"  time="1000"  method="crossfade"  ]
[tb_show_message_window  ]
[chara_face name="Duo" face="cry" storage="chara/0/cry.jpg"]
[chara_face name="Duo" face="lollipop" storage="chara/0/lollipop.jpg"]
#
你眼前一黑，被传送到了一个昏暗的房间里。[p]
[layopt layer=2 visible=true opacity=127] 
[chara_show layer=2  name="Duo" top=250 left=420]
[if exp="sf.data.deathcount <= 1"]
#Duo
哎呦，这么巧，你也死了呀？[p]
我？嗯……之前服务器不是着火了么。[p]
#Duo:cry
在救火过程中我一不小心……[p]
#
说着，Duo哽咽了起来。
[else]
#Duo
。。。你又来了啊？[r]
这是你第[emb exp="sf.data.deathcount"]次来了已经……[p]
[endif]

[glink  color="black"  size="20"  text="比起这个来，还是复活我要紧"  x="261"  y="226"  width=""  height=""  target="*nolisten"  ]
[glink  color="black"  size="20"  text="不要听我是怎么死的么？"  x="271"  y="286"  width=""  height=""  target="*listen"  ]
[s]
*listen
#Duo:cry
嗯嗯？居然被一个正太战翻了？什么激霸世道……[p]
这可不是我设计的，要怪就等复活了去找L娘算账吧，都是他出的鬼主意。。。[p]
#Duo:default
[jump target="*common"]
*nolisten
#Duo:default
嗯嗯，对对。被正太战翻了这种事情还是不要提了……[p]

*common
这次要是复活了，还是别去正面肛这种莫名其妙的正太了，找个其他人代替你好了。[p]
神说了，你还不能在这里死去。[p]
交出你的棒棒糖，我就可以让你复活。[r]
顺便可以帮你更改职业。[p]
不交出450，谁也保不了你！[p]
*choose
[_tb_system_call storage=system/_revive.ks]


*finish
[wait  time="1000"  ]
#Duo:lollipop
好，你的棒棒糖我就收下了。[p]
[if exp="sf.data.deathcount == 2"]
有件事必须告诉你：[r]
复活是我瞒着L娘悄悄做的。千万别告诉她！被她发现就惨了！[p]
[elsif exp="sf.data.deathcount > 2"]

[endif]
祝你这次好运。那么我们后会有期啦~[p]
#
[chara_hide layer=2 name="Duo" time="1000"  wait="true"  ]

[jump  storage="scene4.ks"  target="*start_run"  ]
[s]