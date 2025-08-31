// Modern Authentication JavaScript
class ModernAuth {
    constructor() {
        this.initializeComponents();
        this.bindEvents();
        this.setupAnimations();
    }

    initializeComponents() {
        // Initialize password toggles
        this.initializePasswordToggles();
        
        // Initialize form validation
        this.initializeValidation();
        
        // Initialize loading states
        this.initializeLoadingStates();
        
        // Initialize SweetAlert2
        this.initializeSweetAlert();
    }

    bindEvents() {
        // Form submission events
        document.addEventListener('DOMContentLoaded', () => {
            const loginForm = document.getElementById('loginForm');
            const registerForm = document.getElementById('registerForm');

            if (loginForm) {
                loginForm.addEventListener('submit', (e) => this.handleFormSubmit(e, 'login'));
            }

            if (registerForm) {
                registerForm.addEventListener('submit', (e) => this.handleFormSubmit(e, 'register'));
            }

            // Input focus effects
            this.setupInputEffects();
            
            // Form switching animation
            this.setupFormSwitching();
        });
    }

    initializePasswordToggles() {
        const passwordFields = document.querySelectorAll('input[type="password"]');
        
        passwordFields.forEach(field => {
            const wrapper = field.parentElement;
            if (!wrapper.classList.contains('input-wrapper')) {
                const newWrapper = document.createElement('div');
                newWrapper.className = 'input-wrapper';
                field.parentNode.insertBefore(newWrapper, field);
                newWrapper.appendChild(field);
            }

            const toggleBtn = document.createElement('button');
            toggleBtn.type = 'button';
            toggleBtn.className = 'password-toggle';
            toggleBtn.innerHTML = '<i class="fas fa-eye"></i>';
            toggleBtn.setAttribute('aria-label', 'نمایش/مخفی کردن رمز عبور');

            toggleBtn.addEventListener('click', () => {
                const type = field.type === 'password' ? 'text' : 'password';
                field.type = type;
                toggleBtn.innerHTML = type === 'password' ? 
                    '<i class="fas fa-eye"></i>' : 
                    '<i class="fas fa-eye-slash"></i>';
            });

            field.parentElement.appendChild(toggleBtn);
        });
    }

    initializeValidation() {
        // Custom validation messages
        const validationMessages = {
            required: 'این فیلد الزامی است',
            email: 'لطفاً یک ایمیل معتبر وارد کنید',
            minLength: 'حداقل {0} کاراکتر باید وارد کنید',
            maxLength: 'حداکثر {0} کاراکتر مجاز است',
            passwordMatch: 'رمزهای عبور مطابقت ندارند',
            phoneNumber: 'لطفاً شماره تلفن معتبر وارد کنید'
        };

        // Add custom validation attributes to all required fields
        const requiredInputs = document.querySelectorAll('input[required], input[data-val="true"]');
        requiredInputs.forEach(input => {
            input.addEventListener('blur', () => this.validateField(input));
            input.addEventListener('input', () => this.clearFieldError(input));
        });
    }

    validateField(field) {
        const value = field.value.trim();
        const fieldName = field.name;
        let isValid = true;
        let errorMessage = '';

        // Required validation
        if (field.hasAttribute('data-val-required') && !value) {
            isValid = false;
            errorMessage = 'این فیلد الزامی است';
        }

        // Email validation
        if (fieldName.includes('Email') && value && !this.isValidEmail(value)) {
            isValid = false;
            errorMessage = 'لطفاً یک ایمیل معتبر وارد کنید';
        }

        // Password validation
        if (fieldName.includes('Password') && value && !this.isValidPassword(value)) {
            isValid = false;
            errorMessage = 'رمز عبور باید حداقل 8 کاراکتر و شامل حروف بزرگ، حروف کوچک، اعداد و کاراکترهای خاص باشد';
        }

        // Phone number validation
        if (fieldName.includes('Phone') && value && !this.isValidPhoneNumber(value)) {
            isValid = false;
            errorMessage = 'لطفاً شماره تلفن معتبر وارد کنید';
        }

        // Password confirmation validation
        if (fieldName.includes('ConfirmPassword')) {
            const passwordField = document.querySelector('input[name*="Password"]:not([name*="Confirm"])');
            if (passwordField && value !== passwordField.value) {
                isValid = false;
                errorMessage = 'رمزهای عبور مطابقت ندارند';
            }
        }

        // CAPTCHA validation - automatic, no need for manual validation
        if (fieldName.includes('CaptchaToken')) {
            // CAPTCHA is handled automatically by Google reCAPTCHA v3
            // No manual validation needed
            return true;
        }

        this.showFieldValidation(field, isValid, errorMessage);
        return isValid;
    }

