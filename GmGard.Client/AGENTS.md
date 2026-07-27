# GmGard.Client — Agent Instructions

## Stack
- **Blazor WebAssembly** (.NET 10) — all UI is `.razor` files
- **Tailwind CSS v4** + **DaisyUI v5.5+** — styling via utility classes and DaisyUI component classes
- **C#** for all logic (no JavaScript unless absolutely necessary via `IJSRuntime`)

## Build / asset fingerprinting notes – .NET 10 hosted WASM gotchas

### Bug: dotnet/runtime#121993 (also #121121, aspnetcore#65710) – fingerprint placeholder not replaced in hosted publish
- **Link**: https://github.com/dotnet/runtime/issues/121993
- **Minimal repro**: Server csproj references Client csproj, Client has `StaticWebAssetBasePath=app` + `OverrideHtmlAssetPlaceholders=true` + `<script src="_framework/blazor.webassembly#[.{fingerprint}].js">` in index.html. Publish Server → `publish/wwwroot/index.html` still contains literal `#[.{fingerprint}]`.
- **Why browser requests `/app/_framework/blazor.webassembly` (no .js)**: `#` is URL fragment delimiter. Browser drops fragment before request, so `_framework/blazor.webassembly#[.{fingerprint}].js` becomes fetch for `/app/_framework/blazor.webassembly` → 404.
- **Root cause per maraf (MSFT)**: When WebAssembly project is referenced by Server, MSBuild calls into referenced project several times. Project instance is recreated each call, so `@(_HtmlStaticWebAssets)` collection not shared between Build and Publish. Assigned milestone 10.0.x but not yet fixed as of 2026-07-26.
- **Official workaround per maraf**: Opt-out of placeholder overriding – remove `<OverrideHtmlAssetPlaceholders>true</...>` and update index.html to plain `blazor.webassembly.js`. "You will loose fingerprint on dotnet.js and blazor.webassembly.js scripts, which is the same situation as we had in .NET 9. Fortunately in hosted scenario, you can control server sent headers for caching"
- **Applied in this repo**: Both `GmGard.Client.csproj` and `GmGard.csproj` set `<OverrideHtmlAssetPlaceholders>false</OverrideHtmlAssetPlaceholders>` with comment linking bug. `index.html` is plain (NO preload):
  ```html
  <!-- <link rel="preload" id="webassembly" /> REMOVED – see below -->
  <script type="importmap"></script> <!-- empty importmap stays empty, MapStaticAssets serves framework via no-cache endpoint -->
  <script src="_framework/blazor.webassembly.js"></script>
  ```
  `MapStaticAssets` still creates fingerprinted physical file `blazor.webassembly.958z1vx7fr.js` + endpoint `Route=app/_framework/blazor.webassembly.js` → `AssetFile=app/_framework/blazor.webassembly.{fp}.js` via no-cache mapping (same as .NET 9).
- **Why `<link rel="preload" id="webassembly" />` was removed**: This tag is part of new .NET 10 fingerprinting system. When `OverrideHtmlAssetPlaceholders=true`, SDK replaces it at build with `<link rel="preload" as="fetch" href="_framework/blazor.webassembly.{fp}.js" id="webassembly" />` + more preloads for dotnet.js etc. It's a perf optimization. But with `OverrideHtmlAssetPlaceholders=false` (workaround), the tag stays as `<link rel="preload" id="webassembly" />` with NO `href` → browser ignores it, devtools warns, does nothing. Official MSFT workaround says remove it and use plain `blazor.webassembly.js` ( .NET 9 behavior ). When #121993 is fixed in 10.0.x, re-enable by: set `OverrideHtmlAssetPlaceholders=true` + add `<link rel="preload" id="webassembly" />` back + use `blazor.webassembly#[.{fingerprint}].js`. See commit that removed it: https://github.com/nick-boey/Homespun/commit/35f3833381eecf62b57869593ee22c3c574a2f36 referencing same bug.

