[bg  storage="bg00.jpg"  time="1000"  method="crossfade"  ]
[tb_show_message_window  ]
[chara_face name="Duo" face="cry" storage="chara/0/cry.jpg"]
[chara_face name="Duo" face="lollipop" storage="chara/0/lollipop.jpg"]
#
你眼前一黑，被传送到了一个昏暗的房间里。[p]
[layopt layer=2 visible=true opacity=127] 
[chara_show layer=2  name="Duo" top=250 left=420]
#Duo
哎呦，这么巧，你也死了呀？[p]
我？嗯……之前服务器不是着火了么。[p]
#Duo:cry
在救火过程中我一不小心……[p]
#
说着，Duo哽咽了起来。[p]
#Duo
事到如今，只有棒棒糖可以安慰我的灵魂了。[p]
#Duo:default
神说了，你还不能在这里死去。[p]
交出你的棒棒糖，我就可以让你复活。[r]
顺便可以帮你更改职业。[p]
之前忘了说了，千万别选白学家，会被打死的！[p]
嗯？已经知道了？这tm就尴尬了。。。[p]
*choose
[_tb_system_call storage=system/_revive.ks]

*finish
[wait  time="1000"  ]
#Duo:lollipop
好，你的棒棒糖我就收下了。[p]
祝你这次好运。那么我们后会有期啦~[p]
#
[chara_hide layer=2 name="Duo" time="1000"  wait="true"  ]

[jump  storage="scene2.ks"  target="*start"  ]
[s]