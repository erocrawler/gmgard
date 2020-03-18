///////////////////////////////////////////////////////////////
//      CKEDITOR_mentions helper class
///////////////////////////////////////////////////////////////

/*
 * Helper class needed to handle mentions.
 * This class is a singleton for each instance of CKEDITOR.
 *
 * @param {Object} editor An instance of a CKEDITOR
 * @returns {null}
 */
function CKEDITOR_mentions (editor) {
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

/*
 * Collection of pairs editor id / instance of CKEDITOR_mentions
 *
 * @type Array
 */
CKEDITOR_mentions.instances = [];

/*
 * Delay of the timeout between the last key pressed and the ajax query. It's use to prevent ajax flooding when user types fast.
 *
 * @type Number
 */

CKEDITOR_mentions.timeout_delay = 500;

/*
 * Minimum number of characters needed to start searching for users (includes the @).
 *
 * @type Number
 */

CKEDITOR_mentions.start_observe_count = 2;

/*
 * Method used to get an instance of CKEDITOR_mentions linked to an instance of CKEDITOR.
 * Its design is based on the singleton design pattern.
 *
 * @param {Object} editor An instance of a CKEDITOR
 * @returns An instance of CKEDITOR_mentions
 */
CKEDITOR_mentions.get_instance = function (editor) {
  // we browse our collection of instances
  for (var i in this.instances) {
    // if we find an CKEDITOR instance in our collection
    if (this.instances[i].id === editor.id) {
      // we return the instance of CKEDITOR_mentions that match
      return this.instances[i].instance;
    }
  }

  // if no match was found, we add a row in our collection with the current CKEDITOR id and we instanciate CKEDITOR_mentions
  this.instances.push({
    id: editor.id,
    instance: new CKEDITOR_mentions(editor)
  });
  // we return the instance of CKEDITOR_mentions that was just created
  return this.instances[this.instances.length - 1].instance;
};

/*
 * This method creates the suggestions for the div
 *
 * @returns the ul element node
 */
CKEDITOR_mentions.prototype.create_tooltip = function (users) {
    var ul = document.createElement('ul');
    ul.className = 'typeahead dropdown-menu show';
    for (var i = 0; i < users.length; i++) {
        var li = document.createElement('li');
        jQuery(li).html('<a>' + users[i].nickname + ' (' + users[i].username + ')</a>').addClass('mention-users').attr({
            'data-username': users[i].username,
            'data-nickname': users[i].nickname
        }).appendTo(ul);
    }
    ul.firstElementChild.className += ' active';
    return ul;
};

/*
 * This method delete the div containing the suggestions
 *
 * @returns {null}
 */
CKEDITOR_mentions.prototype.delete_tooltip = function () {
    if (this.mention_suggestions) {
        this.mention_suggestions.remove();
        this.mention_suggestions = null;
    }
};

/*
 * Move selected tooltip to next one
 *
 * @returns {null}
 */
CKEDITOR_mentions.prototype.next_tooltip = function () {
    s = this.mention_suggestions;
    var item = s.find('.active').removeClass('active')
       , next = item.next();
    if (!next.length)
        next = s.find('li').eq(0);
    next.addClass('active');
};

/*
 * Move selected tooltip to previous one
 *
 * @returns {null}
 */
CKEDITOR_mentions.prototype.prev_tooltip = function () {
    s = this.mention_suggestions;
    var item = s.find('.active').removeClass('active')
       , prev = item.prev();
    if (!prev.length)
        prev = s.find('li').last();
    prev.addClass('active');
};

/*
 * This method start the observation of the typed characters
 *
 * @returns {null}
 */
CKEDITOR_mentions.prototype.start_observing = function (textNode, startpos) {
    this.observe = 1;
    this.observe_node = textNode;
    this.observe_startpos = startpos;
};

/*
 * This method halts the observation of the typed characters and flush the properties used by CKEDITOR_mentions
 *
 * @returns {null}
 */
CKEDITOR_mentions.prototype.stop_observing = function () {
    this.observe = 0;
    this.observe_startpos = 0;
    this.char_input = [];
    this.observe_node = null;
    this.delete_tooltip();
};

/*
 * This methods send an ajax query to durpal ckeditor_mentions module and retrieve matching user.
 *
 * @param {Object} selection result of CKEDITOR.editor.getSelection()
 * @returns {null}
 */
CKEDITOR_mentions.prototype.get_people = function (selection) {
  if (null !== this.timeout_id) {
    clearTimeout(this.timeout_id);
  }
  this.timeout_id = setTimeout(this.timeout_callback, CKEDITOR_mentions.timeout_delay, [this, selection]);
}

 /*
 * This methods send an ajax query to durpal ckeditor_mentions module and retrieve matching user.
 * 
 * @param {Array} args An Array of parameters containing the current instance of CKEDITOR_mentions and selection (cf. CKEDITOR_mentions.prototype.get_people)
 * @returns {null}
 */
CKEDITOR_mentions.prototype.timeout_callback = function (args) {
  var mentions   = args[0];
  var selection  = args[1];
  var str        = mentions.char_input.join('');

  //if less than 3 char are input (including @) we don't try to get people
  if (str.length < CKEDITOR_mentions.start_observe_count) {
    mentions.delete_tooltip();
    return;
  }
  var editor       = mentions.editor;
  var element_id = editor.name;
  var range        = selection.getRanges()[0];
  var startOffset = parseInt(range.startOffset - str.length) || 0;
  var endOffset = range.startOffset;
  var element = range.startContainer.$;
  var node = document.createRange();
  node.selectNodeContents(element);
  var update_tooltip = function (rsp) {

      var ckel = $('#cke_' + element_id);
      var par = ckel.find('iframe.cke_wysiwyg_frame');
      var curpos = node.getBoundingClientRect();
      var parpos = par.offset();
      var curtop = parpos.top + curpos.top + curpos.height;//parseInt($(cur).css('line-height'), 10);
      var curleft = parpos.left + curpos.left;
      if (mentions.mention_suggestions) {
          mentions.mention_suggestions.remove();
      }
      if (rsp && rsp.length > 0) {
          var userlist = mentions.create_tooltip(rsp);
          var sdiv = $('<div class="mention-suggestions"></div>').html(userlist).insertAfter('body');
          mentions.mention_suggestions = sdiv;
          var coord = getCaretCoordinatesFn(element.parentElement, range.startOffset);
          sdiv.css({
              zIndex: 1000,
              position: 'absolute',
              top: curtop + sdiv.height(),
              left: curleft + coord.left
          });
          sdiv.find('.mention-users').click(function (e) {
              e.preventDefault();

              var mentions = CKEDITOR_mentions.get_instance(editor);
              mentions.stop_observing();
              // Create link
              var link = document.createElement('a');
              var username = $(this).data('username');
              link.href = '/user/' + username;
              link.textContent = '@' + $(this).data('nickname');
              link.setAttribute('data-mention', true);
              var end_elem = null;
              if (element.textContent.length > endOffset) {
                  end_elem = document.createTextNode(element.textContent.substr(endOffset));
              }
              // Shorten text node
              element.textContent = element.textContent.substr(0, startOffset);

              // Insert link after text node
              // this is used when the link is inserted not at the end of the text
              if (element.nextSibling) {
                  element.parentNode.insertBefore(link, element.nextSibling);
              }
                  // at the end of the editor text
              else {
                  element.parentNode.appendChild(link);
              }
              if (end_elem) {
                  if (link.nextSibling) {
                      link.parentNode.insertBefore(end_elem, link.nextSibling);
                  }
                  else {
                      link.parentNode.appendChild(end_elem);
                  }
              }
              editor.focus();
              var range = editor.createRange(),
              el = new CKEDITOR.dom.element(link.parentNode);
              range.moveToElementEditablePosition(el, link.parentNode.textContent.length);
              range.select();
          });
      } else {
          mentions.fail_cache.push(str);
          return;
      }
  };
  if (mentions.cache[str]) {
      update_tooltip(mentions.cache[str]);
      return;
  } else if (mentions.check_fail(str)){
      return;
  }
  
  $.post('/home/mentions', { typed: str }, function (rsp) {
      mentions.cache[str] = rsp;
      update_tooltip(rsp);
  });

};

CKEDITOR_mentions.prototype.check_fail = function (str) {
    for (var i = 0; i < this.fail_cache.length; i++) {
        if (str.lastIndexOf(this.fail_cache[i], 0) === 0) {
            return true;
        }
    }
    return false
}

/*
 * This method returns if a char should stop the observation.
 *
 * @param {int} charcode A character key code
 * @returns {Boolean} Whether or not the char should stop the observation
 */
CKEDITOR_mentions.prototype.break_on = function (charcode) {
    // 27 = esc
  // 37 = left key
  // 39 = right key
  // 46 = delete
  // 91 = home/end (?)
  var special = [27, 37, 39, 46, 91];
  for (var i = 0; i < special.length; i++) {
    if (special[i] == charcode) {
      return true;
    }
  }
  return false;
};


///////////////////////////////////////////////////////////////
//      Plugin implementation
///////////////////////////////////////////////////////////////
(function($){
  CKEDITOR.plugins.add('mentions', {
    icons: 'mentions', // %REMOVE_LINE_CORE%
    init: function(editor) {
        var mentions = CKEDITOR_mentions.get_instance(editor);

        CKEDITOR.dialog.add('MentionsDialog', function (instance) {
            return {
                title: '提及',
                minWidth: 340,
                minHeight: 180,
                contents:
                      [
                         {
                             id: 'main',
                             expand: true,
                             elements: [{
                                 id: 'mentionArea',
                                 type: 'text',
                                 label: '请输入要提及的用户的昵称或账户名',
                                 'autofocus': 'autofocus',
                                 validate: CKEDITOR.dialog.validate.notEmpty("请输入用户名"),
                                 setup: function (element) {
                                     this.setValue(element.getText().slice(1));
                                 },
                                 commit: function (element) {
                                     var name = this.getValue();
                                     if (name) {
                                         element.setHtml('<a data-mention="1" href="/User/' + name + '">@' + name + '</a>');
                                     }
                                 },
                                 onLoad: function (event) {
                                     var input = this.getInputElement();
                                     $(input.$).typeahead({
                                         source: function (query, cb) {
                                             if (mentions.cache[query]) {
                                                 return $.map(mentions.cache[query], function (item, index) {
                                                     return item.nickname + ' (' + item.username + ')';
                                                 });
                                             } else if (mentions.check_fail(query)) {
                                                 return;
                                             }
                                             $.post('/home/mentions', { typed: query }, function (rsp) {
                                                 if (rsp && rsp.length > 0) {
                                                     mentions.cache[query] = rsp;
                                                     cb($.map(rsp, function (item, index) {
                                                         return item.nickname + ' (' + item.username + ')';
                                                     }));
                                                 } else {
                                                     mentions.fail_cache.push(query);
                                                 }
                                             });
                                         },
                                         updater: function (item) {
                                             var pos = item.lastIndexOf('(') - 1;
                                             if (pos > 0) {
                                                 item = item.slice(0, pos);
                                             }
                                             return item;
                                         },
                                         minLength: 2
                                     });
                                 }
                             },
                             {
                                 id: 'mentionStyle',
                                 type: 'html',
                                 html: '<style>.cke_reset_all .dropdown-menu {\
                                        color: #fff;\
                                        position: absolute;\
                                        display: none;\
                                        float: left;\
                                        min-width: 160px;\
                                        padding: 5px 0;\
                                        margin: 2px 0 0;\
                                        list-style: none;\
                                        background-color: #fff;\
                                        border: 1px solid #ccc;\
                                        border: 1px solid rgba(0,0,0,0.2);\
                                        border-radius: 6px;\
                                        box-shadow: 0 5px 10px rgba(0,0,0,0.2);\
                                        }.dropdown-menu .active strong {color: #fff;}\
                                        </style>'
                             }
                            ]
                         }
                      ],
                onShow: function () {
                    var selection = editor.getSelection(),
                        element = selection.getStartElement();
                    if (element)
                        element = element.getAscendant('a', true);

                    if (!element || element.getName() != 'a' || element.data('cke-realelement') || !element.data('mention')) {
                        element = editor.document.createElement('a');
                        element.data('mention', 1);
                        var txt = selection.getSelectedText();
                        element.setText(txt[0] == '@' ? txt : '@' + txt);
                        this.insertMode = true;
                    }
                    else
                        this.insertMode = false;

                    this.element = element;
                    this.setupContent(this.element);
                },
                onOk: function () {
                    var dialog = this,
                            abbr = this.element;

                    this.commitContent(abbr);

                    if (this.insertMode) {
                        editor.insertElement(abbr);
                    }
                }
            };
        });

        editor.addCommand('Mentions', new CKEDITOR.dialogCommand('MentionsDialog'));

        editor.ui.addButton( 'Mentions',
            {
                label: '提及用户',
                command: 'Mentions',
                toolbar: 'insert,50'
            });
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
                if (e && e.data('mention')) {
                    return { mentionItem: CKEDITOR.TRISTATE_OFF };
                }
            });
        }

        editor.on('key', function (e) {
            if (mentions.observe) {
                switch (e.data.keyCode) {
                    case 9: // tab
                    case 13: // enter
                        e.cancel();
                        mentions.mention_suggestions.find('.active').trigger('click');
                        break;

                    case 38: // up arrow
                        e.cancel();
                        mentions.prev_tooltip();
                        break;

                    case 40: // down arrow
                        e.cancel();
                        mentions.next_tooltip();
                        break;
                    default:
                        // things which should trigger a stop observing, like Enter, home, etc.
                        if (mentions.break_on(e.data.keyCode)) {
                            mentions.stop_observing();
                        }
                        break;
                }
            }
        });

        editor.on('contentDom', function (e) {
            var editable = editor.editable();
            editable.attachListener(editable, 'input', function (evt) {
                //if (mentions.getting) {
                //    mentions.getting = false;
                //    return;
                //}
                var selection = this.editor.getSelection();
                var range = selection.getRanges()[0];
                var epos = range.startOffset;
                var txt = range.startContainer.$.textContent;
                var spos = txt.substring(0, epos).lastIndexOf('@');
                if (spos < mentions.observe_startpos) {
                    if (mentions.observe) mentions.stop_observing();
                    return;
                }
                var typed = txt.substring(spos, epos);
                var typed_char = typed.charCodeAt(typed.length - 1);

                if (typed_char === 64) {
                    if (range.startContainer.$.parentElement.tagName == 'A') {
                        if (mentions.observe) mentions.stop_observing();
                        return;
                    }
                    mentions.start_observing(range.startContainer.$, spos);
                }
                if (mentions.observe === 1) {
                    if ((mentions.char_input.length > 0 && typed_char === 64) || mentions.char_input.length > 20
                        || mentions.observe_node !== range.startContainer.$) {
                        mentions.stop_observing();
                        if (typed_char === 64) {
                            mentions.start_observing(range.startContainer.$, spos);
                            mentions.char_input = typed.split('');
                        }
                    } else {
                        mentions.char_input = typed.split('');
                        var selection = this.editor.getSelection();
                        mentions.get_people(selection);
                    }
                }
            });
        });
    } // end init function
  });
})(jQuery);

