# راهنمای پیاده‌سازی Anti-Forgery (CSRF) Protection

## 🔒 **خلاصه پیاده‌سازی**

✅ **Anti-Forgery (CSRF) Protection پیاده‌سازی شده:**

### 🛡️ **ویژگی‌های امنیتی:**
- **Cookie-based Authentication**: محافظت از توکن‌های ذخیره شده در Cookie
- **Form Protection**: محافظت از فرم‌های HTML با `@Html.AntiForgeryToken()`
- **AJAX Protection**: محافظت از درخواست‌های AJAX با `X-CSRF-TOKEN` header
- **Double-Submit Cookie**: استفاده از Double-Submit Cookie pattern
- **Automatic Token Management**: مدیریت خودکار توکن‌ها

## 🏗️ **معماری پیاده‌سازی**

### **1. Server-Side Configuration (Program.cs):**
```csharp
// Add Anti-Forgery protection
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "CSRF-TOKEN";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.SuppressXFrameOptionsHeader = false;
});

// Use Anti-Forgery middleware
app.UseAntiforgery();
```

### **2. Anti-Forgery Service:**
```csharp
public interface IAntiForgeryService
{
    string GetToken();
    string GetTokenHeaderName();
    string GetTokenCookieName();
    bool ValidateToken(string token);
    void SetTokenInCookie(HttpContext context);
    string GetTokenFromCookie(HttpContext context);
}
```

### **3. BaseController Integration:**
```csharp
protected void SetUserViewBag()
{
    // ... existing code ...
    
    // Add Anti-Forgery token for AJAX requests
    ViewBag.AntiForgeryToken = AntiForgeryService.GetToken();
    ViewBag.AntiForgeryHeaderName = AntiForgeryService.GetTokenHeaderName();
}

protected bool ValidateAntiForgeryToken()
{
    var token = Request.Headers[AntiForgeryService.GetTokenHeaderName()].FirstOrDefault();
    return AntiForgeryService.ValidateToken(token);
}
```

## 📝 **نحوه استفاده**

### **1. در فرم‌های HTML:**
```html
<form asp-action="Login" asp-controller="Auth" method="post">
    @Html.AntiForgeryToken()
    <!-- form fields -->
</form>
```

### **2. در AJAX Requests:**
```javascript
// استفاده از AntiForgeryHelper
AntiForgeryHelper.post('/Auth/LoginAjax', formData, {
    success: function(response) {
        // handle success
    },
    error: function(xhr, status, error) {
        // handle error
    }
});

// یا استفاده مستقیم
$.ajax({
    url: '/Auth/LoginAjax',
    type: 'POST',
    data: formData,
    headers: {
        'X-CSRF-TOKEN': AntiForgeryHelper.getToken()
    }
});
```

### **3. در Controller Actions:**
```csharp
[HttpPost]
[ValidateAntiForgeryToken] // برای فرم‌های HTML
public async Task<IActionResult> Login(LoginDto model)
{
    // ... implementation
}

[HttpPost]
public async Task<IActionResult> LoginAjax(LoginDto model)
{
    // برای AJAX requests
    if (!ValidateAntiForgeryToken())
    {
        return JsonError("خطای امنیتی: توکن نامعتبر است");
    }
    
    // ... implementation
}
```

## 🔧 **تنظیمات امنیتی**

### **Cookie Security:**
```csharp
options.Cookie.HttpOnly = true;        // جلوگیری از دسترسی JavaScript
options.Cookie.SecurePolicy = CookieSecurePolicy.Always;  // فقط HTTPS
options.Cookie.SameSite = SameSiteMode.Strict;  // محافظت از CSRF
```

### **Header Configuration:**
```csharp
options.HeaderName = "X-CSRF-TOKEN";   // نام header برای AJAX
options.SuppressXFrameOptionsHeader = false;  // محافظت از Clickjacking
```

## 🚀 **ویژگی‌های پیشرفته**

### **1. Automatic Token Management:**
```javascript
// اضافه شدن خودکار توکن به تمام AJAX requests
$.ajaxSetup({
    beforeSend: function(xhr, settings) {
        if (settings.type === 'POST' || settings.type === 'PUT' || settings.type === 'DELETE') {
            const token = AntiForgeryHelper.getToken();
            if (token) {
                xhr.setRequestHeader(AntiForgeryHelper.getHeaderName(), token);
            }
        }
    }
});
```

### **2. Token Refresh:**
```javascript
// تمدید خودکار توکن هر 30 دقیقه
setInterval(function() {
    AntiForgeryHelper.refreshToken();
}, 30 * 60 * 1000);
```

