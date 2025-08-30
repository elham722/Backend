/**
 * Simple CAPTCHA Integration for Registration Form
 */

document.addEventListener('DOMContentLoaded', function() {
    const refreshCaptchaBtn = document.getElementById('refreshCaptcha');
    const validateCaptchaBtn = document.getElementById('validateCaptcha');
    const captchaContainer = document.getElementById('captchaContainer');
    const registerForm = document.getElementById('registerForm');
    const captchaAnswerInput = document.getElementById('captchaAnswer');

    if (!refreshCaptchaBtn || !validateCaptchaBtn || !captchaContainer || !registerForm || !captchaAnswerInput) {
        console.error('CAPTCHA elements not found');
        return;
    }

    let captchaValidated = false;

    // Refresh CAPTCHA challenge
    refreshCaptchaBtn.addEventListener('click', async function() {
        try {
            refreshCaptchaBtn.disabled = true;
            refreshCaptchaBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>در حال بارگذاری...';
            
            await simpleCaptchaService.refreshChallenge();
            
            // Reset validation state
            captchaValidated = false;
            setCaptchaState('default');
            
            console.log('CAPTCHA challenge refreshed');
        } catch (error) {
            console.error('Error refreshing CAPTCHA:', error);
            setCaptchaError('خطا در بارگذاری سوال جدید');
        } finally {
            refreshCaptchaBtn.disabled = false;
            refreshCaptchaBtn.innerHTML = '<i class="fas fa-refresh me-1"></i>سوال جدید';
        }
    });

    // Validate CAPTCHA answer
    validateCaptchaBtn.addEventListener('click', async function() {
        if (!simpleCaptchaService.isReady()) {
            setCaptchaError('CAPTCHA آماده نیست');
            return;
        }

        const answer = captchaAnswerInput.value.trim();
        if (!answer) {
            setCaptchaError('لطفاً پاسخ را وارد کنید');
            captchaAnswerInput.focus();
            return;
        }

        try {
            validateCaptchaBtn.disabled = true;
            validateCaptchaBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>در حال بررسی...';
            
            const result = await simpleCaptchaService.validateAnswer(answer);
            
            if (result.isValid) {
                captchaValidated = true;
                setCaptchaSuccess();
                console.log('CAPTCHA validated successfully');
            } else {
                captchaValidated = false;
                setCaptchaError(result.errorMessage || 'پاسخ اشتباه است');
                captchaAnswerInput.focus();
            }
        } catch (error) {
            console.error('Error validating CAPTCHA:', error);
            setCaptchaError('خطا در بررسی پاسخ');
            captchaValidated = false;
        } finally {
            validateCaptchaBtn.disabled = false;
            validateCaptchaBtn.innerHTML = '<i class="fas fa-check me-1"></i>تأیید';
        }
    });

    // Form submission handler
    registerForm.addEventListener('submit', async function(e) {
        if (!captchaValidated) {
            e.preventDefault();
            
            setCaptchaError('لطفاً ابتدا CAPTCHA را تأیید کنید');
            captchaContainer.scrollIntoView({ behavior: 'smooth', block: 'center' });
            
            return false;
        }

        // Form is valid, allow submission
        console.log('Form submission allowed - CAPTCHA validated');
    });

    // Set CAPTCHA default state
    function setCaptchaState(state) {
        captchaContainer.className = `captcha-container captcha-${state}`;
    }

    // Set CAPTCHA success state
    function setCaptchaSuccess() {
        captchaContainer.className = 'captcha-container captcha-success';
        validateCaptchaBtn.disabled = true;
        validateCaptchaBtn.innerHTML = '<i class="fas fa-check me-1"></i>تأیید شده';
        
        // Update info text
        const infoElement = captchaContainer.querySelector('.captcha-info');
        if (infoElement) {
            infoElement.innerHTML = '<i class="fas fa-check-circle me-2"></i><span>تأیید امنیتی با موفقیت انجام شد</span>';
        }
    }

    // Set CAPTCHA error state
    function setCaptchaError(message) {
        captchaContainer.className = 'captcha-container captcha-error';
        validateCaptchaBtn.disabled = false;
        validateCaptchaBtn.innerHTML = '<i class="fas fa-check me-1"></i>تأیید';
        
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
        validateCaptchaBtn.disabled = false;
        validateCaptchaBtn.innerHTML = '<i class="fas fa-check me-1"></i>تأیید';
        
        // Update info text
        const infoElement = captchaContainer.querySelector('.captcha-info');
        if (infoElement) {
            infoElement.innerHTML = '<i class="fas fa-calculator me-2"></i><span>لطفاً پاسخ سوال زیر را وارد کنید:</span>';
        }
        
        captchaValidated = false;
        captchaAnswerInput.value = '';
    }

    // Add form validation for CAPTCHA
    if (window.jQuery && window.jQuery.validator) {
        jQuery.validator.addMethod("captchaRequired", function(value, element) {
            return captchaValidated;
        }, "لطفاً تأیید امنیتی را انجام دهید");

        // Add validation rule to form
        if (registerForm.jquery) {
            registerForm.validate({
                rules: {
                    CaptchaAnswer: {
                        captchaRequired: true
                    }
                }
            });
        }
    }

    // Initialize CAPTCHA when service is ready
    const checkCaptchaReady = setInterval(() => {
        if (simpleCaptchaService && simpleCaptchaService.isReady()) {
            clearInterval(checkCaptchaReady);
            console.log('CAPTCHA integration ready');
        }
    }, 100);
});

// Global function to reset CAPTCHA (can be called from other scripts)
window.resetSimpleCaptcha = function() {
    const captchaContainer = document.getElementById('captchaContainer');
    const captchaAnswerInput = document.getElementById('captchaAnswer');
    const validateCaptchaBtn = document.getElementById('validateCaptcha');
    
    if (captchaContainer && captchaAnswerInput && validateCaptchaBtn) {
        captchaContainer.className = 'captcha-container';
        validateCaptchaBtn.disabled = false;
        validateCaptchaBtn.innerHTML = '<i class="fas fa-check me-1"></i>تأیید';
        
        const infoElement = captchaContainer.querySelector('.captcha-info');
        if (infoElement) {
            infoElement.innerHTML = '<i class="fas fa-calculator me-2"></i><span>لطفاً پاسخ سوال زیر را وارد کنید:</span>';
        }
        
        captchaAnswerInput.value = '';
        window.captchaValidated = false;
    }
}; 