var properties = [
  'direction',  // RTL support
  'boxSizing',
  'width',  // on Chrome and IE, exclude the scrollbar, so the mirror div wraps exactly as the textarea does
  'height',
  'overflowX',
  'overflowY',  // copy the scrollbar for IE

  'borderTopWidth',
  'borderRightWidth',
  'borderBottomWidth',
  'borderLeftWidth',

  'paddingTop',
  'paddingRight',
  'paddingBottom',
  'paddingLeft',

  // https://developer.mozilla.org/en-US/docs/Web/CSS/font
  'fontStyle',
  'fontVariant',
  'fontWeight',
  'fontStretch',
  'fontSize',
  'fontSizeAdjust',
  'lineHeight',
  'fontFamily',

  'textAlign',
  'textTransform',
  'textIndent',
  'textDecoration',  // might not make a difference, but better be safe

  'letterSpacing',
  'wordSpacing'
];

var isFirefox = !(window.mozInnerScreenX == null);

var getCaretCoordinatesFn = function (element, position, recalculate) {
    // mirrored div
    var div = document.createElement('div');
    div.id = 'input-textarea-caret-position-mirror-div';
    var body = $(element).parents('body')[0];
    body.appendChild(div);

    var style = div.style;
    var computed = window.getComputedStyle ? getComputedStyle(element) : element.currentStyle;  // currentStyle for IE < 9

    // default textarea styles
    style.whiteSpace = 'pre-wrap';
    if (element.nodeName !== 'INPUT')
        style.wordWrap = 'break-word';  // only for textarea-s

    // position off-screen
    style.position = 'absolute';  // required to return coordinates properly
    style.visibility = 'hidden';  // not 'display: none' because we want rendering

    // transfer the element's properties to the div
    properties.forEach(function (prop) {
        style[prop] = computed[prop];
    });

    if (isFirefox) {
        style.width = parseInt(computed.width) - 2 + 'px'  // Firefox adds 2 pixels to the padding - https://bugzilla.mozilla.org/show_bug.cgi?id=753662
        // Firefox lies about the overflow property for textareas: https://bugzilla.mozilla.org/show_bug.cgi?id=984275
        if (element.scrollHeight > parseInt(computed.height))
            style.overflowY = 'scroll';
    } else {
        style.overflow = 'hidden';  // for Chrome to not render a scrollbar; IE keeps overflowY = 'scroll'
    }
    var text = jQuery(element).text();
    div.textContent = text.substring(0, position);
    // the second special handling for input type="text" vs textarea: spaces need to be replaced with non-breaking spaces - http://stackoverflow.com/a/13402035/1269037
    if (element.nodeName === 'INPUT')
        div.textContent = div.textContent.replace(/\s/g, "\u00a0");

    var span = document.createElement('span');
    // Wrapping must be replicated *exactly*, including when a long word gets
    // onto the next line, with whitespace at the end of the line before (#7).
    // The  *only* reliable way to do that is to copy the *entire* rest of the
    // textarea's content into the <span> created at the caret position.
    // for inputs, just '.' would be enough, but why bother?
    span.textContent = text.substring(position) || '.';  // || because a completely empty faux span doesn't render at all
    div.appendChild(span);

    var coordinates = {
        top: span.offsetTop + parseInt(computed['borderTopWidth']),
        left: span.offsetLeft + parseInt(computed['borderLeftWidth'])
    };

    body.removeChild(div);

    return coordinates;
}