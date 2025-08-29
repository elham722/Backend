/**
 * Anti-Forgery (CSRF) Token Management for AJAX Requests
 */

// Anti-Forgery token management
const AntiForgeryHelper = {
    // Token from server-side (set in ViewBag)
    token: null,
    headerName: 'X-CSRF-TOKEN',

    /**
     * Initialize Anti-Forgery helper
     */
    init: function() {
        // Get token from meta tag or global variable
        this.token = document.querySelector('meta[name="csrf-token"]')?.getAttribute('content') || 
                    window.antiForgeryToken || 
                    this.getTokenFromCookie();
        
        this.headerName = document.querySelector('meta[name="csrf-header"]')?.getAttribute('content') || 
                         window.antiForgeryHeaderName || 
                         'X-CSRF-TOKEN';

        console.log('Anti-Forgery helper initialized');
    },

    /**
     * Get token from cookie
     */
    getTokenFromCookie: function() {
        const name = 'CSRF-TOKEN=';
        const decodedCookie = decodeURIComponent(document.cookie);
        const cookieArray = decodedCookie.split(';');
        
        for (let i = 0; i < cookieArray.length; i++) {
            let cookie = cookieArray[i];
            while (cookie.charAt(0) === ' ') {
                cookie = cookie.substring(1);
            }
            if (cookie.indexOf(name) === 0) {
                return cookie.substring(name.length, cookie.length);
            }
        }
        return null;
    },

    /**
     * Get current token
     */
    getToken: function() {
        if (!this.token) {
            this.init();
        }
        return this.token;
    },

    /**
     * Get header name
     */
    getHeaderName: function() {
        return this.headerName;
    },

    /**
     * Add Anti-Forgery token to headers
     */
    addTokenToHeaders: function(headers = {}) {
        const token = this.getToken();
        if (token) {
            headers[this.headerName] = token;
        }
        return headers;
    },

    /**
     * Make AJAX request with Anti-Forgery token
     */
    ajax: function(options) {
        // Add Anti-Forgery token to headers
        if (!options.headers) {
            options.headers = {};
        }
        
        this.addTokenToHeaders(options.headers);

        // Add default error handling
        if (!options.error) {
            options.error = function(xhr, status, error) {
                console.error('AJAX request failed:', status, error);
                if (xhr.status === 400 && xhr.responseJSON?.message?.includes('CSRF')) {
                    alert('خطای امنیتی: لطفاً صفحه را رفرش کنید و دوباره تلاش کنید.');
                    location.reload();
                }
            };
        }

        return $.ajax(options);
    },

    /**
     * Make POST request with Anti-Forgery token
     */
    post: function(url, data, options = {}) {
        return this.ajax({
            url: url,
            type: 'POST',
            data: data,
            ...options
        });
    },

    /**
     * Make PUT request with Anti-Forgery token
     */
    put: function(url, data, options = {}) {
        return this.ajax({
            url: url,
            type: 'PUT',
            data: data,
            ...options
        });
    },

    /**
     * Make DELETE request with Anti-Forgery token
     */
    delete: function(url, options = {}) {
        return this.ajax({
            url: url,
            type: 'DELETE',
            ...options
        });
    },

    /**
     * Refresh token from server
     */
    refreshToken: function() {
        return $.get('/api/antiforgery/token')
            .done(function(response) {
                if (response.token) {
                    AntiForgeryHelper.token = response.token;
                    console.log('Anti-Forgery token refreshed');
                }
            })
            .fail(function() {
                console.warn('Failed to refresh Anti-Forgery token');
            });
    }
};

// jQuery plugin for easy integration
$.fn.antiForgeryAjax = function(options) {
    return AntiForgeryHelper.ajax(options);
};

// Global function for easy access
window.antiForgeryAjax = function(options) {
    return AntiForgeryHelper.ajax(options);
};

// Initialize on document ready
$(document).ready(function() {
    AntiForgeryHelper.init();
    
    // Add token to all AJAX requests automatically
    $.ajaxSetup({
        beforeSend: function(xhr, settings) {
            // Only add token for state-changing requests
            if (settings.type === 'POST' || settings.type === 'PUT' || settings.type === 'DELETE') {
                const token = AntiForgeryHelper.getToken();
                if (token) {
                    xhr.setRequestHeader(AntiForgeryHelper.getHeaderName(), token);
                }
            }
        }
    });
});

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = AntiForgeryHelper;
} 