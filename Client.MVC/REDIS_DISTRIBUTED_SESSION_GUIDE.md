# راهنمای پیاده‌سازی Redis برای Distributed Session

## خلاصه پیاده‌سازی

✅ **Redis برای Distributed Session پیاده‌سازی شده:**

### 🔧 **تنظیمات Redis**

```json
"Redis": {
  "ConnectionString": "localhost:6379",
  "InstanceName": "ClientMVC:",
  "DefaultDatabase": 0,
  "ConnectTimeout": 5000,
  "SyncTimeout": 5000,
  "AbortConnect": false,
  "ConnectRetry": 3,
  "ReconnectRetryPolicy": "LinearRetry",
  "KeepAlive": 180
}
```

### 🔐 **تنظیمات Data Protection**

```json
"DataProtection": {
  "ApplicationName": "ClientMVC",
  "KeyLifetime": "90",
  "KeyRingPath": "/keys"
}
```

## ویژگی‌های پیاده‌سازی شده

### 1. **Environment-Based Configuration**
- ✅ **Development**: استفاده از `AddDistributedMemoryCache()`
- ✅ **Production**: استفاده از `AddStackExchangeRedisCache()`
- ✅ **Fallback**: در صورت عدم دسترسی به Redis، استفاده از Memory Cache

### 2. **Data Protection**
- ✅ **Cookie Encryption**: رمزنگاری cookie بین instanceها
- ✅ **Session Encryption**: رمزنگاری session data
- ✅ **Key Management**: مدیریت کلیدهای رمزنگاری در Redis
- ✅ **Key Lifetime**: تنظیم طول عمر کلیدها (90 روز)

### 3. **Redis Configuration**
- ✅ **Connection String**: پیکربندی اتصال به Redis
- ✅ **Instance Name**: نام instance برای namespace
- ✅ **Database Selection**: انتخاب database
- ✅ **Connection Timeout**: تنظیم timeout اتصال
- ✅ **Retry Policy**: سیاست retry برای اتصال

### 4. **Health Checks**
- ✅ **Redis Health Check**: بررسی سلامت Redis در production
- ✅ **Self Health Check**: بررسی سلامت خود برنامه

## نحوه استفاده

### 1. **در Development**
```csharp
// به صورت خودکار از Memory Cache استفاده می‌شود
builder.Services.AddDistributedMemoryCache();
```

### 2. **در Production**
```csharp
// به صورت خودکار از Redis استفاده می‌شود
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "ClientMVC:";
});
```

### 3. **Data Protection**
```csharp
// رمزنگاری cookie/session بین instanceها
builder.Services.AddDataProtection(options =>
{
    options.ApplicationDiscriminator = "ClientMVC";
})
.SetApplicationName("ClientMVC")
.SetDefaultKeyLifetime(TimeSpan.FromDays(90));
```

## تنظیمات پیشنهادی برای Production

### 🔒 **Redis Production Settings**
```json
"Redis": {
  "ConnectionString": "redis-cluster.example.com:6379,password=your-password",
  "InstanceName": "ClientMVC:Prod:",
  "DefaultDatabase": 0,
  "ConnectTimeout": 10000,
  "SyncTimeout": 10000,
  "AbortConnect": false,
  "ConnectRetry": 5,
  "ReconnectRetryPolicy": "ExponentialRetry",
  "KeepAlive": 300
}
```

### 🚀 **Data Protection Production Settings**
```json
"DataProtection": {
  "ApplicationName": "ClientMVC-Production",
  "KeyLifetime": "365",
  "KeyRingPath": "/app/keys"
}
```

## مزایای پیاده‌سازی

### 🎯 **برای Development:**
- **سادگی**: استفاده از Memory Cache
- **سرعت**: دسترسی سریع به داده‌ها
- **عدم نیاز به Redis**: بدون نیاز به نصب Redis

### 🚀 **برای Production:**
- **Scalability**: پشتیبانی از چندین instance
- **Session Sharing**: اشتراک session بین instanceها
- **High Availability**: دسترسی بالا با Redis Cluster
- **Security**: رمزنگاری cookie/session

### 📊 **برای Multi-Instance:**
- **Load Balancing**: توزیع بار بین instanceها
- **Session Persistence**: حفظ session در صورت restart
- **Data Consistency**: سازگاری داده‌ها بین instanceها

## مانیتورینگ و Logging

### 📊 **Logs برای Redis Connection:**
```
Redis Configuration loaded - Connection: localhost:6379, Instance: ClientMVC:
Redis is not available - Connection timeout
Redis health check passed
```

### 🚨 **Logs برای Data Protection:**
```
Data Protection keys loaded from Redis
Key rotation completed successfully
Cookie encryption/decryption successful
```

## نکات مهم

### ⚠️ **برای Development:**
1. **Redis نصب نیست**: به صورت خودکار از Memory Cache استفاده می‌شود
2. **تنظیمات ساده**: بدون نیاز به پیکربندی پیچیده
3. **Performance**: سرعت بالا برای development

### 🎯 **برای Production:**
1. **Redis Cluster**: استفاده از Redis Cluster برای HA
2. **Password Protection**: محافظت با رمز عبور
3. **SSL/TLS**: استفاده از اتصال امن
4. **Monitoring**: مانیتورینگ Redis performance

### 📈 **برای Multi-Instance:**
1. **Session Affinity**: تنظیم session affinity در load balancer
2. **Key Management**: مدیریت کلیدهای رمزنگاری
3. **Backup Strategy**: استراتژی backup برای Redis
4. **Disaster Recovery**: برنامه بازیابی در صورت خرابی

## نحوه تست

### 1. **تست Redis Connection**
```bash
# تست اتصال به Redis
redis-cli ping
# پاسخ: PONG
```

### 2. **تست Session Sharing**
```bash
# در instance اول
curl -c cookies.txt http://instance1/login

# در instance دوم
curl -b cookies.txt http://instance2/profile
# باید session حفظ شود
```

### 3. **تست Health Check**
```bash
# بررسی سلامت Redis
curl http://localhost/health
# پاسخ: {"status":"Healthy","checks":{"redis":"Healthy"}}
```

## نتیجه‌گیری

✅ **تمام الزامات برآورده شده‌اند:**

- ✅ **Distributed Session**: پیاده‌سازی با Redis
- ✅ **Environment-Based**: تنظیمات بر اساس محیط
- ✅ **Data Protection**: رمزنگاری cookie/session
- ✅ **Health Checks**: بررسی سلامت Redis
- ✅ **Fallback Strategy**: استراتژی جایگزین
- ✅ **Multi-Instance Support**: پشتیبانی از چندین instance

این پیاده‌سازی تضمین می‌کند که:
- 🔒 **امنیت**: رمزنگاری cookie/session بین instanceها
- 🚀 **عملکرد**: دسترسی سریع به session data
- 🛡️ **مقاومت**: پشتیبانی از Redis Cluster
- 📊 **قابلیت نگهداری**: مانیتورینگ و logging کامل
- 🔄 **Scalability**: پشتیبانی از چندین instance 