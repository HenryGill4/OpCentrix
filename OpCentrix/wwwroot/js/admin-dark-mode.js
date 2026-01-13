/* Admin Dark Mode JavaScript - Extracted from _Layout.cshtml for consistency */

// Enhanced Dark Mode System - Responds to both system settings and user preference
(function() {
    // Enhanced theme detection that respects both system and user preference
    const getTheme = () => {
        const stored = localStorage.getItem('theme');
        if (stored && (stored === 'dark' || stored === 'light')) {
            return stored;
        }
        // Only fall back to system preference if no user preference is stored
        return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    };
    
    const setTheme = (theme) => {
        // Validate theme value
        if (theme !== 'dark' && theme !== 'light') {
            theme = 'light';
        }
        
        localStorage.setItem('theme', theme);
        
        if (theme === 'dark') {
            document.documentElement.classList.add('dark');
            document.documentElement.setAttribute('data-theme', 'dark');
        } else {
            document.documentElement.classList.remove('dark');
            document.documentElement.setAttribute('data-theme', 'light');
        }
        
        // Dispatch custom event for components that need to react to theme changes
        window.dispatchEvent(new CustomEvent('themeChanged', { 
            detail: { theme } 
        }));
    };
    
    // Set initial theme
    setTheme(getTheme());
    
    // Enhanced system theme change listener - only applies if user hasn't manually set a preference
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
        const userPreference = localStorage.getItem('theme');
        // Only auto-switch if user hasn't explicitly set a preference
        if (!userPreference) {
            setTheme(e.matches ? 'dark' : 'light');
        }
    });
    
    // Enhanced theme toggle with user preference override
    window.toggleTheme = () => {
        const current = getTheme();
        const newTheme = current === 'dark' ? 'light' : 'dark';
        setTheme(newTheme);
        
        // Provide user feedback
        console.log(`Theme switched to ${newTheme} mode`);
    };
    
    // Get current theme state
    window.getCurrentTheme = getTheme;
    
    // Reset to system preference
    window.resetToSystemTheme = () => {
        localStorage.removeItem('theme');
        const systemTheme = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
        setTheme(systemTheme);
    };
    
    // Force theme (for testing or admin controls)
    window.forceTheme = (theme) => {
        if (theme === 'dark' || theme === 'light') {
            setTheme(theme);
        }
    };
    
    // Enhanced Dark Mode Debug and Monitoring
    if (window.location.search.includes('debug=theme')) {
        // Add debug indicator
        const debugIndicator = document.createElement('div');
        debugIndicator.className = 'theme-indicator';
        debugIndicator.textContent = `Theme: ${getCurrentTheme()}`;
        debugIndicator.style = `
            position: fixed;
            top: 10px;
            right: 10px;
            padding: 4px 8px;
            background: rgba(0,0,0,0.7);
            color: white;
            border-radius: 4px;
            font-size: 10px;
            z-index: 9999;
            pointer-events: none;
        `;
        document.body.appendChild(debugIndicator);
        
        // Update indicator when theme changes
        window.addEventListener('themeChanged', (e) => {
            debugIndicator.textContent = `Theme: ${e.detail.theme}`;
        });
        
        console.log('?? Admin Dark Mode Debug Info:', {
            currentTheme: getCurrentTheme(),
            storedPreference: localStorage.getItem('theme'),
            systemPreference: window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light',
            htmlClasses: document.documentElement.className,
            dataTheme: document.documentElement.getAttribute('data-theme')
        });
    }
    
    // Monitor for theme inconsistencies
    window.addEventListener('themeChanged', (e) => {
        const htmlHasDark = document.documentElement.classList.contains('dark');
        const expectedDark = e.detail.theme === 'dark';
        
        if (htmlHasDark !== expectedDark) {
            console.warn('?? Admin Theme inconsistency detected! Fixing...', {
                expected: expectedDark,
                actual: htmlHasDark,
                theme: e.detail.theme
            });
            
            // Force correction
            if (expectedDark) {
                document.documentElement.classList.add('dark');
            } else {
                document.documentElement.classList.remove('dark');
            }
        }
    });
    
    // Initialize theme on DOM ready
    document.addEventListener('DOMContentLoaded', function() {
        console.log('?? Admin Dark Mode initialized with theme:', getCurrentTheme());
        
        // Ensure theme consistency
        const currentTheme = getCurrentTheme();
        setTheme(currentTheme);
    });
})();