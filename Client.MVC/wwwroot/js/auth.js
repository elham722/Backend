// Auth Pages JavaScript
document.addEventListener('DOMContentLoaded', function() {
    
    // Password strength indicator
    const passwordInput = document.querySelector('input[type="password"]');
    if (passwordInput) {
        passwordInput.addEventListener('input', function() {
            const password = this.value;
            const strength = calculatePasswordStrength(password);
            updatePasswordStrengthIndicator(strength);
        });
    }

    // Form validation enhancement - Temporarily disabled for debugging
    /*
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function(e) {
            // Only prevent if there are obvious validation errors
            const requiredInputs = this.querySelectorAll('input[required]');
            let hasEmptyRequired = false;
            
            requiredInputs.forEach(input => {
                if (!input.value.trim()) {
                    hasEmptyRequired = true;
                    input.classList.add('is-invalid');
                } else {
                    input.classList.remove('is-invalid');
                }
            });
            
            if (hasEmptyRequired) {
                e.preventDefault();
                showFormErrors(this);
            }
        });
    });
    */

    // Auto-hide alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(alert => {
        setTimeout(() => {
            alert.style.opacity = '0';
            setTimeout(() => alert.remove(), 300);
        }, 5000);
    });

    // Smooth transitions for form elements
    const formControls = document.querySelectorAll('.form-control');
    formControls.forEach(control => {
        control.addEventListener('focus', function() {
            this.parentElement.classList.add('focused');
        });
        
        control.addEventListener('blur', function() {
            if (!this.value) {
                this.parentElement.classList.remove('focused');
            }
        });
    });

    // Loading state for submit buttons - Fixed
    const submitButtons = document.querySelectorAll('button[type="submit"]');
    submitButtons.forEach(button => {
        button.addEventListener('click', function() {
            console.log('Submit button clicked');
            // Don't disable the button immediately, let the form submit first
            setTimeout(() => {
                this.disabled = true;
                this.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>در حال پردازش...';
            }, 100);
        });
    });
});

// Calculate password strength
function calculatePasswordStrength(password) {
    let score = 0;
    
    if (password.length >= 8) score++;
    if (/[a-z]/.test(password)) score++;
    if (/[A-Z]/.test(password)) score++;
    if (/[0-9]/.test(password)) score++;
    if (/[^A-Za-z0-9]/.test(password)) score++;
    
    if (score < 2) return 'weak';
    if (score < 4) return 'medium';
    return 'strong';
}

// Update password strength indicator
function updatePasswordStrengthIndicator(strength) {
    let strengthElement = document.getElementById('password-strength');
    
    if (!strengthElement) {
        const passwordInput = document.querySelector('input[type="password"]');
        if (passwordInput) {
            strengthElement = document.createElement('div');
            strengthElement.id = 'password-strength';
            strengthElement.className = 'password-strength mt-2';
            passwordInput.parentElement.appendChild(strengthElement);
        }
    }
    
    if (strengthElement) {
        const messages = {
            weak: 'ضعیف',
            medium: 'متوسط',
            strong: 'قوی'
        };
        
        const colors = {
            weak: '#dc3545',
            medium: '#ffc107',
            strong: '#28a745'
        };
        
        strengthElement.textContent = `قدرت رمز عبور: ${messages[strength]}`;
        strengthElement.style.color = colors[strength];
    }
}

// Enhanced form validation
function validateForm(form) {
    let isValid = true;
    const inputs = form.querySelectorAll('input[required], select[required], textarea[required]');
    
    inputs.forEach(input => {
        if (!input.value.trim()) {
            isValid = false;
            input.classList.add('is-invalid');
        } else {
            input.classList.remove('is-invalid');
        }
    });
    
    // Email validation
    const emailInput = form.querySelector('input[type="email"]');
    if (emailInput && emailInput.value) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(emailInput.value)) {
            isValid = false;
            emailInput.classList.add('is-invalid');
        }
    }
    
    // Password confirmation validation
    const passwordInput = form.querySelector('input[name="Password"]');
    const confirmPasswordInput = form.querySelector('input[name="ConfirmPassword"]');
    
    if (passwordInput && confirmPasswordInput) {
        if (passwordInput.value !== confirmPasswordInput.value) {
            isValid = false;
            confirmPasswordInput.classList.add('is-invalid');
        }
    }
    
    return isValid;
}

// Show form errors
function showFormErrors(form) {
    const invalidInputs = form.querySelectorAll('.is-invalid');
    
    invalidInputs.forEach(input => {
        const errorMessage = input.getAttribute('data-error') || 'این فیلد الزامی است';
        
        let errorElement = input.parentElement.querySelector('.invalid-feedback');
        if (!errorElement) {
            errorElement = document.createElement('div');
            errorElement.className = 'invalid-feedback';
            input.parentElement.appendChild(errorElement);
        }
        
        errorElement.textContent = errorMessage;
    });
    
    // Scroll to first error
    if (invalidInputs.length > 0) {
        invalidInputs[0].scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
}

// Show success message
function showSuccessMessage(message) {
    const alertDiv = document.createElement('div');
    alertDiv.className = 'alert alert-success alert-dismissible fade show';
    alertDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    const container = document.querySelector('.auth-container .container');
    if (container) {
        container.insertBefore(alertDiv, container.firstChild);
        
        // Auto-hide after 5 seconds
        setTimeout(() => {
            alertDiv.remove();
        }, 5000);
    }
}

// Show error message
function showErrorMessage(message) {
    const alertDiv = document.createElement('div');
    alertDiv.className = 'alert alert-danger alert-dismissible fade show';
    alertDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    const container = document.querySelector('.auth-container .container');
    if (container) {
        container.insertBefore(alertDiv, container.firstChild);
        
        // Auto-hide after 5 seconds
        setTimeout(() => {
            alertDiv.remove();
        }, 5000);
    }
}

// Utility function to format phone numbers
function formatPhoneNumber(input) {
    let value = input.value.replace(/\D/g, '');
    if (value.length > 0) {
        if (value.startsWith('0')) {
            value = value.substring(1);
        }
        if (value.length > 0) {
            value = '0' + value;
        }
    }
    input.value = value;
}

// Add phone number formatting
const phoneInputs = document.querySelectorAll('input[type="tel"], input[name*="Phone"]');
phoneInputs.forEach(input => {
    input.addEventListener('input', function() {
        formatPhoneNumber(this);
    });
}); 