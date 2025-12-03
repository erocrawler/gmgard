# GmGard Blazor Client

This is the modern rewrite of the GmApps Angular application using Blazor WebAssembly and Tailwind CSS.

## Project Structure

```
GmGard.Client/
├── Pages/               # Razor page components
│   ├── Home.razor       # Home page (/)
│   ├── TitleHelper.razor
│   ├── Gacha.razor
│   ├── Wheel.razor
│   ├── PunchIn.razor
│   ├── Game.razor
│   ├── Bounty.razor
│   └── Message.razor
├── Shared/              # Shared components
│   ├── MainLayout.razor
│   └── NavMenu.razor
├── wwwroot/            # Static files
│   ├── css/
│   │   ├── app.css     # Tailwind CSS source
│   │   └── app.min.css # Compiled CSS (generated)
│   └── index.html
├── App.razor           # Root component
├── Program.cs          # Entry point
└── _Imports.razor      # Global using statements
```

## Technology Stack

- **Blazor WebAssembly (.NET 10.0)** - Modern SPA framework
- **Tailwind CSS 3.4** - Utility-first CSS framework
- **C#** - Primary programming language

## Development Setup

### Prerequisites

- .NET 10.0 SDK
- Node.js (for Tailwind CSS build)

### Initial Setup

1. Install Node.js dependencies:
```powershell
cd GmGard.Client
npm install
```

2. Build Tailwind CSS:
```powershell
npm run build:css
```

3. Restore .NET packages:
```powershell
cd ..
dotnet restore
```

### Development Workflow

#### Watch Mode for Tailwind CSS

While developing, run Tailwind in watch mode to automatically rebuild CSS on changes:

```powershell
cd GmGard.Client
npm run watch:css
```

#### Build the Client

```powershell
dotnet build GmGard.Client/GmGard.Client.csproj
```

#### Build the Entire Solution

```powershell
dotnet build
```

#### Run the Application

```powershell
cd GmGard
dotnet run
```

The Blazor app will be available at: `http://localhost:5000/app/`

## Routing

The Blazor application is hosted under the `/app` path:

- `/app/` - Home page
- `/app/title-helper` - Title helper tool
- `/app/gacha` - Gacha/lottery system
- `/app/wheel` - Wheel of fortune
- `/app/punch-in` - Daily check-in
- `/app/game` - Adventure game
- `/app/bounty` - Bounty board
- `/app/message` - Message center

## Integration with GmGard Server

The Blazor client is integrated into the main GmGard ASP.NET Core application:

1. **GmGard.csproj** references `GmGard.Client.csproj`
2. **Startup.cs** includes:
   - `services.AddRazorPages()` for Blazor support
   - `app.UseBlazorFrameworkFiles()` to serve Blazor static files
   - `endpoints.MapFallbackToFile("/app/{*path:nonfile}", "index.html")` for SPA routing

## Migration from Angular

The following Angular modules have been migrated to Blazor:

| Angular Module | Blazor Page | Status |
|---------------|-------------|---------|
| app-layout | MainLayout.razor | ✅ Complete |
| title-helper | TitleHelper.razor | 🚧 Stub |
| gacha | Gacha.razor | 🚧 Stub |
| wheel | Wheel.razor | 🚧 Stub |
| punch-in | PunchIn.razor | 🚧 Stub |
| game | Game.razor | 🚧 Stub |
| bounty | Bounty.razor | 🚧 Stub |
| message | Message.razor | 🚧 Stub |
| audit-exam | - | ⏳ Pending |
| admin | - | ⏳ Pending |
| account | - | ⏳ Pending |

## Next Steps

### 1. Authentication Integration

Implement authentication using ASP.NET Core Identity:

```csharp
// Add to GmGard.Client Program.cs
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
```

### 2. API Service Layer

Create typed HTTP clients for API communication:

```csharp
builder.Services.AddScoped(sp => new HttpClient 
{ 
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) 
});
builder.Services.AddScoped<IGachaService, GachaService>();
builder.Services.AddScoped<IWheelService, WheelService>();
// etc.
```

### 3. Implement Feature Logic

Each stub page needs to be fully implemented with:
- API service integration
- State management
- Real-time updates (SignalR if needed)
- Form validation
- Error handling

### 4. Shared Components

Create reusable components:
- Loading spinners
- Error messages
- Dialogs/modals
- Form controls
- Data grids

### 5. Styling Improvements

- Create custom Tailwind components
- Add animations and transitions
- Implement responsive design
- Dark mode support

## Benefits Over Angular

1. **Type Safety** - Full C# type checking across client and server
2. **Code Sharing** - Share models, validation, and logic between client/server
3. **Performance** - Compiled to WebAssembly for near-native performance
4. **Modern Tooling** - .NET SDK, hot reload, and better debugging
5. **Single Language** - C# everywhere, no TypeScript/JavaScript
6. **Smaller Bundle** - Optimized IL linking reduces bundle size
7. **Better Integration** - Seamless integration with ASP.NET Core backend

## Troubleshooting

### CSS not updating

Run the Tailwind build command:
```powershell
cd GmGard.Client
npm run build:css
```

### Build errors

Clean and rebuild:
```powershell
dotnet clean
dotnet build
```

### 404 errors in production

Ensure `UseBlazorFrameworkFiles()` is called before `UseStaticFiles()` in Startup.cs.

## Resources

- [Blazor Documentation](https://docs.microsoft.com/aspnet/core/blazor/)
- [Tailwind CSS Documentation](https://tailwindcss.com/docs)
- [ASP.NET Core Integration](https://docs.microsoft.com/aspnet/core/blazor/hosting-models)
