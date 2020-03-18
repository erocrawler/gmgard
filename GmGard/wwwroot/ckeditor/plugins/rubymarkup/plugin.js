/**
 *  Written by Duo
 */
(function () {
    CKEDITOR.plugins.add('rubymarkup',
    {
        icons: 'rubymarkup', // %REMOVE_LINE_CORE%
        hidpi: false, // %REMOVE_LINE_CORE%
        init: function (editor) {

            var getRubyText = function (node) {
                var txt = $(node).clone().find('rp,rt').remove().end().text();
                return txt;
            };
            CKEDITOR.dialog.add('RubyMarkupDialog', function (instance) {
                return {
                    title: '注音',
                    minWidth: 550,
                    minHeight: 200,
                    contents:
                          [
                             {
                                 id: 'main',
                                 expand: true,
                                 elements: [{
                                     id: 'rubyArea',
                                     type: 'text',
                                     label: '注音',
                                     validate: CKEDITOR.dialog.validate.notEmpty("注音不能为空"),
                                     setup: function (element) {
                                         var rts = element.find('rt'), words = [];
                                         for (var i = 0; i < rts.count() ; i++) {
                                             words.push(rts.getItem(i).getText());
                                         }
                                         this.setValue(words.join(''));
                                         if (i > 1) {
                                             this.getDialog().getContentElement('main', 'markuptype').setValue('char');
                                         }
                                     },
                                     commit: function (element) {
                                         var val = this.getValue(),
                                             is_split = this.getDialog().getContentElement('main', 'markuptype').getValue() == 'char',
                                             words = is_split ? val.split(/\s+/) : [val];
                                         element.setHtml($.map(words, function (word) {
                                             if (word) return '<rp>(</rp><rt>' + word + '</rt><rp>)</rp>';
                                         }).join(''));
                                     },
                                     onLoad: function () {
                                         var dialog = this.getDialog(),
                                             previewContent = dialog.getContentElement('main', 'preview'),
                                             timer = null,
                                             updatePreview = function () {
                                                 var previewElement = editor.document.createElement('ruby');
                                                 dialog.getContentElement('main', 'rubyArea').commit(previewElement);
                                                 dialog.getContentElement('main', 'textArea').commit(previewElement);
                                                 //dialog.commitContent(previewElement);
                                                 previewContent.replaceRuby(previewElement.$);
                                             },
                                             input = this.getInputElement();
                                         //input.on('change', updatePreview);
                                         input.on('input', function () {
                                             if (timer) {
                                                 window.clearTimeout(timer);
                                             }
                                             timer = window.setTimeout(updatePreview, 500);
                                         });
                                         //input.once('input', function () { input.removeListener('change'); });
                                     }
                                 }, {
                                     id: 'textArea',
                                     type: 'text',
                                     label: '正文',
                                     validate: CKEDITOR.dialog.validate.notEmpty("正文不能为空"),
                                     setup: function (element) {
                                         this.setValue(getRubyText(element.$));
                                     },
                                     commit: function (element) {
                                         var val = this.getValue(),
                                             is_split = this.getDialog().getContentElement('main', 'markuptype').getValue() == 'char',
                                             words = is_split ? val.split('') : [val],
                                             rps = element.find('rp');
                                         for (var i = 0, j = 0; i < words.length && j < rps.count() ; i++,j+=2) {
                                             rps.getItem(j).insertBeforeMe(editor.document.createText(words[i]));
                                         }
                                         if (j == 0) {
                                             element.setText(val);
                                         }
                                     },
                                     onLoad: function () {
                                         var rubyArea = this.getDialog().getContentElement('main', 'rubyArea').getInputElement(),
                                             updatePreview = function() {rubyArea.fire('input');},
                                             input = this.getInputElement();
                                         //input.on('change', updatePreview);
                                         input.on('input', updatePreview);
                                         //input.once('input', function () { input.removeListener('change'); });
                                     }
                                 }, {
                                     type: 'hbox',
                                     children: [{
                                         type: 'html',
                                         id: 'previewlabel',
                                         html: '<strong>预览:<ruby><rp>您的浏览器不支持ruby注音</rp></ruby></strong>'
                                        }, {
                                         type: 'radio',
                                         id: 'markuptype',
                                         items: [['整句注音', 'word'], ['逐字注音', 'char']],
                                         'default': 'word',
                                         onClick: function () {
                                             var rubyArea = this.getDialog().getContentElement('main', 'rubyArea').getInputElement().fire('input');
                                         }
                                     }]
                                 }, {
                                     type: 'html',
                                     id: 'preview',
                                     style: 'height: 65px;width: 100%;',
                                     html: '<iframe></iframe>',
                                     ruby: document.createElement('ruby'),
                                     onLoad: function () {
                                         var e = this.getElement();
                                         this.framebody = e.$.contentWindow.document.body;
                                         this.framebody.appendChild(this.ruby);
                                     },
                                     onHide: function () {
                                         this.replaceRuby(document.createElement('ruby'));
                                     },
                                     setup: function (element) {
                                         this.replaceRuby(element.clone(true).$);
                                     },
                                     replaceRuby: function (newruby) {
                                         this.framebody.replaceChild(newruby, this.ruby);
                                         this.ruby = newruby;
                                     }
                                 }]
                             }
                          ],
                    onShow: function () {
                        var selection = editor.getSelection(),
                            element = selection.getStartElement();
                        if (element)
                            element = element.getAscendant('ruby', true);

                        if (!element || element.getName() != 'ruby' || element.data('cke-realelement')) {
                            element = editor.document.createElement('ruby');
                            element.setText(selection.getSelectedText());
                            this.insertMode = true;
                        }
                        else
                            this.insertMode = false;
                        debugger;
                        this.element = element;
                        this.setupContent(this.element);
                    },
                    onOk: function () {
                        var dialog = this,
                            abbr = this.element;

                        this.commitContent(abbr);

                        if (this.insertMode) {
                            editor.insertElement(abbr);
                            editor.insertText(' ');
                        }
                    }
                };
            });
            var checkruby = function () {
                var editable = editor.editable();
                if (!editable) return;
                var rubys = editable.find('ruby'),
                    rbs = editable.find('rb');
                for (var i = 0; i < rubys.count() ; i++) {
                    var ruby = rubys.getItem(i),
                        text = getRubyText(rubys.getItem(i).$);
                    if (!text || text.length < 1) {
                        ruby.remove();
                    }
                    else if (ruby.find('rt').count() < 1) {
                        ruby.insertBeforeMe(editor.document.createText(text));
                        ruby.remove();
                    }
                }
            };
            var timer = null;
            editor.on('change', function (e) {
                if (timer)
                    window.clearTimeout(timer);
                timer = window.setTimeout(checkruby, 500);
            });

            editor.addCommand('RubyMarkup', new CKEDITOR.dialogCommand('RubyMarkupDialog',
                { allowedContent: 'ruby;rp;rt;' }
            ));

            editor.ui.addButton('RubyMarkup',
            {
                label: '注音',
                command: 'RubyMarkup',
                toolbar: 'rubymarkup'
            });

            if (editor.contextMenu) {
                editor.addMenuGroup('rubyGroup');
                editor.addMenuItem('rubyItem', {
                    label: '编辑注音',
                    icon: this.path + 'icons/rubymarkup.png',
                    command: 'RubyMarkup',
                    group: 'rubyGroup'
                });

                editor.contextMenu.addListener(function (element) {
                    if (element.getAscendant('ruby', true)) {
                        return { rubyItem: CKEDITOR.TRISTATE_OFF };
                    }
                });
            }
        }
    });
})();
