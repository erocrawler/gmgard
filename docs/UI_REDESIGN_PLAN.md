# 主站 UI 现代化改版计划 / Main-site UI Redesign Plan

> Status: proposal (rev. 2) · Scope: `GmGard` MVC main site (`Views/**`, `wwwroot/**`) and the
> `/app` pages. The Blazor WASM client was exploratory: its component dev experience is worth
> keeping, but its WASM download cost is not. This plan therefore looks at **server-side
> rendered Razor components** as the rendering model.

## 1. Where we are today

| Area | Current state |
|---|---|
| CSS framework | **Bootstrap 2.3.2** (2013) + `bootstrap-responsive`, Glyphicons `icon-*` |
| Site CSS | `Content/Site.css` ~2,650 lines, float-based, fixed widths (`max-width:1000px`, `#main` 750px) |
| JS | **jQuery 2.2.4** (CDN), jQuery UI datepicker, jquery-validation + unobtrusive, unobtrusive-ajax, cycle2, unveil, imgpreview, calendario, lightbox, messenger, mustache, hashdown, custom tagmanager |
| Build | bower + gulp concat/uglify → `site.min.js`, `*.min.css` |
| Views | ~124 `.cshtml`; most-used legacy classes: `btn*` (~400), `label/badge` (~150), `span4/row-fluid` grid, `accordion-*`, `control-group`, `icon-*` |
| JS plugin calls | `.tooltip` ×32, `.typeahead` ×6, `.popover` ×6, `.modal` ×5, `.datepicker` ×4, `.tab`, `.collapse`; `data-toggle` ×40, `data-ajax*` ×12, `data-val*` ×~90 |
| Editor | **CKEditor 4.7.3**, shared by the main site and the Blazor client (`ck-editor.esm.js` loads `/ckeditor/ckeditor.js`) |
| `/app` | Blazor WebAssembly + Tailwind v4 + DaisyUI v5. Good dev experience, but every first visit downloads the .NET WASM runtime before anything renders |
| Razor Components | Already registered in `Startup.cs` (`AddRazorComponents().AddInteractiveWebAssemblyComponents()`) |
| Identity features | user-selectable backgrounds (`Css/bg.cshtml`, `BgTagHelper`), snow / 烟花 / parallax canvas effects, `bodybtn` toolbar |

## 2. Goals & non-goals

**Goals**
1. Modern look (typography, spacing, rounded surfaces, consistent icons), mobile-first responsive, dark mode.
2. **Keep the classic layout** so returning users find everything where it was.
3. One design system and one rendering model for the main site and `/app`.
4. Fast first paint: server-rendered HTML, no WASM download required to see a page.
5. Retire Bootstrap 2, bower, gulp and – eventually – jQuery.
6. Incremental, reversible rollout; no big-bang.

**Non-goals**
- **No editor change.** CKEditor 4 → 5 is a breaking change for stored content and is a separate project (see §8).
- No change to URLs, controllers' business logic, or information architecture.

## 3. "Classic layout" contract — what must NOT move

These are frozen. The redesign restyles them but keeps their position, order and wording:

```
┌──────────────────────────────────────────────────────────────┐
│ [Logo]                                     [登录 / 用户状态]   │  header (_LoginPartial)
│ 主页 · 资源列表 · 汉化组专版 · 意见建议 · 投稿▾ · 审核▾ · 管理 · 求物版 │  _Menu (same order)
│                                         [bg][❄][✺][视差]       │  _BodyMenu toolbar
├──────────────────────────────────────────────────────────────┤
│ 分类栏 (SiteHeader view component)                             │
├───────────────────────────────────────────┬──────────────────┤
│ featured / carousel                        │ sidebar           │
│ main content (list / details / forms)      │ (_SidebarPartial, │
│                                            │  ranking, ads,    │
│                                            │  punch-in …)      │
├───────────────────────────────────────────┴──────────────────┤
│ footer                                                         │
└──────────────────────────────────────────────────────────────┘
```

