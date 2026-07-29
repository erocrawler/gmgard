///////////////////////////////////////////////////////////////
// CKEDITOR_mentions – vanilla JS rewrite for Blazor / no-jQuery env
// Original used jQuery heavily; this version uses plain DOM + fetch
// Maintains backward compat if jQuery exists (typeahead dialog fallback)
///////////////////////////////////////////////////////////////

function CKEDITOR_mentions(editor) {
    this.editor = editor;
    this.observe = 0;
    this.char_input = [];
    this.cache = {};
    this.fail_cache = [];
    this.observe_node = null;
    this.observe_startpos = 0;
    this.timeout_id = null;
    this.mention_suggestions = null;
    if (CKEDITOR_mentions.caller !== CKEDITOR_mentions.get_instance) {
        throw new Error("This object cannot be instanciated");
    }
}

CKEDITOR_mentions.instances = [];
CKEDITOR_mentions.timeout_delay = 400;
CKEDITOR_mentions.start_observe_count = 2;

CKEDITOR_mentions.get_instance = function (editor) {
    for (var i = 0; i < this.instances.length; i++) {
        if (this.instances[i].id === editor.id) {
            return this.instances[i].instance;
        }
    }
    this.instances.push({ id: editor.id, instance: new CKEDITOR_mentions(editor) });
    return this.instances[this.instances.length - 1].instance;
};

CKEDITOR_mentions.prototype.create_tooltip = function (users) {
    var ul = document.createElement('ul');
    ul.className = 'typeahead dropdown-menu show';
    ul.style.listStyle = 'none';
    ul.style.margin = '0';
    ul.style.padding = '4px 0';
    for (var i = 0; i < users.length; i++) {
        var li = document.createElement('li');
        li.className = 'mention-users';
        li.setAttribute('data-username', users[i].username);
        li.setAttribute('data-nickname', users[i].nickname);
        var a = document.createElement('a');
        a.href = 'javascript:void(0)';
        a.textContent = users[i].nickname + ' (' + users[i].username + ')';
        a.style.display = 'block';
        a.style.padding = '3px 8px';
        a.style.cursor = 'pointer';
        li.appendChild(a);
        if (i === 0) li.classList.add('active');
        ul.appendChild(li);
    }
    return ul;
};

CKEDITOR_mentions.prototype.delete_tooltip = function () {
    if (this.mention_suggestions) {
        try { this.mention_suggestions.remove(); } catch (e) { if (this.mention_suggestions.parentNode) this.mention_suggestions.parentNode.removeChild(this.mention_suggestions); }
        this.mention_suggestions = null;
    }
};

CKEDITOR_mentions.prototype._activeLi = function () {
    if (!this.mention_suggestions) return null;
    return this.mention_suggestions.querySelector('li.active');
};

CKEDITOR_mentions.prototype.next_tooltip = function () {
    if (!this.mention_suggestions) return;
    var active = this._activeLi();
    if (!active) return;
    active.classList.remove('active');
    var next = active.nextElementSibling;
    if (!next) {
        var first = this.mention_suggestions.querySelector('li');
        if (first) first.classList.add('active');
        return;
    }
    next.classList.add('active');
};

CKEDITOR_mentions.prototype.prev_tooltip = function () {
    if (!this.mention_suggestions) return;
    var active = this._activeLi();
    if (!active) return;
    active.classList.remove('active');
    var prev = active.previousElementSibling;
    if (!prev) {
        var items = this.mention_suggestions.querySelectorAll('li');
        if (items.length) items[items.length - 1].classList.add('active');
        return;
    }
    prev.classList.add('active');
};

CKEDITOR_mentions.prototype.start_observing = function (textNode, startpos) {
    this.observe = 1;
    this.observe_node = textNode;
    this.observe_startpos = startpos;
};

CKEDITOR_mentions.prototype.stop_observing = function () {
    this.observe = 0;
    this.observe_startpos = 0;
    this.char_input = [];
    this.observe_node = null;
    this.delete_tooltip();
};

