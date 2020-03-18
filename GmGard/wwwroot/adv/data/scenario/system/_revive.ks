[glink  color="black"  size="20"  text="萝莉"  x="80"  y="222"  width=""  height=""  exp="f.choice='萝莉'" target='*chosen' ]
[glink  color="black"  size="20"  text="御姐"  x="255"  y="222"  width=""  height=""  exp="f.choice='御姐'" target='*chosen' ]
[glink  color="black"  size="20"  text="扶她"  x="430"  y="222"  width=""  height=""  exp="f.choice='扶她'" target='*chosen' ]
[glink  color="black"  size="20"  text="伪娘"  x="605"  y="222"  width=""  height=""  exp="f.choice='伪娘'" target='*chosen' ]
请选择职业。
[s]

*chosen
[iscript]
function jumpError() {
TG.kag.ftag.startTag("jump",{target:"*error", storage:"title_screen.ks"}); 
}
$.post('/api/Game/Revive', { choice: f.choice }, function(result) {
  f.result = result;
  sf.data.isdead = false;
  sf.data.profession = f.choice;
  TG.kag.ftag.startTag("jump",{target:"*finish"});
}).fail(jumpError);
[endscript]
[s]

*finish
[return]