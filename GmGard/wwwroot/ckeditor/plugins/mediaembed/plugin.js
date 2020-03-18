/*
* Embed Media Dialog based on http://www.fluidbyte.net/embed-youtube-vimeo-etc-into-ckeditor
*
* Plugin name:      mediaembed
* Menu button name: MediaEmbed
*
* Youtube Editor Icon
* http://paulrobertlloyd.com/
*
* @author Fabian Vogelsteller [frozeman.de]
* @version 0.5
*/
( function() {
    CKEDITOR.plugins.add( 'mediaembed',
    {
        icons: 'mediaembed', // %REMOVE_LINE_CORE%
        hidpi: true, // %REMOVE_LINE_CORE%
        init: function( editor )
        {
           CKEDITOR.dialog.add( 'MediaEmbedDialog', function (instance)
           {
              return {
                 title : '嵌入媒体',
                 minWidth : 550,
                 minHeight : 200,
                 contents :
                       [
                          {
                             id : 'iframe',
                             expand : true,
                             elements :[{
                                id : 'embedArea',
                                type : 'textarea',
                                label : '请在这里粘贴视频的&lt;embed&gt;代码',
                                'autofocus':'autofocus',
                                setup: function(element){
                                },
                                commit: function(element){
                                }
                              }]
                          }
                       ],
                  onOk: function() {
                      var fragment = CKEDITOR.htmlParser.fragment.fromHtml('<p>'+ this.getContentElement('iframe', 'embedArea').getValue() + '</p>');
                      instance.filter.applyTo(fragment);
                      var writer = new CKEDITOR.htmlWriter();
                      fragment.writeHtml(writer);
                      instance.insertHtml(writer.getHtml());
                  }
              };
           } );

            editor.addCommand( 'MediaEmbed', new CKEDITOR.dialogCommand( 'MediaEmbedDialog',
                { allowedContent: 'iframe[*]' }
            ) );

            editor.ui.addButton( 'MediaEmbed',
            {
                label: '嵌入媒体',
                command: 'MediaEmbed',
                toolbar: 'mediaembed'
            } );
        }
    } );
} )();
