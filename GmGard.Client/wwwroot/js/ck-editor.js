window.gmCkEditor = (function () {
  let loadingPromise = null;

  function loadCkEditor() {
    if (window.CKEDITOR) return Promise.resolve();
    if (loadingPromise) return loadingPromise;
    loadingPromise = new Promise((resolve, reject) => {
      const script = document.createElement("script");
      script.type = "text/javascript";
      script.src = "/ckeditor/ckeditor.js";
      script.onload = () => resolve();
      script.onerror = (e) => reject(e);
      document.head.appendChild(script);
    });
    return loadingPromise;
  }

  function getConfig() {
    const config = {};
    config.extraAllowedContent = "a[rel]; span(*); table(*); ruby; video[*]; audio[*]; source[*]; img[src,alt,width,height,class,style]; a[data-mention]";
    config.extraPlugins = "custom_smiley,colorbutton,font,mediaembed,rubymarkup,spoiler,mentions";
    config.removeButtons = "Underline,Subscript,Superscript";
    config.format_tags = "p;h1;h2;h3;h4;h5;pre";
    config.removeDialogTabs = "image:advanced;link:advanced";
    config.removePlugins = "elementspath";
    config.customConfig = "/ckeditor/smiley_config.js";
    // Blazor-only: use flat spoiler style for editor iframe, NOT legacy gradient. Legacy config at /ckeditor/config.js still uses default contents.css only.
    config.contentsCss = ["/ckeditor/contents.css", "/app/css/blazor-ckeditor.css"];
    config.skin = "moono-lisa";
    config.toolbar = [
      ["Undo", "Redo"],
      ["Link", "Unlink", "Anchor"],
      ["MediaEmbed", "Image", "Custom_Smiley"],
      ["Table", "HorizontalRule", "Spoiler", "Mentions", "RubyMarkup"],
      ["Maximize"],
      ["Source"],
      ["TextColor", "BGColor"],
      "/",
      ["Bold", "Italic", "Strike", "-", "RemoveFormat"],
      ["NumberedList", "BulletedList", "-", "Outdent", "Indent", "-", "Blockquote"],
      ["Styles", "Format", "FontSize"]
    ];
    config.height = 300;
    config.allowedContent = true; // let sanitizer handle server side
    return config;
  }

  function getReplyConfig() {
    const cfg = getConfig();
    cfg.height = 120;
    cfg.toolbar = [
      ["Bold","Italic","Link","Unlink","Custom_Smiley"],
      ["Source"]
    ];
    return cfg;
  }

  return {
    init: async function (elementId, dotNetRef, initialData, readOnly) {
      await loadCkEditor();
      const ta = document.getElementById(elementId);
      if (!ta) return;
      if (CKEDITOR.instances[elementId]) {
        CKEDITOR.instances[elementId].destroy(true);
      }
      const config = getConfig();
      config.readOnly = !!readOnly;
      const editor = CKEDITOR.replace(elementId, config);
      editor.setData(initialData || "");
      editor.on("change", function () {
        const data = editor.getData();
        dotNetRef.invokeMethodAsync("OnEditorChange", data);
      });
      editor.on("blur", function () {
        const data = editor.getData();
        dotNetRef.invokeMethodAsync("OnEditorChange", data);
      });
    },
    // ReplyView-like lazy init: hidden textarea -> show CKEditor on demand
    initInlineReply: async function (elementId, dotNetRef) {
      await loadCkEditor();
      const ta = document.getElementById(elementId);
      if (!ta) return;
      if (CKEDITOR.instances[elementId]) {
        CKEDITOR.instances[elementId].destroy(true);
      }
      ta.style.visibility = "";
      ta.style.height = "";
      const config = getReplyConfig();
      const editor = CKEDITOR.replace(elementId, config);
      editor.on("change", function () {
        const data = editor.getData();
        try { dotNetRef.invokeMethodAsync("OnEditorChange", data); } catch{}
      });
      editor.on("blur", function () {
        const data = editor.getData();
        try { dotNetRef.invokeMethodAsync("OnEditorChange", data); } catch{}
      });
    },
    getData: function (elementId) {
      const inst = CKEDITOR.instances[elementId];
      return inst ? inst.getData() : "";
    },
    setDataIfDifferent: function (elementId, data) {
      const inst = CKEDITOR.instances[elementId];
      if (!inst) return;
      const current = inst.getData();
      if (current !== data) {
        inst.setData(data || "");
      }
    },
    destroy: function (elementId) {
      const inst = CKEDITOR.instances[elementId];
      if (inst) {
        try { inst.destroy(true); } catch {}
      }
      const ta = document.getElementById(elementId);
      if (ta) {
        ta.style.visibility = "hidden";
        ta.style.height = "0";
      }
    }
  };
})();
