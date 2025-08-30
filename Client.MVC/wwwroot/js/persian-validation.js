// Persian Validation Configuration for jQuery Validation
$(document).ready(function() {
    // Configure jQuery validation with Persian messages
    $.extend($.validator.messages, {
        required: "این فیلد الزامی است",
        email: "لطفاً یک ایمیل معتبر وارد کنید",
        url: "لطفاً یک آدرس معتبر وارد کنید",
        date: "لطفاً یک تاریخ معتبر وارد کنید",
        dateISO: "لطفاً یک تاریخ معتبر وارد کنید (ISO)",
        number: "لطفاً یک عدد معتبر وارد کنید",
        digits: "لطفاً فقط عدد وارد کنید",
        creditcard: "لطفاً یک شماره کارت اعتباری معتبر وارد کنید",
        equalTo: "لطفاً همان مقدار را دوباره وارد کنید",
        accept: "لطفاً یک فایل با پسوند معتبر وارد کنید",
        maxlength: $.validator.format("لطفاً بیش از {0} کاراکتر وارد نکنید"),
        minlength: $.validator.format("لطفاً حداقل {0} کاراکتر وارد کنید"),
        rangelength: $.validator.format("لطفاً مقداری بین {0} تا {1} کاراکتر وارد کنید"),
        range: $.validator.format("لطفاً مقداری بین {0} تا {1} وارد کنید"),
        max: $.validator.format("لطفاً مقداری کمتر یا مساوی {0} وارد کنید"),
        min: $.validator.format("لطفاً مقداری بیشتر یا مساوی {0} وارد کنید"),
        step: $.validator.format("لطفاً مقداری مضرب {0} وارد کنید")
    });

    // Add custom validation methods
    $.validator.addMethod("persianPhone", function(value, element) {
        if (value === "") return true; // Allow empty
        return /^(\+98|0)?9\d{9}$/.test(value);
    }, "لطفاً یک شماره تلفن معتبر وارد کنید");

    $.validator.addMethod("persianUsername", function(value, element) {
        return /^[a-zA-Z0-9_-]+$/.test(value);
    }, "نام کاربری فقط می‌تواند شامل حروف، اعداد، خط تیره و زیرخط باشد");

    // Strong password rule - Enhanced security
    $.validator.addMethod("strongPassword", function(value, element) {
        if (!value || value.length < 12) return false;
        
        // Check for different character types
        var hasLower = /[a-z]/.test(value);
        var hasUpper = /[A-Z]/.test(value);
        var hasNumber = /\d/.test(value);
        var hasSpecial = /[@$!%*?&]/.test(value);
        
        // Count unique character types
        var uniqueTypes = 0;
        if (hasLower) uniqueTypes++;
        if (hasUpper) uniqueTypes++;
        if (hasNumber) uniqueTypes++;
        if (hasSpecial) uniqueTypes++;
        
        return uniqueTypes >= 4;
    }, "رمز عبور باید حداقل 12 کاراکتر باشد و شامل حداقل 4 نوع کاراکتر مختلف (حروف بزرگ، حروف کوچک، اعداد و کاراکترهای خاص) باشد");

    // Configure validation for login form (ساده‌تر)
    $("#loginForm").validate({
        errorClass: "is-invalid",
        validClass: "is-valid",
        errorElement: "div",
        errorPlacement: function(error, element) {
            error.addClass("invalid-feedback");
            element.closest('.form-group').append(error);
        },
        highlight: function(element, errorClass, validClass) {
            $(element).addClass(errorClass).removeClass(validClass);
            $(element).closest('.form-group').addClass('has-error');
        },
        unhighlight: function(element, errorClass, validClass) {
            $(element).removeClass(errorClass).addClass(validClass);
            $(element).closest('.form-group').removeClass('has-error');
        },
        rules: {
            EmailOrUsername: {
                required: true,
                minlength: 3
            },
            Password: {
                required: true,
                minlength: 1  // فقط خالی نباشه
            }
        },
        submitHandler: function(form) {
            console.log("Login form submitting...");
            
            // Show loading state
            const submitBtn = $(form).find('button[type="submit"]');
            const originalText = submitBtn.html();
            
            submitBtn.prop('disabled', true);
            submitBtn.html('<i class="fas fa-spinner fa-spin me-2"></i>در حال پردازش...');
            
            // Submit the form
            form.submit();
            
            // Reset button after a delay (in case of validation errors)
            setTimeout(() => {
                submitBtn.prop('disabled', false);
                submitBtn.html(originalText);
            }, 3000);
            
            return true; // Allow form submission
        }
    });

    // Configure validation for register form (کامل با strongPassword)
    $("#registerForm").validate({
        errorClass: "is-invalid",
        validClass: "is-valid",
        errorElement: "div",
        errorPlacement: function(error, element) {
            error.addClass("invalid-feedback");
            element.closest('.form-group').append(error);
        },
        highlight: function(element, errorClass, validClass) {
            $(element).addClass(errorClass).removeClass(validClass);
            $(element).closest('.form-group').addClass('has-error');
        },
        unhighlight: function(element, errorClass, validClass) {
            $(element).removeClass(errorClass).addClass(validClass);
            $(element).closest('.form-group').removeClass('has-error');
        },
        rules: {
            Email: {
                required: true,
                email: true
            },
            UserName: {
                required: true,
                minlength: 3,
                maxlength: 50,
                persianUsername: true
            },
            Password: {
                required: true,
                minlength: 12, // ✅ Updated to match server-side
                strongPassword: true  // ✅ Enhanced password validation
            },
            ConfirmPassword: {
                required: true,
                equalTo: "#Password"
            },
            PhoneNumber: {
                persianPhone: true
            },
            AcceptTerms: {
                required: true
            }
        },
        submitHandler: function(form) {
            console.log("Register form submitting...");
            
            // Show loading state
            const submitBtn = $(form).find('button[type="submit"]');
            const originalText = submitBtn.html();
            
            submitBtn.prop('disabled', true);
            submitBtn.html('<i class="fas fa-spinner fa-spin me-2"></i>در حال پردازش...');
            
            // Submit the form
            form.submit();
            
            // Reset button after a delay (in case of validation errors)
            setTimeout(() => {
                submitBtn.prop('disabled', false);
                submitBtn.html(originalText);
            }, 3000);
            
            return true; // Allow form submission
        }
    });

    // Real-time validation feedback
    $("input, select, textarea").on("blur", function() {
        $(this).valid();
    });

    // Clear validation errors when user starts typing
    $("input, select, textarea").on("input", function() {
        if ($(this).valid()) {
            $(this).removeClass("is-invalid").addClass("is-valid");
            $(this).closest('.form-group').removeClass('has-error');
        }
    });

    // Debug: Log form validation status
    $("form").on("submit", function(e) {
        console.log("Form submit event triggered");
        console.log("Form valid:", $(this).valid());
        console.log("Form data:", $(this).serialize());
    });
}); 