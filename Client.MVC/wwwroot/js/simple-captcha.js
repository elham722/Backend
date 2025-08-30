/**
 * Simple CAPTCHA functionality for Client.MVC
 * Generates mathematical challenges
 */

class SimpleCaptchaService {
    constructor() {
        this.isEnabled = false;
        this.currentChallenge = null;
        this.init();
    }

    /**
     * Initialize CAPTCHA service
     */
    async init() {
        try {
            // Check if CAPTCHA is required
            const isRequired = await this.isRequired('register');
            this.isEnabled = isRequired;
            
            if (this.isEnabled) {
                console.log('Simple CAPTCHA is enabled');
                // Generate initial challenge
                await this.generateChallenge();
            } else {
                console.log('Simple CAPTCHA is not required');
            }
        } catch (error) {
            console.error('Error initializing Simple CAPTCHA:', error);
            this.isEnabled = false;
        }
    }

    /**
     * Generate a new CAPTCHA challenge
     */
    async generateChallenge() {
        try {
            const response = await fetch('/api/captcha/generate');
            if (!response.ok) {
                throw new Error('Failed to generate CAPTCHA challenge');
            }

            this.currentChallenge = await response.json();
            console.log('Generated CAPTCHA challenge:', this.currentChallenge);
            
            // Update UI
            this.updateUI();
            
            return this.currentChallenge;
        } catch (error) {
            console.error('Error generating CAPTCHA challenge:', error);
            return null;
        }
    }

    /**
     * Validate CAPTCHA answer
     */
    async validateAnswer(answer) {
        if (!this.currentChallenge) {
            throw new Error('No CAPTCHA challenge available');
        }

        try {
            const response = await fetch('/api/captcha/validate', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    challengeId: this.currentChallenge.id,
                    answer: answer
                })
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
     * Update UI with current challenge
     */
    updateUI() {
        if (!this.currentChallenge) return;

        const questionElement = document.getElementById('captchaQuestion');
        const challengeIdInput = document.getElementById('captchaChallengeId');
        const answerInput = document.getElementById('captchaAnswer');

        if (questionElement) {
            questionElement.textContent = this.currentChallenge.question;
        }

        if (challengeIdInput) {
            challengeIdInput.value = this.currentChallenge.id;
        }

        if (answerInput) {
            answerInput.value = '';
            answerInput.focus();
        }
    }

    /**
     * Refresh CAPTCHA challenge
     */
    async refreshChallenge() {
        await this.generateChallenge();
    }

    /**
     * Get current challenge
     */
    getCurrentChallenge() {
        return this.currentChallenge;
    }

    /**
     * Check if CAPTCHA is ready
     */
    isReady() {
        return this.isEnabled && this.currentChallenge != null;
    }
}

// Global CAPTCHA service instance
let simpleCaptchaService;

// Initialize CAPTCHA when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    simpleCaptchaService = new SimpleCaptchaService();
});

// Export for use in other scripts
window.SimpleCaptchaService = SimpleCaptchaService; 