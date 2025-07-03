# 🎨 BlazorThemes
### Professional Theme Management for Blazor Applications

---

BlazorThemes is a comprehensive theme management solution for Blazor applications that provides advanced features like smooth transitions, theme scheduling, custom themes, and system integration.

## ✨ Features

### 🌓 **Theme Management**
- **Multiple Theme Support**: Light, dark, and auto (system) themes out of the box
- **Custom Themes**: Create and manage your own themes with custom CSS properties
- **System Integration**: Respects OS-level theme preferences
- **Cross-tab Synchronization**: Theme changes sync across browser tabs

### 🎬 **Transitions & Effects**
- **Smooth Transitions**: Fade, slide, ripple, and blur transitions between themes
- **Advanced Effects**: Ripple transitions with click position tracking
- **Optimized Performance**: Debounced updates for frequent theme changes

### ⏰ **Smart Scheduling**
- **Time-based Scheduling**: Automatically switch themes based on time of day
- **Custom Time Ranges**: Configure your own light/dark schedule
- **Forced Themes**: Override user preferences when needed

### 🔧 **Developer Experience**
- **CSS Variable Support**: Apply custom properties for each theme
- **Event System**: Notifications for theme changes and system updates
- **TypeScript Support**: Full type safety and IntelliSense

---

## 📦 Installation

### 1. Install the NuGet Package
```bash
dotnet add package BlazorThemes
```

### 2. Configure Services
Add the service to your `Program.cs`:

```csharp
builder.Services.AddBlazorThemes(options => {
    options.Themes = new[] { "light", "dark", "auto" };
    options.EnableSystem = true;
    options.TransitionType = "fade";
});
```

### 3. Add Theme Provider
Add the theme provider to your root component:

```razor
<BlazorThemesProvider>
    <Router AppAssembly="@typeof(App).Assembly">
        <!-- Your app content -->
    </Router>
</BlazorThemesProvider>
```

---

## 🚀 Basic Usage

### Setting Themes
```razor
<div class="theme-controls">
    <button @onclick='() => ThemesService.SetThemeAsync("light")' 
            class="btn btn-light">
        ☀️ Light
    </button>
    
    <button @onclick='() => ThemesService.SetThemeAsync("dark")' 
            class="btn btn-dark">
        🌙 Dark
    </button>
    
    <button @onclick='() => ThemesService.SetThemeAsync("auto")' 
            class="btn btn-auto">
        🔄 Auto
    </button>
</div>
```

### Quick Theme Toggle
```razor
<button @onclick="ThemesService.ToggleThemeAsync" 
        class="btn btn-toggle">
    🔄 Toggle Theme
</button>
```

### Accessing Theme State
```razor
@if (currentState?.ResolvedTheme == "dark") 
{
    <div class="dark-mode-content">
        <h3>🌙 Dark Mode Active</h3>
        <p>This content is optimized for dark theme.</p>
    </div>
}
else 
{
    <div class="light-mode-content">
        <h3>☀️ Light Mode Active</h3>
        <p>This content is optimized for light theme.</p>
    </div>
}
```

---

## 🔥 Advanced Features

### 🎨 Custom Themes
Create your own unique themes with custom CSS properties:

```csharp
var customThemeConfig = new Dictionary<string, string>
{
    ["primary-color"] = "#ff6b6b",
    ["secondary-color"] = "#4ecdc4", 
    ["background-color"] = "#f8f9fa",
    ["text-color"] = "#2d3748",
    ["border-radius"] = "8px",
    ["shadow-color"] = "rgba(0, 0, 0, 0.1)"
};

await ThemesService.AddCustomThemeAsync("midnight", customThemeConfig);
```

### 🎬 Theme Transitions
Add smooth visual transitions between theme changes:

```csharp
// Fade transition (smooth and elegant)
await ThemesService.SetThemeWithTransitionAsync("dark", ThemeTransitionType.Fade);

// Slide transition (dynamic movement)
await ThemesService.SetThemeWithTransitionAsync("light", ThemeTransitionType.Slide);

// Ripple transition at click position (interactive)
await ThemesService.SetThemeWithRippleAsync("light", e.ClientX, e.ClientY);

// Blur transition (modern effect)
await ThemesService.SetThemeWithTransitionAsync("auto", ThemeTransitionType.Blur);
```

### ⏰ Theme Scheduling
Automatically switch themes based on time of day:

