function SNOW() {
    if (this == window) {
        return new SNOW();
    }
    var s = this;
    var flakes = [],
        stopped = false,
        canvas = document.getElementById("canvas"),
        ctx = canvas.getContext("2d"),
        flakeCount = 200,
        mX = -100,
        mY = -100;
    
    function snow() {
        ctx.clearRect(0, 0, canvas.width, canvas.height);

        for (var i = 0; i < flakeCount; i++) {
            var flake = flakes[i],
                x = mX,
                y = mY,
                minDist = 150,
                x2 = flake.x,
                y2 = flake.y;

            var dist = Math.sqrt((x2 - x) * (x2 - x) + (y2 - y) * (y2 - y)),
                dx = x2 - x,
                dy = y2 - y;

            if (dist < minDist) {
                var force = minDist / (dist * dist),
                    xcomp = (x - x2) / dist,
                    ycomp = (y - y2) / dist,
                    deltaV = force / 2;

                flake.velX -= deltaV * xcomp;
                flake.velY -= deltaV * ycomp;

            } else {
                flake.velX *= .98;
                if (flake.velY <= flake.speed) {
                    flake.velY = flake.speed
                }
                flake.velX += Math.cos(flake.step += .05) * flake.stepSize;
            }

            ctx.fillStyle = "rgba(255,255,255," + flake.opacity + ")";
            flake.y += flake.velY;
            flake.x += flake.velX;

            if (flake.y >= canvas.height || flake.y <= 0) {
                reset(flake);
            }


            if (flake.x >= canvas.width || flake.x <= 0) {
                reset(flake);
            }
            var body = document.getElementById('body');
            var rect = body.getBoundingClientRect();

            if (flake.x >= rect.left && flake.x <= rect.right && flake.y >= rect.top) {
                if (rect.top <= 0) {
                    reset(flake, rect);
                }
                else
                    reset(flake);
            }

            ctx.beginPath();
            ctx.arc(flake.x, flake.y, flake.size, 0, Math.PI * 2);
            ctx.fill();
        }
        if (stopped) {
            return;
        }
        requestAnimationFrame(snow);
    }
    function reset(flake, avoid) {
        if (avoid) {
            var range = [[0, avoid.left], [avoid.right, window.innerWidth]];
            var r = range[Math.round(Math.random())];
            flake.x = Math.floor(Math.random() * (r[1] - r[0] + 1) + r[0]);
            if (r[0][0] >= r[1][1])
                flake.opacity = 0;
        }
        else {
            flake.x = Math.floor(Math.random() * canvas.width);
            flake.opacity = (Math.random() * 0.5) + 0.3;
        }
        flake.y = 0;
        flake.size = (Math.random() * 3) + 2;
        flake.speed = (Math.random() * 1) + 0.5;
        flake.velY = flake.speed;
        flake.velX = 0;

    }

    function init() {
        stopped = false;
        for (var i = 0; i < flakeCount; i++) {
            var x = Math.floor(Math.random() * window.innerWidth),
                y = Math.floor(Math.random() * window.innerHeight),
                size = (Math.random() * 3) + 2,
                speed = (Math.random() * 1) + 0.5,
                opacity = (Math.random() * 0.5) + 0.3;

            flakes.push({
                speed: speed,
                velY: speed,
                velX: 0,
                x: x,
                y: y,
                size: size,
                stepSize: (Math.random()) / 30,
                step: 0,
                angle: 180,
                opacity: opacity
            });
        }

        snow();
    }


    canvas.width = window.screen.availWidth;
    canvas.height = window.screen.availHeight;
    canvas.addEventListener("mousemove", function (e) {
        mX = e.clientX,
        mY = e.clientY
    });
    init();
    s.init = init;
    s.stop = function () {
        stopped = true;
    }
    s.start = function () {
        stopped = false;
        snow();
    }
}