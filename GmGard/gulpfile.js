/// <binding BeforeBuild='min' Clean='clean' />
"use strict";

var gulp = require("gulp"),
    merge = require('merge-stream'),
  rimraf = require("rimraf"),
  concat = require("gulp-concat"),
  cssmin = require("gulp-cssmin"),
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
    ],
    messengerJs: [
        wwwroot + 'bower_components/messenger/build/js/messenger.js',
        wwwroot + 'bower_components/messenger/build/js/messenger-theme-future.js'
    ],
    messengerCss: [
        wwwroot + 'bower_components/messenger/build/css/messenger.css',
        wwwroot + 'bower_components/messenger/build/css/messenger-theme-future.css'
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

paths.messengerJsDest = paths.webroot + "Scripts/messenger.min.js";
paths.messengerCssDest = paths.webroot + "Content/messenger.min.css";

paths.tagManager = paths.webroot + "Scripts/tagmanager.js";

function cleanJs(cb) {
  async.parallel([
    (c) => { rimraf(paths.combineJsDest, c); },
    (c) => { rimraf(paths.combineCanvasJsDest, c); },
    (c) => { rimraf(paths.jqueryDest, c); },
    (c) => { rimraf(paths.jqueryValDest, c); },
    (c) => { rimraf(paths.datepickerDest, c); },
    (c) => { rimraf(paths.messengerJsDest, c); }
  ], cb);
}

function cleanCss(cb) {
  rimraf(paths.minCss, cb);
}

gulp.task("clean:js", cleanJs);

gulp.task("clean:css", cleanCss);

gulp.task("clean", gulp.parallel(cleanJs, cleanCss));

function minJs() {
  var combineJs = gulp.src([paths.combineJs, "!" + paths.minJs, "!" + paths.tagManager], { base: "." })
    .pipe(concat(paths.combineJsDest))
    .pipe(uglify())
    .pipe(gulp.dest("."));
  var minTagJs = gulp.src([paths.tagManager], { base: "." })
    .pipe(uglify())
    .pipe(rename({ suffix: '.min' }))
    .pipe(gulp.dest("."));
  var combineCanvasJs = gulp.src([paths.combineCanvasJs, "!" + paths.combineCanvasJsDest], { base: "." })
    .pipe(concat(paths.combineCanvasJsDest))
    .pipe(uglify())
    .pipe(gulp.dest("."));
  var jquery = gulp.src([paths.jquerysrc]).pipe(gulp.dest(paths.jsPath));
  var jqueryVal = gulp.src(paths.jqueryvalidation).pipe(concat(paths.jqueryValDest)).pipe(gulp.dest('.'));
  var datepicker = gulp.src(paths.datepicker).pipe(concat(paths.datepickerDest)).pipe(gulp.dest('.'));
  var messenger = gulp.src(paths.messengerJs).pipe(concat(paths.messengerJsDest)).pipe(uglify()).pipe(gulp.dest('.'));
  return merge(combineJs, combineCanvasJs, minTagJs, jquery, jqueryVal, datepicker, messenger);
}

function minCss() {
  return merge(
    gulp.src([paths.css, "!" + paths.minCss])
      .pipe(cssmin())
      .pipe(rename({ suffix: '.min' }))
      .pipe(gulp.dest(paths.webroot + "Content/")),
    gulp.src(paths.datepickerCss)
      .pipe(concat(paths.datepickerCssDest))
      .pipe(cssmin())
      .pipe(gulp.dest('.')),
    gulp.src(paths.messengerCss)
      .pipe(concat(paths.messengerCssDest))
      .pipe(cssmin())
      .pipe(gulp.dest('.')));
}

gulp.task("min:js", minJs);

gulp.task("min:css", minCss);

gulp.task("min", gulp.parallel(minJs, minCss));
