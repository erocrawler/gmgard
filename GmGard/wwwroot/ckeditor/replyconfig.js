/**
 * @license Copyright (c) 2003-2013, CKSource - Frederico Knabben. All rights reserved.
 * For licensing, see LICENSE.html or http://ckeditor.com/license
 */

CKEDITOR.editorConfig = function (config) {
    // Define changes to default configuration here.
    // For the complete reference:
    // http://docs.ckeditor.com/#!/api/CKEDITOR.config

    config.toolbar = [['Bold', 'Italic', '-', 'NumberedList', 'BulletedList', '-', 'Link', 'Unlink', '-', 'Mentions', 'RubyMarkup'], ['Custom_Smiley', 'Image'],
                        ['TextColor', 'BGColor', 'Styles']];
    config.uiColor = '#019ed5';
    // Remove some buttons, provided by the standard plugins, which we don't
    // need to have in the Standard(s) toolbar.
    config.removeButtons = 'Underline,Subscript,Superscript';

    // Se the most common block elements.
    config.format_tags = 'p;h1;h2;h3;h4;h5;pre';

    config.image_previewText = "这里是预览文字这里是预览文字这里是预览文字这里是预览文字这里是预览文字这里是预览文字这里是预览文字这里是预览文字这里是预览文字这里是预览文字这里是预览文字这里是预览文字这里是预览文字这里是预览文字这里是预览文字这里是预览文字";

    // Make dialogs simpler.
    config.removeDialogTabs = 'image:advanced;link:advanced';
    config.removePlugins = 'elementspath';

    config.extraAllowedContent = 'a[rel,data-mention]; span(*); embed[*]; table(*); ruby;';
    config.extraPlugins = 'custom_smiley,colorbutton,mentions,rubymarkup';

    config.customConfig = '/ckeditor/smiley_config.js';
};
