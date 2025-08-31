/**
 * CAPTCHA functionality for Client.MVC
 * Google reCAPTCHA v3 - Automatic and Invisible
 */

class CaptchaService {
    constructor() {
        this.siteKey = '';
        this.isEnabled = false;
        this.isLoaded = false;
        this.currentAction = 'default';
        this.scoreThreshold = 0.5;
    }

    /**
     * Initialize CAPTCHA service
     */
    async init() {
        try {
            // Load configuration from API
            await this.loadConfiguration();
            
            if (this.isEnabled) {
                // Load Google reCAPTCHA script
                await this.loadRecaptchaScript();
                console.log('CAPTCHA service initialized successfully');
            } else {
                console.log('CAPTCHA is disabled');
            }
        } catch (error) {
            console.error('Failed to initialize CAPTCHA service:', error);
        }
    }

    /**
     * Load Google reCAPTCHA script
     */
    loadRecaptchaScript() {
        return new Promise((resolve, reject) => {
            if (window.grecaptcha) {
                this.isLoaded = true;
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
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const config = await response.json();
            this.siteKey = config.siteKey;
            this.isEnabled = config.isEnabled;
            this.scoreThreshold = config.threshold;
            this.currentAction = config.action;

            console.log('CAPTCHA configuration loaded:', config);
        } catch (error) {
            console.error('Failed to load CAPTCHA configuration:', error);
            this.isEnabled = false;
        }
    }

    /**
     * Execute reCAPTCHA for a specific action
     * @param {string} action - The action being performed (e.g., 'register', 'login')
     * @returns {Promise<string>} - The reCAPTCHA token
     */
    async execute(action = 'default') {
        if (!this.isEnabled || !this.isLoaded) {
            throw new Error('CAPTCHA service is not available');
        }

        try {
            this.currentAction = action;
            
            // Execute reCAPTCHA automatically
            const token = await grecaptcha.execute(this.siteKey, { action: action });
            
            if (!token) {
                throw new Error('Failed to get reCAPTCHA token');
            }

            console.log(`reCAPTCHA executed for action: ${action}`);
            return token;
        } catch (error) {
            console.error('reCAPTCHA execution failed:', error);
            throw error;
        }
    }

    /**
     * Validate reCAPTCHA token with backend
     * @param {string} token - The reCAPTCHA token
     * @param {string} action - The action being performed
     * @returns {Promise<Object>} - Validation result
     */
    async validate(token, action = 'default') {
        try {
            const response = await fetch('/api/captcha/validate-google', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    token: token,
                    action: action
                })
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const result = await response.json();
            return result;
        } catch (error) {
            console.error('CAPTCHA validation failed:', error);
            throw error;
        }
    }

    /**
     * Execute and validate reCAPTCHA in one step
     * @param {string} action - The action being performed
     * @returns {Promise<Object>} - Validation result
     */
    async executeAndValidate(action = 'default') {
        try {
            const token = await this.execute(action);
            const result = await this.validate(token, action);
            return result;
        } catch (error) {
            console.error('CAPTCHA execution and validation failed:', error);
            throw error;
        }
    }

    /**
     * Check if CAPTCHA is required for an action
     */
    async isRequired(action = 'default') {
        try {
            const response = await fetch(`/api/captcha/config`);
            if (!response.ok) {
                return false;
            }

            const config = await response.json();
            return config.isEnabled && config.action === action;
        } catch (error) {
            console.error('Error checking CAPTCHA requirement:', error);
            return false;
        }
    }

    /**
     * Reset reCAPTCHA
     */
    reset() {
        if (window.grecaptcha) {
            grecaptcha.reset();
        }
    }
}

// Global CAPTCHA service instance
let captchaService;

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', async () => {
    captchaService = new CaptchaService();
    await captchaService.init();
});

// Export for use in other scripts
window.CaptchaService = CaptchaService; 