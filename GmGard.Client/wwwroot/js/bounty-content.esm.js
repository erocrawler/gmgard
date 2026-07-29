let dotNetRef = null;

function makeImagesClickable(rootSelector) {
  const roots = document.querySelectorAll(rootSelector);
  roots.forEach(root => {
    if (root.dataset.lightboxPrepared) {
      root.querySelectorAll("img:not(.lightbox-ready)").forEach(prepImg);
      return;
    }
    root.dataset.lightboxPrepared = "1";
    root.querySelectorAll("img").forEach(prepImg);
    root.addEventListener("click", function(e) {
      const target = e.target;
      if (!(target instanceof Element)) return;
      if (target.tagName !== "IMG") return;
      const src = target.getAttribute("src") || target.src || "";
      if (!src || src.includes("/smiley/")) return;
      if (target.closest(".avatar")) return;
      const allImgs = Array.from(root.querySelectorAll("img"))
        .filter(img => {
          const s = img.getAttribute("src")||img.src||"";
          if (!s || s.includes("/smiley/") || (img.closest && img.closest(".avatar"))) return false;
          return true;
        }).map(img => img.src);
      let idx = allImgs.findIndex(s => s === target.src || target.src.includes(s) || s.includes(target.src));
      if (idx < 0) idx = 0;
      if (dotNetRef) dotNetRef.invokeMethodAsync("OpenLightboxJs", allImgs, idx);
    });
  });
}
function prepImg(img) {
  const src = img.getAttribute("src")||"";
  if (src.includes("/smiley/") || (img.closest && img.closest(".avatar"))) return;
  img.style.cursor = "zoom-in";
  img.classList.add("lightbox-ready","bounty-img-zoomable");
}
function bindSpoilerCore() {
  document.querySelectorAll('.bounty-content div.spoiler, .bounty-answer-content div.spoiler').forEach(wrapper => {
    if (wrapper.dataset.boundSpoiler) return;
    wrapper.dataset.boundSpoiler = "1";
    const title = wrapper.querySelector(':scope > .spoiler-title');
    const content = wrapper.querySelector(':scope > .spoiler-content');
    if (!title || !content) return;
    title.classList.remove('hide-icon'); title.classList.add('show-icon');
    wrapper.classList.remove('is-expanded'); content.classList.remove('is-visible');
    content.style.display = 'none';
    title.addEventListener('click', () => {
      if (content.style.display === 'none') {
        title.classList.remove('show-icon'); title.classList.add('hide-icon');
        wrapper.classList.add('is-expanded'); content.classList.add('is-visible');
        content.style.display = 'block';
      } else {
        title.classList.remove('hide-icon'); title.classList.add('show-icon');
        wrapper.classList.remove('is-expanded'); content.classList.remove('is-visible');
        content.style.display = 'none';
      }
    });
  });
}
function scrollToHashImpl(hash, withOffset, shouldHighlight) {
  if (!hash) hash = window.location.hash;
  if (!hash) return;
  const id = hash.startsWith("#") ? hash.substring(1) : hash;
  let el = document.getElementById(id);
  if (!el) {
    const num = id.replace(/^\D+/g, '');
    if (num) el = document.getElementById("postcontent"+num) || document.getElementById("reply"+num) || document.getElementById("listpost"+num);
  }
  if (el) {
    if (shouldHighlight) {
      const origBg = el.style.backgroundColor;
      el.style.transition = "background-color 0.3s";
      el.style.backgroundColor = "color-mix(in srgb, var(--color-warning) 30%, transparent)";
      setTimeout(() => { el.style.backgroundColor = origBg; }, 3000);
    }
    const top = el.getBoundingClientRect().top + window.scrollY - (withOffset ? 100 : 80);
    window.scrollTo({ top, behavior: "smooth" });
  }
}

export function register(ref) { dotNetRef = ref; }
export function bind() {
  makeImagesClickable(".bounty-content");
  makeImagesClickable(".bounty-answer-content");
  bindSpoilerCore();
}
export const bindSpoiler = bindSpoilerCore;
export function scrollToHash(hash, withOffset, shouldHighlight) { scrollToHashImpl(hash, withOffset, shouldHighlight); }
export function scrollToId(id, highlight) {
  if (!id) return;
  const hash = id.toString().startsWith("#") ? id : "#" + id.replace(/^#/, "");
  setTimeout(() => scrollToHashImpl(hash, true, !!highlight), 150);
}
export function observe() {
  const obs = new MutationObserver(() => { bind(); });
  obs.observe(document.body, { childList: true, subtree: true });
  setTimeout(() => {
    bind();
    if (window.location.hash) setTimeout(() => scrollToHashImpl(window.location.hash, true, false), 300);
  }, 300);
}

// legacy global shim
if (!window.gmBountyContent) {
  window.gmBountyContent = { register, bind, bindSpoiler: bindSpoilerCore, scrollToHash: scrollToHashImpl, scrollToId, observe };
  // keep old behavior: auto-observe was in original file
  window.gmBountyContent.observe();
}
