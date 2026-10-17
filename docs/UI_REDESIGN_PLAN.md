# 主站 UI 现代化改版计划 / Main-site UI Redesign Plan

> Status: proposal · Scope: `GmGard` MVC main site (`Views/**`, `wwwroot/**`).
> `GmGard.Client` (Blazor) is already on Tailwind v4 + DaisyUI v5 and is treated as the
> design-system reference, not as a rewrite target.

## 1. Where we are today

| Area | Current state |
|---|---|
| CSS framework | **Bootstrap 2.3.2** (2013) + `bootstrap-responsive`, Glyphicons `icon-*` |
| Site CSS | `Content/Site.css` ~2,650 lines, float-based, fixed widths (`max-width:1000px`, `#main` 750px) |
| JS | **jQuery 2.2.4** (CDN), jQuery UI datepicker, jquery-validation + unobtrusive, unobtrusive-ajax, cycle2, unveil, imgpreview, calendario, lightbox, messenger, mustache, hashdown, custom tagmanager |
| Build | bower + gulp concat/uglify → `site.min.js`, `*.min.css` |
| Views | ~124 `.cshtml`; most-used legacy classes: `btn*` (~400), `label/badge` (~150), `span4/row-fluid` grid, `accordion-*`, `control-group`, `icon-*` |
| JS plugin calls | `.tooltip` ×32, `.typeahead` ×6, `.popover` ×6, `.modal` ×5, `.datepicker` ×4, `.tab`, `.collapse`; `data-toggle` ×40, `data-ajax*` ×12, `data-val*` ×~90 |
| Editor | CKEditor 4.7.3 on main site; Blazor client already has its own `CkEditor.razor` |
| Identity features | user-selectable backgrounds (`Css/bg.cshtml`, `BgTagHelper`), snow / 烟花 / parallax canvas effects, 春菜 mascot, `bodybtn` toolbar |

Two visual languages now co-exist: the Bootstrap-2 main site and the DaisyUI `/app` client
(with its own `NavMenu`). Users already "jump" between them.

## 2. Goals & non-goals

**Goals**
1. Modern look (typography, spacing, rounded surfaces, consistent icons), mobile-first responsive, dark mode.
2. **Keep the classic layout** so returning users find everything where it was.
3. One design system shared by the main site and `/app`.
4. Retire Bootstrap 2, bower, gulp and – eventually – jQuery.
5. Incremental, reversible rollout; no big-bang.

**Non-goals**
- Not converting the main site to a SPA / Blazor. Content pages stay server-rendered Razor (SEO, first paint, ads).
- No change to URLs, controllers, or information architecture in this project.

## 3. "Classic layout" contract — what must NOT move

These are frozen; the redesign restyles them but keeps position, order and wording:

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

- Two-column body (content left, sidebar right) on desktop; same sidebar widgets in the same order.
- Menu items, labels, badges (待审核 count, 意见建议 counts) unchanged.
- Background-image system, canvas effects and 春菜 stay (they are the site's personality), just layered on the new surfaces.
- List item anatomy (thumbnail, title, category, tags, author, date, counts) keeps the same fields and order.

What *may* change: fonts, colors, spacing, borders/shadows, icon set, widths
(fluid up to ~1200px instead of fixed 1000px), mobile behaviour (menu → drawer, sidebar → below content).

## 4. Target stack

| Concern | Choice | Why |
|---|---|---|
| CSS | **Tailwind v4 + DaisyUI v5**, same as `GmGard.Client` | One design system; DaisyUI gives semantic `btn`/`card`/`badge`/`menu`/`tabs`/`modal` classes close to Bootstrap mental model |
| Design tokens | A custom DaisyUI theme `gmgard` (+ `gmgard-dark`) defined once, imported by both projects | Main site and `/app` look identical |
| Icons | Inline SVG via a small `<icon name="…">` TagHelper (e.g. Lucide/Heroicons subset) | Replaces Glyphicons `icon-*` without a font |
| JS | Native ES modules + platform features (`<dialog>`, `popover`, `<details>`, `loading="lazy"`, `<input type=date>`); **htmx** for the `data-ajax-*` partial-update pattern | Removes most jQuery plugins with little custom code |
| Validation | Keep `jquery-validation-unobtrusive` until the last phase, then swap to `aspnet-client-validation` (jQuery-free, reads the same `data-val-*` attributes) | Zero changes to models/tag helpers |
| Build | npm scripts (`@tailwindcss/cli`, `esbuild`) wired into MSBuild like the Client project; drop bower + gulp | One toolchain |

## 5. Coexistence strategy (the key technical decision)

Bootstrap 2 and Tailwind's preflight conflict (`btn`, `label`, `hide`, `table`, `nav` etc. collide
with DaisyUI names). So we do **not** mix them on the same page:

