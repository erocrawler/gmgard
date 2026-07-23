# GmGard.Client — Agent Instructions

## Stack
- **Blazor WebAssembly** (.NET 10) — all UI is `.razor` files
- **Tailwind CSS v4** + **DaisyUI v5.5+** — styling via utility classes and DaisyUI component classes
- **C#** for all logic (no JavaScript unless absolutely necessary via `IJSRuntime`)

## Build / asset fingerprinting notes

- **`blazor.webassembly.js`** is fingerprinted by the SDK via the `#[.{fingerprint}]` placeholder in `index.html` + `OverrideHtmlAssetPlaceholders=true`. This works through the browser's JS import map.
- **CSS `<link>` tags are NOT updated by the SDK fingerprinting pipeline.** The `#[.{fingerprint}]` placeholder is silently stripped from `<link href>` without inserting the hash. There is no CSS equivalent of the JS import map.
- Cache-busting for `app.min.css` is handled by the **`UpdateCssHash` inline MSBuild task** in `GmGard.Client.csproj`, which computes a SHA-256 hash of the built CSS and stamps `?v={hash}` into `index.html` after Tailwind builds. Do not replace this with a PowerShell script — production Linux CI does not have PowerShell.
- The fingerprinted `app.min.{hash}.css` file that appears in the publish output is a **server-side artifact** consumed by `MapStaticAssets` for ETag generation. The browser never loads it by that filename.

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
- Services live in `Services/`, models in `Models/AppModels.cs`.
- Use `System.Net.Http.Json` (`GetFromJsonAsync`, `PostAsJsonAsync`) — no manual JSON serialization.

## File structure
```
Pages/          # @page routed components
Components/     # Reusable non-routed components
Services/       # HttpClient-based API services
Models/         # C# model classes (AppModels.cs)
Shared/         # Layout components
wwwroot/        # Static assets
```

## API conventions
- All API calls go to the same origin via the injected `HttpClient` (base address = host).
- API paths match the existing ASP.NET backend: `/api/Account/...`, `/api/punchIn/...`, etc.
- Handle errors gracefully — services return `null` on failure; pages show user-facing error toasts.

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

### Tailwind is fine for layout and spacing
Use Tailwind freely for: `flex`, `grid`, `gap-*`, `p-*`, `m-*`, `max-w-*`, `text-*` sizing, `font-*`, `w-*`, `h-*`, `aspect-*`, `overflow-*`, custom calendar day cell colors.

### Theme colors
The Tailwind config extends with:
- `primary` → `#1b6ec2`
- `primary-dark` → `#053967`
- `accent` → `#3a0647`

DaisyUI theme is `light`/`dark`. Use `bg-base-100`, `bg-base-200`, `text-base-content` for semantic base colors rather than hardcoding `bg-white` / `text-gray-*`.

## Component conventions

### Modal pattern (DaisyUI v4)
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

### Toast pattern (DaisyUI v4)
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

## Services
- Register all services in `Program.cs` with `builder.Services.AddScoped<TService>()`.
- Services live in `Services/`, models in `Models/AppModels.cs`.
- Use `System.Net.Http.Json` (`GetFromJsonAsync`, `PostAsJsonAsync`) — no manual JSON serialization.

## File structure
```
Pages/          # @page routed components
Components/     # Reusable non-routed components
Services/       # HttpClient-based API services
Models/         # C# model classes (AppModels.cs)
Shared/         # Layout components
wwwroot/        # Static assets
```

## API conventions
- All API calls go to the same origin via the injected `HttpClient` (base address = host).
- API paths match the existing ASP.NET backend: `/api/Account/...`, `/api/punchIn/...`, etc.
- Handle errors gracefully — services return `null` on failure; pages show user-facing error toasts.
