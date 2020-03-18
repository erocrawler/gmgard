[_tb_system_call storage=system/_checkrevive.ks]

[if exp="sf.data.deathcount < 3"]
[_tb_system_call storage=system/_lastrevive.ks]
[endif]

*choose
[_tb_system_call storage=system/_revive.ks]

*finish
[wait  time="1000"  ]
#Duo:lollipop
好，你的棒棒糖我就收下了。[p]
我就用这最后的棒棒糖能量把你送回正确的时间线吧。[r]
别忘了你最开始的目的：干爆L娘。[p]
#
[chara_hide layer=2 name="Duo" time="1000"  wait="false"  ]
[quake  time="1000"  count="6"  hmax="10"  wait="true"  ]
[wait time=500]
Duo的形体逐渐消失了，房间也在崩塌[p]
#Duo
我只能帮你到这里了，剩下的就靠你自己了，去直面L娘，直面欲望的魔王吧！[p]
#
[bg  time="3000"  method="crossfade"  storage="001.jpg"  ]
“我的目标是，干爆L娘——”[p]

[jump  storage="scene5b.ks"  target="*start_gan"  ]
[s]