CKEDITOR_mentions.prototype.get_people = function (selection) {
    if (this.timeout_id !== null) clearTimeout(this.timeout_id);
    var self = this;
    this.timeout_id = setTimeout(function () { self.timeout_callback([self, selection]); }, CKEDITOR_mentions.timeout_delay);
};

CKEDITOR_mentions.prototype._getCaretCoords = function (editor, range) {
    try {
        var iframe = document.getElementById('cke_' + editor.name) ? document.getElementById('cke_' + editor.name).querySelector('iframe.cke_wysiwyg_frame') : null;
        if (!iframe) return { top: 0, left: 0 };
        var iframeRect = iframe.getBoundingClientRect();
        var doc = iframe.contentDocument || iframe.contentWindow.document;
        var sel = doc.getSelection ? doc.getSelection() : null;
        var clientRect = null;
        if (range && range.getClientRects) {
            var rects = range.getClientRects();
            if (rects.length) clientRect = rects[0];
        }
        // fallback via range.startContainer bounding
        if (!clientRect) {
            try {
                var tmpRange = doc.createRange();
                tmpRange.setStart(range.startContainer, Math.max(0, range.startOffset - 1));
                tmpRange.collapse(true);
                var r = tmpRange.getClientRects();
                if (r.length) clientRect = r[0];
            } catch (e) {}
        }
        var top = iframeRect.top + (clientRect ? clientRect.top : 20) + window.scrollY;
        var left = iframeRect.left + (clientRect ? clientRect.left : 0) + window.scrollX;
        return { top: top, left: left, height: clientRect ? clientRect.height : 16 };
    } catch (e) { return { top: 100, left: 100, height: 16 }; }
};

