

;メニューボタン非表示
;[hidemenubutton]


;ゲームに必要なライブラリ読み込み
[call storage="system/tyrano.ks"]
[call storage="system/builder.ks"]
[call storage="system/novel_booster.ks"]
[call storage="system/chara_define.ks"]

;live2D対応



[layopt layer="message0" visible=false]
[call storage="system/message_window.ks"]

;タイトルの設定
;[title name="now loading"]

;タイトル画面表示
[jump storage="title_screen.ks"]

;--------------------------

[s]




