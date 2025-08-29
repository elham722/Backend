// Form Switch Animations
class FormSwitchAnimations {
    constructor() {
        this.currentForm = 'login';
        this.isAnimating = false;
        this.initializeFormSwitch();
    }

    initializeFormSwitch() {
        document.addEventListener('DOMContentLoaded', () => {
            this.setupFormContainer();
            this.setupSwitchButtons();
            this.showForm('login'); // Show login form by default
        });
    }

    setupFormContainer() {
        const container = document.querySelector('.auth-container .container .row .col-md-6');
        if (container) {
            container.innerHTML = `
                <div class="auth-forms-container">
                    <button class="form-switch-btn" id="switchToRegister">
                        <i class="fas fa-user-plus"></i>
                        ثبت نام ندارید؟ اینجا ثبت‌نام کنید
                    </button>
                    
                    <div class="auth-form" id="loginForm">
                        <div class="auth-card">
                            <div class="card-header">
                                <h3>
                                    <i class="fas fa-sign-in-alt auth-icon"></i>
                                    ورود به سیستم
                                </h3>
                                <p class="text-white-50 mb-0">خوش آمدید! لطفاً اطلاعات خود را وارد کنید</p>
                            </div>
                            
                            <div class="card-body">
                                <form id="loginFormElement" asp-action="Login" asp-controller="Auth" method="post">
                                    @Html.AntiForgeryToken()
                                    <div asp-validation-summary="ModelOnly" class="validation-summary-errors"></div>
                                    
                                    <div class="form-group">
                                        <label class="form-label">
                                            <i class="fas fa-user me-1"></i>ایمیل یا نام کاربری
                                        </label>
                                        <div class="input-wrapper">
                                            <i class="fas fa-envelope input-icon"></i>
                                            <input name="EmailOrUsername" 
                                                   class="form-control" 
                                                   placeholder="example@email.com یا نام کاربری"
                                                   data-val="true"
                                                   data-val-required="این فیلد الزامی است" />
                                        </div>
                                        <span class="field-validation-error"></span>
                                    </div>

                                    <div class="form-group">
                                        <label class="form-label">
                                            <i class="fas fa-lock me-1"></i>رمز عبور
                                        </label>
                                        <div class="input-wrapper">
                                            <i class="fas fa-lock input-icon"></i>
                                            <input name="Password" 
                                                   type="password"
                                                   class="form-control" 
                                                   placeholder="رمز عبور خود را وارد کنید"
                                                   data-val="true"
                                                   data-val-required="این فیلد الزامی است" />
                                        </div>
                                        <span class="field-validation-error"></span>
                                    </div>

                                    <div class="form-check">
                                        <input name="RememberMe" type="checkbox" class="form-check-input" id="rememberMe" />
                                        <label class="form-check-label" for="rememberMe">
                                            <i class="fas fa-clock me-1"></i>مرا به خاطر بسپار
                                        </label>
                                    </div>

                                    <div class="d-grid gap-2 mt-4">
                                        <button type="submit" class="btn btn-auth btn-login">
                                            <span class="btn-text">
                                                <i class="fas fa-sign-in-alt me-2"></i>ورود
                                            </span>
                                            <div class="spinner"></div>
                                        </button>
                                    </div>
                                </form>
                            </div>
                        </div>
                    </div>
                    
                    <div class="auth-form" id="registerForm">
                        <div class="auth-card">
                            <div class="card-header">
                                <h3>
                                    <i class="fas fa-user-plus auth-icon"></i>
                                    ثبت نام
                                </h3>
                                <p class="text-white-50 mb-0">حساب کاربری جدید ایجاد کنید</p>
                            </div>
                            
                            <div class="card-body">
                                <form id="registerFormElement" asp-action="Register" asp-controller="Auth" method="post">
                                    @Html.AntiForgeryToken()
                                    <div asp-validation-summary="ModelOnly" class="validation-summary-errors"></div>
                                    
                                    <div class="form-group">
                                        <label class="form-label">
                                            <i class="fas fa-envelope me-1"></i>ایمیل
                                        </label>
                                        <div class="input-wrapper">
                                            <i class="fas fa-envelope input-icon"></i>
                                            <input name="Email" 
                                                   class="form-control" 
                                                   placeholder="example@email.com"
                                                   data-val="true"
                                                   data-val-required="این فیلد الزامی است"
                                                   data-val-email="لطفاً یک ایمیل معتبر وارد کنید" />
                                        </div>
                                        <span class="field-validation-error"></span>
                                    </div>

                                    <div class="form-group">
                                        <label class="form-label">
                                            <i class="fas fa-user me-1"></i>نام کاربری
                                        </label>
                                        <div class="input-wrapper">
                                            <i class="fas fa-user input-icon"></i>
                                            <input name="UserName" 
                                                   class="form-control" 
                                                   placeholder="نام کاربری دلخواه"
                                                   data-val="true"
                                                   data-val-required="این فیلد الزامی است"
                                                   data-val-minlength="حداقل 3 کاراکتر"
                                                   data-val-minlength-min="3" />
                                        </div>
                                        <span class="field-validation-error"></span>
                                    </div>

                                    <div class="form-group">
                                        <label class="form-label">
                                            <i class="fas fa-lock me-1"></i>رمز عبور
                                        </label>
                                        <div class="input-wrapper">
                                            <i class="fas fa-lock input-icon"></i>
                                            <input name="Password" 
                                                   type="password"
                                                   class="form-control" 
                                                   placeholder="حداقل 8 کاراکتر"
                                                   data-val="true"
                                                   data-val-required="این فیلد الزامی است"
                                                   data-val-minlength="حداقل 8 کاراکتر"
                                                   data-val-minlength-min="8" />
                                        </div>
                                        <span class="field-validation-error"></span>
                                    </div>

                                    <div class="form-group">
                                        <label class="form-label">
                                            <i class="fas fa-lock me-1"></i>تکرار رمز عبور
                                        </label>
                                        <div class="input-wrapper">
                                            <i class="fas fa-lock input-icon"></i>
                                            <input name="ConfirmPassword" 
                                                   type="password"
                                                   class="form-control" 
                                                   placeholder="تکرار رمز عبور"
                                                   data-val="true"
                                                   data-val-required="این فیلد الزامی است" />
                                        </div>
                                        <span class="field-validation-error"></span>
                                    </div>

                                    <div class="form-group">
                                        <label class="form-label">
                                            <i class="fas fa-phone me-1"></i>شماره تلفن
                                        </label>
                                        <div class="input-wrapper">
                                            <i class="fas fa-phone input-icon"></i>
                                            <input name="PhoneNumber" 
                                                   class="form-control" 
                                                   placeholder="09123456789"
                                                   data-val="true"
                                                   data-val-required="این فیلد الزامی است" />
                                        </div>
                                        <span class="field-validation-error"></span>
                                    </div>

                                    <div class="form-check">
                                        <input name="AcceptTerms" type="checkbox" class="form-check-input" id="acceptTerms" />
                                        <label class="form-check-label" for="acceptTerms">
                                            <i class="fas fa-check-circle me-1"></i>
                                            <span>قوانین و شرایط استفاده را می‌پذیرم</span>
                                        </label>
                                        <span class="field-validation-error"></span>
                                    </div>

                                    <div class="form-check">
                                        <input name="SubscribeToNewsletter" type="checkbox" class="form-check-input" id="subscribeNewsletter" />
                                        <label class="form-check-label" for="subscribeNewsletter">
                                            <i class="fas fa-bell me-1"></i>
                                            <span>عضویت در خبرنامه</span>
                                        </label>
                                    </div>

                                    <div class="d-grid gap-2 mt-4">
                                        <button type="submit" class="btn btn-auth btn-register">
                                            <span class="btn-text">
                                                <i class="fas fa-user-plus me-2"></i>ثبت نام
                                            </span>
                                            <div class="spinner"></div>
                                        </button>
                                    </div>
                                </form>
                            </div>
                        </div>
                    </div>
                </div>
            `;
        }
    }