CKEDITOR_mentions.prototype.timeout_callback = function (args) {
    var mentions = args[0];
    var selection = args[1];
    var str = mentions.char_input.join('');

    if (str.length < CKEDITOR_mentions.start_observe_count) {
        mentions.delete_tooltip();
        return;
    }
    var editor = mentions.editor;
    var element_id = editor.name;
    var range;
    try { range = selection.getRanges()[0]; } catch (e) { return; }
    var startOffset = (parseInt(range.startOffset - str.length) || 0);
    var endOffset = range.startOffset;
    var element;
    try { element = range.startContainer.$; } catch (e) { element = range.startContainer; }
    if (!element) return;

    var update_tooltip = function (rsp) {
        // position near caret
        try {
            var coords = mentions._getCaretCoords(editor, range);
            if (mentions.mention_suggestions) {
                try { mentions.mention_suggestions.remove(); } catch (ex) {}
            }
            if (rsp && rsp.length > 0) {
                var userlist = mentions.create_tooltip(rsp);
                var sdiv = document.createElement('div');
                sdiv.className = 'mention-suggestions';
                sdiv.style.zIndex = '10000';
                sdiv.style.position = 'absolute';
                sdiv.style.background = '#fff';
                sdiv.style.border = '1px solid #ccc';
                sdiv.style.borderRadius = '6px';
                sdiv.style.boxShadow = '0 5px 10px rgba(0,0,0,0.2)';
                sdiv.style.minWidth = '200px';
                sdiv.appendChild(userlist);
                document.body.appendChild(sdiv);
                mentions.mention_suggestions = sdiv;

                // style active
                var styleActive = function () {
                    var lis = sdiv.querySelectorAll('li');
                    lis.forEach(function (li) {
                        li.style.background = '';
                        li.querySelector('a').style.color = '';
                    });
                    var act = sdiv.querySelector('li.active');
                    if (act) {
                        act.style.background = '#0081c2';
                        var a = act.querySelector('a');
                        if (a) a.style.color = '#fff';
                    }
                };
                styleActive();

                // mouseenter highlight
                sdiv.querySelectorAll('li.mention-users').forEach(function (li) {
                    li.addEventListener('mouseenter', function () {
                        sdiv.querySelectorAll('li.active').forEach(function (x) { x.classList.remove('active'); x.style.background=''; var ax=x.querySelector('a'); if(ax) ax.style.color=''; });
                        li.classList.add('active');
                        styleActive();
                    });
                    li.addEventListener('click', function (e) {
                        e.preventDefault();
                        insertMention(li);
                    });
                });

                sdiv.style.top = (coords.top + coords.height + 6) + 'px';
                sdiv.style.left = coords.left + 'px';

                // keep inside viewport
                var rect = sdiv.getBoundingClientRect();
                if (rect.right > window.innerWidth - 10) {
                    sdiv.style.left = (window.innerWidth - rect.width - 10) + 'px';
                }
            } else {
                mentions.fail_cache.push(str);
                return;
            }
        } catch (err) { /* ignore */ }

        function insertMention(liEl) {
            try {
                var m = CKEDITOR_mentions.get_instance(editor);
                m.stop_observing();
                var link = document.createElement('a');
                var username = liEl.getAttribute('data-username');
                var nickname = liEl.getAttribute('data-nickname');
                link.href = '/User/' + encodeURIComponent(username);
                link.textContent = '@' + nickname;
                link.setAttribute('data-mention', '1');
                link.setAttribute('data-cke-saved-href', '/User/' + username);

                var end_elem = null;
                if (element.textContent.length > endOffset) {
                    end_elem = document.createTextNode(element.textContent.substr(endOffset));
                }
                element.textContent = element.textContent.substr(0, startOffset);

                if (element.nextSibling) {
                    element.parentNode.insertBefore(link, element.nextSibling);
                } else {
                    element.parentNode.appendChild(link);
                }
                if (end_elem) {
                    if (link.nextSibling) link.parentNode.insertBefore(end_elem, link.nextSibling);
                    else link.parentNode.appendChild(end_elem);
                }

                // trailing space to avoid sticking
                var space = document.createTextNode('\u00a0');
                if (link.nextSibling) link.parentNode.insertBefore(space, link.nextSibling);
                else link.parentNode.appendChild(space);

                editor.focus();
                try {
                    var ckeRange = editor.createRange();
                    var el = new CKEDITOR.dom.element(link.parentNode);
                    var offset = link.parentNode.textContent.length;
                    // move caret after inserted mention + space
                    ckeRange.moveToPosition(link, CKEDITOR.POSITION_AFTER_END);
                    ckeRange.select();
                } catch (e2) {
                    editor.focus();
                }
            } catch (err) {}
        }
    };

    if (mentions.cache[str]) {
        update_tooltip(mentions.cache[str]);
        return;
    } else if (mentions.check_fail(str)) {
        return;
    }

    // fetch vanilla – support both JSON and form-encoded
    var body = 'typed=' + encodeURIComponent(str);
    fetch('/home/mentions', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
        body: body
    }).then(function (r) { return r.json(); }).then(function (rsp) {
        mentions.cache[str] = rsp;
        update_tooltip(rsp);
    }).catch(function () {
        // fallback silent
    });
};

CKEDITOR_mentions.prototype.check_fail = function (str) {
    for (var i = 0; i < this.fail_cache.length; i++) {
        if (str.indexOf(this.fail_cache[i]) === 0) return true;
    }
    return false;
};

CKEDITOR_mentions.prototype.break_on = function (charcode) {
    var specials = [27, 37, 39, 46, 91];
    for (var i = 0; i < specials.length; i++) if (specials[i] === charcode) return true;
    return false;
};

// legacy helper for typeahead dialog (kept but vanilla fallback)
var properties = [
    'direction','boxSizing','width','height','overflowX','overflowY',
    'borderTopWidth','borderRightWidth','borderBottomWidth','borderLeftWidth',
    'paddingTop','paddingRight','paddingBottom','paddingLeft',
    'fontStyle','fontVariant','fontWeight','fontStretch','fontSize','fontSizeAdjust','lineHeight','fontFamily',
    'textAlign','textTransform','textIndent','textDecoration','letterSpacing','wordSpacing'
];
var isFirefox = !(window.mozInnerScreenX == null);

