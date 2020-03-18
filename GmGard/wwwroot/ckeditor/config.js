/**
 * @license Copyright (c) 2003-2013, CKSource - Frederico Knabben. All rights reserved.
 * For licensing, see LICENSE.html or http://ckeditor.com/license
 */

CKEDITOR.editorConfig = function (config) {
    // Define changes to default configuration here.
    // For the complete reference:
    // http://docs.ckeditor.com/#!/api/CKEDITOR.config

    config.toolbar = [
	{ name: 'undo', groups: ['undo'], items: ['Undo', 'Redo'] },
	{ name: 'links', items: ['Link', 'Unlink', 'Anchor'] },
    { name: 'pics', items: ['MediaEmbed', 'Flash', 'Image', 'Custom_Smiley'] },
	{ name: 'insert', items: ['Table', 'HorizontalRule', 'Spoiler', 'Mentions', 'RubyMarkup'] },
	{ name: 'tools', items: ['Maximize'] },
	{ name: 'document', groups: ['mode', 'document', 'doctools'], items: ['Source'] },
	{ name: 'others', items: ['-'] },
	{ name: 'colors', items: ['TextColor', 'BGColor'] },
	'/',
	{ name: 'basicstyles', groups: ['basicstyles', 'cleanup'], items: ['Bold', 'Italic', 'Strike', '-', 'RemoveFormat'] },
	{ name: 'paragraph', groups: ['list', 'indent', 'blocks', 'align', 'bidi'], items: ['NumberedList', 'BulletedList', '-', 'Outdent', 'Indent', '-', 'Blockquote'] },
	{ name: 'styles', items: ['Styles', 'Format', 'FontSize'] },
	{ name: 'about', items: ['About'] }
    ];

    // Remove some buttons, provided by the standard plugins, which we don't
    // need to have in the Standard(s) toolbar.
    config.removeButtons = 'Underline,Subscript,Superscript';

    // Se the most common block elements.
    config.format_tags = 'p;h1;h2;h3;h4;h5;pre';

    config.image_previewText = "这里是预览文字这里是预览文字这里是预览文字这里是预览文字这里是预览文字这里是预览文字这里是预览文字这里是预览文字这里是预览文字这里是预览文字这里是预览文字这里是预览文字这里是预览文字这里是预览文字这里是预览文字这里是预览文字";

    // Make dialogs simpler.
    config.removeDialogTabs = 'image:advanced;link:advanced';
    config.removePlugins = 'elementspath';

    config.extraAllowedContent = 'a[rel,data-mention]; span(*); embed[*]; table(*); ruby; iframe{*}';
    config.extraPlugins = 'custom_smiley,colorbutton,font,mediaembed,flash,mentions,rubymarkup,autosave,spoiler';

    config.autosave_saveDetectionSelectors = '#submitbtn,#SubmitButton,#editversion';

    config.customConfig = '/ckeditor/smiley_config.js';
};
