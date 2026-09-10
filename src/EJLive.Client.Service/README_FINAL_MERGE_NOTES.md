# EJLive Client Service — Final Integrated v4

هذه الحزمة هي نسخة توحيد نهائية لملفات `EJLive.Client.Service` بناءً على مبدأ:

> الاحتفاظ بالقديم المستقر + البناء عليه بالكود الجديد + Adapter/Compatibility Layer + Health/Backoff/Safe File Read.

## الملفات

- `IAgentController.cs`
- `AgentHealthReporter.cs`
- `ClientAgentWindowsService.cs`
- `AgentHeadlessController.cs`
- `Program.cs`
- `Compatibility/ReflectionSafe.cs`
- `Compatibility/BackoffPolicy.cs`
- `Compatibility/SafeLiveFileReader.cs`
- `Compatibility/JournalOutboxAdapter.cs`

## ماذا تم تعزيزه؟

1. اعتماد `BackgroundService` بدل `ServiceBase` كمسار إنتاجي.
2. الحفاظ على توافق `AgentBootstrapper` القديم عبر Reflection.
3. إضافة State Machine: `Stopped / Starting / Running / Paused / Failed`.
4. إضافة Health JSON endpoint مع atomic writes.
5. إضافة Reconnect Backoff + Jitter.
6. منع تداخل Heartbeat/Reconnect.
7. قراءة ملفات EJ/LOG باستخدام `FileShare.ReadWrite | FileShare.Delete`.
8. إضافة `JournalOutboxAdapter` لدعم اختلاف دوال `JournalOutbox` القديمة والجديدة.
9. منع الأوامر الحساسة داخل Headless Controller، وتمريرها لاحقاً إلى SafeRemoteCommandExecutor.
10. منع الاعتماد المباشر على WinForms داخل خدمة الإنتاج.

## طريقة الدمج

لا تستخدم Copy/Replace عشوائي.

1. خذ Backup أو Git commit.
2. انسخ الملفات إلى:
   `src/EJLive.Client.Service/`
3. انسخ مجلد:
   `Compatibility/`
   داخل نفس المشروع.
4. راجع ملف `.csproj` فقط إذا كان المشروع لا يجمع ملفات `.cs` تلقائياً.
5. شغل:
   ```powershell
   dotnet restore .\EJLive.Unified.sln
   dotnet build .\EJLive.Unified.sln -m:1 /p:BuildInParallel=false -v:m
   dotnet test .\src\EJLive.Tests\EJLive.Tests.csproj -m:1
   dotnet run --project .\src\EJLive.Verification\EJLive.Verification.csproj
   ```

## ملاحظات مهمة

- إذا ظهر اختلاف في خصائص `AppConfig` مثل `ATM_TYPE` أو `ATM_Type`، تم دعمهما في الكود.
- إذا اختلفت دوال `JournalOutbox.Enqueue`، تم تغطية أكثر من توقيع عبر `JournalOutboxAdapter`.
- إذا كانت `NetworkEngine` في نسختك مختلفة جذرياً، لا تحذف الكود؛ أنشئ `NetworkEngineAdapter` بنفس أسلوب `JournalOutboxAdapter`.
- هذه الحزمة لا تدعي نجاح build داخل هذه البيئة. يجب تشغيل build على جهاز Windows/.NET 8 الحقيقي.
