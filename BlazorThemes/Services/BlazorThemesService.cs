using Microsoft.JSInterop;
using System.Text.Json;


namespace BlazorThemes.Services
{
    public class ThemeState
    {
        public string Theme { get; set; } = "light";
        public string ResolvedTheme { get; set; } = "light";
        public string SystemTheme { get; set; } = "light";
        public string[] Themes { get; set; } = Array.Empty<string>();
        public string? ForcedTheme { get; set; }
        public bool EnableSystem { get; set; } = true;
        public string[] CustomThemes { get; set; } = Array.Empty<string>();
        public bool IsTransitioning { get; set; }
        public bool SchedulingEnabled { get; set; }
        public ThemeScheduleConfig ScheduleConfig { get; set; } = new();
        public string TransitionType { get; set; } = "fade";
        public int TransitionDuration { get; set; } = 300;
    }

    public class ThemeScheduleConfig
    {
        public string LightStart { get; set; } = "06:00";
        public string DarkStart { get; set; } = "18:00";
        public string Timezone { get; set; } = "local";
    }

    public class ThemeTransitionOptions
    {
        public string? Type { get; set; }
        public int? Duration { get; set; }
        public string? TargetTheme { get; set; }
        public ThemeClickPosition? ClickPosition { get; set; }
    }

    public class ThemeClickPosition
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class ThemeOptions
    {
        public string[] Themes { get; set; } = new[] { "light", "dark" };
        public string[] CustomThemes { get; set; } = Array.Empty<string>();
        public string StorageKey { get; set; } = "theme";
        public string AttributeName { get; set; } = "data-theme";
        public bool EnableSystem { get; set; } = true;
        public bool EnableColorScheme { get; set; } = true;
        public bool DisableTransitionOnChange { get; set; } = false; // Changed default for better UX
        public string? ForcedTheme { get; set; }
        public string? Nonce { get; set; }

        // Enhanced options
        public int DebounceDelay { get; set; } = 150;
        public int TransitionDuration { get; set; } = 300;
        public string TransitionType { get; set; } = "fade";
        public bool EnableScheduling { get; set; } = false;
        public ThemeScheduleConfig? ScheduleConfig { get; set; }
    }

    public enum ThemeTransitionType
    {
        None,
        Fade,
        Slide,
        Ripple,
        Blur
    }

    public class BlazorThemesService : IAsyncDisposable
    {
        private readonly IJSRuntime _jsRuntime;
        private IJSObjectReference? _jsModule;
        private DotNetObjectReference<BlazorThemesService>? _dotNetRef;
        private IJSObjectReference? _themeSubscription;
        private IJSObjectReference? _systemThemeSubscription;
        private bool _initialized;
        private Timer? _debounceTimer;

        public ThemeState State { get; private set; } = new();
        public ThemeOptions Options { get; private set; } = new();

        // Events
        public event Action<ThemeState>? OnThemeChanged;
        public event Action<string>? OnSystemThemeChanged;
        public event Action? OnInitialized;
        public event Action<bool>? OnTransitionStateChanged;

        // Convenience properties
        public string CurrentTheme => State.Theme;
        public string ResolvedTheme => State.ResolvedTheme;
        public string SystemTheme => State.SystemTheme;
        public IEnumerable<string> AvailableThemes => State.Themes;
        public string? ForcedTheme => State.ForcedTheme;
        public bool IsSystemTheme => CurrentTheme == "auto" || string.IsNullOrEmpty(CurrentTheme);
        public bool IsDarkTheme => ResolvedTheme == "dark";
        public bool IsLightTheme => ResolvedTheme == "light";
        public bool IsTransitioning => State.IsTransitioning;
        public bool IsSchedulingEnabled => State.SchedulingEnabled;

