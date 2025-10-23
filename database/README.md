# Настройка ролей в базе данных

## Проблема
Если при создании пользователя нельзя выбрать роль "Редактор отдела", это означает, что роли не существуют в базе данных.

## Решение

### 1. Выполните SQL скрипт

Используйте файл `setup_roles.sql` для безопасного добавления ролей:

```bash
# Подключитесь к вашей базе данных PostgreSQL
psql -h localhost -U your_username -d your_database_name

# Выполните скрипт
\i database/setup_roles.sql
```

### 2. Или выполните SQL вручную

```sql
-- Проверяем существующие роли
SELECT "Name" FROM "AspNetRoles" WHERE "Name" IN ('Manager', 'Administrator', 'DepartmentEditor');

-- Добавляем роли, если их нет
INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp", "Description", "CreatedAt")
SELECT 
    '1', 
    'Manager', 
    'MANAGER', 
    gen_random_uuid()::text, 
    'Начальник отдела', 
    NOW()
WHERE NOT EXISTS (SELECT 1 FROM "AspNetRoles" WHERE "Name" = 'Manager');

INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp", "Description", "CreatedAt")
SELECT 
    '2', 
    'Administrator', 
    'ADMINISTRATOR', 
    gen_random_uuid()::text, 
    'Администратор', 
    NOW()
WHERE NOT EXISTS (SELECT 1 FROM "AspNetRoles" WHERE "Name" = 'Administrator');

INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp", "Description", "CreatedAt")
SELECT 
    '3', 
    'DepartmentEditor', 
    'DEPARTMENTEDITOR', 
    gen_random_uuid()::text, 
    'Редактор отдела', 
    NOW()
WHERE NOT EXISTS (SELECT 1 FROM "AspNetRoles" WHERE "Name" = 'DepartmentEditor');
```

## Роли в системе:

1. **Manager** - Начальник отдела
   - Может управлять сотрудниками своего отдела
   - Может создавать редакторов для своего отдела

2. **Administrator** - Администратор
   - Полный доступ ко всем функциям системы
   - Может создавать пользователей с любыми ролями

3. **DepartmentEditor** - Редактор отдела
   - Может управлять сотрудниками только своего отдела
   - Ограниченные права доступа

## Выполнение скрипта:

1. Подключитесь к вашей базе данных PostgreSQL
2. Выполните SQL скрипт выше
3. Перезапустите приложение

После выполнения скрипта роли будут доступны в интерфейсе создания пользователей.
