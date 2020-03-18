
;==============================
; タイトル画面
;==============================
[hidemenubutton]

[tb_show_message_window  ]
读取中...

[bg  storage="bg_101.png" time=500 wait=false ]

[iscript]
function jumpError(e) {
if (e.status == 401) {
	f.error = "请先前往主页进行登录。";
}
TG.kag.ftag.startTag("jump",{target:"*error"}); 
}
$.ajax({
	url: '/api/Game/Data',
	type: 'GET',
	cache: false,
}).done(function(data) {
  sf.data = data;
  f.scene = 0;
  f.startPoint="";
  f.hasProgress = sf.data.progress > 0;

  if (sf.data.progress == 1) {
	f.scene = 1;
	f.startPoint="*choose";
  } else if (sf.data.progress == 2) {
	f.scene = 1;
	f.startPoint="*stats";
  } else if (sf.data.progress == 3) {
	if (sf.data.isdead) {
		f.scene = -2;
	} else {
		f.scene = 2;
		f.startPoint="*start";
	}
  } else if (sf.data.progress == 4) {
	f.scene = 2;
	f.startPoint = "*afterchoose";
  } else if (sf.data.progress == 5) {
	if (sf.data.isdead) {
		f.scene = -3;
	} else {
		f.scene = 3;
		f.startPoint = "*start_" + sf.data.act2choice;
	}
  } else if (sf.data.progress == 6) {
	f.scene = 3;
	f.startPoint = "*afterchoose";
  } else if (sf.data.progress == 7) {
	if (sf.data.isdead) {
		f.scene = -4;
	} else {
		f.scene = 4;
		f.startPoint = "*start_" + sf.data.act3choice;
	}
  } else if (sf.data.progress == 8) {
	f.scene = 4;
	f.startPoint = "*afterchoose";
  } else if (sf.data.progress == 9 || sf.data.progress == 10) {
	if (sf.data.isdead) {
		f.scene = -5;
	} else {
		f.scene = 5;
		if (sf.data.progress == 10 && sf.data.act4choice == "right") {
			f.startPoint = "*start_before_chapter";
		} else {
			f.startPoint = "*start_" + sf.data.act4choice;
		}
	}
  } else if (sf.data.progress == 11) {
	f.scene = 5;
	f.startPoint = "*start_chapter";
  } else if (sf.data.progress == 12) {
	f.scene = 5;
	f.startPoint = "*afterchoose";
  } else if (sf.data.progress == 13) {
	if (sf.data.isdead) {
		f.scene = -6;
	} else {
		f.scene = 6;
		f.startPoint = "*start_gan";
	}
  } else if (sf.data.progress == 14) {
	f.scene = 6;
	f.startPoint = "*afterchoose";
  } else if (sf.data.progress > 14 && sf.data.progress < 23) {
	if (sf.data.isdead) {
		f.scene = -7;
	} else {
		f.scene = 7;
		var startPoints = {
			15: '*start',
			16: '*boss',
			17: '*q1_show',
			18: '*q2_show',
			19: '*q3_show',
			20: '*ending',
			21: '*normal_ending',
			22: '*stay_ending',
		};
		f.startPoint = startPoints[sf.data.progress];
	}
  }
  TG.kag.ftag.startTag("jump",{target:"*title"}); 
}).fail(jumpError);
[endscript]

[s  ]
*title

;標準のメッセージレイヤを非表示
[tb_hide_message_window]
[position layer=message1 width=500 height=65 top=425 left=70]
[layopt layer=message1 visible=true]
[eval exp="tf.msg = '玩家：'+sf.data.nickname"]
[ptext name=title_msg layer=message1 page=fore text=&tf.msg size=30 x=80 y=440 ]
[if exp="f.hasProgress"]
	[glink text="新游戏"  x="605"  y="505"  target="*new"  ]
	[glink text="继续游戏" x="605" y="425" target="*start"]
[else]
[glink  text="开始游戏"  x="605"  y="425"  target="*start"  ]
[endif]
[s  ]

*new
[ptext name=title_msg overwrite=true layer=message1 page=fore text="重新开始将覆盖当前进度，请确认。" size=30 x=80 y=440 ]
[wait time=1000]
[glink text="确认重来"  x="605"  y="505"  target="*confirm"  ]
[glink text="继续游戏" x="605" y="425" target="*start"]
[s]

*confirm
[iscript]
$.ajax({
	url: '/api/Game/Reset',
	type: 'PUT',
	cache: false,
}).done(function(data) {
	sf.data = data;
	f.scene = 0;
	f.startPoint="";
	TG.kag.ftag.startTag("jump",{target:"*start"}); 
}).fail(function (e) {
if (e.status == 401) {
	f.error = "请先前往主页进行登录。";
}
TG.kag.ftag.startTag("jump",{target:"*error"}); 
});
[endscript]
[s]

*start
[layopt layer=message1 visible=false]
[ct]
[showmenubutton]

[cm  ]

[if exp="f.scene==0"]
[jump storage="scene0.ks"]
[elsif exp="f.scene==1"]
[jump storage="scene1.ks"]
[elsif exp="f.scene==2"]
[jump storage="scene2.ks"]
[elsif exp="f.scene==3"]
[jump storage="scene3.ks"]
[elsif exp="f.scene==4"]
[jump storage="scene4.ks"]
[elsif exp="f.scene==5"]
[jump storage="scene5.ks"]
[elsif exp="f.scene==6"]
[jump storage="scene5b.ks"]
[elsif exp="f.scene==7"]
[jump storage="scene6.ks"]
[elsif exp="f.scene==-2"]
[jump storage="scene2_dead.ks"]
[elsif exp="f.scene==-3"]
[jump storage="scene3_dead.ks"]
[elsif exp="f.scene==-4"]
[jump storage="scene4_dead.ks"]
[elsif exp="f.scene==-5"]
[jump storage="scene5_dead.ks"]
[elsif exp="f.scene==-6"]
[jump storage="scene5b_dead.ks"]
[elsif exp="f.scene==-7"]
[jump storage="be.ks"]
[endif]

*error
[gm_show_error]
[s  ]
