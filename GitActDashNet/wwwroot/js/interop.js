// JavaScript interop functions for GitActDash

window.gitActDashInterop = {
    // Theme management
    theme: {
        setTheme: function (theme) {
            document.documentElement.setAttribute('data-bs-theme', theme);
            localStorage.setItem('gitactdash-theme', theme);
        },
        
        getTheme: function () {
            return localStorage.getItem('gitactdash-theme') || 'light';
        },
        
        toggleTheme: function () {
            const currentTheme = this.getTheme();
            const newTheme = currentTheme === 'light' ? 'dark' : 'light';
            this.setTheme(newTheme);
            return newTheme;
        }
    },

    // Sidebar management
    sidebar: {
        setSidebarState: function (isCollapsed) {
            const pageElement = document.querySelector('.page');
            const sidebarElement = document.querySelector('.sidebar');
            
            if (pageElement && sidebarElement) {
                if (isCollapsed) {
                    pageElement.classList.add('sidebar-collapsed');
                    sidebarElement.classList.add('collapsed');
                } else {
                    pageElement.classList.remove('sidebar-collapsed');
                    sidebarElement.classList.remove('collapsed');
                }
                
                // Save state to localStorage
                localStorage.setItem('gitactdash-sidebar-collapsed', isCollapsed.toString());
                
                // Trigger custom event for components that need to react
                window.dispatchEvent(new CustomEvent('sidebarStateChanged', { 
                    detail: { isCollapsed: isCollapsed } 
                }));
            }
        },
        
        getSidebarState: function () {
            const saved = localStorage.getItem('gitactdash-sidebar-collapsed');
            return saved === 'true';
        },
        
        initializeSidebar: function () {
            const isCollapsed = this.getSidebarState();
            this.setSidebarState(isCollapsed);
            return isCollapsed;
        },
        
        toggleSidebar: function () {
            const currentState = this.getSidebarState();
            const newState = !currentState;
            this.setSidebarState(newState);
            return newState;
        }
    },

    // Fullscreen management
    fullscreen: {
        isFullscreen: function () {
            return !!(document.fullscreenElement || document.webkitFullscreenElement || 
                     document.mozFullScreenElement || document.msFullscreenElement);
        },
        
        enterFullscreen: function (element) {
            if (element.requestFullscreen) {
                element.requestFullscreen();
            } else if (element.webkitRequestFullscreen) {
                element.webkitRequestFullscreen();
            } else if (element.mozRequestFullScreen) {
                element.mozRequestFullScreen();
            } else if (element.msRequestFullscreen) {
                element.msRequestFullscreen();
            }
        },
        
        exitFullscreen: function () {
            if (document.exitFullscreen) {
                document.exitFullscreen();
            } else if (document.webkitExitFullscreen) {
                document.webkitExitFullscreen();
            } else if (document.mozCancelFullScreen) {
                document.mozCancelFullScreen();
            } else if (document.msExitFullscreen) {
                document.msExitFullscreen();
            }
        },
        
        toggleFullscreen: function (elementId) {
            const element = elementId ? document.getElementById(elementId) : document.documentElement;
            
            if (this.isFullscreen()) {
                this.exitFullscreen();
                return false;
            } else {
                this.enterFullscreen(element);
                return true;
            }
        }
    },

    // Utility functions
    scrollToTop: function () {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    },
    
    scrollToElement: function (elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            element.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
    },
    
    copyToClipboard: function (text) {
        return navigator.clipboard.writeText(text).then(() => true).catch(() => false);
    },
    
    openInNewTab: function (url) {
        window.open(url, '_blank', 'noopener,noreferrer');
    }
};

// Legacy functions for backward compatibility
window.updateSidebarState = function (isCollapsed) {
    window.gitActDashInterop.sidebar.setSidebarState(isCollapsed);
};

window.initializeSidebar = function () {
    return window.gitActDashInterop.sidebar.initializeSidebar();
};

// Keyboard shortcuts
function setupKeyboardShortcuts() {
    document.addEventListener('keydown', function (event) {
        // Ctrl/Cmd + B to toggle sidebar
        if ((event.ctrlKey || event.metaKey) && event.key === 'b') {
            event.preventDefault();
            window.gitActDashInterop.sidebar.toggleSidebar();
            
            // Update the toggle button if it exists
            const toggleButton = document.getElementById('sidebar-toggle-btn');
            if (toggleButton) {
                toggleButton.dispatchEvent(new Event('sidebarToggled'));
            }
        }
        
        // Ctrl/Cmd + Shift + D to toggle theme
        if ((event.ctrlKey || event.metaKey) && event.shiftKey && event.key === 'D') {
            event.preventDefault();
            window.gitActDashInterop.theme.toggleTheme();
        }
    });
}

// Initialize theme and sidebar on page load
document.addEventListener('DOMContentLoaded', function () {
    const savedTheme = window.gitActDashInterop.theme.getTheme();
    window.gitActDashInterop.theme.setTheme(savedTheme);
    
    // Initialize sidebar state from localStorage
    window.gitActDashInterop.sidebar.initializeSidebar();
    
    // Setup keyboard shortcuts
    setupKeyboardShortcuts();
});
