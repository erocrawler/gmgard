function HANABI() {
    if (this == window) {
        return new HANABI();
    }
    var h = this;
    function Color(r,g,b,a) {
        this.r = r;
        this.b = b;
        this.g = g;
        this.a = a || 1;
        this.toString = function (a) {
            return 'rgba(' + [this.r, this.g, this.b, a || this.a].join(',') + ')';
        }
    }
    h.colors = [new Color(0xba, 0x14, 0x14), new Color(0xeb, 0x97, 0x06), new Color(0xff, 0xff, 0)];

    h.fire = function () {
        function getRandomColor() {
            return h.colors[Math.floor(Math.random() * h.colors.length)];
        }
        var hanabi = {
            // 火花の数
            'quantity': 128,
            // 火花の大きさ
            'size': 3,

            // 減衰力（花火自体の大きさに影響
            'circle': 0.98,

            // 重力
            'gravity': 1.1,
            // 火花の速度
            'speed': 5,

            // 爆発縦位置
            'top': 3,
            // 爆発横位置
            'left': 2,

            // 花火の色。cssと同じ形式で指定可能（rgba(200, 200, 200, 0.5)形式も可能）。'random'でランダム色
            'color': getRandomColor()
        };

        // 以下花火の制御コードです

        Math.Radian = Math.PI * 2;
        var hibana = [/*{
    'pos_x' : left,
    'pos_y' : top,
    'vel_x' : Math.cos(angle) * speed,
    'vel_y' : Math.sin(angle) * speed
  }, ...*/];
        var cvs = {
            // canvas element
            'elem': undefined,
            // canvas width(window max)
            'width': 0,
            // canvas width(window height)
            'height': 0,
            // 2d context
            'ctx': undefined,
            // element offset(left)
            'left': 0,
            // element offset(top)
            'top': 0,
            // explode point(x)
            'pos_x': 0,
            // explode point(y)
            'pos_y': 0
        };

        setTimeout(function () {
            var body = document.getElementById('body');
            var rect = body.getBoundingClientRect();
            var range_y = [100, window.innerHeight - 100];
            cvs.pos_y = Math.floor(Math.random() * (range_y[1] - range_y[0] + 1) + range_y[0]);
            var range_x = [10, window.innerWidth - 10];
            if (cvs.pos_y >= rect.top) {
                range_x = [[0, rect.left], [rect.right, window.innerWidth]][Math.round(Math.random())];
            }
            cvs.pos_x = Math.floor(Math.random() * (range_x[1] - range_x[0] + 1) + range_x[0]);
            for (var i = 0; i < hanabi.quantity; ++i) {
                var angle = Math.random() * Math.Radian;
                var speed = Math.random() * hanabi.speed;
                hibana.push({
                    'pos_x': cvs.pos_x,
                    'pos_y': cvs.pos_y,
                    'vel_x': Math.cos(angle) * speed,
                    'vel_y': Math.sin(angle) * speed,
                    'tail': []
                });
            };
            requestAnimationFrame(render);
        }, 0);

        function clone_point(p) {
            return {
                'pos_x': p.pos_x,
                'pos_y': p.pos_y,
                'vel_x': p.vel_x,
                'vel_y': p.vel_y,
                'tail': p.tail.slice()
            }
        }

        var frame = 0;
        function render() {
            if (!hibana.length) {
                return;
            };
            cvs.ctx.clearRect(0, 0, cvs.width, cvs.height);
            var clear = 0;
            frame++;
            for (var i = 0, len = hibana.length; i < len; i++) {
                var s = hibana[i];
                s.tail.push(clone_point(s));
                if (s.tail.length > 3) {
                    s.tail.shift();
                }
                s.pos_x += s.vel_x;
                s.pos_y += s.vel_y;
                s.vel_x *= hanabi.circle;
                s.vel_y *= hanabi.circle;
                s.pos_y += hanabi.gravity;
                if (hanabi.size < 0.1 || !s.pos_x || !s.pos_y || s.pos_x > cvs.width || s.pos_y > cvs.height) {
                    hibana[i] = undefined;
                    if (len < ++clear) {
                        try { window.parent.endAnimation(location.href); } catch (e) { };
                    };
                    return;
                };
                cvs.ctx.fillStyle = hanabi.color.toString();
                cvs.ctx.beginPath();
                cvs.ctx.arc(s.pos_x, s.pos_y, hanabi.size, 0, Math.Radian, true);
                cvs.ctx.fill();
                for (var j = 0; j < s.tail.length; j++) {
                    var t = s.tail[j];
                    cvs.ctx.fillStyle = hanabi.color.toString(Math.pow(.7, s.tail.length - j));
                    cvs.ctx.beginPath();
                    cvs.ctx.arc(t.pos_x, t.pos_y, Math.pow(hanabi.circle, (s.tail.length - j)*2) * hanabi.size, 0, Math.Radian, true);
                    cvs.ctx.fill();
                }
            
            };
            hanabi.size *= hanabi.circle;
            cvs.ctx.fillStyle = 'rgba(0, 0, 0, 0.3)';
            requestAnimationFrame(render);
        }

        cvs.elem = document.getElementById('canvas');
        if (!cvs.elem || !cvs.elem.getContext) {
            return; //alert('require canvas support');
        };
        cvs.elem.height = cvs.height = window.screen.availHeight;
        cvs.elem.width = cvs.width = window.screen.availWidth;
        cvs.ctx = cvs.elem.getContext('2d');
        cvs.left = cvs.elem.getBoundingClientRect
            ? cvs.elem.getBoundingClientRect().left
            : 0
        ;
        cvs.top = cvs.elem.getBoundingClientRect
            ? cvs.elem.getBoundingClientRect().top
            : 0
        ;
    };
    h.stop = function () { };
    h.start = function () {
        h.fire();
        var handle = window.setInterval(h.fire, 3000);
        h.stop = function () {
            window.clearInterval(handle);
        }
    };
    h.start();
}