### Pipeline (after workaround)
- Client: `StaticWebAssetBasePath=app` + `StaticWebAssetFingerprintingEnabled=true` + `OverrideHtmlAssetPlaceholders=false`. Server: `endpoints.MapStaticAssets()` **no** `MapGroup("/app")` – JSON already has `app/` prefix, using MapGroup would cause `app/app/`.
- Do NOT use `UseBlazorFrameworkFiles("/app")` alongside MapStaticAssets – legacy middleware expects plain physical file (which doesn't exist, only fingerprinted exists) and masks 404 via `UseStatusCodePagesWithReExecute("/Error/Index/{0}")` → returns MVC 404 page `Images/404.jpg` hiding real error.
- **CSS**: `#[.{fingerprint}]` placeholder is ignored for CSS (SDK strips it). Fingerprinted `app.min.ydx3wd2ji5.css` is server artifact for ETag only, never loaded by browser. Cache-busting via `UpdateCssHash` inline task (`RoslynCodeTaskFactory` using `SHA256.Create().ComputeHash` – NOT `SHA256.HashData()` which is unavailable in inline task's older ref set – error `CS0117`) stamps `?v={hash}` into index.html **Before** `ResolveStaticWebAssetsInputs`. Incremental Inputs/Outputs to avoid race.
- **Publish**: SDK 10.0.10+ correctly puts Client `index.html` to `wwwroot/app/index.html` (was incorrectly at `wwwroot/index.html` in 10.0.9, hence old `CopyIndexHtmlToAppFolder` target – removed now since we are on 10.0.10+). Dev also has `bin/Debug/net10.0/GmGard.staticwebassets.endpoints.json` with ~6075 routes including `app/index.html` endpoint.
- **Fallback ordering**: `MapFallbackToFile("/app/{*path:nonfile}", "app/index.html")` MUST be placed **before** `MapControllerRoute default` – otherwise `/app/title-helper` matches `{controller=app}/{action=title-helper}` → MVC error page.
- **Dev `_content` rewrite**: Blazor with `base href="/app/"` requests `GET /app/_content/Microsoft.DotNet.HotReload.WebAssembly.Browser...lib.module.js` for HotReload. But `MapStaticAssets` registers `_content` at root `/_content` (verified: Debug endpoints.json has `_content/...` not `app/_content/...`). Middleware in `Startup.cs` before `UseStaticFiles` rewrites `/app/_content/*` → `/_content/*`. Also rewrites legacy `GmGard.Client.styles.css` → `GmGard.Client.bundle.scp.css`. Without this, dev `dotnet run` shows `Failed to load config file undefined TypeError: Failed to fetch dynamically imported module: .../app/_content/...` and page stuck at "加载中".
- **Launch profile gotcha**: Running `dotnet run --no-launch-profile --urls https://...` forces Production environment → hits `OpenIddict signing certificate not found at App_Data/Certificates/signing-cert.pfx` (your last terminal error). Use `dotnet run` without flag (launchSettings.json sets Development) or set `ASPNETCORE_ENVIRONMENT=Development` or `GMGARD_USE_DEV_CERTS=true` for prod binary.

### Deploy checklist (prod)
```powershell
dotnet publish GmGard/GmGard.csproj -c Release -o publish_prod --nologo -v:q
# Verify:
cat publish_prod/wwwroot/index.html   # Must be plain blazor.webassembly.js, NO #[.{fingerprint}]
Test-Path publish_prod/wwwroot/app/index.html  # Must exist via CopyIndexHtmlToAppFolder
# Run test:
$env:GMGARD_USE_DEV_CERTS="true"; dotnet publish_prod/GmGard.dll --urls https://127.0.0.1:5001
# Check endpoints:
# GET /app/_framework/blazor.webassembly.js -> 200 (was 404 before fix)
# GET /app/ -> 200 text/html
# GET /app/title-helper -> 200 text/html (SPA fallback, not MVC 404)
```
For real prod: need pfx certs + `OPENIDDICT_CERT_PASSWORD` env, `GMGARD_USE_DEV_CERTS` only for local testing.

## CSS setup (Tailwind v4 + DaisyUI v5)

Configuration lives entirely in [wwwroot/css/app.css](wwwroot/css/app.css) — there is no `tailwind.config.js`.

```css
@import "tailwindcss";
@plugin "daisyui" {
  themes: light --default, dark;
}

@source "../../**/*.{razor,html,cshtml}";

@theme {
  --color-primary-dark: #053967;  /* custom extension */
}
```

Build: `npx @tailwindcss/cli -i ./wwwroot/css/app.css -o ./wwwroot/css/app.min.css`

## Styling rules

### Always prefer DaisyUI semantic classes over raw Tailwind equivalents

| Element | Use DaisyUI | Avoid |
|---|---|---|
| Buttons | `btn`, `btn-primary`, `btn-ghost`, `btn-error`, `btn-sm` | `px-4 py-2 rounded bg-primary text-white ...` |
| Cards | `card`, `card-body`, `card-title` | `bg-white shadow-md rounded-lg p-6` |
| Modal / dialog | `modal`, `modal-box`, `modal-action`, `modal-backdrop` | Custom fixed overlay divs |
| Loading spinner | `loading loading-spinner` | `animate-spin rounded-full border-4 ...` |
| Progress bar | `progress progress-primary` | `h-1 bg-primary animate-pulse` |
| Toast / notification | `toast toast-bottom toast-center` + `alert alert-success/alert-error` | Custom fixed bottom positioned divs |
| Badges | `badge badge-primary` | `text-xs px-2 rounded-full bg-...` |
| Dividers | `divider` | `border-t my-4` |
| Alerts | `alert alert-info/warning/error/success` | Custom colored divs |
| Avatar | `avatar` + inner `div.w-* rounded-full` | Plain `<img class="rounded-full">` |

### DaisyUI v5 breaking changes to be aware of
- `card-bordered` → `card-border`
- `card-compact` → `card-sm`
- `avatar online` → `avatar avatar-online`
- `tabs-lifted` → `tabs-lift`
- `menu` items: `active` → `menu-active`, `disabled` → `menu-disabled`
- `input-bordered`, `select-bordered`, `textarea-bordered` — removed; border is default now
- `file-input-bordered` — removed; use `file-input-ghost` to remove border
- `form-control`, `label-text`, `label-text-alt` — removed; use `fieldset`/`legend`/`label`
- `stats` background is now transparent; add `bg-base-100` if needed
- Bottom nav `btm-nav` → `dock`

### Tailwind is fine for layout and spacing
Use Tailwind freely for: `flex`, `grid`, `gap-*`, `p-*`, `m-*`, `max-w-*`, `text-*` sizing, `font-*`, `w-*`, `h-*`, `aspect-*`, `overflow-*`, custom calendar day cell colors.

### Theme colors
DaisyUI v5 provides semantic colors: `primary`, `secondary`, `accent`, `neutral`, `base-100/200/300`, `success`, `error`, `warning`, `info` — use these via `bg-primary`, `text-error`, etc.

Custom extension in `@theme`:
- `primary-dark` → `#053967` — use as `bg-primary-dark`, `hover:bg-primary-dark`
- `primary` → `#1b6ec2` (from legacy Tailwind config, kept for compat)
- `accent` → `#3a0647`

DaisyUI theme is `light`/`dark`. Use `bg-base-100`, `bg-base-200`, `text-base-content` for semantic base colors rather than hardcoding `bg-white` / `text-gray-*`.

### Calendar component
DaisyUI v5 supports the **Cally** web component via the `cally` CSS class. However, for cases requiring per-day state coloring from API data (e.g. punch-in history), use a **custom CSS grid calendar** — Cally requires JS interop to set individual day states, which is more complex than a Blazor grid in this case.

## Component conventions

### Modal pattern (DaisyUI v5)
```razor
<dialog class="modal @(show ? "modal-open" : "")">
    <div class="modal-box">
        <h3 class="font-bold text-lg">Title</h3>
        <!-- content -->
        <div class="modal-action">
            <button class="btn btn-ghost" @onclick="Close">取消</button>
            <button class="btn btn-primary" @onclick="Confirm">确认</button>
        </div>
    </div>
    <form method="dialog" class="modal-backdrop">
        <button @onclick="Close">close</button>
    </form>
</dialog>
```

### Toast pattern (DaisyUI v5)
```razor
@if (toastVisible)
{
    <div class="toast toast-bottom toast-center z-50">
        <div class="alert @(toastIsError ? "alert-error" : "alert-success")">
            <span>@toastMessage</span>
        </div>
    </div>
}
```

### Loading spinner
```razor
<span class="loading loading-spinner loading-lg"></span>
```

### Progress bar (indeterminate)
```razor
<progress class="progress progress-primary w-full"></progress>
```

### Card
```razor
<div class="card bg-base-100 shadow-md">
    <div class="card-body">
        <h2 class="card-title">Title</h2>
        <!-- content -->
    </div>
</div>
```

## Services
- Register all services in `Program.cs` with `builder.Services.AddScoped<TService>()`.
- Services live in `Services/`, models in `Models/AppModels.cs` (+ `TitleCategory.cs`).
- Use `System.Net.Http.Json` (`GetFromJsonAsync`, `PostAsJsonAsync`) — no manual JSON serialization.

## File structure
```
Pages/                    # @page routed components (feature folders: Home/, Login/, Message/, Admin/, etc)
  Admin/
    AdminIndex.razor              # registration / invite code management
    AdminCategory.razor           # category CRUD
    AdminRaffle.razor             # raffle / lottery
    AdminTitleCategories.razor    # title-helper categories editor (server-driven)
  Account/
  AuditExam/
  Home/
  Login/
  Message/
  PunchIn/
  Raffle/
  TitleHelper/
Components/               # Reusable non-routed components (AdminCatIcon, CategoryFieldsComponent, MessageDetailsComponent)
Services/                 # HttpClient-based API services
Models/                   # C# model classes
Shared/                   # Layout components (NavMenu)
wwwroot/                  # Static assets
```

## API conventions
- All API calls go to the same origin via the injected `HttpClient` (base address = host).
- API paths match the existing ASP.NET backend: `/api/Account/...`, `/api/punchIn/...`, `/api/TitleHelper/...`, `/api/Admin/TitleCategories/...`, etc.
- CORS policy `GmAppOrigin` must allow these routes.
- Handle errors gracefully — services return `null` on failure; pages show user-facing error toasts.

## TitleHelper / Title Categories (server-driven)

- **Source of truth**: `GmGard/App_Data/TitleCategories.json` — editable via admin UI.
- **Backend**:
  - `GmGard/Models/App/TitleCategoriesConfig.cs` — DTOs `TitleCategoriesConfig`, `TitleHelperCategory`, `TitleHelperField`
  - `GmGard/Controllers/App/TitleHelperController.cs` — public `GET /api/TitleHelper/Categories`, admin `GET/PUT/POST/DELETE /api/Admin/TitleCategories/*`
  - Registered in `Startup.cs` via `AddOptions<TitleCategoriesConfig>` pattern.
  - **Frontend**:
  - `Models/TitleCategory.cs` (moved from `Data/`)
  - `Services/TitleHelperService.cs` — fetches categories from API
  - `Pages/TitleHelper/TitleHelper.razor` — renders fields from API
  - `Pages/Admin/AdminTitleCategories.razor` — CRUD editor
  - `Components/CategoryFieldsComponent.razor` — field editor component

## Routing / deep-linking from main site

### AppHost migration with flip flag
- Legacy main site used `ConstantUtil.AppHost` (external Angular host `https://.../app`) via `HomeController.App(string path)` which did `Redirect(AppHost + path)`.
- **Migrated**: `HomeController.App` now checks `DefaultBlazorPrefixes` (`title-helper`, `punch-in`, `message`, `raffle`, `admin`, `login`, `account`, `audit-exam`, `title-categories`) and if matched, redirects to local `/app/{path}` instead of external host.
- The Blazor app is served at `/app/{*path}` fallback to `app/index.html` (see `Startup.cs` static file mapping).
- Navigation from main site MVC views still references `constUtil.AppHost` for unmigrated modules, but `App` action itself now short-circuits for Blazor routes.

**Feature flag to flip back to Angular:**

Config lives in `GmGard/App_Data/SiteConfig.json` `BlazorApp`:

```json
{
  "BlazorApp": {
    "Enabled": true,                          // global: false => all App() routes go to legacy AppHost
    "RedirectOverrides": {                    // per-prefix override
      "bounty": false,                        // keep bounty on Angular even when Enabled=true
      "wheel": false,
      "gacha": false,
      "game": false
    },
    "ExtraPrefixes": []                       // add new Blazor routes without code change e.g. ["bounty"]
  }
}
```

- `Enabled=false` => `HomeController.App` always redirects to `AppHost` (Angular fallback for rollback)
- `RedirectOverrides[prefix]=false` => keep that single route on Angular
- `ExtraPrefixes` => force additional prefixes to Blazor without changing `DefaultBlazorPrefixes` code
- Query string override for manual testing: `?blazor=0` forces Angular, `?blazor=1` forces Blazor (e.g. `/App?path=audit-exam&blazor=0`)

To rollback entirely: set `BlazorApp.Enabled=false` in `SiteConfig.json` (no build required, file is watched with `reloadOnChange:true`), or revert via `appsettings.override.json`.

### Future: direct `/app/...` links
- Consider updating `Views/Shared/_Layout` and other MVC views that link to `/App?path=...` to directly link to `/app/...` for migrated routes, avoiding an extra redirect.

## Blazor Migration Status (merged from TODO)

### Completed as of 2026-07-26
| Area | Status | Notes |
|---|---|---|
| `title-helper` `/title-helper`, `/title-helper/{id}` | ✅ | Now server-driven via `App_Data/TitleCategories.json` + `TitleHelperController`. Verify field parity with Angular still TODO. |
| `punch-in` `/punch-in` | ✅ | Calendar UI (custom grid), streak logic, `PunchIn/Do`, `History` — compare with Angular. |
| `message` `/message`, `/inbox`, `/outbox`, `/write` | ✅ | MessageBatch, unread count verification needed. |
| `admin` `/admin`, `/admin/category`, `/admin/raffle`, `/admin/title-categories` | ✅ | Registration (invite codes), category, raffle, title-categories tabs unified with DaisyUI `tabs tabs-boxed`. Missing `draft-result`? Check legacy `AdminController.Manage`. |
| `account` `/account/2fa`, `/enable2fa`, `/twofactor` | ✅ | AuthService verification needed. |
| `login` `/login` | ✅ | |
| `audit-exam` `/audit-exam`, `/do/:version`, `/admin` | ✅ | Assets moved from `GmApps/src/assets/` → `GmGard/wwwroot/assets/` (`audit-exam-201705.json`, `201707.json` + images folder). `ExamService` fetches via absolute `/assets/audit-exam-{v}.json` served by main site static files, works for both Angular and Blazor. Model camelCase vs PascalCase handled by case-insensitive JSON. Remaining verification items (CORS/auth/type handling) tracked separately. |
| `raffle` `/raffle/{Id}` | ✅ | |
| Deep-link `HomeController.App` → Blazor | ✅ | `DefaultBlazorPrefixes` + `SiteConfig.BlazorApp` flip flag (`Enabled`, `RedirectOverrides`, `ExtraPrefixes`) + `?blazor=0/1` query override. |

### Still TODO / Missing (not in Blazor yet)
| Angular Module | Angular Routes | Blazor | Decision needed |
|---|---|---|---|
| `bounty` | `/bounty/*` (list, ask) | 🚧 TODO | WIP |
| `wheel` | `/wheel` (admin + vouchers + winwheel.js) | ❌ Deprecated |  |
| `gacha` | `/gacha/*` (list, detail, pools, animation) | ❌ Deprecated | Heavy JS animation (`gacha-animation.ts`, `gacha-video-animation.ts`). Low priority? |
| `game` | `/game/*` (eternal-circle, tarnished-world, treasure-hunt) | ❌ Deprecated | Large modules — may remaster in future |
| `account` extras | profile, follow, favorite | ❌ Not in Blazor | Keep legacy MVC for user profiles (`Views/Account/*`), or migrate later. |

### Verification plan for remaining parity
1. Run Angular (`GmApps` `ng serve`) and Blazor side-by-side
2. For each Angular component template, compare UI/behavior/API usage with Blazor counterpart
3. Check services parity: `TitleHelperService`, `PunchInService`, `MessageService`, `RaffleService`, `AccountService`, `ExamService`, `AdminService` vs Angular services
4. Ensure all `api/*/[action]` endpoints still reachable (CORS `GmAppOrigin`)
5. Test auth flows, 2FA (CookieAuthenticationStateProvider), AuthorizeView guards
6. For unmigrated modules (bounty/wheel/gacha/game) — product decision: drop, keep Angular host fallback, or re-implement in Blazor.

### Admin navigation convention
All admin pages (`AdminIndex`, `AdminCategory`, `AdminRaffle`, `AdminTitleCategories`) must include unified tab bar:

```razor
<div class="tabs tabs-boxed w-fit flex-wrap">
    <a href="admin/registration" class="tab @(IsActive("registration") ? "tab-active" : "")">注册码管理</a>
    <a href="admin/category" class="tab @(IsActive("category") ? "tab-active" : "")">栏目管理</a>
    <a href="admin/raffle" class="tab @(IsActive("raffle") ? "tab-active" : "")">抽奖管理</a>
    <a href="admin/title-categories" class="tab @(IsActive("title-categories") ? "tab-active" : "")">标题助手</a>
</div>
```

## MSBuild / npm ordering notes
- `GmGard.Client.csproj` orders npm/gulp (Tailwind build) and CSS fingerprinting (`UpdateCssHash`) **before** `ResolveStaticWebAssetsInputs` and excludes `tmp/` (or `GmGard/tmp/`) via `.gitignore`.
- If adding new static asset steps, ensure they run before static web asset resolution to avoid race conditions.
