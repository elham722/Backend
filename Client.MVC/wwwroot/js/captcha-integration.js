/**
 * CAPTCHA Integration for Registration Form
 * Google reCAPTCHA v3 - Automatic and Invisible
 */

document.addEventListener('DOMContentLoaded', function() {
    const captchaTokenInput = document.getElementById('captchaToken');
    const registerForm = document.getElementById('registerForm');

    if (!captchaTokenInput || !registerForm) {
        console.error('CAPTCHA elements not found');
        return;
    }

    // Form submission handler
    registerForm.addEventListener('submit', async function(e) {
        e.preventDefault();
        
        try {
            // Execute reCAPTCHA automatically when form is submitted
            const token = await captchaService.execute('register');
            
            if (token) {
                // Store token
                captchaTokenInput.value = token;
                
                // Submit form with token
                await submitFormWithCaptcha();
            } else {
                throw new Error('Failed to get CAPTCHA token');
            }
        } catch (error) {
            console.error('CAPTCHA execution failed:', error);
            showCaptchaError('خطا در تأیید امنیتی. لطفاً دوباره تلاش کنید.');
        }
    });

    // Submit form with CAPTCHA token
    async function submitFormWithCaptcha() {
        try {
            // Get form data
            const formData = new FormData(registerForm);
            
            // Add CAPTCHA token
            formData.append('CaptchaToken', captchaTokenInput.value);
            
            // Submit to backend
            const response = await fetch('/api/auth/register', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(Object.fromEntries(formData))
            });

            if (response.ok) {
                const result = await response.json();
                showFormSuccess('ثبت‌نام با موفقیت انجام شد!');
                console.log('Registration successful:', result);
            } else {
                const error = await response.json();
                showFormError(`خطا در ثبت‌نام: ${error.detail || 'خطای نامشخص'}`);
            }
        } catch (error) {
            console.error('Form submission error:', error);
            showFormError('خطا در ارسال فرم. لطفاً دوباره تلاش کنید.');
        }
    }

    // Helper functions for UI state management
    function showCaptchaError(message) {
        const errorDiv = document.createElement('div');
        errorDiv.className = 'alert alert-danger mt-2';
        errorDiv.textContent = message;
        
        // Remove existing error messages
        const existingErrors = document.querySelectorAll('.alert-danger');
        existingErrors.forEach(err => err.remove());
        
        registerForm.appendChild(errorDiv);
        
        // Remove error after 5 seconds
        setTimeout(() => errorDiv.remove(), 5000);
    }

    function showFormSuccess(message) {
        const successDiv = document.createElement('div');
        successDiv.className = 'alert alert-success mt-2';
        successDiv.textContent = message;
        
        // Remove existing messages
        const existingMessages = document.querySelectorAll('.alert');
        existingMessages.forEach(msg => msg.remove());
        
        registerForm.appendChild(successDiv);
        
        // Reset form after 3 seconds
        setTimeout(() => {
            registerForm.reset();
            captchaTokenInput.value = '';
            successDiv.remove();
        }, 3000);
    }

    function showFormError(message) {
        const errorDiv = document.createElement('div');
        errorDiv.className = 'alert alert-danger mt-2';
        errorDiv.textContent = message;
        
        // Remove existing messages
        const existingMessages = document.querySelectorAll('.alert');
        existingMessages.forEach(msg => msg.remove());
        
        registerForm.appendChild(errorDiv);
        
        // Remove error after 5 seconds
        setTimeout(() => errorDiv.remove(), 5000);
    }
}); 