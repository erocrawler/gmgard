function FLAME(id, W, H) {
    if (this == window) {
        return new FLAME(id, W, H);
    }
    var self = this;
    this.run = false;
    this.start = function () {
        this.run = true;
        var canvas = document.getElementById(id);
        var ctx = canvas.getContext("2d");

        //Make the canvas occupy the full page
        //var W = window.innerWidth, H = window.innerHeight;
        canvas.width = W;
        canvas.height = H;

        var particles = [];
        var mouse = {};

        //Lets create some particles now
        var particle_count = 50;
        for (var i = 0; i < particle_count; i++) {
            particles.push(new particle());
        }

        //finally some mouse tracking
        //canvas.addEventListener('mousemove', track_mouse, false);

        //function track_mouse(e) {
        //    //since the canvas = full page the position of the mouse 
        //    //relative to the document will suffice
        //    mouse.x = e.pageX;
        //    mouse.y = e.pageY;
        //}

        function particle() {
            //speed, life, location, life, colors
            //speed.x range = -2.5 to 2.5 
            //speed.y range = -15 to -5 to make it move upwards
            //lets change the Y speed to make it look like a flame
            this.speed = { x: -4 + Math.random() * 2, y: -6 + Math.random() * 4 };
            //location = mouse coordinates
            //Now the flame follows the mouse coordinates
            //if (mouse.x && mouse.y) {
            //    this.location = { x: mouse.x, y: mouse.y };
            //}
            //else {
                this.location = { x: W - 30, y: H -30 };
            //}
            //radius range = 10-30
            this.radius = 8 + Math.random() * 20;
            //life range = 20-30
            this.life = 20 + Math.random() * 4;
            this.remaining_life = this.life;
            //colors
            this.r = 0;//Math.round(Math.random()*255);
            this.g = Math.round(10 + Math.random() * 50);
            this.b = Math.round(150 + Math.random() * 105);
        }

        function resetParticle(particle) {
            particle.speed = { x: -4 + Math.random() * 2, y: -6 + Math.random() * 4 };
            //location = mouse coordinates
            //Now the flame follows the mouse coordinates
            //if (mouse.x && mouse.y) {
            //    particle.location = { x: mouse.x, y: mouse.y };
            //}
            //else {
                particle.location = { x: W - 30, y: H - 30 };
            //}
            //radius range = 10-30
            particle.radius = 8 + Math.random() * 20;
            //life range = 20-30
            particle.life = 20 + Math.random() * 4;
            particle.remaining_life = particle.life;
            //colors
            particle.r = 0;//Math.round(Math.random()*255);
            particle.g = Math.round(10 + Math.random() * 50);
            particle.b = Math.round(150 + Math.random() * 105);
        }

        function draw() {
            //Painting the canvas black
            //Time for lighting magic
            //particles are painted with "lighter"
            //In the next frame the background is painted normally without blending to the 
            //previous frame
            ctx.clearRect(0, 0, W, H);
            //ctx.globalCompositeOperation = "source-over";
            //ctx.fillStyle = "transparent";
            //ctx.fillRect(0, 0, W, H);
            ctx.globalCompositeOperation = "lighter";

            for (var i = 0; i < particle_count; i++) {
                var p = particles[i];
                if (p.remaining_life < 0 || p.radius < 0){
                    if (self.run == false)
                        continue;
                    else {
                        resetParticle(p);
                    }
                }
                ctx.beginPath();
                //changing opacity according to the life.
                //opacity goes to 0 at the end of life of a particle
                p.opacity = Math.round(p.remaining_life / p.life * 100) / 100
                //a gradient instead of white fill
                var gradient = ctx.createRadialGradient(p.location.x, p.location.y, 0, p.location.x, p.location.y, p.radius);
                gradient.addColorStop(0, "rgba(" + p.r + ", " + p.g + ", " + p.b + ", " + p.opacity + ")");
                gradient.addColorStop(0.5, "rgba(" + p.r + ", " + p.g + ", " + p.b + ", " + p.opacity + ")");
                gradient.addColorStop(1, "rgba(" + p.r + ", " + p.g + ", " + p.b + ", 0)");
                ctx.fillStyle = gradient;
                ctx.arc(p.location.x, p.location.y, p.radius, Math.PI * 2, false);
                ctx.fill();

                //lets move the particles
                p.remaining_life--;
                p.radius-=1;
                p.location.x += p.speed.x;
                p.location.y += p.speed.y;

                //regenerate particles
                if (self.run == true && (p.remaining_life < 0 || p.radius < 0)) {
                    resetParticle(p);
                }
            }
            requestAnimationFrame(draw);
        }

        draw();
    }
}