// used only as fallback; now vanilla _getCaretCoords is preferred
var getCaretCoordinatesFn = function (element, position) {
    var div = document.createElement('div');
    div.id = 'input-textarea-caret-position-mirror-div';
    document.body.appendChild(div);
    var style = div.style;
    var computed = window.getComputedStyle ? getComputedStyle(element) : element.currentStyle;
    style.whiteSpace = 'pre-wrap';
    if (element.nodeName !== 'INPUT') style.wordWrap = 'break-word';
    style.position = 'absolute';
    style.visibility = 'hidden';
    properties.forEach(function (prop) { try { style[prop] = computed[prop]; } catch (e) {} });
    if (isFirefox) {
        style.width = parseInt(computed.width) - 2 + 'px';
        if (element.scrollHeight > parseInt(computed.height)) style.overflowY = 'scroll';
    } else style.overflow = 'hidden';
    var text = (element.textContent || element.innerText || '');
    div.textContent = text.substring(0, position);
    if (element.nodeName === 'INPUT') div.textContent = div.textContent.replace(/\s/g, "\u00a0");
    var span = document.createElement('span');
    span.textContent = text.substring(position) || '.';
    div.appendChild(span);
    var coordinates = {
        top: span.offsetTop + parseInt(computed['borderTopWidth'] || 0),
        left: span.offsetLeft + parseInt(computed['borderLeftWidth'] || 0)
    };
    document.body.removeChild(div);
    return coordinates;
};