- Two-column body (content left, sidebar right) on desktop, with the same sidebar widgets in the same order.
- Menu items, labels and badges (待审核 count, 意见建议 counts) unchanged.
- The background-image system stays in full, both layers: the full-width banner (`BannerUrl`, behind the header) and the side art (`BackgroundUrl`, visible on both sides of the content column). The canvas effects stay too. See §3.1.
- List item anatomy (thumbnail, title, category, tags, author, date, counts) keeps the same fields in the same order.
- **Removed on purpose:** the 春菜 mascot (see Phase 0).

What *may* change: fonts, colours, spacing, borders and shadows, the icon set, and mobile behaviour
(menu → drawer, sidebar → below content). The **1000px content column stays**, because the background art is drawn around it.

### 3.1 Backgrounds

Today `Css/bg.cshtml` paints two layers per background:

| Type | `html` | `body` | < 1000px |
|---|---|---|---|
| `Normal` | side art `BackgroundUrl` (`_1440`/`_2560` variants), `repeat-y`, centred | banner `BannerUrl` at the top, `no-repeat` | side art removed, banner `_1024` only |
| `Fixed` | one full-screen image, `background-attachment: fixed` | none | `BannerUrl1024` as the `html` background |

The art assumes a ~1000px opaque column in the middle with a shadow (`#body`). The new shell keeps all of that:
- **Reuse `bg.cshtml` / `BgTagHelper` output unchanged.** The new layout must not paint backgrounds on `html` or `body`, so per-background overrides (`body.bg13` offsets, `html.bg5` colours, etc.) keep working.
- **Column width stays 1000px** (content + 280px sidebar). Widening to 1200px would hide 100px of art on each side, which is about half of it on a 1440px screen.
- **The header stays transparent and keeps today's height** (~270px: logo area + menu), so each banner keeps the same crop. The logo, user card and menu sit on the banner as they do now.
- **The column is an opaque panel** (`base-100` + shadow), as `#body` is today. That keeps text readable over any art, avoids blur costs, and means no background needs re-testing for contrast.
- **Dark mode** changes only the panel and cards. The art is unchanged; backgrounds without a `background-color` get a dark fallback.
- **Mobile** follows the existing < 1000px rule: banner only, no side art (as in the phone mock).
- The 查看背景 / 选择背景 / 视差 buttons work as before.
- Optional later: a 「无背景」 choice in the picker for users who want a plain page. Only that mode could use a wider fluid column.

## 4. Rendering model: static SSR Razor components

The styling work (Tailwind/DaisyUI) does not depend on the rendering model. Because the WASM client
felt heavy, though, it makes sense to settle the rendering model before porting pages.

**Recommendation: Blazor static server-side rendering (static SSR), not WASM and not InteractiveServer.**

| Option | First load | Interactivity | Dev experience | Verdict |
|---|---|---|---|---|
| Keep MVC `.cshtml` | Fast (HTML) | jQuery / small JS | Partials + TagHelpers, so-so | Fallback |
| **Razor components, static SSR** | **Fast (HTML)**, no WASM, no websocket | Forms post normally; *enhanced navigation* and *streaming rendering* built in; small JS / htmx for widgets | `.razor` components, same as `/app` today | **Preferred** |
| InteractiveServer | Fast, then a SignalR circuit per visitor | Full C# interactivity | Great | Too costly for a public, mostly-anonymous content site; only consider it for admin |
| InteractiveWebAssembly / Auto | Slow first load (WASM) | Full | Great | Only for an isolated heavy tool, if ever |

Why static SSR fits:
- It is the same `.razor` component model the `/app` work already uses, so components, `@inject`ed services and C# logic carry over. The difference is that they run on the server and talk to services directly instead of over `HttpClient` → `/api`.
- The output is plain HTML, so SEO, ads and first paint are as good as MVC.
- It can coexist with MVC in the same app: MVC routes keep working, and a ported page is a component with the same `@page` URL whose MVC action is removed. During the transition, components can also be embedded in MVC views (`<component type="…" render-mode="Static" />`).
- `[SupplyParameterFromForm]` + `EditForm` with `FormName` handles posts. `DataAnnotations` validation still works server-side.

