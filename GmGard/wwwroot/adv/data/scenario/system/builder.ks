;ビルダーでシナリオごとに必ず呼び出されるシステム系のKSファイル

;メッセージウィンドウを非表示にする
[macro name="tb_show_message_window"]
	[layopt  layer="message0"  visible="true"  ]
	[layopt  layer="fixlayer"  visible="true"  ]
[endmacro]

;メッセージウィンドウを表示する
[macro name="tb_hide_message_window"]
	[layopt  layer="message0"  visible="false"  ]
	[layopt  layer="fixlayer"  visible="false"  ]
[endmacro]

[macro name="_tb_system_call"]
	[call storage=%storage ]
[endmacro]

[macro name="tb_image_show"]
	[image storage=%storage layer=1 page=fore visible=true y=%y x=%x width=%width height=%height time=%time ]	
[endmacro]
	
[macro name="tb_image_hide"]
	[freeimage layer=1 page=fore time=%time]	
[endmacro]

[macro name="tb_eval"]
	[eval exp=%exp ]	
[endmacro]


;生ティラノ用のマーカー
[macro name="tb_start_tyrano_code"]
[endmacro]

[macro name="_tb_end_tyrano_code"]
[endmacro]

[macro name="lr"]
[l][r]
[endmacro]

[macro name="gm_show_error"]
[tb_image_show  time="1000"  storage="default/error.jpg"  width="700"  height="400" x=125  ]
[tb_show_message_window  ]
[cm]
连接服务器出错了。
[if exp=f.error]
[emb exp="f.error"]
[endif]
[endmacro]

[macro name="jump_to_startpoint"]
[if exp="f.startPoint"]
[eval exp="tf.startPoint=f.startPoint"]
[eval exp="f.startPoint=''"]
[jump target=&tf.startPoint storage=%storage]
[endif]
[endmacro]