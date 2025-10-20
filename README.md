## EmployeeDirectory — справочник сотрудников

Короткая инструкция по запуску, публикации и базовой настройке.

### Возможности
- Управление сотрудниками и отделами (назначение начальников отделов)
- Управление пользователями и ролями (Administrator, Manager)
- Логи системы с фильтрами и печатью
- Печать списков сотрудников (все, по отделам, по фильтрам)
- PostgreSQL + ASP.NET Core Identity

### Требования
- .NET 8 SDK
- PostgreSQL 14+ (или совместимая версия)

### Быстрый старт (локально)
1. Настройте строку подключения в `EmployeeDirectory/appsettings.Development.json`.
2. Выполните миграции:
```bash
dotnet ef database update --project EmployeeDirectory/EmployeeDirectory.csproj
```
3. Запустите приложение:
```bash
dotnet run --project EmployeeDirectory/EmployeeDirectory.csproj
```
По умолчанию сайт будет доступен на `http://localhost:5000` или `https://localhost:5001` (в зависимости от профиля запуска).

### Администратор по умолчанию
При инициализации создаётся только пользователь:
- Логин: `admin`
- Пароль: `admin123`
- Роль: `Administrator`

### Публикация (Release)
```bash
dotnet publish EmployeeDirectory/EmployeeDirectory.csproj -c Release -o publish
```
Содержимое папки `publish` — готовый артефакт. Запуск:
```bash
cd publish
dotnet EmployeeDirectory.dll
```

### Доступ не только с localhost
По умолчанию Kestrel слушает loopback. Чтобы слушать на всех интерфейсах:
- Одноразово (сеанс):
```bash
# Windows
set ASPNETCORE_URLS=http://0.0.0.0:5000 && dotnet EmployeeDirectory.dll

# Linux
ASPNETCORE_URLS=http://0.0.0.0:5000 dotnet EmployeeDirectory.dll
```
- Либо добавьте в `appsettings.Production.json`:
```json
{
  "AllowedHosts": "*",
  "Kestrel": {
    "Endpoints": {
      "Http": { "Url": "http://0.0.0.0:5000" }
    }
  }
}
```
Откройте порт 5000 в фаерволе. Затем сайт будет доступен по `http://<IP_сервера>:5000`.

### Запуск под IIS (Windows)
1. Установите .NET Hosting Bundle.
2. Создайте сайт в IIS, укажите путь на папку `publish`, пул — «No Managed Code».
3. Настройте привязки (80/443), сертификат для HTTPS.

### Переменные окружения (прод)
- `ASPNETCORE_ENVIRONMENT=Production`
- `ASPNETCORE_URLS=http://0.0.0.0:5000` (если self-host)
- Строки подключения и секреты храните во внешних переменных/секретах ОС.

### Печать
- Главная страница: печать всех/отфильтрованных сотрудников, доступна анонимно
- Админ: печать логов с учётом фильтров
- Начальник отдела: печать сотрудников своего отдела

### Active Directory (импорт — кратко)
- Подход: разовый импорт или периодическая синхронизация
- Библиотеки: `System.DirectoryServices.Protocols` или `Novell.Directory.Ldap`
- Минимальные поля: sAMAccountName, displayName, department, title, phone
- Маппинг выполняйте в сервисе импорта; не храните пароли из AD

### Полезные команды
```bash
# Миграции
dotnet ef migrations add Initial
dotnet ef database update

# Публикация
dotnet publish -c Release -o publish
```

### Обратная связь
Вопросы и предложения — создавайте issue или сообщайте разработчику.


