// Simple Persian Validation - برای تست
$(document).ready(function() {
    console.log("Simple validation loaded");
    
    // فقط پیام‌های فارسی
    $.extend($.validator.messages, {
        required: "این فیلد الزامی است",
        email: "لطفاً یک ایمیل معتبر وارد کنید",
        minlength: $.validator.format("لطفاً حداقل {0} کاراکتر وارد کنید")
    });

    // فرم لاگین - ساده
    $("#loginForm").validate({
        rules: {
            EmailOrUsername: { required: true },
            Password: { required: true }
        },
        submitHandler: function(form) {
            console.log("Login form submitting...");
            form.submit();
            return true;
        }
    });

    // فرم رجیستر - ساده
    $("#registerForm").validate({
        rules: {
            Email: { required: true, email: true },
            UserName: { required: true, minlength: 3 },
            Password: { required: true, minlength: 6 },
            ConfirmPassword: { required: true, equalTo: "#Password" },
            AcceptTerms: { required: true }
        },
        submitHandler: function(form) {
            console.log("Register form submitting...");
            form.submit();
            return true;
        }
    });

    // Debug
    $("form").on("submit", function(e) {
        console.log("Form submit event:", this.id);
        console.log("Form valid:", $(this).valid());
    });
}); 