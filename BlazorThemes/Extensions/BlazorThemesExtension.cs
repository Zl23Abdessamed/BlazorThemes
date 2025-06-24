using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using BlazorThemes.Services;

namespace BlazorThemes.Extensions
{
    
    public static class BlazorThemesExtensions
    {
        
        public static IServiceCollection AddBlazorThemes(this IServiceCollection services)
        {
            return services.AddScoped<BlazorThemesService>();
        }

        
        public static IServiceCollection AddBlazorThemes(this IServiceCollection services, Action<ThemeOptions> configure)
        {
            services.AddScoped<BlazorThemesService>();
            services.Configure(configure);
            return services;
        }

        
        public static IServiceCollection AddBlazorThemes(this IServiceCollection services, IConfiguration configuration, string sectionName = "ThemeOptions")
        {
            services.AddScoped<BlazorThemesService>();
            services.Configure<ThemeOptions>(opts =>
                configuration.GetSection(sectionName).Bind(opts)
            );
            return services;
        }

       
        public static IServiceCollection AddBlazorThemes(this IServiceCollection services, ThemeOptions options)
        {
            services.AddScoped<BlazorThemesService>();
            services.Configure<ThemeOptions>(opts =>
            {
                opts.Themes = options.Themes;
                opts.CustomThemes = options.CustomThemes;
                opts.StorageKey = options.StorageKey;
                opts.AttributeName = options.AttributeName;
                opts.EnableSystem = options.EnableSystem;
                opts.EnableColorScheme = options.EnableColorScheme;
                opts.DisableTransitionOnChange = options.DisableTransitionOnChange;
                opts.ForcedTheme = options.ForcedTheme;
                opts.Nonce = options.Nonce;
                opts.DebounceDelay = options.DebounceDelay;
                opts.TransitionDuration = options.TransitionDuration;
                opts.TransitionType = options.TransitionType;
                opts.EnableScheduling = options.EnableScheduling;
                opts.ScheduleConfig = options.ScheduleConfig;
            });
            return services;
        }

        public static IServiceCollection AddBlazorThemes(
            this IServiceCollection services,
            Action<ThemeOptions> configureOptions,
            Action<ThemeScheduleConfig>? configureScheduling = null)
        {
            services.AddScoped<BlazorThemesService>();
            services.Configure<ThemeOptions>(options =>
            {
                configureOptions(options);

                if (configureScheduling != null && options.ScheduleConfig != null)
                {
                    configureScheduling(options.ScheduleConfig);
                }
            });
            return services;
        }

        public static IServiceCollection AddBlazorThemes(this IServiceCollection services, ThemePreset preset)
        {
            services.AddScoped<BlazorThemesService>();

            services.Configure<ThemeOptions>(options =>
            {
                switch (preset)
                {
                    case ThemePreset.Basic:
                        ConfigureBasicPreset(options);
                        break;
                    case ThemePreset.Enhanced:
                        ConfigureEnhancedPreset(options);
                        break;
                    case ThemePreset.Scheduled:
                        ConfigureScheduledPreset(options);
                        break;
                    case ThemePreset.Animated:
                        ConfigureAnimatedPreset(options);
                        break;
                    case ThemePreset.MultiTheme:
                        ConfigureMultiThemePreset(options);
                        break;
                    default:
                        ConfigureBasicPreset(options);
                        break;
                }
            });

            return services;
        }

        private static void ConfigureBasicPreset(ThemeOptions options)
        {
            options.Themes = new[] { "light", "dark" };
            options.EnableSystem = true;
            options.EnableColorScheme = true;
            options.DisableTransitionOnChange = true;
            options.TransitionType = "none";
        }

        private static void ConfigureEnhancedPreset(ThemeOptions options)
        {
            options.Themes = new[] { "light", "dark", "auto" };
            options.EnableSystem = true;
            options.EnableColorScheme = true;
            options.DisableTransitionOnChange = false;
            options.TransitionType = "fade";
            options.TransitionDuration = 300;
            options.DebounceDelay = 150;
        }

        private static void ConfigureScheduledPreset(ThemeOptions options)
        {
            ConfigureEnhancedPreset(options);
            options.EnableScheduling = true;
            options.ScheduleConfig = new ThemeScheduleConfig
            {
                LightStart = "06:00",
                DarkStart = "18:00",
                Timezone = "local"
            };
        }

        private static void ConfigureAnimatedPreset(ThemeOptions options)
        {
            ConfigureEnhancedPreset(options);
            options.TransitionType = "ripple";
            options.TransitionDuration = 500;
        }

        private static void ConfigureMultiThemePreset(ThemeOptions options)
        {
            options.Themes = new[] { "light", "dark", "sepia", "high-contrast", "auto" };
            options.CustomThemes = new[] { "sepia", "high-contrast" };
            options.EnableSystem = true;
            options.EnableColorScheme = true;
            options.DisableTransitionOnChange = false;
            options.TransitionType = "fade";
            options.TransitionDuration = 250;
            options.DebounceDelay = 100;
        }
    }

    // Predefined theme configuration presets
    public enum ThemePreset
    {
     
        Basic, 
        Enhanced,
        Scheduled,
        Animated,
        MultiTheme
    }
}