Caveats to validate in the Phase 0 spike:
- Antiforgery, cookie auth and role checks (`[Authorize]`, `AuthorizeView`) in SSR components, using the existing Identity/OpenIddict setup.
- Interaction with `UseStatusCodePagesWithReExecute`, the `/app` fallback route and conventional MVC routes (route-ordering gotchas are already documented in `GmGard.Client/AGENTS.md`).
- Partial updates (pagers, reply lists, follow buttons): use enhanced navigation/forms where possible, otherwise htmx against small endpoints that return component HTML (`RazorComponentResult`).
- Output caching for anonymous list/detail pages.

**`/app` pages:** move them from WASM to static SSR one at a time, keeping only the pieces that truly
need client-side state as small JS islands. Once nothing needs WASM, remove the WASM runtime from the
bundle. The services in `GmGard.Client/Services` become thin wrappers over server services, or go away.

If the spike fails on any of these points, fall back to restyling the MVC `.cshtml` views. Everything
else in this plan (design system, layout contract, phases) stays the same.

## 5. Target stack

| Concern | Choice | Why |
|---|---|---|
| Rendering | Static SSR Razor components (§4), MVC views remain until ported | Fast HTML, component dev experience |
| CSS | **Tailwind v4 + DaisyUI v5** (already used by `/app`) | DaisyUI's semantic `btn`/`card`/`badge`/`menu`/`tabs`/`modal` classes are close to the Bootstrap mental model |
| Design tokens | Custom DaisyUI themes `gmgard` / `gmgard-dark`, defined once | Main site and `/app` look identical |
| Icons | Inline SVG via an `<Icon Name="…" />` component (Lucide/Heroicons subset) | Replaces Glyphicons `icon-*` without an icon font |
| JS | Native ES modules + platform features (`<dialog>`, `popover`, `<details>`, `loading="lazy"`, `<input type=date>`); **htmx** where enhanced forms aren't enough | Removes most jQuery plugins |
| Validation | Server-side DataAnnotations in components. On remaining MVC views, keep `jquery-validation-unobtrusive` until the end, then use `aspnet-client-validation` | No model changes |
| Editor | **CKEditor 4 as is**, restyled (skin + `contents.css`) | See §8 |
| Build | npm scripts (`@tailwindcss/cli`, `esbuild`) wired into MSBuild, as the Client project already does; drop bower + gulp | One toolchain |

## 6. Coexistence strategy

Bootstrap 2 and Tailwind's preflight conflict, because names like `btn`, `label`, `hide`, `table` and
`nav` collide with DaisyUI. So we never mix them on one page:

1. Add a new layout: `MainLayout.razor` for components (plus an equivalent `_LayoutModern.cshtml` if we need to restyle an MVC view before porting it). It loads only the new CSS/JS.
2. A `UiVersion` service decides classic vs. modern per request:
   - `SiteConfig.json` → `ModernUi: { Enabled, Pages: [...], DefaultForNewUsers }` (same hot-reload pattern as `BlazorApp`).
   - Per-user cookie `ui=classic|modern`, toggled by a "切换新/旧版" link in the header and footer.
   - `?ui=classic|modern` query override for testing.
   - For a ported page, the classic MVC action stays in place while the flag is off or the user picks classic, so both versions can serve the same URL. Remove the classic action once modern is the default.
3. A page is listed in `ModernUi.Pages` only once it is fully ported, so users never see a half-converted page.
4. **Post content:** the editor limits which classes posts can contain, so we only need a small scoped stylesheet `.post-content` that styles exactly these:
   - from `ckeditor/styles.js`: `well`, `label label-inverse`, `marker`, `code-featured`, `math-featured`, `left`, `right`, `image-clean`, `image-grayscale`, `embed-240p/360p/480p/720p/1080p`
   - from the spoiler plugin: `spoiler`, `spoiler-title`, `spoiler-content`
   - plus elements the editor allows: `table`, `blockquote`, `pre`, `ruby`, `video`/`audio`, smileys, mentions (`a[data-mention]`)

   `span(*)` / `table(*)` let any class through, so run a one-off DB query for distinct class names in post/reply HTML to confirm nothing else matters. Anything unrecognised just renders unstyled.

