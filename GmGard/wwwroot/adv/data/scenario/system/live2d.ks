;============================================================
; Live2D x tyranoscript対応マクロ
;============================================================
; ライブラリ読込
[loadjs storage = "live2d/lib/live2d.min.js"]
[loadjs storage = "live2d/framework/Live2DFramework.js"]
[loadjs storage = "live2d/framework/PlatformManager.js"]
[loadjs storage = "live2d/framework/LAppLive2DManager.js"]
[loadjs storage = "live2d/Live2Dmodel.js"]
[loadjs storage = "live2d/Live2Dtyrano.js"]

[iscript]
;live2Dが初めての場合、初期化する
if(f.live2d_models == undefined){
    f.live2d_models = {};
}
[endscript]


;------------------------------------------------------------
; キャンバスとLive2Dモデル生成
;------------------------------------------------------------
[macro name = "live2d_new"]
;*パラメータ1  : name         【必須】Live2DモデルID(一意なもの)
; パラメータ2  : left         Live2Dモデルの横位置(Canvasの横位置)
; パラメータ3  : top          Live2Dモデルの縦位置(Canvasの縦位置)
; パラメータ4  : width        Live2Dモデルの横幅(Canvasの横幅)
; パラメータ5  : height       Live2Dモデルの縦幅(Canvasの縦幅)
; パラメータ6  : zindex       Live2Dモデルの奥行き(Canvasの奥行き)
; パラメータ7  : opacity      Live2Dモデルの透明度（0.0〜1.0）
; ぱらめーた　　can_visible Live2Dモデルの表示、非表示制御
; パラメータ8  : glleft       Canvas内のLive2Dモデル横位置(0.0〜2.0ぐらい)
; パラメータ9  : gltop        Canvas内のLive2Dモデル縦位置(0.0〜2.0ぐらい)
; パラメータ10 : glscale      Canvas内のLive2Dモデル拡大縮小サイズ(0.0〜2.0ぐらい)
[iscript]
; optinal
if(mp.name ==null)console.error('nameは必須です');
if(mp.left == null)mp.left = 0;
if(mp.top == null)mp.top = 0;
if(mp.width == null)mp.width = TYRANO.kag.config.scWidth;
if(mp.height == null)mp.height = TYRANO.kag.config.scWidth;
if(mp.zindex == null)mp.zindex = 12;
if(mp.opacity == null)mp.opacity = 0.0;
if(mp.can_visible == null)mp.can_visible = false;
if(mp.glleft == null)mp.glleft = 0.0;
if(mp.gltop == null)mp.gltop = 0.0;
if(mp.glscale == null)mp.glscale = 1.0;

; Live2DのCanvas追加する親ID
parentID = 'tyrano_base';
; Canvasを生成し、Live2Dモデルを透明で表示[Live2Dtyrano.js]
live2d_new(
    LIVE2D_MODEL[mp.name],
    mp.name,
    mp.left,
    mp.top,
    mp.width,
    mp.height,
    mp.zindex,
    mp.opacity,
    mp.can_visible,
    Number(mp.glleft),
    Number(mp.gltop),
    Number(mp.glscale),
    parentID);
[endscript]
[endmacro]


;------------------------------------------------------------
; Live2Dキャラの表示
;------------------------------------------------------------
[macro name = "live2d_show"]
;*パラメータ1  : name         【必須】Live2DモデルID(一意なもの)
;*パラメータ2  : time                切り替え時間
;*パラメータ3  : left                横位置
;*パラメータ4  : top                縦位置
;*パラメータ5  : scale                比率


[iscript]
; optinal
if(mp.name ==null)console.error('nameは必須です');
if(mp.time == null)mp.time = 100;
if(mp.left == null)mp.left = 0;
if(mp.top == null)mp.top = 0;
if(mp.scale == null)mp.scale = 1;

; Live2Dモデルの表示[Live2Dtyrano.js]
live2d_show(mp.name, mp.time,mp.left,mp.top,mp.scale);
[endscript]
[endmacro]


;------------------------------------------------------------
; Live2Dキャラの非表示
;------------------------------------------------------------
[macro name = "live2d_hide"]
;*パラメータ1  : name         【必須】Live2DモデルID(一意なもの)
;*パラメータ2  : time                切り替え時間
[iscript]
; optinal
if(mp.name ==null)console.error('nameは必須です');
if(mp.time == null)mp.time = 100;

; Live2Dモデルの非表示[Live2Dtyrano.js]
live2d_hide(mp.name, mp.time);
[endscript]
[endmacro]


;------------------------------------------------------------
; Live2Dキャラの透明度
;------------------------------------------------------------
[macro name = "live2d_opacity"]
;*パラメータ1  : name         【必須】Live2DモデルID(一意なもの)
;*パラメータ2  : opacity      【必須】透明度(0.0～1.0)
;*パラメータ3  : time                切り替え時間
[iscript]
; optinal
if(mp.name ==null)console.error('nameは必須です');
if(mp.opacity ==null)console.error('opacityは必須です');
if(mp.time == null)mp.time = 100;