1. Add `Views/Shared/_LayoutModern.cshtml` alongside the existing `_Layout.cshtml`. It loads only the new CSS/JS.
2. Pick the layout per request in `_ViewStart.cshtml` via a `UiVersion` service:
   - `SiteConfig.json` → `ModernUi: { Enabled, Pages: [...], DefaultForNewUsers }` (same hot-reload pattern as `BlazorApp`).
   - Per-user cookie `ui=classic|modern`, toggled by a "切换新/旧版" link in the header and footer.
   - `?ui=classic|modern` query override for testing.
3. A view is only listed in `ModernUi.Pages` once it has been ported; unported views always render with the classic layout. Users never see a half-converted page.
4. User-generated post HTML (blog bodies, replies) may contain Bootstrap-2 classes. Render it inside a `.prose .legacy-content` wrapper with a small scoped stylesheet that re-implements the handful of legacy classes that appear in content (`label`, `badge`, `well`, `alert`, `btn`, `table`). Audit real content with a DB query before writing it.

## 6. Phased plan

### Phase 0 — Foundation (≈1–2 weeks)
- [ ] Baseline screenshots: Playwright script capturing top pages at 1440 / 1024 / 390 widths, logged-out & logged-in, classic layout. Used later for side-by-side review.
- [ ] Usage audit: per-view list of Bootstrap components & jQuery plugins (start from the counts in §1).
- [ ] Extract the current palette from `Site.css` (pink/red menu, `#1b6ec2` primary, etc.) into the `gmgard` DaisyUI theme so the new UI *feels* like the old one.
- [ ] Shared CSS package: move theme to e.g. `shared/gmgard-theme.css`, `@import` from both `GmGard/wwwroot/css/site.css` and `GmGard.Client/wwwroot/css/app.css`.
- [ ] Add npm + Tailwind build to `GmGard.csproj` (copy the ordered `NpmBuildCss` target pattern from the Client csproj).
- [ ] `UiVersion` service, `_ViewStart` switch, cookie toggle, `SiteConfig` flag.

### Phase 1 — Shell (≈2 weeks)
Port the frame, keeping §3 exactly:
- [ ] `_LayoutModern`: header, `_Menu` (DaisyUI `menu menu-horizontal` + dropdowns; collapses to a `drawer` on mobile), `_LoginPartial`, `_BodyMenu` toolbar, `SiteHeader` category bar, sidebar, footer.
- [ ] Grid: CSS grid `minmax(0,1fr) 300px`, max ~1200px; sidebar stacks under content < 1024px.
- [ ] Background system: keep `bg.cshtml` output; content surfaces become `bg-base-100/90 backdrop-blur` cards so backgrounds still show. Verify every background in `BackgroundSetting`.
- [ ] Canvas effects + 春菜 loaded as standalone modules (no jQuery dependency where cheap).
- [ ] Dark mode via `data-theme`, remembered per user.
- [ ] Make Blazor `NavMenu` render the same header so `/app` and main site match.

### Phase 2 — Shared partials (≈2 weeks)
`_ListPartial`, `_ListBriefPartial`, `_ListUserBlogPartial`, `_TopicListPartial`, `_PagerPartial`/`_AjaxPager`, `_SearchPagerPartial`, `SearchPartial`, `_TagsSearchedPartial`, `_FollowBtn`, `_PunchInPartial`, `AddReply`, `_Captcha`, `_CategorySelectPartial`, view components (`RankingSidebar`, `Advertisement`, `HomeCarousel`, `ReplyView`, `UserStatus`, `MessageView`).
Build a small set of Razor TagHelpers / partials (`<gm-card>`, `<gm-pager>`, `<icon>`) so pages don't repeat utility soup.