        public BlazorThemesService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task InitializeAsync(ThemeOptions? options = null)
        {
            if (_initialized) return;

            try
            {
                Options = options ?? new ThemeOptions();

                _jsModule = await _jsRuntime.InvokeAsync<IJSObjectReference>(
                    "import", "./_content/BlazorThemes/BlazorThemesJsService.js");

                _dotNetRef = DotNetObjectReference.Create(this);

                // Initialize the JavaScript theme manager with options
                var initialState = await _jsModule.InvokeAsync<ThemeState>("init", Options);
                State = initialState;

                // Subscribe to theme changes
                _themeSubscription = await _jsModule.InvokeAsync<IJSObjectReference>(
                    "subscribe", _dotNetRef, nameof(OnThemeChangedCallback));

                // Subscribe to system theme changes
                _systemThemeSubscription = await _jsModule.InvokeAsync<IJSObjectReference>(
                    "subscribeToSystemTheme", _dotNetRef, nameof(OnSystemThemeChangedCallback));

                _initialized = true;
                OnInitialized?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Theme service initialization failed: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> SetThemeAsync(string themeName, ThemeTransitionOptions? transitionOptions = null)
        {
            if (_jsModule is null || !_initialized) return false;

            try
            {
                var newState = await _jsModule.InvokeAsync<ThemeState>("setTheme", themeName, transitionOptions);
                State = newState;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set theme: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetThemeWithTransitionAsync(string themeName, ThemeTransitionType transitionType, int? duration = null)
        {
            var options = new ThemeTransitionOptions
            {
                Type = transitionType.ToString().ToLower(),
                Duration = duration,
                TargetTheme = themeName
            };

            return await SetThemeAsync(themeName, options);
        }

        public async Task<bool> SetThemeWithRippleAsync(string themeName, double clickX, double clickY)
        {
            var options = new ThemeTransitionOptions
            {
                Type = "ripple",
                TargetTheme = themeName,
                ClickPosition = new ThemeClickPosition { X = clickX, Y = clickY }
            };

            return await SetThemeAsync(themeName, options);
        }

        public async Task<bool> ToggleThemeAsync(ThemeTransitionOptions? transitionOptions = null)
        {
            var newTheme = ResolvedTheme == "light" ? "dark" : "light";
            return await SetThemeAsync(newTheme, transitionOptions);
        }

        public async Task<bool> ToggleThemeWithTransitionAsync(ThemeTransitionType transitionType, int? duration = null)
        {
            var newTheme = ResolvedTheme == "light" ? "dark" : "light";
            return await SetThemeWithTransitionAsync(newTheme, transitionType, duration);
        }

        public async Task<bool> SetSystemThemeAsync()
        {
            return await SetThemeAsync("auto");
        }

        public async Task<bool> AddCustomThemeAsync(string themeName, Dictionary<string, string>? themeConfig = null)
        {
            if (_jsModule is null) return false;

            try
            {
                var result = await _jsModule.InvokeAsync<bool>("addCustomTheme", themeName, themeConfig ?? new Dictionary<string, string>());
                if (result)
                {
                    await RefreshStateAsync();
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to add custom theme: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RemoveCustomThemeAsync(string themeName)
        {
            if (_jsModule is null) return false;

            try
            {
                var result = await _jsModule.InvokeAsync<bool>("removeCustomTheme", themeName);
                if (result)
                {
                    await RefreshStateAsync();
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to remove custom theme: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ForceThemeAsync(string? theme)
        {
            if (_jsModule is null) return false;

            try
            {
                ThemeState newState;
                if (string.IsNullOrEmpty(theme))
                {
                    newState = await _jsModule.InvokeAsync<ThemeState>("clearForcedTheme");
                }
                else
                {
                    newState = await _jsModule.InvokeAsync<ThemeState>("forceTheme", theme);
                }

                State = newState;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to force theme: {ex.Message}");
                return false;
            }
        }

        public async Task ClearForcedThemeAsync()
        {
            await ForceThemeAsync(null);
        }

        // Enhanced scheduling methods
        public async Task<bool> EnableSchedulingAsync(bool enable = true)
        {
            if (_jsModule is null) return false;

            try
            {
                var newState = await _jsModule.InvokeAsync<ThemeState>("enableScheduling", enable);
                State = newState;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to enable scheduling: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetScheduleAsync(ThemeScheduleConfig scheduleConfig)
        {
            if (_jsModule is null) return false;

            try
            {
                var newState = await _jsModule.InvokeAsync<ThemeState>("setSchedule", scheduleConfig);
                State = newState;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set schedule: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetScheduleAsync(string lightStart, string darkStart, string timezone = "local")
        {
            var config = new ThemeScheduleConfig
            {
                LightStart = lightStart,
                DarkStart = darkStart,
                Timezone = timezone
            };

            return await SetScheduleAsync(config);
        }

        // Enhanced transition methods
        public async Task<bool> SetTransitionTypeAsync(ThemeTransitionType transitionType, int? duration = null)
        {
            if (_jsModule is null) return false;

            try
            {
                var newState = await _jsModule.InvokeAsync<ThemeState>("setTransitionType",
                    transitionType.ToString().ToLower(), duration);
                State = newState;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to set transition type: {ex.Message}");
                return false;
            }
        }

        public async Task RefreshStateAsync()
        {
            if (_jsModule is null) return;

            try
            {
                State = await _jsModule.InvokeAsync<ThemeState>("getThemeState");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to refresh theme state: {ex.Message}");
            }
        }

        public bool IsThemeAvailable(string theme)
        {
            return AvailableThemes.Contains(theme);
        }

        public bool IsCustomTheme(string theme)
        {
            return State.CustomThemes.Contains(theme);
        }

        public string GetNextTheme()
        {
            var themes = AvailableThemes.Where(t => t != "auto").ToArray();
            var currentIndex = Array.IndexOf(themes, ResolvedTheme);
            return themes[(currentIndex + 1) % themes.Length];
        }

        public string GetPreviousTheme()
        {
            var themes = AvailableThemes.Where(t => t != "auto").ToArray();
            var currentIndex = Array.IndexOf(themes, ResolvedTheme);
            return themes[currentIndex == 0 ? themes.Length - 1 : currentIndex - 1];
        }

        public async Task CycleThemeAsync(ThemeTransitionOptions? transitionOptions = null)
        {
            await SetThemeAsync(GetNextTheme(), transitionOptions);
        }

        public async Task CycleThemeWithTransitionAsync(ThemeTransitionType transitionType, int? duration = null)
        {
            await SetThemeWithTransitionAsync(GetNextTheme(), transitionType, duration);
        }

        // Debounced theme operations
        public void DebouncedSetTheme(string themeName, int delayMs = 150)
        {
            _debounceTimer?.Dispose();
            _debounceTimer = new Timer(async _ =>
            {
                await SetThemeAsync(themeName);
                _debounceTimer?.Dispose();
                _debounceTimer = null;
            }, null, delayMs, Timeout.Infinite);
        }

        public void DebouncedToggleTheme(int delayMs = 150)
        {
            var newTheme = ResolvedTheme == "light" ? "dark" : "light";
            DebouncedSetTheme(newTheme, delayMs);
        }

        // Time-based helpers
        public TimeSpan GetTimeUntilNextScheduledChange()
        {
            if (!IsSchedulingEnabled) return TimeSpan.Zero;

            var now = DateTime.Now;
            var lightTime = ParseTime(State.ScheduleConfig.LightStart);
            var darkTime = ParseTime(State.ScheduleConfig.DarkStart);

            var todayLight = now.Date.Add(lightTime);
            var todayDark = now.Date.Add(darkTime);
            var tomorrowLight = todayLight.AddDays(1);
            var tomorrowDark = todayDark.AddDays(1);

            var nextChanges = new[] { todayLight, todayDark, tomorrowLight, tomorrowDark }
                .Where(t => t > now)
                .OrderBy(t => t);

            var nextChange = nextChanges.FirstOrDefault();
            return nextChange == default ? TimeSpan.Zero : nextChange - now;
        }

        public string GetScheduledThemeAt(DateTime dateTime)
        {
            if (!IsSchedulingEnabled) return "light";

            var time = dateTime.TimeOfDay;
            var lightStart = ParseTime(State.ScheduleConfig.LightStart);
            var darkStart = ParseTime(State.ScheduleConfig.DarkStart);

            if (lightStart < darkStart)
            {
                return time >= darkStart || time < lightStart ? "dark" : "light";
            }
            else
            {
                return time >= darkStart && time < lightStart ? "dark" : "light";
            }
        }

        private TimeSpan ParseTime(string timeStr)
        {
            if (TimeSpan.TryParse(timeStr, out var time))
                return time;

            // Fallback parsing for HH:mm format
            var parts = timeStr.Split(':');
            if (parts.Length == 2 &&
                int.TryParse(parts[0], out var hours) &&
                int.TryParse(parts[1], out var minutes))
            {
                return new TimeSpan(hours, minutes, 0);
            }

            return TimeSpan.Zero;
        }

        // JavaScript callback methods
        [JSInvokable]
        public Task OnThemeChangedCallback(ThemeState newState)
        {
            var wasTransitioning = State.IsTransitioning;
            State = newState;

            // Fire transition state change event if transition state changed
            if (wasTransitioning != newState.IsTransitioning)
            {
                OnTransitionStateChanged?.Invoke(newState.IsTransitioning);
            }

            OnThemeChanged?.Invoke(newState);
            return Task.CompletedTask;
        }

        [JSInvokable]
        public Task OnSystemThemeChangedCallback(string systemTheme)
        {
            State.SystemTheme = systemTheme;
            OnSystemThemeChanged?.Invoke(systemTheme);
            return Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                _debounceTimer?.Dispose();

                if (_themeSubscription is not null)
                {
                    await _themeSubscription.InvokeVoidAsync("dispose");
                    await _themeSubscription.DisposeAsync();
                }

                if (_systemThemeSubscription is not null)
                {
                    await _systemThemeSubscription.InvokeVoidAsync("dispose");
                    await _systemThemeSubscription.DisposeAsync();
                }

                _dotNetRef?.Dispose();

                if (_jsModule is not null)
                {
                    await _jsModule.InvokeVoidAsync("destroy");
                    await _jsModule.DisposeAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error disposing BlazorThemesService: {ex.Message}");
            }
        }
    }

    // Extension methods for easier theme management
    public static class BlazorThemesServiceExtensions
    {
        public static async Task<bool> SetLightThemeAsync(this BlazorThemesService service, ThemeTransitionOptions? options = null)
            => await service.SetThemeAsync("light", options);

        public static async Task<bool> SetDarkThemeAsync(this BlazorThemesService service, ThemeTransitionOptions? options = null)
            => await service.SetThemeAsync("dark", options);

        public static async Task<bool> EnableAutoThemeAsync(this BlazorThemesService service, ThemeTransitionOptions? options = null)
            => await service.SetThemeAsync("auto", options);

        public static async Task<bool> SetLightThemeWithTransitionAsync(this BlazorThemesService service, ThemeTransitionType transition, int? duration = null)
            => await service.SetThemeWithTransitionAsync("light", transition, duration);

        public static async Task<bool> SetDarkThemeWithTransitionAsync(this BlazorThemesService service, ThemeTransitionType transition, int? duration = null)
            => await service.SetThemeWithTransitionAsync("dark", transition, duration);

        public static async Task<bool> FadeToLightAsync(this BlazorThemesService service, int duration = 300)
            => await service.SetThemeWithTransitionAsync("light", ThemeTransitionType.Fade, duration);

        public static async Task<bool> FadeToDarkAsync(this BlazorThemesService service, int duration = 300)
            => await service.SetThemeWithTransitionAsync("dark", ThemeTransitionType.Fade, duration);

        public static async Task<bool> SlideToThemeAsync(this BlazorThemesService service, string theme, int duration = 300)
            => await service.SetThemeWithTransitionAsync(theme, ThemeTransitionType.Slide, duration);

        public static async Task<bool> BlurToThemeAsync(this BlazorThemesService service, string theme, int duration = 300)
            => await service.SetThemeWithTransitionAsync(theme, ThemeTransitionType.Blur, duration);
    }
}