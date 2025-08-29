/**
 * Example AJAX requests with Anti-Forgery (CSRF) protection
 */

// Example 1: Login with AJAX
function loginWithAjax(loginData) {
    return AntiForgeryHelper.post('/Auth/LoginAjax', loginData, {
        success: function(response) {
            if (response.success) {
                // Handle successful login
                showSuccessMessage('ورود با موفقیت انجام شد');
                setTimeout(() => {
                    window.location.href = '/Home';
                }, 1000);
            } else {
                // Handle login error
                showErrorMessage(response.message || 'خطا در ورود');
            }
        },
        error: function(xhr, status, error) {
            console.error('Login failed:', error);
            showErrorMessage('خطا در ارتباط با سرور');
        }
    });
}

// Example 2: Register with AJAX
function registerWithAjax(registerData) {
    return AntiForgeryHelper.post('/Auth/RegisterAjax', registerData, {
        success: function(response) {
            if (response.success) {
                // Handle successful registration
                showSuccessMessage('ثبت نام با موفقیت انجام شد');
                setTimeout(() => {
                    window.location.href = '/Home';
                }, 1000);
            } else {
                // Handle registration error
                showErrorMessage(response.message || 'خطا در ثبت نام');
            }
        },
        error: function(xhr, status, error) {
            console.error('Registration failed:', error);
            showErrorMessage('خطا در ارتباط با سرور');
        }
    });
}

// Example 3: Change password with AJAX
function changePasswordWithAjax(passwordData) {
    return AntiForgeryHelper.put('/Auth/ChangePasswordAjax', passwordData, {
        success: function(response) {
            if (response.success) {
                showSuccessMessage('رمز عبور با موفقیت تغییر یافت');
            } else {
                showErrorMessage(response.message || 'خطا در تغییر رمز عبور');
            }
        },
        error: function(xhr, status, error) {
            console.error('Change password failed:', error);
            showErrorMessage('خطا در ارتباط با سرور');
        }
    });
}

// Example 4: Logout with AJAX
function logoutWithAjax() {
    return AntiForgeryHelper.post('/Auth/LogoutAjax', {}, {
        success: function(response) {
            if (response.success) {
                showSuccessMessage('خروج با موفقیت انجام شد');
                setTimeout(() => {
                    window.location.href = '/Auth/Login';
                }, 1000);
            } else {
                showErrorMessage(response.message || 'خطا در خروج');
            }
        },
        error: function(xhr, status, error) {
            console.error('Logout failed:', error);
            showErrorMessage('خطا در ارتباط با سرور');
        }
    });
}

// Example 5: Update profile with AJAX
function updateProfileWithAjax(profileData) {
    return AntiForgeryHelper.put('/Auth/UpdateProfileAjax', profileData, {
        success: function(response) {
            if (response.success) {
                showSuccessMessage('پروفایل با موفقیت به‌روزرسانی شد');
            } else {
                showErrorMessage(response.message || 'خطا در به‌روزرسانی پروفایل');
            }
        },
        error: function(xhr, status, error) {
            console.error('Update profile failed:', error);
            showErrorMessage('خطا در ارتباط با سرور');
        }
    });
}

// Example 6: Delete account with AJAX
function deleteAccountWithAjax() {
    if (confirm('آیا مطمئن هستید که می‌خواهید حساب کاربری خود را حذف کنید؟')) {
        return AntiForgeryHelper.delete('/Auth/DeleteAccountAjax', {
            success: function(response) {
                if (response.success) {
                    showSuccessMessage('حساب کاربری با موفقیت حذف شد');
                    setTimeout(() => {
                        window.location.href = '/Auth/Register';
                    }, 1000);
                } else {
                    showErrorMessage(response.message || 'خطا در حذف حساب کاربری');
                }
            },
            error: function(xhr, status, error) {
                console.error('Delete account failed:', error);
                showErrorMessage('خطا در ارتباط با سرور');
            }
        });
    }
}

// Example 7: Refresh Anti-Forgery token
function refreshAntiForgeryToken() {
    return AntiForgeryHelper.refreshToken()
        .done(function() {
            console.log('Anti-Forgery token refreshed successfully');
        })
        .fail(function() {
            console.warn('Failed to refresh Anti-Forgery token');
        });
}

// Example 8: Form submission with manual token handling
function submitFormWithManualToken(formData, url) {
    const token = AntiForgeryHelper.getToken();
    if (!token) {
        showErrorMessage('خطای امنیتی: لطفاً صفحه را رفرش کنید');
        return;
    }

    return $.ajax({
        url: url,
        type: 'POST',
        data: formData,
        headers: {
            [AntiForgeryHelper.getHeaderName()]: token
        },
        success: function(response) {
            if (response.success) {
                showSuccessMessage('عملیات با موفقیت انجام شد');
            } else {
                showErrorMessage(response.message || 'خطا در انجام عملیات');
            }
        },
        error: function(xhr, status, error) {
            console.error('Request failed:', error);
            if (xhr.status === 400 && xhr.responseJSON?.message?.includes('CSRF')) {
                showErrorMessage('خطای امنیتی: لطفاً صفحه را رفرش کنید و دوباره تلاش کنید');
                setTimeout(() => {
                    location.reload();
                }, 2000);
            } else {
                showErrorMessage('خطا در ارتباط با سرور');
            }
        }
    });
}

// Utility functions for showing messages
function showSuccessMessage(message) {
    // You can implement this based on your UI framework
    console.log('Success:', message);
    // Example with Bootstrap alert
    const alertHtml = `
        <div class="alert alert-success alert-dismissible fade show" role="alert">
            <i class="fas fa-check-circle me-2"></i>${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;
    $('.main-content').prepend(alertHtml);
}

function showErrorMessage(message) {
    // You can implement this based on your UI framework
    console.error('Error:', message);
    // Example with Bootstrap alert
    const alertHtml = `
        <div class="alert alert-danger alert-dismissible fade show" role="alert">
            <i class="fas fa-exclamation-circle me-2"></i>${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;
    $('.main-content').prepend(alertHtml);
}

// Example usage in form submit handlers
$(document).ready(function() {
    // Example: Handle login form submission
    $('#loginForm').on('submit', function(e) {
        e.preventDefault();
        
        const formData = $(this).serialize();
        loginWithAjax(formData);
    });

    // Example: Handle register form submission
    $('#registerForm').on('submit', function(e) {
        e.preventDefault();
        
        const formData = $(this).serialize();
        registerWithAjax(formData);
    });

    // Example: Handle logout button click
    $('.logout-btn').on('click', function(e) {
        e.preventDefault();
        logoutWithAjax();
    });

    // Example: Refresh token periodically (every 30 minutes)
    setInterval(function() {
        refreshAntiForgeryToken();
    }, 30 * 60 * 1000);
}); 