## 7. Phased plan

### Phase 0 — Foundation & spike (≈2 weeks)
- [ ] **Remove 春菜** (standalone PR, can ship right away on the classic UI):
  - delete `Views/Shared/ChunCai.cshtml`, `Scripts/chuncai.js`, `Scripts/chuncai_settings.js`, `Content/chuncai*.css`
  - remove the include in `_FooterPartial.cshtml` and the `chuncai.chuncaiSay` calls in `_PunchInPartial.cshtml` and `Scripts/detailScripts.js`
  - remove `chuncaiNotice` from `AdminViewModel`, `WidgetUtil` and `Admin/ParameterPartial.cshtml`, then rebuild `site.min.js`
- [ ] **SSR spike:** port one read-only page (e.g. `Blog/List`) and one form page (e.g. `Account/Login` or reply-post) to static SSR components behind `?ui=modern`. Validate the caveats in §4 and measure TTFB/LCP against MVC and against the WASM `/app`.
- [ ] Baseline screenshots: Playwright script capturing the top pages at 1440 / 1024 / 390 widths, logged out and logged in, on the classic layout. Used later for side-by-side review.
- [ ] Usage audit: per-view list of Bootstrap components and jQuery plugins (start from the counts in §1), plus the post-content class query from §6.4.
- [ ] Extract the current palette from `Site.css` (pink/red menu, `#1b6ec2` primary, etc.) into the `gmgard` DaisyUI theme, so the new UI *feels* like the old one.
- [ ] Add the npm + Tailwind build to `GmGard.csproj` (reuse the ordered `NpmBuildCss` target pattern from the Client csproj).
- [ ] `UiVersion` service, layout switch, cookie toggle, `SiteConfig.ModernUi` flag.

### Phase 1 — Shell (≈2 weeks)
Port the frame, keeping §3 exactly:
- [ ] `MainLayout.razor` with components for the header, menu (DaisyUI `menu menu-horizontal` + dropdowns; collapses to a `drawer` on mobile), login/user status, `bodybtn` toolbar, category bar (`SiteHeader`), sidebar and footer.
- [ ] Grid: CSS grid `minmax(0,1fr) 280px` inside the 1000px column. The sidebar stacks under the content below 1000px.
- [ ] Background system per §3.1: reuse the `bg.cshtml` output, keep the header transparent at today's height, make the column an opaque panel. Screenshot every background in `BackgroundSetting` at 1920 / 1440 / 1000 / 390 widths, old vs. new.
- [ ] Canvas effects loaded as standalone ES modules (no jQuery).
- [ ] Dark mode via `data-theme`, remembered per user.

### Phase 2 — Shared components (≈2 weeks)
Port as components:
- partials: list / brief list / user list / topic list, pager + ajax pager, search box + search pager, searched tags, follow button, punch-in, add reply, captcha, category select
- view components: `RankingSidebar`, `Advertisement`, `HomeCarousel`, `ReplyView`, `UserStatus`, `MessageView`

Small primitives (`<Card>`, `<Pager>`, `<Icon>`, `<Toast>`) keep pages free of repeated utility-class soup.

### Phase 3 — Pages, by traffic (≈4–6 weeks; ship each when done)
1. `Home/Index`, `Blog/List`, `Blog/Details` (+ reply/rate, lightbox, `.post-content`), search results
2. `Home/UserInfo`, `Favorite`, `Follow/*`, `Ranking/*`, `Topic/*`, `Han/Index`
3. `Blog/Create|Edit`, `Topic/Create|Edit`, `Han/Edit` (CKEditor 4 hosted in an SSR form + the new tag input)
4. `Account/*` (login, register, manage, 2FA)
5. `Audit/*`, `Home/Suggestions`
6. `Admin/*` and the `/app` pages (punch-in, message, title-helper, bounty, raffle, audit-exam): move from WASM to SSR

