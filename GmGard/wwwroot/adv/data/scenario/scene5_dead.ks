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
#Duo:cry
那你呢？[p]
[else]
#Duo
欢迎回来~[r]
这是你第[emb exp="sf.data.deathcount"]次来了，这次你是怎么死的？[p]
[endif]

[glink  color="black"  size="20"  text="被一个扶她莫名其妙的从菊花顶到了喉咙……"  x="262"  y="186"  width=""  height=""  target="*fuck"  ]
[glink  color="black"  size="20"  text="被做成了淫虫的苗床……"  x="348"  y="246"  width=""  height=""  target="*bug"  ]
[glink  color="black"  size="20"  text="重要的东西全都不见了……"  x="338"  y="306"  width=""  height=""  target="*gone"  ]
[s]
*fuck
#Duo:cry
哦，我的老天。[p]
扶她可不是我的菜，该死的L娘搞得都是什么激霸。。。[p]
如果能打到最后一定要干死她！[p]
[jump target="*common"]
*bug
#Duo:default
噫……听起来好像还有点小兴奋呢……[p]
可是为什么会变成这样呢？明明是作为一名绅士前去冒险的……[p]
[jump target="*common"]
*gone
#Duo:cry
呜呜，可以理解……[p]
不过别担心，只要复活，你就又是一个生龙活虎的[emb exp="sf.data.profession"]了！[p]

*common
#Duo:default
我听L娘说右面的传送门是给强者前进的，左面反之。[r]
而强弱是根据凸度决定的……[p]
神说了，你还不能在这里死去。[p]
交出你的棒棒糖，我就可以让你复活，并把你送到正确的传送门中。[r]
顺便可以帮你更改职业。[p]
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
祝你这次好运。那么我们后会有期啦~[p]
#
[chara_hide layer=2 name="Duo" time="1000"  wait="true"  ]

[jump  storage="scene5.ks"  target="*start_chapter"  ]
[s]