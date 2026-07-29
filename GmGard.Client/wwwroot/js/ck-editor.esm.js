let loadingPromise = null;

function loadCkEditorCore() {
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
  config.allowedContent = true;
  return config;
}

function getReplyConfig() {
  const cfg = getConfig();
  cfg.height = 120;
  cfg.toolbar = [["Bold", "Italic", "Link", "Unlink", "Custom_Smiley"], ["Source"]];
  return cfg;
}

export async function init(elementId, dotNetRef, initialData, readOnly) {
  await loadCkEditorCore();
  const ta = document.getElementById(elementId);
  if (!ta) return;
  if (CKEDITOR.instances[elementId]) {
    CKEDITOR.instances[elementId].destroy(true);
  }
  const config = getConfig();
  config.readOnly = !!readOnly;
  const editor = CKEDITOR.replace(elementId, config);
  editor.setData(initialData || "");
  editor.on("change", () => dotNetRef.invokeMethodAsync("OnEditorChange", editor.getData()));
  editor.on("blur", () => dotNetRef.invokeMethodAsync("OnEditorChange", editor.getData()));
}

export async function initInlineReply(elementId, dotNetRef) {
  await loadCkEditorCore();
  const ta = document.getElementById(elementId);
  if (!ta) return;
  if (CKEDITOR.instances[elementId]) CKEDITOR.instances[elementId].destroy(true);
  ta.style.visibility = "";
  ta.style.height = "";
  const config = getReplyConfig();
  const editor = CKEDITOR.replace(elementId, config);
  editor.on("change", () => { try { dotNetRef.invokeMethodAsync("OnEditorChange", editor.getData()); } catch {} });
  editor.on("blur", () => { try { dotNetRef.invokeMethodAsync("OnEditorChange", editor.getData()); } catch {} });
}

export function getData(elementId) {
  const inst = CKEDITOR.instances[elementId];
  return inst ? inst.getData() : "";
}

export function setDataIfDifferent(elementId, data) {
  const inst = CKEDITOR.instances[elementId];
  if (!inst) return;
  const current = inst.getData();
  if (current !== data) inst.setData(data || "");
}

export function destroy(elementId) {
  const inst = CKEDITOR.instances[elementId];
  if (inst) { try { inst.destroy(true); } catch {} }
  const ta = document.getElementById(elementId);
  if (ta) { ta.style.visibility = "hidden"; ta.style.height = "0"; }
}

// backward compat: keep window global for legacy callers still using gmCkEditor.*
if (!window.gmCkEditor) {
  window.gmCkEditor = { init, initInlineReply, getData, setDataIfDifferent, destroy };
}