    isValidEmail(email) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    }

    isValidPassword(password) {
        // Check minimum length
        if (password.length < 8) return false;
        
        // Check for at least one lowercase letter
        if (!/[a-z]/.test(password)) return false;
        
        // Check for at least one uppercase letter
        if (!/[A-Z]/.test(password)) return false;
        
        // Check for at least one number
        if (!/\d/.test(password)) return false;
        
        // Check for at least one special character
        if (!/[@$!%*?&]/.test(password)) return false;
        
        // Check for at least 4 different character types
        const uniqueChars = new Set(password).size;
        if (uniqueChars < 4) return false;
        
        return true;
    }

    isValidPhoneNumber(phone) {
        const phoneRegex = /^09\d{9}$/;
        return phoneRegex.test(phone);
    }

    showFieldValidation(field, isValid, message) {
        const errorElement = field.parentElement.querySelector('.field-validation-error');
        
        if (!isValid) {
            field.classList.add('is-invalid');
            if (!errorElement) {
                const error = document.createElement('span');
                error.className = 'field-validation-error';
                error.textContent = message;
                field.parentElement.appendChild(error);
            } else {
                errorElement.textContent = message;
            }
        } else {
            field.classList.remove('is-invalid');
            if (errorElement) {
                errorElement.remove();
            }
        }
    }

    clearFieldError(field) {
        field.classList.remove('is-invalid');
        const errorElement = field.parentElement.querySelector('.field-validation-error');
        if (errorElement) {
            errorElement.remove();
        }
    }

    initializeLoadingStates() {
        // Add loading spinner to buttons
        const authButtons = document.querySelectorAll('.btn-auth');
        authButtons.forEach(button => {
            const text = button.innerHTML;
            button.innerHTML = `
                <span class="btn-text">${text}</span>
                <div class="spinner"></div>
            `;
        });
    }

    initializeSweetAlert() {
        // Configure SweetAlert2 defaults
        if (typeof Swal !== 'undefined') {
            Swal.mixin({
                toast: true,
                position: 'top-end',
                showConfirmButton: false,
                timer: 3000,
                timerProgressBar: true,
                didOpen: (toast) => {
                    toast.addEventListener('mouseenter', Swal.stopTimer)
                    toast.addEventListener('mouseleave', Swal.resumeTimer)
                }
            });
        }
    }

    setupInputEffects() {
        const inputs = document.querySelectorAll('.form-group input');
        
        inputs.forEach(input => {
            // Add floating label effect
            input.addEventListener('focus', () => {
                input.parentElement.classList.add('focused');
            });

            input.addEventListener('blur', () => {
                if (!input.value) {
                    input.parentElement.classList.remove('focused');
                }
            });

            // Add input animation
            input.addEventListener('input', () => {
                if (input.value) {
                    input.classList.add('has-value');
                } else {
                    input.classList.remove('has-value');
                }
            });
        });
    }

    setupFormSwitching() {
        const authLinks = document.querySelectorAll('.auth-card .card-footer a');
        
        authLinks.forEach(link => {
            link.addEventListener('click', (e) => {
                e.preventDefault();
                const targetUrl = link.getAttribute('href');
                
                // Add exit animation
                const card = document.querySelector('.auth-card');
                card.style.animation = 'slideOutLeft 0.3s ease-in forwards';
                
                setTimeout(() => {
                    window.location.href = targetUrl;
                }, 300);
            });
        });
    }

    setupAnimations() {
        // Add CSS animations
        const style = document.createElement('style');
        style.textContent = `
            @keyframes slideOutLeft {
                to {
                    opacity: 0;
                    transform: translateX(-30px);
                }
            }
            
            .form-group.focused label {
                transform: translateY(-20px) scale(0.85);
                color: var(--primary-color);
            }
            
            .form-group input.has-value + label {
                transform: translateY(-20px) scale(0.85);
            }
            
            .form-group input.is-invalid {
                border-color: var(--error-color);
                animation: shake 0.5s ease-in-out;
            }
        `;
        document.head.appendChild(style);
    }

    async handleFormSubmit(event, formType) {
        event.preventDefault();
        
        const form = event.target;
        const submitButton = form.querySelector('.btn-auth');
        const formData = new FormData(form);
        
        // Validate all required fields
        const requiredInputs = form.querySelectorAll('input[required], input[data-val="true"]');
        let isValid = true;
        
        requiredInputs.forEach(input => {
            if (!this.validateField(input)) {
                isValid = false;
            }
        });

        // CAPTCHA is handled automatically by Google reCAPTCHA v3
        // No manual validation needed

        if (!isValid) {
            this.showError('لطفاً خطاهای فرم را برطرف کنید');
            return;
        }

        // Show loading state
        submitButton.classList.add('loading');
        
        try {
            const response = await fetch(form.action, {
                method: 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            const result = await response.json();

            if (response.ok) {
                if (result.success) {
                    this.showSuccess(result.message || 'عملیات با موفقیت انجام شد');
                    
                    // Redirect after success
                    setTimeout(() => {
                        window.location.href = result.redirectUrl || '/';
                    }, 1500);
                } else {
                    this.showError(result.message || 'خطا در انجام عملیات');
                }
            } else {
                this.showError('خطا در ارتباط با سرور');
            }
        } catch (error) {
            console.error('Form submission error:', error);
            this.showError('خطا در ارتباط با سرور');
        } finally {
            submitButton.classList.remove('loading');
        }
    }

    showSuccess(message) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                icon: 'success',
                title: 'موفقیت',
                text: message,
                toast: true,
                position: 'top-end',
                showConfirmButton: false,
                timer: 3000,
                timerProgressBar: true
            });
        } else {
            // Fallback to alert
            alert(message);
        }
    }

    showError(message) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                icon: 'error',
                title: 'خطا',
                text: message,
                toast: true,
                position: 'top-end',
                showConfirmButton: false,
                timer: 4000,
                timerProgressBar: true
            });
        } else {
            // Fallback to alert
            alert(message);
        }
    }

    showWarning(message) {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                icon: 'warning',
                title: 'هشدار',
                text: message,
                toast: true,
                position: 'top-end',
                showConfirmButton: false,
                timer: 3500,
                timerProgressBar: true
            });
        } else {
            // Fallback to alert
            alert(message);
        }
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new ModernAuth();
});

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ModernAuth;
} 