///////////////////////////////////////////////////////////////
// Plugin implementation – no jQuery dependency
///////////////////////////////////////////////////////////////
(function () {
    CKEDITOR.plugins.add('mentions', {
        icons: 'mentions',
        init: function (editor) {
            var mentions = CKEDITOR_mentions.get_instance(editor);

            CKEDITOR.dialog.add('MentionsDialog', function () {
                return {
                    title: '提及',
                    minWidth: 340,
                    minHeight: 180,
                    contents: [{
                        id: 'main',
                        expand: true,
                        elements: [
                            {
                                id: 'mentionArea',
                                type: 'text',
                                label: '请输入要提及的用户的昵称或账户名',
                                autofocus: 'autofocus',
                                validate: CKEDITOR.dialog.validate.notEmpty("请输入用户名"),
                                setup: function (element) {
                                    var txt = element.getText ? element.getText() : element.getHtml ? element.getHtml() : '';
                                    txt = txt.replace(/^@/, '');
                                    this.setValue(txt);
                                },
                                commit: function (element) {
                                    var name = this.getValue();
                                    if (name) {
                                        element.setHtml('<a data-mention="1" href="/User/' + CKEDITOR.tools.htmlEncode(name) + '">@' + CKEDITOR.tools.htmlEncode(name) + '</a>');
                                    }
                                },
                                onLoad: function () {
                                    // vanilla autocomplete for dialog – simple datalist if jQuery typeahead not present
                                    var inputEl = this.getInputElement().$;
                                    if (!inputEl) return;
                                    // if jQuery typeahead exists, use it; else vanilla fetch
                                    if (window.jQuery && jQuery.fn && jQuery.fn.typeahead) {
                                        try {
                                            jQuery(inputEl).typeahead({
                                                source: function (query, cb) {
                                                    if (mentions.cache[query]) {
                                                        return jQuery.map(mentions.cache[query], function (item) { return item.nickname + ' (' + item.username + ')'; });
                                                    } else if (mentions.check_fail(query)) return;
                                                    jQuery.post('/home/mentions', { typed: query }, function (rsp) {
                                                        if (rsp && rsp.length > 0) {
                                                            mentions.cache[query] = rsp;
                                                            cb(jQuery.map(rsp, function (item) { return item.nickname + ' (' + item.username + ')'; }));
                                                        } else mentions.fail_cache.push(query);
                                                    });
                                                },
                                                updater: function (item) {
                                                    var pos = item.lastIndexOf('(') - 1;
                                                    if (pos > 0) item = item.slice(0, pos);
                                                    return item;
                                                },
                                                minLength: 2
                                            });
                                        } catch (e) {}
                                    } else {
                                        // minimal vanilla – show datalist-style dropdown
                                        var wrapper = document.createElement('div');
                                        wrapper.style.position = 'relative';
                                        inputEl.parentNode.insertBefore(wrapper, inputEl);
                                        wrapper.appendChild(inputEl);
                                        var listBox = document.createElement('div');
                                        listBox.style.position = 'absolute';
                                        listBox.style.zIndex = '10001';
                                        listBox.style.background = '#fff';
                                        listBox.style.border = '1px solid #ccc';
                                        listBox.style.width = '100%';
                                        listBox.style.display = 'none';
                                        wrapper.appendChild(listBox);
                                        var debounceTimer = null;
                                        inputEl.addEventListener('input', function () {
                                            var q = inputEl.value.trim();
                                            if (q.length < 2) { listBox.style.display = 'none'; return; }
                                            clearTimeout(debounceTimer);
                                            debounceTimer = setTimeout(function () {
                                                if (mentions.cache[q]) { renderList(mentions.cache[q]); return; }
                                                fetch('/home/mentions', { method: 'POST', headers: { 'Content-Type': 'application/x-www-form-urlencoded' }, body: 'typed=' + encodeURIComponent(q) })
                                                  .then(function (r) { return r.json(); }).then(function (rsp) {
                                                      if (rsp && rsp.length) { mentions.cache[q] = rsp; renderList(rsp); }
                                                      else { mentions.fail_cache.push(q); listBox.style.display='none'; }
                                                  });
                                            }, 300);
                                        });
                                        function renderList(users) {
                                            listBox.innerHTML = '';
                                            users.forEach(function (u) {
                                                var div = document.createElement('div');
                                                div.textContent = u.nickname + ' (' + u.username + ')';
                                                div.style.padding = '4px 6px';
                                                div.style.cursor = 'pointer';
                                                div.addEventListener('mouseenter', function () { div.style.background = '#e8e8e8'; });
                                                div.addEventListener('mouseleave', function () { div.style.background = ''; });
                                                div.addEventListener('click', function () {
                                                    inputEl.value = u.nickname; // or username
                                                    listBox.style.display = 'none';
                                                });
                                                listBox.appendChild(div);
                                            });
                                            listBox.style.display = users.length ? 'block' : 'none';
                                        }
                                    }
                                }
                            },
                            {
                                id: 'mentionStyle',
                                type: 'html',
                                html: '<style>.mention-suggestions ul{margin:0;padding:0;} .mention-suggestions li{list-style:none;} .mention-suggestions li.active{background:#0081c2;} .mention-suggestions li.active a{color:#fff;} .mention-suggestions a{text-decoration:none;color:#0070d6;} .typeahead.dropdown-menu{max-height:200px;overflow-y:auto;}</style>'
                            }
                        ]
                    }],
                    onShow: function () {
                        var selection = editor.getSelection(),
                            element = selection.getStartElement();
                        if (element) element = element.getAscendant('a', true);
                        if (!element || element.getName() !== 'a' || element.data('cke-realelement') || !element.data('mention')) {
                            element = editor.document.createElement('a');
                            element.data('mention', 1);
                            var txt = selection.getSelectedText();
                            element.setText(txt && txt[0] === '@' ? txt : '@' + (txt || ''));
                            this.insertMode = true;
                        } else this.insertMode = false;
                        this.element = element;
                        this.setupContent(this.element);
                    },
                    onOk: function () {
                        var abbr = this.element;
                        this.commitContent(abbr);
                        if (this.insertMode) editor.insertElement(abbr);
                    }
                };
            });

            editor.addCommand('Mentions', new CKEDITOR.dialogCommand('MentionsDialog'));
            editor.ui.addButton('Mentions', { label: '提及用户', command: 'Mentions', toolbar: 'insert,50' });

            if (editor.contextMenu) {
                editor.addMenuGroup('mentionGroup');
                editor.addMenuItem('mentionItem', {
                    label: '编辑提及',
                    icon: this.path + 'icons/mentions.png',
                    command: 'Mentions',
                    group: 'mentionGroup'
                });
                editor.contextMenu.addListener(function (element) {
                    var e = element.getAscendant('a', true);
                    if (e && e.data('mention')) return { mentionItem: CKEDITOR.TRISTATE_OFF };
                });
            }

            editor.on('key', function (e) {
                if (!mentions.observe) return;
                switch (e.data.keyCode) {
                    case 9: // tab
                    case 13: // enter
                        e.cancel();
                        var active = mentions.mention_suggestions ? mentions.mention_suggestions.querySelector('li.active') : null;
                        if (active) {
                            // simulate click
                            active.click();
                        }
                        break;
                    case 38: // up
                        e.cancel();
                        mentions.prev_tooltip();
                        // re-apply active style
                        if (mentions.mention_suggestions) {
                            var lis = mentions.mention_suggestions.querySelectorAll('li');
                            lis.forEach(function (li) { li.style.background = ''; var a = li.querySelector('a'); if (a) a.style.color = ''; });
                            var act = mentions.mention_suggestions.querySelector('li.active');
                            if (act) { act.style.background = '#0081c2'; var a2 = act.querySelector('a'); if (a2) a2.style.color = '#fff'; }
                        }
                        break;
                    case 40: // down
                        e.cancel();
                        mentions.next_tooltip();
                        if (mentions.mention_suggestions) {
                            var lis2 = mentions.mention_suggestions.querySelectorAll('li');
                            lis2.forEach(function (li) { li.style.background = ''; var a = li.querySelector('a'); if (a) a.style.color = ''; });
                            var act2 = mentions.mention_suggestions.querySelector('li.active');
                            if (act2) { act2.style.background = '#0081c2'; var a3 = act2.querySelector('a'); if (a3) a3.style.color = '#fff'; }
                        }
                        break;
                    default:
                        if (mentions.break_on(e.data.keyCode)) mentions.stop_observing();
                        break;
                }
            });

            editor.on('contentDom', function () {
                var editable = editor.editable();
                editable.attachListener(editable, 'input', function () {
                    var selection, range, epos, txt, spos, typed, typed_char;
                    try {
                        selection = this.editor.getSelection();
                        range = selection.getRanges()[0];
                        epos = range.startOffset;
                        var container = range.startContainer;
                        var domNode = container.$ || container;
                        txt = domNode.textContent || '';
                        spos = txt.substring(0, epos).lastIndexOf('@');
                        if (spos < mentions.observe_startpos) {
                            if (mentions.observe) mentions.stop_observing();
                            return;
                        }
                        typed = txt.substring(spos, epos);
                        typed_char = typed.length ? typed.charCodeAt(typed.length - 1) : 0;

                        if (typed_char === 64) {
                            var parent = domNode.parentElement || domNode.parentNode;
                            if (parent && parent.tagName === 'A') {
                                if (mentions.observe) mentions.stop_observing();
                                return;
                            }
                            mentions.start_observing(domNode, spos);
                        }
                        if (mentions.observe === 1) {
                            if ((mentions.char_input.length > 0 && typed_char === 64) || mentions.char_input.length > 20 || mentions.observe_node !== domNode) {
                                mentions.stop_observing();
                                if (typed_char === 64) {
                                    mentions.start_observing(domNode, spos);
                                    mentions.char_input = typed.split('');
                                }
                            } else {
                                mentions.char_input = typed.split('');
                                mentions.get_people(selection);
                            }
                        }
                    } catch (ex) {}
                });
            });

            // cleanup on destroy
            editor.on('destroy', function () {
                try { mentions.delete_tooltip(); } catch (e) {}
            });
        }
    });
})();
