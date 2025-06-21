using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using BlazorThemes.Services;

namespace BlazorThemes.Extensions
{
    /// <summary>
    /// Extension methods for registering BlazorThemes services
    /// </summary>
    public static class BlazorThemesExtensions
    {
        /// <summary>
        /// Adds BlazorThemes services to the service collection with default configuration
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddBlazorThemes(this IServiceCollection services)
        {
            return services.AddScoped<BlazorThemesService>();
        }

        /// <summary>
        /// Adds BlazorThemes services to the service collection with custom configuration
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configure">Configuration action for ThemeOptions</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddBlazorThemes(this IServiceCollection services, Action<ThemeOptions> configure)
        {
            services.AddScoped<BlazorThemesService>();
            services.Configure(configure);
            return services;
        }

        /// <summary>
        /// Adds BlazorThemes services with configuration from IConfiguration
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration instance</param>
        /// <param name="sectionName">Configuration section name (defaults to "ThemeOptions")</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddBlazorThemes(this IServiceCollection services, IConfiguration configuration, string sectionName = "ThemeOptions")
        {
            services.AddScoped<BlazorThemesService>();
            // Fixed: Use the overload that accepts IConfiguration and section name
            services.Configure<ThemeOptions>(opts =>
                configuration.GetSection(sectionName).Bind(opts)
            );
            return services;
        }

        /// <summary>
        /// Adds BlazorThemes services with a pre-configured ThemeOptions instance
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="options">Pre-configured theme options</param>
        /// <returns>The service collection for chaining</returns>
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

        /// <summary>
        /// Adds BlazorThemes services with enhanced configuration options
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configureOptions">Configuration action for ThemeOptions</param>
        /// <param name="configureScheduling">Configuration action for scheduling options</param>
        /// <returns>The service collection for chaining</returns>
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

        /// <summary>
        /// Adds BlazorThemes services with common preset configurations
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="preset">Preset configuration type</param>
        /// <returns>The service collection for chaining</returns>
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

    /// <summary>
    /// Predefined theme configuration presets
    /// </summary>
    public enum ThemePreset
    {
        /// <summary>
        /// Basic light/dark theme switching without transitions
        /// </summary>
        Basic,

        /// <summary>
        /// Enhanced theme switching with smooth transitions
        /// </summary>
        Enhanced,

        /// <summary>
        /// Automatic theme scheduling based on time of day
        /// </summary>
        Scheduled,

        /// <summary>
        /// Advanced animated transitions between themes
        /// </summary>
        Animated,

        /// <summary>
        /// Multiple theme support with custom themes
        /// </summary>
        MultiTheme
    }
}