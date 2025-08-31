// Authentication Enhancements
class AuthEnhancements {
    constructor() {
        this.initializePasswordStrength();
        this.initializeRealTimeValidation();
        this.initializeAutoComplete();
        this.initializeKeyboardNavigation();
        this.initializeAccessibility();
    }

    initializePasswordStrength() {
        // Only show password strength for register form
        const registerForm = document.getElementById('registerForm');
        if (!registerForm) return;
        
        const passwordFields = registerForm.querySelectorAll('input[name*="Password"]:not([name*="Confirm"])');
        
        passwordFields.forEach(field => {
            const strengthIndicator = document.createElement('div');
            strengthIndicator.className = 'password-strength';
            strengthIndicator.innerHTML = '<div class="password-strength-bar"></div>';
            
            field.parentElement.appendChild(strengthIndicator);
            
            field.addEventListener('input', () => {
                this.updatePasswordStrength(field, strengthIndicator);
            });
        });
    }

    updatePasswordStrength(field, indicator) {
        const password = field.value;
        const strength = this.calculatePasswordStrength(password);
        
        indicator.className = `password-strength ${strength.level}`;
        
        // Update confirm password validation
        const confirmField = document.querySelector('input[name*="ConfirmPassword"]');
        if (confirmField) {
            this.validatePasswordConfirmation(password, confirmField.value);
        }
    }

    calculatePasswordStrength(password) {
        let score = 0;
        let feedback = [];

        if (password.length >= 8) score += 1;
        else feedback.push('حداقل 8 کاراکتر');

        if (/[a-z]/.test(password)) score += 1;
        else feedback.push('حروف کوچک');

        if (/[A-Z]/.test(password)) score += 1;
        else feedback.push('حروف بزرگ');

        if (/[0-9]/.test(password)) score += 1;
        else feedback.push('اعداد');

        if (/[^A-Za-z0-9]/.test(password)) score += 1;
        else feedback.push('کاراکترهای خاص');

        let level = 'weak';
        if (score >= 4) level = 'strong';
        else if (score >= 3) level = 'good';
        else if (score >= 2) level = 'fair';

        return { level, score, feedback };
    }

    validatePasswordConfirmation(password, confirmPassword) {
        const confirmField = document.querySelector('input[name*="ConfirmPassword"]');
        if (!confirmField) return;

        if (confirmPassword && password !== confirmPassword) {
            confirmField.classList.add('is-invalid');
            this.showFieldError(confirmField, 'رمزهای عبور مطابقت ندارند');
        } else {
            confirmField.classList.remove('is-invalid');
            this.clearFieldError(confirmField);
        }
    }

    initializeRealTimeValidation() {
        const inputs = document.querySelectorAll('input[data-val="true"]');
        
        inputs.forEach(input => {
            // Real-time email validation
            if (input.name.includes('Email')) {
                input.addEventListener('blur', () => {
                    if (input.value && !this.isValidEmail(input.value)) {
                        this.showFieldError(input, 'لطفاً یک ایمیل معتبر وارد کنید');
                    }
                });
            }

            // Real-time phone validation
            if (input.name.includes('Phone')) {
                input.addEventListener('blur', () => {
                    if (input.value && !this.isValidPhoneNumber(input.value)) {
                        this.showFieldError(input, 'لطفاً شماره تلفن معتبر وارد کنید');
                    }
                });
            }

            // Real-time username validation
            if (input.name.includes('UserName')) {
                input.addEventListener('blur', () => {
                    if (input.value && input.value.length < 3) {
                        this.showFieldError(input, 'نام کاربری باید حداقل 3 کاراکتر باشد');
                    }
                });
            }
        });
    }

