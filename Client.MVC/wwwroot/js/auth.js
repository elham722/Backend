// Auth Pages JavaScript
document.addEventListener('DOMContentLoaded', function() {
    
    // Password strength indicator - فقط برای فرم رجیستر
    const registerPasswordInput = document.querySelector('#registerForm input[name="Password"]');
    if (registerPasswordInput) {
        registerPasswordInput.addEventListener('input', function() {
            const password = this.value;
            const strength = calculatePasswordStrength(password);
            updatePasswordStrengthIndicator(strength);
        });
    }

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

    // Phone number formatting
    const phoneInputs = document.querySelectorAll('input[type="tel"], input[name*="Phone"]');
    phoneInputs.forEach(input => {
        input.addEventListener('input', function() {
            formatPhoneNumber(this);
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
        const passwordInput = document.querySelector('#registerForm input[name="Password"]');
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