    setupSwitchButtons() {
        const switchBtn = document.getElementById('switchToRegister');
        if (switchBtn) {
            switchBtn.addEventListener('click', (e) => {
                e.preventDefault();
                this.switchForm();
            });
        }
    }

    switchForm() {
        if (this.isAnimating) return;
        
        this.isAnimating = true;
        const newForm = this.currentForm === 'login' ? 'register' : 'login';
        
        // Update button text
        const switchBtn = document.getElementById('switchToRegister');
        if (switchBtn) {
            if (newForm === 'register') {
                switchBtn.innerHTML = '<i class="fas fa-sign-in-alt"></i>قبلاً حساب دارید؟ اینجا وارد شوید';
            } else {
                switchBtn.innerHTML = '<i class="fas fa-user-plus"></i>ثبت نام ندارید؟ اینجا ثبت‌نام کنید';
            }
        }
        
        // Animate form switch
        this.animateFormSwitch(newForm);
    }

    animateFormSwitch(newForm) {
        const currentFormElement = document.getElementById(this.currentForm + 'Form');
        const newFormElement = document.getElementById(newForm + 'Form');
        
        if (!currentFormElement || !newFormElement) return;
        
        // Add slide-out animation to current form
        currentFormElement.classList.add('slide-out');
        
        setTimeout(() => {
            // Hide current form
            this.hideForm(this.currentForm);
            
            // Show new form
            this.showForm(newForm);
            
            // Update current form
            this.currentForm = newForm;
            
            // Remove animation classes
            currentFormElement.classList.remove('slide-out');
            newFormElement.classList.remove('slide-out');
            
            this.isAnimating = false;
        }, 300);
    }

