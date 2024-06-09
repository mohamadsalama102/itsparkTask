
1. استنساخ المشروع:
git clone https://github.com/mohamadsalama102/itsparkTask.git


2. افتح المشروع في محرر النصوص المفضل لديك.
## تكوين البيانات

### قاعدة البيانات

يجب عليك تعيين سلسلة اتصال قاعدة البيانات في ملف appsettings.json:

{
  "ConnectionStrings": {
    "DefaultConnection": "Your_Connection_String"
  }
}

dotnet ef database update

1. إنشاء مشروع Google Cloud Platform:
قم بزيارة Google Cloud Console.
قم بتسجيل الدخول باستخدام حساب Google الخاص بك أو قم بإنشاء واحد إذا لم يكن لديك.
انشئ مشروع جديد من خلال النقر على "اختيار مشروع" في الزاوية العلوية اليمنى ومن ثم "إنشاء مشروع".
2. تكوين Google OAuth:
انتقل إلى "APIs & Services" > "Credentials".
اختر "Create Credentials" واختر "OAuth client ID".
اختر نوع التطبيق (Web application أو Other) حسب نوع التطبيق الخاص بك.
أدخل معلومات التكوين المطلوبة مثل عنوان التطبيق (redirect URIs)، والمسارات المسموح بها للوصول (authorized redirect URIs)، وغيرها.
بعد الانتهاء، ستحصل على Client ID و Client Secret.
3. إعداد GoogleCredentials:
احتفظ بـ Client ID و Client Secret بشكل آمن.
في مشروع ASP.NET Core الخاص بك، يمكنك تخزين هذه المعلومات في ملف appsettings.json:
{
  "GoogleCredentials": {
    "ClientID": "Your_Client_ID",
    "ClientSecret": "Your_Client_Secret"
  }
}

### 5. استخدام التطبيق:

وفر توجيهات حول كيفية استخدام التطبيق بما في ذلك الخطوات اللازمة لعرض آخر 10 رسائل بريد جيميل.

### 6. تعديل البيانات:

أوضح كيف يمكن للمستخدمين تعديل بيانات التكوين الخاصة بالتطبيق، مثل سلسلة الاتصال بقاعدة البيانات أو بيانات الاتصال مع خدمات جوجل.
