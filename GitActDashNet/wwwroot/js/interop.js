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

// Initialize theme on page load
document.addEventListener('DOMContentLoaded', function () {
    const savedTheme = window.gitActDashInterop.theme.getTheme();
    window.gitActDashInterop.theme.setTheme(savedTheme);
});