    showForm(formType) {
        const formElement = document.getElementById(formType + 'Form');
        if (formElement) {
            formElement.classList.add('active');
        }
    }

    hideForm(formType) {
        const formElement = document.getElementById(formType + 'Form');
        if (formElement) {
            formElement.classList.remove('active');
        }
    }

    // Alternative animation methods
    flipAnimation(newForm) {
        const container = document.querySelector('.auth-forms-container');
        if (!container) return;
        
        container.style.transform = 'rotateY(180deg)';
        
        setTimeout(() => {
            this.currentForm = newForm;
            container.style.transform = 'rotateY(0deg)';
        }, 300);
    }

    slideAnimation(newForm) {
        const currentForm = document.getElementById(this.currentForm + 'Form');
        const newFormElement = document.getElementById(newForm + 'Form');
        
        if (!currentForm || !newFormElement) return;
        
        // Slide current form out
        currentForm.style.transform = 'translateX(-100%)';
        currentForm.style.opacity = '0';
        
        setTimeout(() => {
            this.hideForm(this.currentForm);
            this.showForm(newForm);
            
            // Slide new form in
            newFormElement.style.transform = 'translateX(100%)';
            newFormElement.style.opacity = '0';
            
            setTimeout(() => {
                newFormElement.style.transform = 'translateX(0)';
                newFormElement.style.opacity = '1';
                this.currentForm = newForm;
            }, 50);
        }, 300);
    }

    // Fade animation
    fadeAnimation(newForm) {
        const currentForm = document.getElementById(this.currentForm + 'Form');
        const newFormElement = document.getElementById(newForm + 'Form');
        
        if (!currentForm || !newFormElement) return;
        
        // Fade out current form
        currentForm.style.opacity = '0';
        currentForm.style.transform = 'scale(0.95)';
        
        setTimeout(() => {
            this.hideForm(this.currentForm);
            this.showForm(newForm);
            
            // Fade in new form
            newFormElement.style.opacity = '0';
            newFormElement.style.transform = 'scale(1.05)';
            
            setTimeout(() => {
                newFormElement.style.opacity = '1';
                newFormElement.style.transform = 'scale(1)';
                this.currentForm = newForm;
            }, 50);
        }, 300);
    }
}

// Initialize form switch animations
document.addEventListener('DOMContentLoaded', () => {
    new FormSwitchAnimations();
});

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = FormSwitchAnimations;
} 