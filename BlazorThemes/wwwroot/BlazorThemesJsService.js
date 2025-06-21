class ThemeManager {
    static themes = ['light', 'dark', 'auto']; // Extensible theme list
    static storageKey = 'theme';
    static attributeName = 'data-theme';
    static enableSystem = true;
    static enableColorScheme = true;
    static disableTransitionOnChange = true;
    static forcedTheme = null;
    static nonce = null;
    static customThemes = new Set();
    static observers = new Set();
    static colorSchemeChangeListeners = new Set();

    // Enhanced features
    static debounceDelay = 150;
    static transitionDuration = 300;
    static transitionType = 'fade';
    static enableScheduling = false;
    static scheduleConfig = {
        lightStart: '06:00',
        darkStart: '18:00',
        timezone: 'local'
    };

    // Internal state
    static _debounceTimer = null;
    static _schedulingTimer = null;
    static _isTransitioning = false;
    static _transitionElement = null;

    // REPLACE the old init function with this one

    static init(options = {}) {
        // Hold onto the custom themes from the options before doing anything else.
        const customThemesFromOptions = options.customThemes || [];

        // CRITICAL FIX: Remove customThemes from the options object before assigning.
        // This prevents Object.assign from overwriting our Set with an Array.
        if (options.customThemes) {
            delete options.customThemes;
        }

        // Now, safely apply the rest of the configuration options.
        Object.assign(this, options);

        // Now, loop through the themes from options and add them to our existing Set.
        if (customThemesFromOptions && Array.isArray(customThemesFromOptions)) {
            customThemesFromOptions.forEach(theme => {
                this.customThemes.add(theme);
            });
        }

        // Now that the customThemes Set is correctly populated,
        // add all custom themes to the main 'themes' array.
        this.customThemes.forEach(theme => {
            if (!this.themes.includes(theme)) {
                this.themes.push(theme);
            }
        });

        // --- The rest of the function remains the same ---

        // Setup transition styles
        this.setupTransitionStyles();

        // Prevent flash by immediately setting the theme
        this.applyTheme(this.getPreferredTheme(), false);

        // Watch for system theme changes only if system is enabled
        if (this.enableSystem) {
            this.setupSystemThemeWatcher();
        }

        // Sync across tabs
        this.setupStorageSync();

        // Setup color scheme meta tag if enabled
        if (this.enableColorScheme) {
            this.updateColorScheme();
        }

        // Setup theme scheduling if enabled
        if (this.enableScheduling) {
            this.setupThemeScheduling();
        }

        // Notify initial state
        this.notifyObservers();
    }

    static setupTransitionStyles() {
        const style = document.createElement('style');
        style.id = 'theme-transition-styles';

        const css = `
            .theme-transition-overlay {
                position: fixed;
                top: 0;
                left: 0;
                width: 100vw;
                height: 100vh;
                pointer-events: none;
                z-index: 999999;
                transition: opacity ${this.transitionDuration}ms ease;
            }
            
            .theme-transition-fade {
                background: var(--theme-transition-color, #000);
                opacity: 0;
            }
            
            .theme-transition-fade.active {
                opacity: 0.3;
            }
            
            .theme-transition-slide {
                background: linear-gradient(90deg, 
                    var(--theme-transition-color, #000) 0%, 
                    var(--theme-transition-color, #000) 50%, 
                    transparent 50%, 
                    transparent 100%);
                transform: translateX(-100%);
                transition: transform ${this.transitionDuration}ms cubic-bezier(0.4, 0, 0.2, 1);
            }
            
            .theme-transition-slide.active {
                transform: translateX(0);
            }
            
            .theme-transition-ripple {
                background: radial-gradient(circle at var(--ripple-x, 50%) var(--ripple-y, 50%), 
                    var(--theme-transition-color, #000) 0%, 
                    var(--theme-transition-color, #000) 0%, 
                    transparent 0%);
                transition: background ${this.transitionDuration}ms cubic-bezier(0.4, 0, 0.2, 1);
            }
            
            .theme-transition-ripple.active {
                background: radial-gradient(circle at var(--ripple-x, 50%) var(--ripple-y, 50%), 
                    var(--theme-transition-color, #000) 0%, 
                    var(--theme-transition-color, #000) 150%, 
                    transparent 150%);
            }
            
            .theme-transition-blur {
                backdrop-filter: blur(0px);
                background: rgba(0, 0, 0, 0);
                transition: backdrop-filter ${this.transitionDuration}ms ease, 
                           background ${this.transitionDuration}ms ease;
            }
            
            .theme-transition-blur.active {
                backdrop-filter: blur(10px);
                background: rgba(0, 0, 0, 0.1);
            }
        `;

        style.textContent = css;
        if (this.nonce) style.nonce = this.nonce;
        document.head.appendChild(style);
    }

    static setupThemeScheduling() {
        this.updateScheduledTheme();

        // Check every minute for theme updates
        this._schedulingTimer = setInterval(() => {
            this.updateScheduledTheme();
        }, 60000);

        // Also check when page becomes visible
        document.addEventListener('visibilitychange', () => {
            if (!document.hidden) {
                this.updateScheduledTheme();
            }
        });
    }

    static updateScheduledTheme() {
        if (!this.enableScheduling || this.forcedTheme || !this.scheduleConfig) return;

        const now = new Date();
        const currentTime = now.getHours() * 60 + now.getMinutes();

        // Parse schedule times
        const lightStart = this.parseTime(this.scheduleConfig.lightStart);
        const darkStart = this.parseTime(this.scheduleConfig.darkStart);

        // Determine if it should be dark theme
        let shouldBeDark;
        if (lightStart < darkStart) {
            // Normal case: light 6:00-18:00, dark 18:00-6:00
            shouldBeDark = currentTime >= darkStart || currentTime < lightStart;
        } else {
            // Inverted case: light 18:00-6:00, dark 6:00-18:00
            shouldBeDark = currentTime >= darkStart && currentTime < lightStart;
        }

        const scheduledTheme = shouldBeDark ? 'dark' : 'light';
        const currentTheme = this.getStoredTheme();

        // Only change if current theme is auto or matches the scheduled theme
        if (!currentTheme || currentTheme === 'auto') {
            if (this.getResolvedTheme() !== scheduledTheme) {
                this.applyThemeWithTransition(scheduledTheme, false);
            }
        }
    }

    static parseTime(timeStr) {
        const [hours, minutes] = timeStr.split(':').map(Number);
        return hours * 60 + minutes;
    }

    static setupSystemThemeWatcher() {
        const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
        const handler = (e) => {
            this.debouncedThemeUpdate(() => {
                const currentTheme = this.getStoredTheme();
                if (!currentTheme || currentTheme === 'auto') {
                    this.applyThemeWithTransition(this.getPreferredTheme());
                }
                this.notifySystemThemeChange(e.matches ? 'dark' : 'light');
            });
        };

        mediaQuery.addEventListener('change', handler);

        // Store reference for cleanup
        this._systemThemeHandler = handler;
        this._systemMediaQuery = mediaQuery;
    }

    static setupStorageSync() {
        window.addEventListener('storage', (event) => {
            if (event.key === this.storageKey) {
                this.debouncedThemeUpdate(() => {
                    const newTheme = event.newValue || this.getSystemTheme();
                    this.applyThemeWithTransition(newTheme, false);
                });
            }
        });
    }

    static debouncedThemeUpdate(callback) {
        if (this._debounceTimer) {
            clearTimeout(this._debounceTimer);
        }

        this._debounceTimer = setTimeout(() => {
            callback();
            this._debounceTimer = null;
        }, this.debounceDelay);
    }

    static async applyThemeWithTransition(theme, persist = true, transitionOptions = {}) {
        if (this._isTransitioning) return;

        if (!this.themes.includes(theme)) {
            console.warn(`Invalid theme: ${theme}`);
            theme = this.getPreferredTheme();
        }
        const currentTheme = this.getResolvedTheme();
        const resolvedNewTheme = theme === 'auto' ? this.getSystemTheme() : theme;

        // Skip transition if theme hasn't actually changed
        if (currentTheme === resolvedNewTheme) {
            if (persist) this.persistTheme(theme);
            return;
        }

        // Apply transition if enabled and themes are different
        if (!this.disableTransitionOnChange && this.transitionType !== 'none') {
            await this.performThemeTransition(theme, persist, transitionOptions);
        } else {
            this.applyTheme(theme, persist);
        }
    }

    // AFTER THE FIX
    static async performThemeTransition(newTheme, persist, options = {}) {
        this._isTransitioning = true;

        // CRITICAL FIX: Guard against null options passed from Blazor.
        const finalOptions = options || {};

        const transitionType = finalOptions.type || this.transitionType;
        const duration = finalOptions.duration || this.transitionDuration;

        // Create transition overlay - Pass the guarded object!
        this.createTransitionOverlay(transitionType, finalOptions);

        try {
            // Start transition
            await this.startTransition(transitionType);

            // Apply theme during transition
            setTimeout(() => {
                this.applyTheme(newTheme, persist);
            }, duration / 2);

            // End transition
            setTimeout(async () => {
                await this.endTransition();
                this._isTransitioning = false;
            }, duration);

        } catch (error) {
            console.error('Theme transition failed:', error);
            this.applyTheme(newTheme, persist);
            this._isTransitioning = false;
        }
        finally {
            if (this._transitionElement) {
                this._transitionElement.remove();
                this._transitionElement = null;
            }
            this._isTransitioning = false;
        }
    }

    static createTransitionOverlay(type, options) {
        // Remove existing overlay
        if (this._transitionElement) {
            this._transitionElement.remove();
        }

        const overlay = document.createElement('div');
        overlay.className = `theme-transition-overlay theme-transition-${type}`;

        // Set transition color based on target theme
        const targetTheme = options.targetTheme || 'dark';
        const transitionColor = targetTheme === 'dark' ? '#000000' : '#ffffff';
        overlay.style.setProperty('--theme-transition-color', transitionColor);

        // Special handling for ripple effect
        if (type === 'ripple' && options.clickPosition) {
            const { x, y } = options.clickPosition;
            overlay.style.setProperty('--ripple-x', `${x}px`);
            overlay.style.setProperty('--ripple-y', `${y}px`);
        }

        document.body.appendChild(overlay);
        this._transitionElement = overlay;
    }

    static async startTransition(type) {
        return new Promise((resolve) => {
            if (!this._transitionElement) {
                resolve();
                return;
            }

            // Force reflow
            this._transitionElement.offsetHeight;

            // Start transition
            this._transitionElement.classList.add('active');

            // Resolve after transition starts
            setTimeout(resolve, 50);
        });
    }

    static async endTransition() {
        return new Promise((resolve) => {
            if (!this._transitionElement) {
                resolve();
                return;
            }

            const element = this._transitionElement;

            // End transition
            element.classList.remove('active');

            // Remove element after transition
            setTimeout(() => {
                if (element.parentNode) {
                    element.parentNode.removeChild(element);
                }
                if (this._transitionElement === element) {
                    this._transitionElement = null;
                }
                resolve();
            }, this.transitionDuration);
        });
    }

    static getPreferredTheme() {
        if (this.forcedTheme) return this.forcedTheme;

        const storedTheme = this.getStoredTheme();
        if (storedTheme && storedTheme !== 'auto') return storedTheme;

        return this.enableSystem ? this.getSystemTheme() : 'light';
    }

    static getStoredTheme() {
        try {
            // Return only if value is valid
            const theme = localStorage.getItem(this.storageKey);
            return this.themes.includes(theme) ? theme : null;
        } catch {
            return null;
        }
    }

    static getSystemTheme() {
        return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    }

    static applyTheme(theme, persist = true) {
        // Handle forced theme
        if (this.forcedTheme) {
            theme = this.forcedTheme;
            persist = false;
        }

        // Validate theme
        if (!this.themes.includes(theme)) {
            theme = 'light';
        }

        // Resolve 'auto' theme
        const resolvedTheme = theme === 'auto' ? this.getSystemTheme() : theme;

        // Disable transitions temporarily if configured (for non-animated changes)
        if (this.disableTransitionOnChange && !this._isTransitioning) {
            this.disableTransitions();
        }

        // Apply to document
        document.documentElement.setAttribute(this.attributeName, resolvedTheme);
        document.documentElement.setAttribute(`${this.attributeName}-resolved`, resolvedTheme);

        // Store the selected theme (not resolved)
        if (persist) {
            this.persistTheme(theme);
        }

        // Update color scheme
        if (this.enableColorScheme) {
            this.updateColorScheme(resolvedTheme);
        }

        // Apply custom CSS properties
        this.applyThemeVariables(resolvedTheme);

        // Re-enable transitions
        if (this.disableTransitionOnChange && !this._isTransitioning) {
            requestAnimationFrame(() => this.enableTransitions());
        }

        // Notify observers
        this.notifyObservers();
    }

    static persistTheme(theme) {
        try {
            if (theme === this.getSystemTheme() && this.enableSystem) {
                localStorage.removeItem(this.storageKey);
            } else {
                localStorage.setItem(this.storageKey, theme);
            }
        } catch (error) {
            console.warn('Theme persistence failed:', error);
        }
    }

    static disableTransitions() {
        const css = document.createElement('style');
        css.appendChild(document.createTextNode(
            `*,*::before,*::after{-webkit-transition:none!important;-moz-transition:none!important;-o-transition:none!important;-ms-transition:none!important;transition:none!important}`
        ));
        if (this.nonce) css.nonce = this.nonce;
        document.head.appendChild(css);

        this._transitionDisableStyle = css;
    }

    static enableTransitions() {
        if (this._transitionDisableStyle) {
            document.head.removeChild(this._transitionDisableStyle);
            this._transitionDisableStyle = null;
        }
    }

    static updateColorScheme(theme = null) {
        const resolvedTheme = theme || this.getResolvedTheme();
        let metaTag = document.querySelector('meta[name="color-scheme"]');

        if (!metaTag) {
            metaTag = document.createElement('meta');
            metaTag.name = 'color-scheme';
            document.head.appendChild(metaTag);
        }

        metaTag.content = resolvedTheme === 'dark' ? 'dark light' : 'light dark';
    }

    static applyThemeVariables(theme) {
        // Remove existing theme classes
        document.documentElement.className = document.documentElement.className
            .replace(/theme-\w+/g, '');

        // Add theme class
        document.documentElement.classList.add(`theme-${theme}`);

        // Set CSS custom properties
        document.documentElement.style.setProperty('--theme-name', theme);
    }

    static getTheme() {
        return this.getStoredTheme() || 'auto';
    }

    static getResolvedTheme() {
        return document.documentElement.getAttribute(`${this.attributeName}-resolved`) || 'light';
    }

    static setTheme(theme, transitionOptions = {}) {
        this.debouncedThemeUpdate(() => {
            this.applyThemeWithTransition(theme, true, transitionOptions);
        });
    }

    static addCustomTheme(themeName, themeConfig = {}) {
        if (!this.themes.includes(themeName)) {
            this.themes.push(themeName);
            this.customThemes.add(themeName);

            // Apply theme configuration if provided
            if (Object.keys(themeConfig).length > 0) {
                this.applyCustomThemeConfig(themeName, themeConfig);
            }

            return true;
        }
        return false;
    }

    static removeCustomTheme(themeName) {
        if (this.customThemes.has(themeName)) {
            this.themes = this.themes.filter(t => t !== themeName);
            this.customThemes.delete(themeName);

            // Switch to default theme if current theme is being removed
            if (this.getTheme() === themeName) {
                this.setTheme('light');
            }

            return true;
        }
        return false;
    }

    static applyCustomThemeConfig(themeName, config) {
        // Create CSS variables for custom theme
        const style = document.createElement('style');
        let css = `[${this.attributeName}="${themeName}"] {\n`;

        Object.entries(config).forEach(([key, value]) => {
            css += `  --${key}: ${value};\n`;
        });

        css += '}\n';
        style.textContent = css;

        if (this.nonce) style.nonce = this.nonce;
        document.head.appendChild(style);
    }

    static setSchedule(config) {
        this.scheduleConfig = { ...this.scheduleConfig, ...config };
        if (this.enableScheduling) {
            this.updateScheduledTheme();
        }
    }

    static enableThemeScheduling(enable = true) {
        this.enableScheduling = enable;

        if (enable) {
            this.setupThemeScheduling();
        } else if (this._schedulingTimer) {
            clearInterval(this._schedulingTimer);
            this._schedulingTimer = null;
        }
    }

    static setTransitionType(type, duration = null) {
        this.transitionType = type;
        if (duration !== null) {
            this.transitionDuration = duration;
        }
    }

    static subscribe(callback) {
        this.observers.add(callback);
        return () => this.observers.delete(callback);
    }

    static subscribeToSystemTheme(callback) {
        this.colorSchemeChangeListeners.add(callback);
        return () => this.colorSchemeChangeListeners.delete(callback);
    }

    static notifyObservers() {
        const currentState = {
            theme: this.getTheme(),
            resolvedTheme: this.getResolvedTheme(),
            systemTheme: this.getSystemTheme(),
            themes: [...this.themes],
            forcedTheme: this.forcedTheme,
            isTransitioning: this._isTransitioning,
            schedulingEnabled: this.enableScheduling,
            scheduleConfig: { ...this.scheduleConfig }
        };

        this.observers.forEach(callback => {
            try {
                callback(currentState);
            } catch (error) {
                console.error('Theme observer error:', error);
            }
        });
    }

    static notifySystemThemeChange(systemTheme) {
        this.colorSchemeChangeListeners.forEach(callback => {
            try {
                callback(systemTheme);
            } catch (error) {
                console.error('System theme observer error:', error);
            }
        });
    }

    static forceTheme(theme) {
        this.forcedTheme = theme;
        if (theme) {
            this.applyThemeWithTransition(theme, false);
        } else {
            this.applyThemeWithTransition(this.getPreferredTheme());
        }
    }

    static clearForcedTheme() {
        this.forceTheme(null);
    }

    static getThemeState() {
        return {
            theme: this.getTheme(),
            resolvedTheme: this.getResolvedTheme(),
            systemTheme: this.getSystemTheme(),
            themes: [...this.themes],
            forcedTheme: this.forcedTheme,
            enableSystem: this.enableSystem,
            customThemes: [...this.customThemes],
            isTransitioning: this._isTransitioning,
            schedulingEnabled: this.enableScheduling,
            scheduleConfig: { ...this.scheduleConfig },
            transitionType: this.transitionType,
            transitionDuration: this.transitionDuration
        };
    }

    static destroy() {
        // Clear timers
        if (this._debounceTimer) {
            clearTimeout(this._debounceTimer);
            this._debounceTimer = null;
        }

        if (this._schedulingTimer) {
            clearInterval(this._schedulingTimer);
            this._schedulingTimer = null;
        }

        // Clean up event listeners
        if (this._systemThemeHandler && this._systemMediaQuery) {
            this._systemMediaQuery.removeEventListener('change', this._systemThemeHandler);
        }

        // Clear observers
        this.observers.clear();
        this.colorSchemeChangeListeners.clear();

        // Remove transition elements
        if (this._transitionElement) {
            this._transitionElement.remove();
            this._transitionElement = null;
        }

        // Remove transition disable style if exists
        if (this._transitionDisableStyle) {
            this.enableTransitions();
        }

        // Remove transition styles
        const transitionStyles = document.getElementById('theme-transition-styles');
        if (transitionStyles) {
            transitionStyles.remove();
        }
    }
}

