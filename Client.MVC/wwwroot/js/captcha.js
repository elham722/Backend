/**
 * CAPTCHA functionality for Client.MVC
 * Uses Google reCAPTCHA v3
 */

class CaptchaService {
    constructor() {
        this.siteKey = '';
        this.action = 'register';
        this.threshold = 0.5;
        this.isEnabled = false;
        this.isLoaded = false;
        this.init();
    }

    /**
     * Initialize CAPTCHA service
     */
    async init() {
        try {
            // Load reCAPTCHA script
            await this.loadRecaptchaScript();
            
            // Get configuration from API
            await this.loadConfiguration();
            
            // Initialize reCAPTCHA
            if (this.isEnabled) {
                this.initializeRecaptcha();
            }
        } catch (error) {
            console.error('Error initializing CAPTCHA:', error);
        }
    }

    /**
     * Load Google reCAPTCHA script
     */
    loadRecaptchaScript() {
        return new Promise((resolve, reject) => {
            if (window.grecaptcha) {
                resolve();
                return;
            }

            const script = document.createElement('script');
            script.src = 'https://www.google.com/recaptcha/api.js?render=' + this.siteKey;
            script.async = true;
            script.defer = true;
            
            script.onload = () => {
                this.isLoaded = true;
                resolve();
            };
            
            script.onerror = () => {
                reject(new Error('Failed to load reCAPTCHA script'));
            };

            document.head.appendChild(script);
        });
    }

    /**
     * Load CAPTCHA configuration from API
     */
    async loadConfiguration() {
        try {
            const response = await fetch('/api/captcha/config');
            if (!response.ok) {
                throw new Error('Failed to load CAPTCHA configuration');
            }

            const config = await response.json();
            this.siteKey = config.siteKey;
            this.action = config.action;
            this.threshold = config.threshold;
            this.isEnabled = config.isEnabled;

            console.log('CAPTCHA configuration loaded:', config);
        } catch (error) {
            console.error('Error loading CAPTCHA configuration:', error);
            this.isEnabled = false;
        }
    }

    /**
     * Initialize reCAPTCHA
     */
    initializeRecaptcha() {
        if (!this.isLoaded || !this.isEnabled) {
            return;
        }

        try {
            grecaptcha.ready(() => {
                console.log('reCAPTCHA is ready');
            });
        } catch (error) {
            console.error('Error initializing reCAPTCHA:', error);
        }
    }

    /**
     * Execute CAPTCHA and get token
     */
    async execute(action = null) {
        if (!this.isEnabled || !this.isLoaded) {
            console.warn('CAPTCHA is not enabled or not loaded');
            return null;
        }

        try {
            const actionName = action || this.action;
            const token = await grecaptcha.execute(this.siteKey, { action: actionName });
            
            console.log('CAPTCHA executed successfully for action:', actionName);
            return token;
        } catch (error) {
            console.error('Error executing CAPTCHA:', error);
            return null;
        }
    }

    /**
     * Check if CAPTCHA is required for an action
     */
    async isRequired(action = 'default') {
        try {
            const response = await fetch(`/api/captcha/required?action=${action}`);
            if (!response.ok) {
                return true; // Default to requiring CAPTCHA on error
            }

            const result = await response.json();
            return result.isRequired;
        } catch (error) {
            console.error('Error checking CAPTCHA requirement:', error);
            return true; // Default to requiring CAPTCHA on error
        }
    }

    /**
     * Validate CAPTCHA token
     */
    async validate(token) {
        try {
            const response = await fetch('/api/captcha/validate', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ token: token })
            });

            if (!response.ok) {
                throw new Error('CAPTCHA validation failed');
            }

            const result = await response.json();
            return result;
        } catch (error) {
            console.error('Error validating CAPTCHA:', error);
            return {
                isValid: false,
                errorMessage: 'CAPTCHA validation failed'
            };
        }
    }

    /**
     * Get CAPTCHA widget for forms
     */
    createWidget(containerId, options = {}) {
        if (!this.isEnabled) {
            return null;
        }

        const container = document.getElementById(containerId);
        if (!container) {
            console.error('Container not found:', containerId);
            return null;
        }

        // Create invisible CAPTCHA widget
        const widget = document.createElement('div');
        widget.className = 'g-recaptcha';
        widget.setAttribute('data-sitekey', this.siteKey);
        widget.setAttribute('data-size', 'invisible');
        widget.setAttribute('data-callback', options.callback || 'onCaptchaSuccess');
        widget.setAttribute('data-expired-callback', options.expiredCallback || 'onCaptchaExpired');
        widget.setAttribute('data-error-callback', options.errorCallback || 'onCaptchaError');

        container.appendChild(widget);
        return widget;
    }
}

// Global CAPTCHA service instance
let captchaService;

// Initialize CAPTCHA when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    captchaService = new CaptchaService();
});

// Global callback functions for reCAPTCHA
window.onCaptchaSuccess = function(token) {
    console.log('CAPTCHA success, token:', token);
    // Store token in form or handle as needed
    if (window.currentCaptchaCallback) {
        window.currentCaptchaCallback(token);
    }
};

window.onCaptchaExpired = function() {
    console.log('CAPTCHA expired');
    // Handle expired CAPTCHA
    if (window.currentCaptchaExpiredCallback) {
        window.currentCaptchaExpiredCallback();
    }
};

window.onCaptchaError = function() {
    console.log('CAPTCHA error');
    // Handle CAPTCHA error
    if (window.currentCaptchaErrorCallback) {
        window.currentCaptchaErrorCallback();
    }
};

// Export for use in other scripts
window.CaptchaService = CaptchaService; 