    isValidEmail(email) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    }

    isValidPhoneNumber(phone) {
        const phoneRegex = /^09\d{9}$/;
        return phoneRegex.test(phone);
    }

    showFieldError(field, message) {
        let errorElement = field.parentElement.querySelector('.field-validation-error');
        
        if (!errorElement) {
            errorElement = document.createElement('span');
            errorElement.className = 'field-validation-error';
            field.parentElement.appendChild(errorElement);
        }
        
        errorElement.textContent = message;
        field.classList.add('is-invalid');
    }

    clearFieldError(field) {
        const errorElement = field.parentElement.querySelector('.field-validation-error');
        if (errorElement) {
            errorElement.remove();
        }
        field.classList.remove('is-invalid');
    }

    initializeAutoComplete() {
        // Auto-complete for common domains
        const emailField = document.querySelector('input[name*="Email"]');
        if (emailField) {
            const commonDomains = ['gmail.com', 'yahoo.com', 'hotmail.com', 'outlook.com'];
            
            emailField.addEventListener('input', (e) => {
                const value = e.target.value;
                const atIndex = value.indexOf('@');
                
                if (atIndex > 0) {
                    const domain = value.substring(atIndex + 1);
                    const matchingDomain = commonDomains.find(d => d.startsWith(domain));
                    
                    if (matchingDomain && domain.length > 0) {
                        const suggestion = value.substring(0, atIndex + 1) + matchingDomain;
                        
                        // Create suggestion element
                        let suggestionElement = document.getElementById('email-suggestion');
                        if (!suggestionElement) {
                            suggestionElement = document.createElement('div');
                            suggestionElement.id = 'email-suggestion';
                            suggestionElement.className = 'email-suggestion';
                            suggestionElement.style.cssText = `
                                position: absolute;
                                top: 100%;
                                left: 0;
                                right: 0;
                                background: white;
                                border: 1px solid #ddd;
                                border-top: none;
                                border-radius: 0 0 8px 8px;
                                padding: 8px 12px;
                                font-size: 14px;
                                color: #666;
                                cursor: pointer;
                                z-index: 1000;
                            `;
                            emailField.parentElement.style.position = 'relative';
                            emailField.parentElement.appendChild(suggestionElement);
                        }
                        
                        suggestionElement.textContent = `پیشنهاد: ${suggestion}`;
                        suggestionElement.style.display = 'block';
                        
                        suggestionElement.onclick = () => {
                            emailField.value = suggestion;
                            suggestionElement.style.display = 'none';
                        };
                    } else {
                        const suggestionElement = document.getElementById('email-suggestion');
                        if (suggestionElement) {
                            suggestionElement.style.display = 'none';
                        }
                    }
                }
            });
        }
    }

    initializeKeyboardNavigation() {
        // Tab navigation enhancement
        const inputs = document.querySelectorAll('input, button, select, textarea');
        
        inputs.forEach((input, index) => {
            input.addEventListener('keydown', (e) => {
                if (e.key === 'Enter' && e.target.type !== 'textarea') {
                    e.preventDefault();
                    
                    // Find next input or submit button
                    const nextInput = inputs[index + 1];
                    if (nextInput) {
                        nextInput.focus();
                    } else {
                        // Submit form if we're on the last input
                        const form = input.closest('form');
                        if (form) {
                            form.dispatchEvent(new Event('submit'));
                        }
                    }
                }
            });
        });
    }

    initializeAccessibility() {
        // Add ARIA labels and descriptions
        const inputs = document.querySelectorAll('input');
        
        inputs.forEach(input => {
            const label = input.parentElement.querySelector('label');
            if (label) {
                input.setAttribute('aria-labelledby', label.getAttribute('for') || label.id);
            }
            
            // Add error description
            input.addEventListener('invalid', () => {
                const errorElement = input.parentElement.querySelector('.field-validation-error');
                if (errorElement) {
                    input.setAttribute('aria-describedby', errorElement.id);
                }
            });
        });

        // Focus management
        const forms = document.querySelectorAll('form');
        forms.forEach(form => {
            form.addEventListener('submit', () => {
                // Store focus for return
                const activeElement = document.activeElement;
                sessionStorage.setItem('lastFocusedElement', activeElement.id);
            });
        });

        // Restore focus on page load
        window.addEventListener('load', () => {
            const lastFocusedId = sessionStorage.getItem('lastFocusedElement');
            if (lastFocusedId) {
                const element = document.getElementById(lastFocusedId);
                if (element) {
                    element.focus();
                }
                sessionStorage.removeItem('lastFocusedElement');
            }
        });
    }

    // Utility methods
    showLoadingOverlay() {
        let overlay = document.querySelector('.loading-overlay');
        if (!overlay) {
            overlay = document.createElement('div');
            overlay.className = 'loading-overlay';
            overlay.innerHTML = '<div class="loading-spinner"></div>';
            document.body.appendChild(overlay);
        }
        overlay.classList.add('active');
    }

    hideLoadingOverlay() {
        const overlay = document.querySelector('.loading-overlay');
        if (overlay) {
            overlay.classList.remove('active');
        }
    }

    showSuccessAnimation() {
        const successElement = document.createElement('div');
        successElement.className = 'success-checkmark';
        successElement.innerHTML = `
            <svg class="success-checkmark__circle" cx="26" cy="26" r="25" fill="none"/>
            <svg class="success-checkmark__check" fill="none" d="M14.1 27.2l7.1 7.2 16.7-16.8"/>
        `;
        
        document.body.appendChild(successElement);
        
        setTimeout(() => {
            successElement.remove();
        }, 2000);
    }
}

// Initialize enhancements when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new AuthEnhancements();
});

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = AuthEnhancements;
} 