; Live2Dモデルの透明度[Live2Dtyrano.js]
live2d_opacity(mp.name, mp.opacity, mp.time);
[endscript]
[endmacro]


;------------------------------------------------------------
; Live2Dキャラの退場
;------------------------------------------------------------
[macro name = "live2d_delete"]
;*パラメータ1  : name         【必須】Live2DモデルID(一意なもの)
[iscript]
; optinal
if(mp.name ==null)console.error('nameは必須です');

; Live2DのCanvas追加する親ID
parentID = 'tyrano_base';
; Live2Dモデル表示[Live2Dtyrano.js]
live2d_delete(mp.name, parentID);
[endscript]
[endmacro]


;------------------------------------------------------------
; Live2Dキャラのモーション再生
;------------------------------------------------------------
[macro name = "live2d_motion"]
;*パラメータ1  : name         【必須】Live2DモデルID(一意なもの)
;*パラメータ2  : filenm              Live2Dモーションファイル名
; パラメータ3  : idle                アイドリング有無
[iscript]
; optinal
if(mp.name ==null)console.error('nameは必須です');
;モーション番号を指定しない場合、モーションストップ
if(mp.filenm == null)mp.filenm = '';
if(mp.idle == null)mp.idle = '';
; Live2Dモデルのモーション再生[Live2Dtyrano.js]
Live2Dcanvas[mp.name].motionChange(mp.filenm, mp.idle);
[endscript]
[endmacro]


;------------------------------------------------------------
; Live2Dキャラの移動
;------------------------------------------------------------
[macro name = "live2d_trans"]
;*パラメータ1  : name         【必須】Live2DモデルID(一意なもの)
;*パラメータ2  : left         【必須】X位置
;*パラメータ3  : top          【必須】Y位置
;*パラメータ4  : time                切り替え時間
[iscript]
; optinal
if(mp.name ==null)console.error('nameは必須です');
if(mp.left ==null)console.error('leftは必須です');
if(mp.top ==null)console.error('topは必須です');
if(mp.time == null)mp.time = 100;
; Live2Dモデルの移動[Live2Dtyrano.js]
Live2Dcanvas[mp.name].transChange(mp.left, mp.top, mp.time);
[endscript]
[endmacro]


;------------------------------------------------------------
; Live2Dキャラの回転
;------------------------------------------------------------
[macro name = "live2d_rotate"]
;*パラメータ1  : name         【必須】Live2DモデルID(一意なもの)
;*パラメータ2  : rotate       【必須】回転角度
;*パラメータ3  : time                切り替え時間
[iscript]
; optinal
if(mp.name ==null)console.error('nameは必須です');
if(mp.rotate ==null)console.error('rotateは必須です');
if(mp.time == null)mp.time = 100;
; Live2Dモデルの回転[Live2Dtyrano.js]
Live2Dcanvas[mp.name].rotateChange(mp.rotate, mp.time);
[endscript]
[endmacro]


;------------------------------------------------------------
; Live2Dキャラの拡大・縮小
;------------------------------------------------------------
[macro name = "live2d_scale"]
;*パラメータ1  : name         【必須】Live2DモデルID(一意なもの)
;*パラメータ2  : scaleX       【必須】Xスケール
;*パラメータ3  : scaleY       【必須】Yスケール
;*パラメータ4  : time                切り替え時間
[iscript]
; optinal
if(mp.name ==null)console.error('nameは必須です');
if(mp.scaleX ==null)console.error('scaleXは必須です');
if(mp.scaleY ==null)console.error('scaleYは必須です');
if(mp.time == null)mp.time = 100;
; Live2Dモデルの移動[Live2Dtyrano.js]
Live2Dcanvas[mp.name].scaleChange(mp.scaleX, mp.scaleY, mp.time);
[endscript]
[endmacro]


;------------------------------------------------------------
; Live2Dキャラのシェイク
;------------------------------------------------------------
[macro name = "live2d_shake"]
;*パラメータ1  : name         【必須】Live2DモデルID(一意なもの)
[iscript]
; optinal
if(mp.name ==null)console.error('nameは必須です');
; キャラクターを揺らす[Live2Dtyrano.js]
Live2Dcanvas[mp.name].vibration();
[endscript]
[endmacro]


;------------------------------------------------------------
; Live2Dモデルの復元（セーブ対応）
;------------------------------------------------------------
[macro name = "live2d_restore"]
[iscript]
var live2d_models = TG.stat.f.live2d_models;
for (var name in live2d_models){
    var model = live2d_models[name];
    
    ; live2D を作成する
    live2d_new(
        model["model_def"],
        model["model_id"],
        model["can_left"],
        model["can_top"],
        model["can_width"],
        model["can_height"],
        model["can_zindex"],
        model["can_opacity"],
        model["can_visible"],
        model["gl_left"],
        model["gl_top"],
        model["gl_scale"],
        model["paraent_id"]
    );
    
    
    
}
[endscript]
[endmacro]