```csharp
// Enable automatic scheduling
await ThemesService.EnableSchedulingAsync(true);

// Set custom schedule (light theme starts at 6 AM, dark at 6 PM)
await ThemesService.SetScheduleAsync("06:00", "18:00");

// Advanced scheduling with different weekday/weekend times
await ThemesService.SetAdvancedScheduleAsync(
    weekdayLight: "07:00", 
    weekdayDark: "19:00",
    weekendLight: "08:00", 
    weekendDark: "20:00"
);
```

### 🔒 Forced Themes
Override user preferences for specific scenarios:

```csharp
// Force dark theme (useful for presentations, demos)
await ThemesService.ForceThemeAsync("dark");

// Force custom theme for special events
await ThemesService.ForceThemeAsync("midnight");

// Clear forced theme and return to user preference
await ThemesService.ClearForcedThemeAsync();
```

---

## 🧩 Component Structure

### Theme Provider Configuration
```razor
<BlazorThemesProvider 
    Options="@themeOptions"
    OnThemeChanged="HandleThemeChanged"
    EnableScheduling="true"
    LightStartTime="06:00"
    DarkStartTime="18:00"
    DefaultTransition="ThemeTransitionType.Fade"
    TransitionDuration="300ms"
    EnableCrossTabs="true">
    
    <!-- Your application content -->
    <Router AppAssembly="@typeof(App).Assembly">
        <RouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)" />
    </Router>
    
</BlazorThemesProvider>
```

### Cascading Values
Access theme services throughout your component tree:

```razor
<CascadingValue Value="BlazorThemesService" Name="BlazorThemesService">
    <CascadingValue Value="@BlazorThemesService.State" Name="ThemeState">
        <CascadingValue Value="@BlazorThemesService.Config" Name="ThemeConfig">
            <!-- Your app content with full theme context -->
            @Body
        </CascadingValue>
    </CascadingValue>
</CascadingValue>
```

---

## 📚 API Reference

### 🔧 BlazorThemesService Methods

| Method | Parameters | Description |
|--------|------------|-------------|
| `SetThemeAsync` | `theme`, `options?` | Set theme with optional transition effects |
| `ToggleThemeAsync` | `options?` | Toggle between light/dark themes |
| `AddCustomThemeAsync` | `name`, `config` | Add custom theme with CSS variables |
| `RemoveCustomThemeAsync` | `name` | Remove a custom theme |
| `SetThemeWithTransitionAsync` | `theme`, `type` | Change theme with specific transition |
| `SetThemeWithRippleAsync` | `theme`, `x`, `y` | Change theme with ripple effect |
| `CycleThemesAsync` | `options?` | Cycle through available themes |
| `EnableSchedulingAsync` | `enable` | Enable/disable time-based scheduling |
| `SetScheduleAsync` | `lightStart`, `darkStart` | Configure schedule times |
| `ForceThemeAsync` | `theme` | Force a specific theme |
| `ClearForcedThemeAsync` | - | Clear forced theme |
| `RefreshSystemThemeAsync` | - | Refresh system theme detection |
| `GetThemeVariablesAsync` | `theme` | Get CSS variables for a theme |

### 📊 ThemeState Properties

| Property | Type | Description |
|----------|------|-------------|
| `Theme` | `string` | Currently selected theme |
| `ResolvedTheme` | `string` | Actual applied theme (resolves 'auto') |
| `SystemTheme` | `string` | Current operating system theme |
| `Themes` | `string[]` | Available built-in themes |
| `CustomThemes` | `string[]` | Available custom theme names |
| `ForcedTheme` | `string?` | Currently forced theme |
| `IsTransitioning` | `bool` | Whether a transition is in progress |
| `SchedulingEnabled` | `bool` | Whether scheduling is active |
| `ScheduleConfig` | `ScheduleConfig` | Current scheduling configuration |
| `LastChanged` | `DateTime` | When theme was last changed |
| `TransitionDuration` | `TimeSpan` | Current transition duration |

### ⚙️ Configuration Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `Themes` | `string[]` | `["light", "dark", "auto"]` | Available themes |
| `DefaultTheme` | `string` | `"light"` | Default theme on first load |
| `EnableSystem` | `bool` | `true` | Respect OS theme preferences |
| `TransitionType` | `string` | `"fade"` | Default transition effect |
| `TransitionDuration` | `string` | `"300ms"` | Transition duration |
| `EnableCrossTabs` | `bool` | `true` | Sync themes across browser tabs |
| `StorageKey` | `string` | `"blazor-themes"` | LocalStorage key |
| `EnableScheduling` | `bool` | `false` | Enable time-based scheduling |
| `ScheduleConfig` | `ScheduleConfig` | - | Scheduling configuration |