// ES6 Export Functions for Blazor Integration
export function init(options) {
    ThemeManager.init(options);
    return ThemeManager.getThemeState();
}

export function setTheme(theme, transitionOptions) {
    ThemeManager.setTheme(theme, transitionOptions);
    return ThemeManager.getThemeState();
}

export function getTheme() {
    return ThemeManager.getTheme();
}

export function getResolvedTheme() {
    return ThemeManager.getResolvedTheme();
}

export function getSystemTheme() {
    return ThemeManager.getSystemTheme();
}

export function getAvailableThemes() {
    return ThemeManager.themes;
}

export function getThemeState() {
    return ThemeManager.getThemeState();
}

export function addCustomTheme(name, config) {
    return ThemeManager.addCustomTheme(name, config);
}

export function removeCustomTheme(name) {
    return ThemeManager.removeCustomTheme(name);
}

export function forceTheme(theme) {
    ThemeManager.forceTheme(theme);
    return ThemeManager.getThemeState();
}

export function clearForcedTheme() {
    ThemeManager.clearForcedTheme();
    return ThemeManager.getThemeState();
}

export function setSchedule(config) {
    ThemeManager.setSchedule(config);
    return ThemeManager.getThemeState();
}

export function enableScheduling(enable) {
    ThemeManager.enableThemeScheduling(enable);
    return ThemeManager.getThemeState();
}

export function setTransitionType(type, duration) {
    ThemeManager.setTransitionType(type, duration);
    return ThemeManager.getThemeState();
}

export function subscribe(dotNetHelper, methodName = 'OnThemeChanged') {
    const unsubscribe = ThemeManager.subscribe((state) => {
        dotNetHelper.invokeMethodAsync(methodName, state);
    });

    return {
        dispose: unsubscribe
    };
}

export function subscribeToSystemTheme(dotNetHelper, methodName = 'OnSystemThemeChanged') {
    const unsubscribe = ThemeManager.subscribeToSystemTheme((systemTheme) => {
        dotNetHelper.invokeMethodAsync(methodName, systemTheme);
    });

    return {
        dispose: unsubscribe
    };
}

export function destroy() {
    ThemeManager.destroy();
}