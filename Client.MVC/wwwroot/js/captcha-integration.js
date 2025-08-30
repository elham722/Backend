/**
 * CAPTCHA Integration for Registration Form
 */

document.addEventListener('DOMContentLoaded', function() {
    const executeCaptchaBtn = document.getElementById('executeCaptcha');
    const captchaTokenInput = document.getElementById('captchaToken');
    const captchaContainer = document.getElementById('captchaContainer');
    const registerForm = document.getElementById('registerForm');

    if (!executeCaptchaBtn || !captchaTokenInput || !captchaContainer || !registerForm) {
        console.error('CAPTCHA elements not found');
        return;
    }

    let captchaExecuted = false;

    // Execute CAPTCHA when button is clicked
    executeCaptchaBtn.addEventListener('click', async function() {
        if (captchaExecuted) {
            return; // Already executed
        }

        try {
            // Show loading state
            setCaptchaLoading(true);

            // Execute CAPTCHA
            const token = await captchaService.execute('register');
            
            if (token) {
                // Store token
                captchaTokenInput.value = token;
                captchaExecuted = true;
                
                // Show success state
                setCaptchaSuccess();
                
                // Enable form submission
                enableFormSubmission();
                
                console.log('CAPTCHA executed successfully');
            } else {
                throw new Error('Failed to get CAPTCHA token');
            }
        } catch (error) {
            console.error('CAPTCHA execution failed:', error);
            setCaptchaError('خطا در اجرای CAPTCHA. لطفاً دوباره تلاش کنید.');
            
            // Reset state
            captchaExecuted = false;
            captchaTokenInput.value = '';
        }
    });

    // Form submission handler
    registerForm.addEventListener('submit', async function(e) {
        if (!captchaExecuted || !captchaTokenInput.value) {
            e.preventDefault();
            
            // Show error message
            setCaptchaError('لطفاً ابتدا تأیید امنیتی را انجام دهید.');
            
            // Scroll to CAPTCHA
            captchaContainer.scrollIntoView({ behavior: 'smooth', block: 'center' });
            
            return false;
        }

        // Form is valid, allow submission
        console.log('Form submission allowed - CAPTCHA validated');
    });

    // Set CAPTCHA loading state
    function setCaptchaLoading(isLoading) {
        if (isLoading) {
            captchaContainer.className = 'captcha-container captcha-loading';
            executeCaptchaBtn.disabled = true;
            executeCaptchaBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>در حال پردازش...';
        } else {
            captchaContainer.className = 'captcha-container';
            executeCaptchaBtn.disabled = false;
            executeCaptchaBtn.innerHTML = '<i class="fas fa-shield-alt me-2"></i>تأیید امنیتی';
        }
    }

    // Set CAPTCHA success state
    function setCaptchaSuccess() {
        captchaContainer.className = 'captcha-container captcha-success';
        executeCaptchaBtn.innerHTML = '<i class="fas fa-check me-2"></i>تأیید شده';
        executeCaptchaBtn.disabled = true;
        
        // Update info text
        const infoElement = captchaContainer.querySelector('.captcha-info');
        if (infoElement) {
            infoElement.innerHTML = '<i class="fas fa-check-circle me-2"></i><span>تأیید امنیتی با موفقیت انجام شد</span>';
        }
    }

    // Set CAPTCHA error state
    function setCaptchaError(message) {
        captchaContainer.className = 'captcha-container captcha-error';
        executeCaptchaBtn.disabled = false;
        executeCaptchaBtn.innerHTML = '<i class="fas fa-shield-alt me-2"></i>تلاش مجدد';
        
        // Update info text
        const infoElement = captchaContainer.querySelector('.captcha-info');
        if (infoElement) {
            infoElement.innerHTML = `<i class="fas fa-exclamation-triangle me-2"></i><span>${message}</span>`;
        }
        
        // Reset state after 5 seconds
        setTimeout(() => {
            if (captchaContainer.className.includes('captcha-error')) {
                resetCaptchaState();
            }
        }, 5000);
    }

    // Reset CAPTCHA state
    function resetCaptchaState() {
        captchaContainer.className = 'captcha-container';
        executeCaptchaBtn.disabled = false;
        executeCaptchaBtn.innerHTML = '<i class="fas fa-shield-alt me-2"></i>تأیید امنیتی';
        
        // Update info text
        const infoElement = captchaContainer.querySelector('.captcha-info');
        if (infoElement) {
            infoElement.innerHTML = '<i class="fas fa-info-circle me-2"></i><span>برای تأیید امنیتی، روی دکمه زیر کلیک کنید</span>';
        }
        
        captchaExecuted = false;
        captchaTokenInput.value = '';
    }

    // Enable form submission
    function enableFormSubmission() {
        // Remove any disabled state from submit button
        const submitBtn = registerForm.querySelector('button[type="submit"]');
        if (submitBtn) {
            submitBtn.disabled = false;
        }
    }

    // Check if CAPTCHA is required
    async function checkCaptchaRequirement() {
        try {
            const isRequired = await captchaService.isRequired('register');
            if (!isRequired) {
                // CAPTCHA not required, hide container
                captchaContainer.style.display = 'none';
                captchaExecuted = true;
                captchaTokenInput.value = 'NOT_REQUIRED';
                enableFormSubmission();
            }
        } catch (error) {
            console.error('Error checking CAPTCHA requirement:', error);
            // Default to showing CAPTCHA on error
        }
    }

    // Initialize CAPTCHA requirement check
    checkCaptchaRequirement();

    // Add form validation for CAPTCHA
    if (window.jQuery && window.jQuery.validator) {
        jQuery.validator.addMethod("captchaRequired", function(value, element) {
            return captchaExecuted && captchaTokenInput.value.length > 0;
        }, "لطفاً تأیید امنیتی را انجام دهید");

        // Add validation rule to form
        if (registerForm.jquery) {
            registerForm.validate({
                rules: {
                    CaptchaToken: {
                        captchaRequired: true
                    }
                }
            });
        }
    }
});

// Global function to reset CAPTCHA (can be called from other scripts)
window.resetCaptcha = function() {
    const captchaContainer = document.getElementById('captchaContainer');
    const captchaTokenInput = document.getElementById('captchaToken');
    const executeCaptchaBtn = document.getElementById('executeCaptcha');
    
    if (captchaContainer && captchaTokenInput && executeCaptchaBtn) {
        captchaContainer.className = 'captcha-container';
        executeCaptchaBtn.disabled = false;
        executeCaptchaBtn.innerHTML = '<i class="fas fa-shield-alt me-2"></i>تأیید امنیتی';
        
        const infoElement = captchaContainer.querySelector('.captcha-info');
        if (infoElement) {
            infoElement.innerHTML = '<i class="fas fa-info-circle me-2"></i><span>برای تأیید امنیتی، روی دکمه زیر کلیک کنید</span>';
        }
        
        captchaTokenInput.value = '';
        window.captchaExecuted = false;
    }
}; 