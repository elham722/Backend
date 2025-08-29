// No Validation - برای تست
$(document).ready(function() {
    console.log("No validation loaded");
    
    // فقط loading state
    $("#loginForm, #registerForm").on("submit", function(e) {
        console.log("Form submitting:", this.id);
        
        const submitBtn = $(this).find('button[type="submit"]');
        const originalText = submitBtn.html();
        
        submitBtn.prop('disabled', true);
        submitBtn.html('<i class="fas fa-spinner fa-spin me-2"></i>در حال پردازش...');
        
        // Reset button after a delay
        setTimeout(() => {
            submitBtn.prop('disabled', false);
            submitBtn.html(originalText);
        }, 3000);
        
        console.log("Form will submit normally");
    });
}); 