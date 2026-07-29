window.gmBountyContent = (function() {
  let dotNetRef = null;

  function makeImagesClickable(rootSelector) {
    const roots = document.querySelectorAll(rootSelector);
    roots.forEach(root => {
      // Avoid double listener but we need to refresh image list - use flag
      if (root.dataset.lightboxPrepared) {
        // just ensure new imgs have cursor
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
        if (!src) return;
        if (src.includes("/smiley/")) return;
        if (target.closest(".avatar")) return;

        const allImgs = Array.from(root.querySelectorAll("img"))
          .filter(img => {
            const s = img.getAttribute("src")||img.src||"";
            if (!s) return false;
            if (s.includes("/smiley/")) return false;
            if (img.closest(".avatar")) return false;
            return true;
          })
          .map(img => img.src);

        let idx = allImgs.findIndex(s => s === target.src || target.src.includes(s) || s.includes(target.src));
        if (idx < 0) idx = 0;

        if (dotNetRef) {
          dotNetRef.invokeMethodAsync("OpenLightboxJs", allImgs, idx);
        }
      });
    });
  }

  function prepImg(img) {
    const src = img.getAttribute("src")||"";
    if (src.includes("/smiley/")) return;
    if (img.closest && img.closest(".avatar")) return;
    img.style.cursor = "zoom-in";
    img.classList.add("lightbox-ready");
    img.classList.add("bounty-img-zoomable");
  }

  function bindSpoiler() {
    document.querySelectorAll('.bounty-content div.spoiler, .bounty-answer-content div.spoiler').forEach(function(wrapper){
      if (wrapper.dataset.boundSpoiler) return;
      wrapper.dataset.boundSpoiler = "1";
      var title = wrapper.querySelector(':scope > .spoiler-title');
      var content = wrapper.querySelector(':scope > .spoiler-content');
      if (!title || !content) return;
      // ALWAYS collapsed by default, matching detailScripts.js $('div.spoiler-title').trigger('click') behavior
      title.classList.remove('hide-icon');
      title.classList.add('show-icon');
      wrapper.classList.remove('is-expanded');
      content.classList.remove('is-visible');
      content.style.display = 'none';
      title.addEventListener('click', function(){
        var isNowHidden = content.style.display === 'none' || content.style.display === '' && !content.classList.contains('is-visible');
        // Actually toggle based on current display
        if (content.style.display === 'none') {
          title.classList.remove('show-icon');
          title.classList.add('hide-icon');
          wrapper.classList.add('is-expanded');
          content.classList.add('is-visible');
          content.style.display = 'block';
        } else {
          title.classList.remove('hide-icon');
          title.classList.add('show-icon');
          wrapper.classList.remove('is-expanded');
          content.classList.remove('is-visible');
          content.style.display = 'none';
        }
      });
    });
  }

  function scrollToHash(hash, withOffset, shouldHighlight) {
    if (!hash) hash = window.location.hash;
    if (!hash) return;
    // Support #postcontent{id}, #reply{id}, #listpost, etc.
    const id = hash.startsWith("#") ? hash.substring(1) : hash;
    let el = document.getElementById(id);
    if (!el) {
      // Try numeric id only variant
      const num = id.replace(/^\D+/g, '');
      if (num) {
        el = document.getElementById("postcontent" + num) || document.getElementById("reply" + num) || document.getElementById("listpost" + num);
      }
    }
    if (el) {
      if (shouldHighlight) {
        // highlight like detailScripts.js does $(hash).css bg - only for reported/new
        const origBg = el.style.backgroundColor;
        el.style.transition = "background-color 0.3s";
        el.style.backgroundColor = "color-mix(in srgb, var(--color-warning) 30%, transparent)";
        setTimeout(() => { el.style.backgroundColor = origBg; }, 3000);
      }
      const top = el.getBoundingClientRect().top + window.scrollY - (withOffset ? 100 : 80);
      window.scrollTo({ top, behavior: "smooth" });
    }
  }

  return {
    register: function(ref) { dotNetRef = ref; },
    bind: function() {
      makeImagesClickable(".bounty-content");
      makeImagesClickable(".bounty-answer-content");
      bindSpoiler();
    },
    bindSpoiler: bindSpoiler,
    scrollToHash: scrollToHash,
    scrollToId: function(id, highlight) {
      if (!id) return;
      const hash = id.toString().startsWith("#") ? id : "#" + id.replace(/^#/, "");
      // Delay to allow Blazor render
      setTimeout(() => scrollToHash(hash, true, !!highlight), 150);
    },
    observe: function() {
      const obs = new MutationObserver(() => {
        makeImagesClickable(".bounty-content");
        makeImagesClickable(".bounty-answer-content");
        bindSpoiler();
      });
      obs.observe(document.body, { childList: true, subtree: true });
      setTimeout(() => {
        makeImagesClickable(".bounty-content");
        makeImagesClickable(".bounty-answer-content");
        bindSpoiler();
        // Auto scroll on initial load if hash exists, mimicking detailScripts.js line ~235 behavior:
        // if(location.hash && listpost/reply) scrollTop hash -100
        // For bounty, we want highlight when visiting via report link, but Blazor also handles paging + highlight in C#
        // So here we only auto-scroll without highlight for non-Blazor fallback, Blazor's OnAfterRender will handle highlight=true
        if (window.location.hash) {
          // short delay for content rendered
          setTimeout(() => scrollToHash(window.location.hash, true, false), 300);
        }
      }, 300);
    }
  };
})();

window.gmBountyContent.observe();
