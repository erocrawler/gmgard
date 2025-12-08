/// <binding BeforeBuild='min' Clean='clean' />
"use strict";

var gulp = require("gulp"),
    merge = require('merge-stream'),
  concat = require("gulp-concat"),
  cssmin = require("gulp-clean-css"),
  rename = require('gulp-rename'),
  uglify = require("gulp-uglify"),
  async = require('async');

var wwwroot = "./wwwroot/",
    paths = {
        webroot: wwwroot,

        jquerysrc: wwwroot + 'bower_components/jquery/dist/jquery.min.js',
    jqueryvalidation: [
        wwwroot + 'bower_components/jquery-validation/dist/jquery.validate.min.js',
        wwwroot + 'bower_components/jquery-validation-unobtrusive/jquery.validate.unobtrusive.min.js',
        wwwroot + 'bower_components/jquery-ajax-unobtrusive/jquery.unobtrusive-ajax.min.js',
        wwwroot + 'Scripts/lib/jquery.validation.messages_zh.js'
    ],
    datepicker: [
        wwwroot + 'bower_components/jquery-ui/ui/minified/datepicker.min.js',
        wwwroot + 'bower_components/jquery-ui/ui/minified/i18n/datepicker-zh-CN.min.js'
    ],
    datepickerCss: [
        wwwroot + 'bower_components/jquery-ui/themes/base/core.css',
        wwwroot + 'bower_components/jquery-ui/themes/base/datepicker.css',
        wwwroot + 'bower_components/jquery-ui/themes/base/theme.css'
    ]
};

paths.jsPath = paths.webroot + "Scripts/";
paths.combineJs = paths.webroot + "Scripts/*.js";
paths.minJs = paths.webroot + "Scripts/**/*.min.js";
paths.combineCanvasJs = paths.webroot + "Scripts/canvas/*.js";
paths.combineJsDest = paths.webroot + "Scripts/site.min.js";
paths.combineCanvasJsDest = paths.webroot + "Scripts/canvas/min.js";

paths.css = paths.webroot + "Content/**/*.css";
paths.minCss = paths.webroot + "Content/**/*.min.css";
paths.datepickerCssDest = paths.webroot + "Content/datepicker.min.css";

paths.jqueryDest = paths.webroot + "Scripts/jquery.min.js";
paths.jqueryValDest = paths.webroot + "Scripts/jquery.validation.min.js";
paths.datepickerDest = paths.webroot + "Scripts/datepicker.min.js";

paths.tagManager = paths.webroot + "Scripts/tagmanager.js";

function cleanJs(done) {
  var fs = require('fs');
  [paths.combineJsDest, paths.combineCanvasJsDest, paths.jqueryDest, paths.jqueryValDest, paths.datepickerDest]
    .forEach(file => { if (fs.existsSync(file)) fs.unlinkSync(file); });
  done();
}

function cleanCss(done) {
  var fs = require('fs');
  var path = require('path');
  var glob = require('glob');
  // Exclude messenger.min.css (pre-minified library file with no source)
  glob.sync(paths.minCss).forEach(file => { 
    if (fs.existsSync(file) && !file.includes('messenger.min.css')) {
      fs.unlinkSync(file);
    }
  });
  done();
}

gulp.task("clean:js", cleanJs);

gulp.task("clean:css", cleanCss);

gulp.task("clean", gulp.parallel(cleanJs, cleanCss));

function minJsSite() {
  return gulp.src([paths.combineJs, "!" + paths.minJs, "!" + paths.tagManager], { allowEmpty: true })
    .pipe(concat('site.min.js'))
    .pipe(uglify().on('error', function(e) { console.error('Uglify error:', e); this.emit('end'); }))
    .pipe(gulp.dest(paths.jsPath));
}

function minJsTag() {
  return gulp.src([paths.tagManager], { base: ".", allowEmpty: true })
    .pipe(uglify().on('error', function(e) { console.error('Uglify error:', e); this.emit('end'); }))
    .pipe(rename({ suffix: '.min' }))
    .pipe(gulp.dest("."));
}

function minJsCanvas() {
  return gulp.src([paths.combineCanvasJs, "!" + paths.combineCanvasJsDest], { allowEmpty: true })
    .pipe(concat('min.js'))
    .pipe(uglify().on('error', function(e) { console.error('Uglify error:', e); this.emit('end'); }))
    .pipe(gulp.dest(paths.jsPath + 'canvas/'));
}

function minJsJquery() {
  return gulp.src([paths.jquerysrc], { allowEmpty: true })
    .pipe(gulp.dest(paths.jsPath));
}

function minJsJqueryVal() {
  return gulp.src(paths.jqueryvalidation, { allowEmpty: true })
    .pipe(concat('jquery.validation.min.js'))
    .pipe(gulp.dest(paths.jsPath));
}

function minJsDatepicker() {
  return gulp.src(paths.datepicker, { allowEmpty: true })
    .pipe(concat('datepicker.min.js'))
    .pipe(gulp.dest(paths.jsPath));
}

const minJs = gulp.parallel(minJsSite, minJsTag, minJsCanvas, minJsJquery, minJsJqueryVal, minJsDatepicker);

function minCssContent() {
  return gulp.src([paths.css, "!" + paths.minCss])
    .pipe(cssmin())
    .pipe(rename({ suffix: '.min' }))
    .pipe(gulp.dest(paths.webroot + "Content/"));
}

function minCssDatepicker() {
  return gulp.src(paths.datepickerCss)
    .pipe(concat(paths.datepickerCssDest))
    .pipe(cssmin())
    .pipe(gulp.dest('.'));
}

const minCss = gulp.parallel(minCssContent, minCssDatepicker);

gulp.task("min:js", minJs);

gulp.task("min:css", minCss);

gulp.task("min", gulp.parallel(minJs, minCss));
