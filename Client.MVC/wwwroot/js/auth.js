// Authentication Pages JavaScript
document.addEventListener('DOMContentLoaded', function() {
    const inputs = document.querySelectorAll('.form-control');
    const passwordInput = document.getElementById('password');
    const confirmPasswordInput = document.getElementById('confirmPassword');
    const passwordStrength = document.getElementById('passwordStrength');
    const passwordStrengthText = document.getElementById('passwordStrengthText');
    const form = document.querySelector('form');
    const submitBtn = document.querySelector('.btn-login, .btn-register');
    
    // Input focus effects
    inputs.forEach(input => {
        input.addEventListener('focus', function() {
            this.parentElement.style.transform = 'scale(1.02)';
        });
        
        input.addEventListener('blur', function() {
            this.parentElement.style.transform = 'scale(1)';
        });
    });
    
    // Password strength checker (for register page)
    if (passwordInput && passwordStrength && passwordStrengthText) {
        passwordInput.addEventListener('input', function() {
            const password = this.value;
            let strength = 0;
            let strengthText = '';
            let strengthClass = '';
            
            if (password.length >= 8) strength += 25;
            if (/[a-z]/.test(password)) strength += 25;
            if (/[A-Z]/.test(password)) strength += 25;
            if (/[0-9]/.test(password)) strength += 25;
            
            if (strength <= 25) {
                strengthText = 'Weak';
                strengthClass = 'strength-weak';
            } else if (strength <= 50) {
                strengthText = 'Medium';
                strengthClass = 'strength-medium';
            } else if (strength <= 75) {
                strengthText = 'Good';
                strengthClass = 'strength-medium';
            } else {
                strengthText = 'Strong';
                strengthClass = 'strength-strong';
            }
            
            passwordStrength.style.width = strength + '%';
            passwordStrengthText.textContent = 'Password strength: ' + strengthText;
            passwordStrengthText.className = 'password-strength ' + strengthClass;
        });
    }
    
    // Password confirmation check (for register page)
    if (confirmPasswordInput && passwordInput) {
        confirmPasswordInput.addEventListener('input', function() {
            const password = passwordInput.value;
            const confirmPassword = this.value;
            
            if (confirmPassword && password !== confirmPassword) {
                this.setCustomValidity('Passwords do not match');
            } else {
                this.setCustomValidity('');
            }
        });
    }
    
    // Form submission
    if (form && submitBtn) {
        form.addEventListener('submit', function() {
            const btnText = submitBtn.classList.contains('btn-login') ? 'Signing In...' : 'Creating Account...';
            submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> ' + btnText;
            submitBtn.disabled = true;
        });
    }
    
    // Real-time validation feedback
    inputs.forEach(input => {
        input.addEventListener('blur', function() {
            if (this.checkValidity()) {
                this.style.borderColor = '#28a745';
            } else {
                this.style.borderColor = '#dc3545';
            }
        });
    });
}); 