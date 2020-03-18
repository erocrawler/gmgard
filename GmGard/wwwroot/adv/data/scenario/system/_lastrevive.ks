[bg  storage="bg01.jpg"  time="1000"  method="crossfade"  ]
[tb_show_message_window  ]
[chara_face name="Duo" face="cry" storage="chara/0/cry.jpg"]
[chara_face name="Duo" face="lollipop" storage="chara/0/lollipop.jpg"]
#
你眼前一黑，被传送到了一个昏暗的房间里。[p]
房间似乎勉强维持着形体，墙似乎都是透明的，背后的虚空若隐若现。[p]
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
都过了这么久了没想到你头一次来到这里。。不过。。[r]
[else]
#Duo
欢迎回来~[r]
这是你第[emb exp="sf.data.deathcount"]次来了。[p]
#Duo:cry
[endif]
这可能也是最后一次了。。。[p]

[glink  color="black"  size="20"  text="这个房间是怎么回事？"  x="348"  y="186"  width=""  height=""  target="*next"  ]
[glink  color="black"  size="20"  text="为什么说是最后一次？"  x="348"  y="246"  width=""  height=""  target="*next"  ]
[s]
*next
#Duo:cry
如你所见，这个房间正在消失……[p]
事实上这个房间本身并不在L娘的剧本里，是我临时搭建的。[p]

[chara_hide layer=2 name="Duo" time="1000"  wait="true"  ]
[tb_image_show  time="1000"  storage="default/bg_02_e.jpg"  width="1200"  height="746"  ]
那场大火中，我被砸下来的服务器架压住无法动弹，只得拼尽最后的力量，利用服务器资源创建了这个房间。[p]
[tb_image_hide  time="1000"  ]
[chara_show layer=2 face="lollipop" name="Duo" top=250 left=420]
#Duo
虽然我一直在通过棒棒糖能量维持这里的形态，但是这里似乎被L娘发现了。。。[r]
他正在试图删除这里，我无法阻拦。[p]

时间宝贵，我们还是先来帮你复活吧。[p]
神说了，你还不能在这里死去。[r]
交出你的棒棒糖，我就可以让你复活。[r]
顺便可以帮你更改职业。[p]

[return]