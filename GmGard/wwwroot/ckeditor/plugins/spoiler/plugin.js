CKEDITOR.plugins.add( 'spoiler' , {
	lang: 'en,ru,zh,zh-cn',
	icons: 'spoiler',
	init: function( editor ) {
		if ( editor.blockless )
			return;

		function registerCssFile( url ) {
			var head = editor.document.getHead();
			var link = editor.document.createElement( 'link' , {
				attributes: {
					type: 'text/css',
					rel: 'stylesheet',
					href: url
				}
			} );
			head.append( link );
		}

		function toggle( element ) {
			element.setStyle( 'display' , ( ( element.getStyle('display') == 'none' ) ? '' : 'none' ) );
		}

		function toggleClass( element, className ) {
			if ( element.hasClass( className ) ) {
				element.removeClass( className );
			}
			else {
				element.addClass( className );
			}
		}

		function createSpoiler() {
			var spoilerContainer = editor.document.createElement( 'div', { 'attributes' : { 'class': 'spoiler' } } );
			var spoilerTitle = editor.document.createElement('div', { 'attributes': { 'class': 'spoiler-title hide-icon' } });
			var spoilerContent = editor.document.createElement('div', { 'attributes': { 'class': 'spoiler-content' } });
			spoilerTitle.appendHtml( '<br>' );
			spoilerContent.appendHtml( '<p><br></p>' );
			spoilerContainer.append( spoilerTitle );
			spoilerContainer.append( spoilerContent );
			return spoilerContainer;
		}

		function getDivWithClass( className )
		{
			var divs =  editor.document.getElementsByTag( 'div' ),
				len = divs.count(),
				elements = [],
				element;
			for ( var i = 0; i < len; ++i ) {
				element = divs.getItem( i );
				if ( element.hasClass( className ) ) {
					elements.push( element );
				}
			}
			return elements;
		}

		editor.addCommand( 'spoiler', {
			exec: function( editor ) {
				var spoiler = createSpoiler();
				editor.insertElement( spoiler );
			},
			allowedContent: 'div{*}(*);br'
		});

		editor.ui.addButton( 'Spoiler', {
			label: editor.lang.spoiler.toolbar,
			command: 'spoiler',
			toolbar: 'insert'
		});

		var path = this.path;
		editor.on( 'mode', function() {
			if ( this.mode != 'wysiwyg' ) {
				return;
			}
			registerCssFile( path + 'css/spoiler.css' );
		});

		editor.on('key', function (e) {
		    if (e.data.keyCode == 13) {
		        var selection = editor.getSelection();
		        var element = selection.getStartElement();
		        if (element.getName().toLowerCase() == 'div' && element.hasClass('spoiler-title')) {
		            e.cancel();
		            var range = editor.createRange();
		            var nextEl = element.getNext();
		            range.setStart(nextEl, 0);
		            range.setEnd(nextEl, 0);
		            selection.selectRanges([range]);
		        }
		    }
		});
	}
});