### **3. Error Handling:**
```javascript
// مدیریت خطاهای CSRF
error: function(xhr, status, error) {
    if (xhr.status === 400 && xhr.responseJSON?.message?.includes('CSRF')) {
        alert('خطای امنیتی: لطفاً صفحه را رفرش کنید و دوباره تلاش کنید.');
        location.reload();
    }
}
```

## 📊 **مثال‌های کاربردی**

### **1. Login Form:**
```html
<form id="loginForm" asp-action="Login" asp-controller="Auth" method="post">
    @Html.AntiForgeryToken()
    <input asp-for="Email" class="form-control" />
    <input asp-for="Password" class="form-control" />
    <button type="submit">ورود</button>
</form>
```

### **2. AJAX Login:**
```javascript
$('#loginForm').on('submit', function(e) {
    e.preventDefault();
    
    const formData = $(this).serialize();
    AntiForgeryHelper.post('/Auth/LoginAjax', formData, {
        success: function(response) {
            if (response.success) {
                window.location.href = '/Home';
            } else {
                showErrorMessage(response.message);
            }
        }
    });
});
```

### **3. Profile Update:**
```javascript
function updateProfile(profileData) {
    return AntiForgeryHelper.put('/Auth/UpdateProfileAjax', profileData, {
        success: function(response) {
            if (response.success) {
                showSuccessMessage('پروفایل با موفقیت به‌روزرسانی شد');
            } else {
                showErrorMessage(response.message);
            }
        }
    });
}
```

### **4. Delete Account:**
```javascript
function deleteAccount() {
    if (confirm('آیا مطمئن هستید؟')) {
        return AntiForgeryHelper.delete('/Auth/DeleteAccountAjax', {
            success: function(response) {
                if (response.success) {
                    window.location.href = '/Auth/Register';
                }
            }
        });
    }
}
```

## 🔍 **تست و Debugging**

### **1. بررسی توکن در Browser:**
```javascript
// بررسی توکن در Console
console.log('Token:', AntiForgeryHelper.getToken());
console.log('Header Name:', AntiForgeryHelper.getHeaderName());
```

### **2. بررسی Cookie:**
```javascript
// بررسی CSRF Cookie
console.log('CSRF Cookie:', document.cookie);
```

### **3. بررسی Network Requests:**
```javascript
// بررسی header در Network tab
// باید X-CSRF-TOKEN header در POST/PUT/DELETE requests دیده شود
```

## 🛡️ **مزایای امنیتی**

### **🔒 محافظت از CSRF:**
- **Double-Submit Cookie**: توکن در cookie و header ارسال می‌شود
- **Token Validation**: بررسی اعتبار توکن در هر درخواست
- **Automatic Protection**: محافظت خودکار از تمام state-changing operations

### **🛡️ محافظت از Clickjacking:**
- **X-Frame-Options**: جلوگیری از embedding در iframe
- **SameSite Cookie**: محافظت از cross-site requests

### **🔐 محافظت از XSS:**
- **HttpOnly Cookie**: جلوگیری از دسترسی JavaScript به توکن
- **Secure Cookie**: فقط در HTTPS ارسال می‌شود

## 📋 **چک‌لیست امنیتی**

### ✅ **Server-Side:**
- [x] Anti-Forgery middleware فعال
- [x] Cookie security تنظیم شده
- [x] Header validation پیاده‌سازی شده
- [x] Error handling مناسب

### ✅ **Client-Side:**
- [x] JavaScript helper پیاده‌سازی شده
- [x] Automatic token injection فعال
- [x] Error handling مناسب
- [x] Token refresh mechanism

### ✅ **Forms:**
- [x] `@Html.AntiForgeryToken()` در تمام فرم‌ها
- [x] `[ValidateAntiForgeryToken]` در actions
- [x] AJAX protection فعال

### ✅ **Testing:**
- [x] Manual testing انجام شده
- [x] Browser developer tools بررسی شده
- [x] Network requests بررسی شده

## 🎯 **نتیجه‌گیری**

✅ **Anti-Forgery (CSRF) Protection کامل پیاده‌سازی شد:**

- 🔒 **امنیت**: محافظت کامل از CSRF attacks
- 🛡️ **محافظت**: Double-Submit Cookie pattern
- 🔄 **اتوماتیک**: مدیریت خودکار توکن‌ها
- 📱 **AJAX Support**: پشتیبانی کامل از AJAX requests
- 🧪 **Testing**: قابلیت تست و debugging

این پیاده‌سازی تضمین می‌کند که:
- تمام فرم‌ها از CSRF محافظت می‌شوند
- تمام AJAX requests امن هستند
- توکن‌ها به صورت خودکار مدیریت می‌شوند
- خطاهای امنیتی به درستی handle می‌شوند

**نکته مهم**: این پیاده‌سازی برای Cookie-based authentication طراحی شده و با JWT tokens در cookie کار می‌کند! 🚀 