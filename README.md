# Family Task Management - Frontend (MVC)

## نظام إدارة مهام العائلة - الواجهة الأمامية

هذا المشروع هو الواجهة الأمامية لنظام إدارة مهام العائلة، مبني باستخدام ASP.NET Core MVC ويتصل مع الـ API الخلفي لتوفير واجهة مستخدم سهلة وبديهية.

## المميزات

### 🔐 نظام المصادقة والتخويل
- تسجيل دخول آمن باستخدام JWT
- إدارة الجلسات باستخدام Cookie Authentication
- تخويل مبني على الأدوار (Admin/Member)
- صفحات محمية حسب الصلاحيات

### 👥 إدارة المستخدمين
- عرض قائمة المستخدمين (للمدير)
- إنشاء مستخدمين جدد (للمدير)
- تعديل بيانات المستخدمين
- عرض الملف الشخصي

### 📋 إدارة المهام
- عرض قائمة المهام
- إنشاء مهام جديدة (للمدير)
- تعديل وحذف المهام (للمدير)
- عرض تفاصيل المهام

### 📅 إسناد المهام
- إسناد المهام للمستخدمين (للمدير)
- عرض مهام المستخدم الحالي
- تحديث حالة المهام
- عرض مهام الأسبوع

### 📊 لوحات التحكم
- لوحة تحكم المدير مع الإحصائيات
- لوحة تحكم المستخدم مع مهامه
- عرض سريع للمهام والحالات

## البنية المعمارية

### Controllers
- `HomeController`: الصفحة الرئيسية ولوحات التحكم
- `AccountController`: المصادقة والملف الشخصي
- `UsersController`: إدارة المستخدمين
- `TasksController`: إدارة المهام
- `TaskAssignmentsController`: إسناد المهام

### Services
- `IApiService`: خدمة الاتصال مع الـ API
- `IAuthService`: خدمة المصادقة والتخويل

### Models & DTOs
- `UserDto`: بيانات المستخدم
- `TaskDto`: بيانات المهمة
- `TaskAssignmentDto`: بيانات إسناد المهام
- `ApiResponse<T>`: نموذج استجابة الـ API

### Views
- صفحات Razor محسنة للعرض
- تصميم متجاوب يدعم الأجهزة المحمولة
- واجهة باللغة العربية

## التقنيات المستخدمة

- **ASP.NET Core 8.0 MVC**
- **Bootstrap 5** للتصميم
- **jQuery** للتفاعل
- **Cookie Authentication** لإدارة الجلسات
- **HttpClient** للاتصال مع الـ API
- **Newtonsoft.Json** لمعالجة JSON

## متطلبات التشغيل

- .NET 8.0 SDK
- Family Task Management API (يجب أن يكون يعمل على المنفذ 5000)

## كيفية التشغيل

### 1. استنساخ المشروع
```bash
git clone [repository-url]
cd FamilyTaskManagement.Web
```

### 2. تثبيت التبعيات
```bash
dotnet restore
```

### 3. تكوين الإعدادات
تأكد من أن ملف `appsettings.json` يحتوي على عنوان الـ API الصحيح:
```json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:5000/api",
    "Timeout": 30
  }
}
```

### 4. تشغيل التطبيق
```bash
dotnet run
```

التطبيق سيعمل على: `https://localhost:5001` أو `http://localhost:5000`

## الإعدادات

### appsettings.json
```json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:5000/api",
    "Timeout": 30
  },
  "JwtSettings": {
    "SecretKey": "YourSecretKeyThatIsAtLeast32CharactersLong!@#$%^&*()",
    "Issuer": "FamilyTaskManagement",
    "Audience": "FamilyTaskManagement",
    "ExpiryInMinutes": 60
  }
}
```

## الصفحات الرئيسية

### للزوار غير المسجلين
- `/`: صفحة الترحيب
- `/Account/Login`: تسجيل الدخول

### للمستخدمين المسجلين
- `/`: لوحة التحكم (حسب الدور)
- `/Account/Profile`: الملف الشخصي
- `/Tasks/MyTasks`: مهامي

### للمديرين
- `/Users`: إدارة المستخدمين
- `/Tasks`: إدارة المهام
- `/TaskAssignments`: إسناد المهام
- `/TaskAssignments/WeeklyAssignments`: مهام الأسبوع

## الأمان

- جميع الصفحات محمية بنظام المصادقة
- التخويل مبني على الأدوار
- حماية من CSRF attacks
- تشفير كلمات المرور
- إدارة آمنة للجلسات

## التطوير

### إضافة صفحة جديدة
1. إنشاء Action في Controller المناسب
2. إنشاء View في المجلد المناسب
3. إضافة التخويل المطلوب

### إضافة خدمة جديدة
1. إنشاء Interface في مجلد Services
2. تطبيق الخدمة
3. تسجيل الخدمة في Program.cs

## الاختبار

### اختبار محلي
```bash
dotnet run
```

### اختبار البناء
```bash
dotnet build
```

## النشر

### نشر محلي
```bash
dotnet publish -c Release
```

### متطلبات الخادم
- .NET 8.0 Runtime
- IIS أو Nginx (اختياري)
- اتصال بالـ API الخلفي

## المساهمة

1. Fork المشروع
2. إنشاء فرع للميزة الجديدة
3. Commit التغييرات
4. Push للفرع
5. إنشاء Pull Request

## الترخيص

هذا المشروع مرخص تحت رخصة MIT.

## الدعم

للحصول على الدعم أو الإبلاغ عن مشاكل، يرجى إنشاء Issue في المستودع.

---

**ملاحظة**: تأكد من تشغيل الـ API الخلفي قبل تشغيل هذا التطبيق.

Authur Juan Khanjar