---

## 🧪 Example Implementation

Here's a complete example of a theme-aware component:

```razor
@page "/theme-demo"
@inject BlazorThemesService ThemesService

<div class="theme-demo-container">
    <h1>🎨 Theme Demo</h1>
    
    <!-- Theme Controls -->
    <div class="theme-controls">
        <h3>Basic Controls</h3>
        <button @onclick='() => SetTheme("light")' class="btn btn-light">
            ☀️ Light
        </button>
        <button @onclick='() => SetTheme("dark")' class="btn btn-dark">
            🌙 Dark
        </button>
        <button @onclick='() => SetTheme("auto")' class="btn btn-auto">
            🔄 Auto
        </button>
        <button @onclick="ToggleTheme" class="btn btn-toggle">
            ⚡ Toggle
        </button>
    </div>
    
    <!-- Transition Effects -->
    <div class="transition-controls">
        <h3>Transition Effects</h3>
        <button @onclick='() => SetThemeWithTransition("dark", "fade")'>
            🌅 Fade to Dark
        </button>
        <button @onclick='() => SetThemeWithTransition("light", "slide")'>
            ⬅️ Slide to Light
        </button>
        <button @onclick="(e) => SetThemeWithRipple("auto", e)">
            💫 Ripple Effect
        </button>
    </div>
    
    <!-- Theme Information -->
    <div class="theme-info">
        <h3>Current Theme State</h3>
        <ul>
            <li><strong>Selected:</strong> @currentState?.Theme</li>
            <li><strong>Applied:</strong> @currentState?.ResolvedTheme</li>
            <li><strong>System:</strong> @currentState?.SystemTheme</li>
            <li><strong>Transitioning:</strong> @currentState?.IsTransitioning</li>
            <li><strong>Last Changed:</strong> @currentState?.LastChanged.ToString("HH:mm:ss")</li>
        </ul>
    </div>
</div>

@code {
    private ThemeState? currentState;
    
    protected override async Task OnInitializedAsync()
    {
        currentState = ThemesService.State;
        ThemesService.OnThemeChanged += HandleThemeChanged;
    }
    
    private async Task SetTheme(string theme)
    {
        await ThemesService.SetThemeAsync(theme);
    }
    
    private async Task ToggleTheme()
    {
        await ThemesService.ToggleThemeAsync();
    }
    
    private async Task SetThemeWithTransition(string theme, string transition)
    {
        await ThemesService.SetThemeWithTransitionAsync(theme, transition);
    }
    
    private async Task SetThemeWithRipple(string theme, MouseEventArgs e)
    {
        await ThemesService.SetThemeWithRippleAsync(theme, e.ClientX, e.ClientY);
    }
    
    private void HandleThemeChanged(ThemeState state)
    {
        currentState = state;
        InvokeAsync(StateHasChanged);
    }
    
    public void Dispose()
    {
        ThemesService.OnThemeChanged -= HandleThemeChanged;
    }
}
```

---
## 🤝 Contributing

We welcome contributions!

Whether it's enhancing current features, squashing bugs, refining documentation, or dreaming up something entirely new—your input is valued. Got a cool feature idea? Feel free to bring it to life here. We love seeing the community shape the future of this project.

Ways you can get involved:
- **Suggest or develop new features** that could benefit others.
- **Report bugs** or unexpected behavior.
- **Improve the docs** to make everything clearer and more helpful.
- **Join the discussion** and share your thoughts—we're always open to fresh perspectives.
- **Spread the word** if you find this useful!

Your contributions help this project grow. Let’s build something great together.


### Development Setup
```bash
# Clone the repository
git clone https://github.com/Zl23Abdessamed/BlazorThemes

# Navigate to project directory
cd BlazorThemes

# Restore dependencies
dotnet restore

```

---

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

---

## 🆘 Support

- 📖 **Documentation**: [View full docs](https://blazorthemes.dev/docs)
- 🐛 **Issues**: [Report bugs](https://github.com/BlazorThemes/BlazorThemes/issues)
- 💬 **Discussions**: [Join the community](https://github.com/BlazorThemes/BlazorThemes/discussions)
- 📧 **Email**: a_zalla@estin.dz

---

<div align="center">

**Made with ❤️ for the Blazor community**

[⭐ Star us on GitHub](https://github.com/BlazorThemes/BlazorThemes) | [📖 Documentation](https://blazorthemes.dev) | [🚀 Get Started](https://blazorthemes.dev/get-started)

</div>