### Phase 3 — Pages, by traffic (≈4–6 weeks, ship each when done)
1. `Home/Index`, `Blog/List`, `Blog/Details` (+ reply/rate, lightbox), search results
2. `Home/UserInfo`, `Favorite`, `Follow/*`, `Ranking/*`, `Topic/*`, `Han/Index`
3. `Blog/Create|Edit`, `Topic/Create|Edit`, `Han/Edit` (editor + tagmanager — see §7)
4. `Account/*` (login, register, manage, 2FA)
5. `Audit/*`, `Home/Suggestions`
6. `Admin/*` last (few users; consider moving into the Blazor admin instead of restyling)

Each page: port → screenshot diff vs. baseline → add to `ModernUi.Pages` → beta → default.

### Phase 4 — JS retirement
| Legacy | Replacement |
|---|---|
| `.tooltip()` | DaisyUI `tooltip` (`data-tip`), CSS only |
| `.popover()` | Native `popover` attribute / DaisyUI `dropdown` |
| `.modal()` | `<dialog>` + DaisyUI `modal` |
| `.typeahead()`, `tagmanager.js` | Small ES module tag input + `fetch` (or `<datalist>` for simple cases) |
| jQuery UI datepicker | `<input type="date">` |
| accordion / `.collapse()` / `.tab()` | `<details>` + DaisyUI `collapse`, DaisyUI `tabs` (radio-based, no JS) |
| `data-ajax-*` (unobtrusive-ajax) | htmx `hx-get/hx-post/hx-target/hx-swap` (1:1 mapping) |
| cycle2 carousel | CSS scroll-snap carousel (DaisyUI `carousel`) |
| unveil | `loading="lazy"` |
| imgpreview / lightbox | One lightbox module (reuse logic from `ImageLightbox.razor`) |
| messenger | DaisyUI `toast` + tiny `toast()` helper |
| mustache | Template literals or server-rendered partials via htmx |
| jquery-validation-unobtrusive | `aspnet-client-validation` |
| CKEditor 4.7.3 (EOL, has CVEs) | Same CKEditor 5 build the Blazor client uses; must round-trip existing post HTML |

### Phase 5 — Cleanup
- [ ] Make modern the default; keep classic toggle for ~1–2 months; then remove.
- [ ] Delete `wwwroot/bootstrap`, `bower_components`, `bower`, `gulpfile.js`, unused `Content/*.css`, `Scripts/lib/*`, jQuery.
- [ ] Update README / AGENTS.md styling rules to cover the main site.

## 7. Risks & mitigations

| Risk | Mitigation |
|---|---|
| Users feel lost | Frozen layout contract (§3); same labels/order; classic toggle; announce in 意见建议 + site banner with before/after screenshots |
| Old post HTML depends on Bootstrap 2 classes | `.legacy-content` scoped stylesheet; DB audit of class usage in content |
| Backgrounds clash with new surfaces | Translucent cards; test every configured background; keep per-background overrides possible |
| Ads / banners have fixed sizes | Keep ad slot dimensions identical; check with AdManager role |
| CKEditor migration breaks existing posts | Do it last in Phase 3; test edit→save round-trip on a sample of real posts; keep CKEditor 4 behind flag until verified |
| Dual layouts double maintenance | Strict "page is either fully classic or fully modern"; time-box the coexistence window |
| Regressions in forms/validation | Keep `data-val-*` pipeline untouched until Phase 4; Playwright smoke tests for login, post, reply, search |

## 8. Suggested first PRs

1. `UiVersion` service + `_LayoutModern` skeleton + toggle + `SiteConfig.ModernUi` flag (no visible change while disabled).
2. Tailwind/DaisyUI build in `GmGard.csproj` + shared `gmgard` theme consumed by both projects.
3. Playwright baseline screenshot script under `tools/ui-baseline/`.
4. Modern shell (header/menu/sidebar/footer) with `Home/Index` as the first ported page, enabled only via `?ui=modern`.
