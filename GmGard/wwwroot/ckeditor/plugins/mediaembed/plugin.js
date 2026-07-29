/*
* Embed Media Dialog based on http://www.fluidbyte.net/embed-youtube-vimeo-etc-into-ckeditor
* Extended to allow <video> and <audio> (2026 patch) and removed Flash dependency.
*
* Plugin name:      mediaembed
* Menu button name: MediaEmbed
*
* Youtube Editor Icon
* http://paulrobertlloyd.com/
*
* @author Fabian Vogelsteller [frozeman.de]
* @version 0.6 - video/audio support
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
                                label : '请在这里粘贴视频/音频的&lt;video&gt; / &lt;audio&gt; 代码 (支持 &lt;video controls&gt;&lt;source src=&quot;&quot;&gt;&lt;/video&gt; 直链)',
                                'autofocus':'autofocus',
                                setup: function(element){
                                },
                                commit: function(element){
                                }
                              }]
                          }
                       ],
                  onOk: function() {
                      var raw = this.getContentElement('iframe', 'embedArea').getValue();
                      // Strip iframe/embed completely - only allow video/audio/source
                      raw = raw.replace(/<iframe[\s\S]*?<\/iframe>/gi, '');
                      raw = raw.replace(/<iframe[^>]*\/?>/gi, '');
                      raw = raw.replace(/<embed[^>]*>/gi, '');
                      raw = raw.replace(/<object[\s\S]*?<\/object>/gi, '');
                      var fragment = CKEDITOR.htmlParser.fragment.fromHtml('<p>'+ raw + '</p>');
                      instance.filter.applyTo(fragment);
                      var writer = new CKEDITOR.htmlWriter();
                      fragment.writeHtml(writer);
                      instance.insertHtml(writer.getHtml());
                  }
              };
           } );

            editor.addCommand( 'MediaEmbed', new CKEDITOR.dialogCommand( 'MediaEmbedDialog',
                { allowedContent: 'video[*]; audio[*]; source[*]' }
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