Each page: port → screenshot diff against the baseline → add to `ModernUi.Pages` → beta → default.

### Phase 4 — JS retirement
| Legacy | Replacement |
|---|---|
| `.tooltip()` | DaisyUI `tooltip` (`data-tip`), CSS only |
| `.popover()` | Native `popover` attribute / DaisyUI `dropdown` |
| `.modal()` | `<dialog>` + DaisyUI `modal` |
| `.typeahead()`, `tagmanager.js` | Small ES module tag input + `fetch` (or `<datalist>` for simple cases) |
| jQuery UI datepicker | `<input type="date">` |
| accordion / `.collapse()` / `.tab()` | `<details>` + DaisyUI `collapse`; DaisyUI `tabs` (radio-based, no JS) |
| `data-ajax-*` (unobtrusive-ajax) | Enhanced forms/navigation, or htmx `hx-get/hx-post/hx-target/hx-swap` |
| cycle2 carousel | CSS scroll-snap carousel (DaisyUI `carousel`) |
| unveil | `loading="lazy"` |
| imgpreview / lightbox | One small lightbox module |
| messenger | DaisyUI `toast` + a tiny `toast()` helper |
| mustache | Server-rendered component HTML |
| jquery-validation-unobtrusive | Server-side validation in components; `aspnet-client-validation` on leftover MVC views |
| CKEditor 4 | **Unchanged.** It doesn't need jQuery, so it doesn't block removing jQuery |

### Phase 5 — Cleanup
- [ ] Make modern the default; keep the classic toggle for ~1–2 months, then remove it along with the classic MVC actions and views.
- [ ] Delete `wwwroot/bootstrap`, `bower_components`, `bower`, `gulpfile.js`, unused `Content/*.css`, `Scripts/lib/*`, jQuery.
- [ ] Drop the WASM project/runtime once no `/app` page needs it.
- [ ] Update README / AGENTS.md (rendering model + styling rules for the whole site).

## 8. Editor (out of scope, tracked separately)

CKEditor 4.7.3 stays for this redesign, on both the main site and `/app`. In this project we only:
- restyle the editor chrome (`moono-lisa` skin + a few CSS overrides) and `contents.css`, so the editing view matches `.post-content`;
- make sure it mounts correctly in SSR forms (it is a plain `<textarea>` + `CKEDITOR.replace`, so this should be easy).

A CKEditor 5 (or other editor) migration changes the HTML that gets stored. That is its own project with
its own content-migration plan. One thing to keep in mind: 4.7.3 is end-of-life and has known XSS issues,
so the server-side HTML sanitiser stays the real security boundary either way.

## 9. Risks & mitigations

| Risk | Mitigation |
|---|---|
| Users feel lost | Frozen layout contract (§3); same labels and order; classic toggle; announce in 意见建议 plus a site banner with before/after screenshots |
| Static SSR doesn't cover some interaction well | Phase 0 spike before committing; htmx / small JS islands; MVC fallback for that page |
| Route conflicts between components and MVC | Port one URL at a time; integration tests on routing; reuse the route-ordering notes in `GmGard.Client/AGENTS.md` |
| Post content looks different | `.post-content` stylesheet built from the editor's actual class list (§6.4) + DB class audit |
| Backgrounds look different | Same two layers, same 1000px column, same header height (§3.1); old/new screenshot of every background |
| Ads / banners have fixed sizes | Keep ad slot dimensions identical; check with the AdManager role |
| Two UIs double the maintenance | Strict rule: a page is either fully classic or fully modern; time-box the coexistence window |
| Regressions in forms/auth | Playwright smoke tests for login, post, reply, search, punch-in |

## 10. Suggested first PRs

1. Remove 春菜 (independent, low-risk).
2. SSR spike: `MainLayout.razor` skeleton + `UiVersion` flag/toggle + `Blog/List` as a static SSR component behind `?ui=modern`, with a short write-up of the measurements.
3. Tailwind/DaisyUI build in `GmGard.csproj` + the shared `gmgard` theme.
4. Playwright baseline screenshot script under `tools/ui-baseline/`.
