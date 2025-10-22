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

### Фото
<img width="1907" height="874" alt="{FDDCBA42-5624-4842-BF8C-A828883AC0D7}" src="https://github.com/user-attachments/assets/aea52473-359a-494c-bf0a-19525487a961" />
<img width="1920" height="926" alt="{5C431DC7-3F9B-4B69-AB8A-B52B5F097FE5}" src="https://github.com/user-attachments/assets/91921e7b-9a4f-41fe-a1fc-7755d1a29717" />
<img width="1918" height="813" alt="{D903FDEC-C649-424E-B7AC-806DB33ADA17}" src="https://github.com/user-attachments/assets/3d9a8a69-dc9c-4305-beed-be27c5292818" />


### Обратная связь
Вопросы и предложения — создавайте issue или сообщайте разработчику.


