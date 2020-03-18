/**
* @license Copyright (c) 2003-2013, CKSource - Frederico Knabben. All rights reserved.
* For licensing, see LICENSE.html or http://ckeditor.com/license
*/

CKEDITOR.dialog.add('custom_smiley', function (editor) {
    var config = editor.config,
		lang = editor.lang.custom_smiley,
		columns = config.smiley_columns || 8;

    var onClick = function (evt) {
        var target = evt.data.getTarget(),
            targetName = target.getName();

        if (targetName == 'a')
            target = target.getChild(0);
        else if (targetName != 'img')
            return;
        var src = target.getAttribute('cke_src'),
            title = target.getAttribute('title');

        var img = editor.document.createElement('img', {
            attributes: {
                src: src,
                'data-cke-saved-src': src,
                title: title,
                alt: title
            }
        });

        editor.insertElement(img);

        this.getDialog().hide();
        evt.data.preventDefault();
    };

    function switchPage (dlg, tabPrefix, curPage, tgtPage) {
        dlg.showPage(tabPrefix + tgtPage);
        dlg.selectPage(tabPrefix + tgtPage);
        dlg.hidePage(tabPrefix + curPage);
    }
    function getPageIndex(currentPage, totalPage) {
        var pageNumHtml = [];
        for (var j = 0; j < totalPage; j++) {
            if (j == currentPage) {
                pageNumHtml.push('&nbsp;&nbsp;<span style="font-size:large">' + (j + 1) + '</span>&nbsp;&nbsp;');
            }
            else {
                pageNumHtml.push('&nbsp;&nbsp;<a style="font-size:large; cursor:pointer; text-decoration:underline" data-page="' + j + '">' + (j + 1) + '</a>&nbsp;&nbsp;');
            }
        }
        return pageNumHtml.join('')
    }
    function alignCenter(elem) {
        var nl = elem.getElement().find('td');
        for (var i = 0; i < nl.count() ; ++i) {
            nl.getItem(i).setStyle('text-align', 'center');
        }
    }
    var smileyGroups = config.smiley_images,
        contents = [];
    for (var smileyGroup in smileyGroups) {
        var smileyElements = smileyGroups[smileyGroup],
            pagerowsize = 3, //three rows each page.
            rowcount = 0, pagecount = 0, size = smileyElements.length,
            pageamount = Math.ceil(size / columns / pagerowsize),
            tabPrefix = 'tab_' + smileyGroup + '_',
            pagecontent = {
                id: tabPrefix + pagecount,
                label: smileyGroup,
                title: smileyGroup,
                expand: true,
                padding: 0,
                elements: [],
            };
        for (var i = 0; i < size; ++rowcount) {
            var element = {
                type: 'hbox',
                children:[],
            }
            for (var j = 0; i < size && j < columns; ++i, ++j) {
                var smileyLabelId = 'cke_smile_label_' + i + '_' + CKEDITOR.tools.getNextNumber(),
                    smileyElement = smileyElements[i],
                    smileyDesc = smileyElement.substring(0, smileyElement.lastIndexOf('.'));
                element.children.push({
                    type: 'html',
                    id: 'smiley_img_' + i,
                    html: ('<a href="javascript:void(0)" role="option" aria-posinset="' + (i + 1) + '" aria-setsize="' + size + '" aria-labelledby="' + smileyLabelId + '"' +
                           ' class="cke_smile cke_hand" tabindex="-1">' +
                           '<img class="cke_hand" title="' + smileyDesc + '" cke_src="' + CKEDITOR.tools.htmlEncode(config.smiley_path + smileyElement) + '" alt="' + smileyDesc + '"' +
                           '         src="' + CKEDITOR.tools.htmlEncode(config.smiley_path + smileyElement) + '" style="max-width:100px;max-height:100px;"' +
                           (CKEDITOR.env.ie ? ' onload="this.setAttribute(\'width\', 2); this.removeAttribute(\'width\');" ' : '') + '/>' +
                           '  <span id="' + smileyLabelId + '" class="cke_voice_label">' + smileyDesc + '</span></a>'),   
                    onClick: onClick,
                })
            }
            pagecontent.elements.push(element);
            if ((rowcount + 1) % pagerowsize == 0 && i < size) {
                // paging
                var pageElement = {
                    type: 'hbox',
                    align: 'center',
                    widths: ['25%', '50%', '25%'],
                    onLoad: function () { alignCenter(this) },
                    children: [pagecount > 0 ? {
                        type: 'button',
                        id: 'tabprev_' + smileyGroup + '_' + pagecount,
                        label: '上一页',
                        title: '上一页',
                        tabPrefix: tabPrefix,
                        tabNum: pagecount,
                        onClick: function () {
                            switchPage(this.getDialog(), this.tabPrefix, this.tabNum, this.tabNum - 1);
                        },
                    } : {type: 'html', html: ''}, {
                        type: 'html',
                        id: 'tabjump_' + smileyGroup + '_' + pagecount,
                        html: getPageIndex(pagecount, pageamount),
                        tabPrefix: tabPrefix,
                        tabNum: pagecount,
                        onClick: function (event) {
                            var tgtTab = parseInt(event.data.getTarget().getAttribute('data-page'), 10);
                            !isNaN(tgtTab) && switchPage(this.getDialog(), this.tabPrefix, this.tabNum, tgtTab);
                        },
                    }, {
                        type: 'button',
                        id: 'tabnext_' + smileyGroup + '_' + pagecount,
                        label: '下一页',
                        title: '下一页',
                        tabPrefix: tabPrefix,
                        tabNum: pagecount,
                        onClick: function () {
                            switchPage(this.getDialog(), this.tabPrefix, this.tabNum, this.tabNum + 1);
                        },
                    }],
                };
                pagecontent.elements.push(pageElement);
                contents.push(pagecontent);
                pagecontent = {
                    id: tabPrefix + ++pagecount,
                    label: smileyGroup,
                    title: smileyGroup,
                    hidden: true,
                    padding: 0,
                    elements: [],
                }
            }
        }
        if (pagecontent.elements.length > 0) {
            if (pagecount > 0) {
                pagecontent.elements.push({
                    type: 'hbox',
                    align: 'center',
                    widths: ['25%', '50%', '25%'],
                    onLoad: function () { alignCenter(this) },
                    children: [{
                        type: 'button',
                        id: 'tabprev_' + smileyGroup + '_' + pagecount,
                        label: '上一页',
                        title: '上一页',
                        tabPrefix: tabPrefix,
                        tabNum: pagecount,
                        onClick: function () {
                            switchPage(this.getDialog(), this.tabPrefix, this.tabNum, this.tabNum - 1);
                        },
                    }, {
                        type: 'html',
                        id: 'tabjump_' + smileyGroup + '_' + pagecount,
                        html: getPageIndex(pagecount, pageamount), 
                        tabPrefix: tabPrefix,
                        tabNum: pagecount,
                        onClick: function (event) {
                            var tgtTab = parseInt(event.data.getTarget().getAttribute('data-page'), 10);
                            !isNaN(tgtTab) && switchPage(this.getDialog(), this.tabPrefix, this.tabNum, tgtTab);
                        },
                    }, {type: 'html', html: ''}],
                });
            }
            contents.push(pagecontent);
        }
    }

    return {
        title: editor.lang.custom_smiley.title,
        minWidth: 400,
        minHeight: 120,
        contents: contents,
        buttons: [CKEDITOR.dialog.cancelButton]
    };
});