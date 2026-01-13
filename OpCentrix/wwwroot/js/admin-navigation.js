/* Admin Navigation JavaScript - Clean functionality extracted from layout */

document.addEventListener('DOMContentLoaded', function() {
    // Initialize admin navigation functionality
    initializeAdminNavigation();
    
    console.log('??? Admin navigation initialized');
});

function initializeAdminNavigation() {
    // Sidebar toggle functionality
    initializeSidebarToggle();
    
    // Collapsible sections with persisted state
    initializeCollapsibleSections();
    
    // Active path highlighting
    initializeActivePathHighlighting();
    
    // Task notifications
    initializeTaskNotifications();
    
    // Enhanced responsive behavior
    initializeResponsiveBehavior();
}

function initializeTaskNotifications() {
    // Load task notifications for admin users
    loadAdminTaskNotifications();
    
    // Refresh task notifications every 60 seconds
    setInterval(loadAdminTaskNotifications, 60000);
    
    console.log('?? Admin task notifications initialized');
}

function loadAdminTaskNotifications() {
    fetch('/api/tasks/admin-notifications', {
        method: 'GET',
        credentials: 'same-origin',
        headers: {
            'Accept': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
    .then(response => {
        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }
        return response.json();
    })
    .then(data => {
        updateAdminTaskBadges(data);
    })
    .catch(error => {
        console.log('Task notifications not available:', error.message);
        // Gracefully handle when task API is not available
        updateAdminTaskBadges({ totalTasks: 0, urgentTasks: 0, taskPriorities: {} });
    });
}

function updateAdminTaskBadges(data) {
    // Update any task count badges in the admin navigation
    const taskBadges = document.querySelectorAll('.task-badge, .nav-badge[data-task-type]');
    
    taskBadges.forEach(badge => {
        const taskType = badge.getAttribute('data-task-type');
        let count = 0;
        
        switch (taskType) {
            case 'total':
                count = data.totalTasks || 0;
                break;
            case 'urgent':
                count = data.urgentTasks || 0;
                break;
            case 'pending':
                count = data.pendingTasks || 0;
                break;
            default:
                count = data.totalTasks || 0;
        }
        
        badge.textContent = count;
        badge.style.display = count > 0 ? 'inline' : 'none';
        
        // Update badge class based on urgency
        if (data.taskPriorities && data.taskPriorities.critical > 0) {
            badge.className = badge.className.replace(/task-badge-\w+/g, '') + ' task-badge-critical';
        } else if (data.taskPriorities && data.taskPriorities.urgent > 0) {
            badge.className = badge.className.replace(/task-badge-\w+/g, '') + ' task-badge-urgent';
        } else if (count > 0) {
            badge.className = badge.className.replace(/task-badge-\w+/g, '') + ' task-badge-normal';
        }
    });
}

function initializeResponsiveBehavior() {
    // Enhanced responsive behavior for admin navigation
    let isMobile = window.innerWidth < 768;
    
    function handleResize() {
        const wasMobile = isMobile;
        isMobile = window.innerWidth < 768;
        
        if (wasMobile !== isMobile) {
            // Responsive state changed
            if (isMobile) {
                // Switched to mobile - auto-close sidebar
                const sidebar = document.getElementById('sidebar');
                if (sidebar) {
                    sidebar.classList.add('-translate-x-full');
                }
            } else {
                // Switched to desktop - show sidebar
                const sidebar = document.getElementById('sidebar');
                if (sidebar) {
                    sidebar.classList.remove('-translate-x-full');
                }
            }
        }
    }
    
    window.addEventListener('resize', handleResize);
    
    // Handle keyboard navigation
    document.addEventListener('keydown', function(e) {
        // Escape key closes mobile sidebar
        if (e.key === 'Escape' && isMobile) {
            const sidebar = document.getElementById('sidebar');
            if (sidebar && !sidebar.classList.contains('-translate-x-full')) {
                sidebar.classList.add('-translate-x-full');
            }
        }
    });
}

function initializeSidebarToggle() {
    const sidebar = document.getElementById('sidebar');
    
    // Mobile sidebar toggle function
    window.toggleSidebar = function() {
        if (sidebar) {
            sidebar.classList.toggle('-translate-x-full');
        }
    };
}

function initializeCollapsibleSections() {
    const sections = document.querySelectorAll('.nav-section');
    const storageKey = (key) => `oc:admin:nav:${key}`;

    function setCollapsed(section, collapsed) {
        const btn = section.querySelector('.nav-section-toggle');
        const content = section.querySelector('.nav-section-content');
        if (!btn || !content) return;
        
        if (collapsed) {
            section.classList.add('collapsed');
            btn.setAttribute('aria-expanded', 'false');
            content.style.display = 'none';
            
            // Update chevron
            const chevron = btn.querySelector('.chevron i');
            if (chevron) chevron.className = 'fa-solid fa-chevron-right';
        } else {
            section.classList.remove('collapsed');
            btn.setAttribute('aria-expanded', 'true');
            content.style.display = '';
            
            // Update chevron
            const chevron = btn.querySelector('.chevron i');
            if (chevron) chevron.className = 'fa-solid fa-chevron-down';
        }
    }

    sections.forEach(sec => {
        const key = sec.getAttribute('data-key');
        const btn = sec.querySelector('.nav-section-toggle');
        
        if (!key || !btn) return;
        
        // Restore saved state or default to expanded
        const saved = localStorage.getItem(storageKey(key));
        const collapsed = saved === 'true'; // Default to expanded for admin
        setCollapsed(sec, collapsed);

        // Toggle handler
        btn.addEventListener('click', (e) => {
            e.preventDefault();
            const isCollapsed = sec.classList.contains('collapsed');
            setCollapsed(sec, !isCollapsed);
            localStorage.setItem(storageKey(key), (!isCollapsed).toString());
        });

        // Keyboard accessibility
        btn.setAttribute('role', 'button');
        btn.setAttribute('tabindex', '0');
        btn.addEventListener('keydown', (e) => {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                btn.click();
            }
        });
    });

    // Global expand/collapse functions (if buttons exist)
    const expandAllBtn = document.getElementById('expandAllBtn');
    const collapseAllBtn = document.getElementById('collapseAllBtn');
    
    if (expandAllBtn && collapseAllBtn) {
        expandAllBtn.addEventListener('click', () => {
            sections.forEach(sec => {
                const key = sec.getAttribute('data-key');
                if (key) {
                    setCollapsed(sec, false);
                    localStorage.setItem(storageKey(key), 'false');
                }
            });
        });
        
        collapseAllBtn.addEventListener('click', () => {
            sections.forEach(sec => {
                const key = sec.getAttribute('data-key');
                if (key) {
                    setCollapsed(sec, true);
                    localStorage.setItem(storageKey(key), 'true');
                }
            });
        });
    }
}

function initializeActivePathHighlighting() {
    const currentPath = window.location.pathname;
    const routeData = getCurrentRouteData();
    
    console.log('?? Admin nav highlighting for path:', currentPath, 'Route data:', routeData);
    
    // Clear any existing active states
    document.querySelectorAll('.nav-item-active').forEach(item => {
        item.classList.remove('nav-item-active');
    });
    
    // Find and highlight the current page with enhanced matching
    let activeItem = null;
    let bestMatchScore = 0;
    
    document.querySelectorAll('.nav-item').forEach(item => {
        const href = item.getAttribute('href');
        if (!href) return;
        
        let matchScore = 0;
        
        // Exact path match (highest priority)
        if (currentPath === href) {
            matchScore = 100;
        }
        // Path starts with href (for sub-pages)
        else if (currentPath.startsWith(href + '/')) {
            matchScore = 80;
        }
        // Route-based matching for admin pages
        else if (routeData.page && href.includes(routeData.page)) {
            matchScore = 60;
        }
        // Handler-based matching
        else if (routeData.handler && href.includes(routeData.handler)) {
            matchScore = 40;
        }
        // Contains check for complex routes
        else if (href !== '/' && currentPath.includes(href)) {
            matchScore = 20;
        }
        
        if (matchScore > bestMatchScore) {
            bestMatchScore = matchScore;
            activeItem = item;
        }
    });
    
    // Apply active state to best match
    if (activeItem) {
        activeItem.classList.add('nav-item-active');
        console.log('? Admin nav activated:', activeItem.getAttribute('href'), 'Score:', bestMatchScore);
        
        // Auto-expand parent section if collapsed
        const parent = activeItem.closest('.nav-section');
        if (parent && parent.classList.contains('collapsed')) {
            const key = parent.getAttribute('data-key');
            if (key) {
                expandSection(parent, key);
                console.log('?? Auto-expanded section:', key);
            }
        }
        
        // Scroll into view if needed
        setTimeout(() => {
            const rect = activeItem.getBoundingClientRect();
            const sidebar = document.getElementById('sidebar');
            if (sidebar && (rect.top < 0 || rect.bottom > window.innerHeight)) {
                activeItem.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }
        }, 100);
    } else {
        console.log('? No matching nav item found for:', currentPath);
    }
}

function getCurrentRouteData() {
    const path = window.location.pathname;
    const search = window.location.search;
    const url = new URLSearchParams(search);
    
    return {
        page: extractPageFromPath(path),
        handler: url.get('handler'),
        area: extractAreaFromPath(path),
        controller: extractControllerFromPath(path)
    };
}

function extractPageFromPath(path) {
    // Extract page name from admin paths like /Admin/Users -> Users
    const parts = path.split('/').filter(p => p);
    if (parts.length >= 2 && parts[0] === 'Admin') {
        return parts[1];
    }
    return null;
}

function extractAreaFromPath(path) {
    const parts = path.split('/').filter(p => p);
    return parts.length > 0 ? parts[0] : null;
}

function extractControllerFromPath(path) {
    const parts = path.split('/').filter(p => p);
    return parts.length > 1 ? parts[1] : null;
}

function expandSection(section, key) {
    const btn = section.querySelector('.nav-section-toggle');
    const content = section.querySelector('.nav-section-content');
    
    if (btn && content) {
        section.classList.remove('collapsed');
        btn.setAttribute('aria-expanded', 'true');
        content.style.display = '';
        
        // Update chevron
        const chevron = btn.querySelector('.chevron i');
        if (chevron) chevron.className = 'fa-solid fa-chevron-down';
        
        // Save state
        localStorage.setItem(`oc:admin:nav:${key}`, 'false');
    }
}

// Enhanced admin navigation helpers with main layout features
window.AdminNavigation = {
    // Programmatically expand a section
    expandSection: function(sectionKey) {
        const section = document.querySelector(`[data-key="${sectionKey}"]`);
        if (section) {
            expandSection(section, sectionKey);
        }
    },
    
    // Programmatically collapse a section
    collapseSection: function(sectionKey) {
        const section = document.querySelector(`[data-key="${sectionKey}"]`);
        if (section) {
            collapseSection(section, sectionKey);
        }
    },
    
    // Highlight a specific navigation item
    highlightNavItem: function(href) {
        // Clear existing highlights
        document.querySelectorAll('.nav-item-active').forEach(item => {
            item.classList.remove('nav-item-active');
        });
        
        // Find and highlight the specific item
        const item = document.querySelector(`.nav-item[href="${href}"]`);
        if (item) {
            item.classList.add('nav-item-active');
            
            // Auto-expand parent section
            const parent = item.closest('.nav-section');
            if (parent && parent.classList.contains('collapsed')) {
                const key = parent.getAttribute('data-key');
                if (key) {
                    this.expandSection(key);
                }
            }
            
            return true;
        }
        return false;
    },
    
    // Toggle mobile sidebar
    toggleMobileSidebar: function() {
        const sidebar = document.getElementById('sidebar');
        if (sidebar) {
            sidebar.classList.toggle('-translate-x-full');
            return !sidebar.classList.contains('-translate-x-full');
        }
        return false;
    },
    
    // Refresh task notifications
    refreshTaskNotifications: function() {
        if (typeof loadAdminTaskNotifications === 'function') {
            loadAdminTaskNotifications();
        }
    },
    
    // Reset all navigation state
    resetNavigationState: function() {
        const sections = document.querySelectorAll('.nav-section');
        sections.forEach(sec => {
            const key = sec.getAttribute('data-key');
            if (key) {
                localStorage.removeItem(`oc:admin:nav:${key}`);
            }
        });
        location.reload();
    },
    
    // Get current navigation state
    getNavigationState: function() {
        const state = {
            currentPath: window.location.pathname,
            activeItem: null,
            expandedSections: {},
            isMobile: window.innerWidth < 768
        };
        
        // Get active item
        const activeItem = document.querySelector('.nav-item-active');
        if (activeItem) {
            state.activeItem = activeItem.getAttribute('href');
        }
        
        // Get expanded sections
        document.querySelectorAll('.nav-section').forEach(sec => {
            const key = sec.getAttribute('data-key');
            if (key) {
                state.expandedSections[key] = !sec.classList.contains('collapsed');
            }
        });
        
        return state;
    },
    
    // Navigate to admin page with proper highlighting
    navigateToPage: function(href, openInNewTab = false) {
        if (openInNewTab) {
            window.open(href, '_blank');
        } else {
            // Pre-highlight the navigation item for immediate feedback
            this.highlightNavItem(href);
            window.location.href = href;
        }
    }
};

function collapseSection(section, key) {
    const btn = section.querySelector('.nav-section-toggle');
    const content = section.querySelector('.nav-section-content');
    
    if (btn && content) {
        section.classList.add('collapsed');
        btn.setAttribute('aria-expanded', 'false');
        content.style.display = 'none';
        
        // Update chevron
        const chevron = btn.querySelector('.chevron i');
        if (chevron) chevron.className = 'fa-solid fa-chevron-right';
        
        // Save state
        localStorage.setItem(`oc:admin:nav:${key}`, 'true');
    }
}

// Enhanced debug helper (only in development)
if (window.location.search.includes('debug=nav')) {
    console.log('??? Admin Navigation Debug Mode Enabled');
    console.log('Navigation state:', window.AdminNavigation.getNavigationState());
    console.log('Current path:', window.location.pathname);
    console.log('Available commands:');
    console.log('- AdminNavigation.expandSection("sectionKey")');
    console.log('- AdminNavigation.collapseSection("sectionKey")');
    console.log('- AdminNavigation.highlightNavItem("/path")');
    console.log('- AdminNavigation.toggleMobileSidebar()');
    console.log('- AdminNavigation.refreshTaskNotifications()');
    console.log('- AdminNavigation.navigateToPage("/path")');
    console.log('- AdminNavigation.resetNavigationState()');
    console.log('- AdminNavigation.getNavigationState()');
    
    // Add debug panel
    setTimeout(() => {
        if (!document.getElementById('nav-debug-panel')) {
            const panel = document.createElement('div');
            panel.id = 'nav-debug-panel';
            panel.style.cssText = `
                position: fixed;
                top: 10px;
                right: 10px;
                background: rgba(0, 0, 0, 0.8);
                color: white;
                padding: 10px;
                border-radius: 8px;
                font-size: 12px;
                z-index: 9999;
                max-width: 300px;
                font-family: monospace;
            `;
            panel.innerHTML = `
                <div><strong>??? Admin Nav Debug</strong></div>
                <div>Path: ${window.location.pathname}</div>
                <div>Active: ${document.querySelector('.nav-item-active')?.getAttribute('href') || 'None'}</div>
                <button onclick="initializeActivePathHighlighting()" style="margin-top: 5px; padding: 2px 6px; font-size: 11px;">Refresh Highlighting</button>
                <button onclick="this.parentElement.remove()" style="margin-top: 5px; margin-left: 5px; padding: 2px 6px; font-size: 11px;">Close</button>
            `;
            document.body.appendChild(panel);
        }
    }, 100);
}