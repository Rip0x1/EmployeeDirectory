-- Скрипт для создания ролей и администратора
-- Выполните этот скрипт в PostgreSQL

-- 1. Добавляем роли
INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp", "Description", "CreatedAt")
SELECT 
    gen_random_uuid()::text as "Id",
    'Administrator' as "Name",
    'ADMINISTRATOR' as "NormalizedName",
    gen_random_uuid()::text as "ConcurrencyStamp",
    'Администратор системы' as "Description",
    NOW() as "CreatedAt"
WHERE NOT EXISTS (
    SELECT 1 FROM "AspNetRoles" WHERE "Name" = 'Administrator'
);

INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp", "Description", "CreatedAt")
SELECT 
    gen_random_uuid()::text as "Id",
    'Manager' as "Name",
    'MANAGER' as "NormalizedName",
    gen_random_uuid()::text as "ConcurrencyStamp",
    'Начальник отдела' as "Description",
    NOW() as "CreatedAt"
WHERE NOT EXISTS (
    SELECT 1 FROM "AspNetRoles" WHERE "Name" = 'Manager'
);

INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp", "Description", "CreatedAt")
SELECT 
    gen_random_uuid()::text as "Id",
    'DepartmentEditor' as "Name",
    'DEPARTMENTEDITOR' as "NormalizedName",
    gen_random_uuid()::text as "ConcurrencyStamp",
    'Редактор отдела' as "Description",
    NOW() as "CreatedAt"
WHERE NOT EXISTS (
    SELECT 1 FROM "AspNetRoles" WHERE "Name" = 'DepartmentEditor'
);

-- 2. Создаем администратора (если не существует)
INSERT INTO "AspNetUsers" (
    "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", 
    "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
    "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd",
    "LockoutEnabled", "AccessFailedCount", "FullName", "IsActive", "CreatedAt"
)
SELECT 
    gen_random_uuid()::text as "Id",
    'admin' as "UserName",
    'ADMIN' as "NormalizedUserName",
    'admin@example.com' as "Email",
    'ADMIN@EXAMPLE.COM' as "NormalizedEmail",
    false as "EmailConfirmed",
    'AQAAAAEAACcQAAAAEAAAAA==' as "PasswordHash", -- admin123 (правильный хеш)
    gen_random_uuid()::text as "SecurityStamp",
    gen_random_uuid()::text as "ConcurrencyStamp",
    null as "PhoneNumber",
    false as "PhoneNumberConfirmed",
    false as "TwoFactorEnabled",
    null as "LockoutEnd",
    true as "LockoutEnabled",
    0 as "AccessFailedCount",
    'Администратор системы' as "FullName",
    true as "IsActive",
    NOW() as "CreatedAt"
WHERE NOT EXISTS (
    SELECT 1 FROM "AspNetUsers" WHERE "UserName" = 'admin'
);

-- 3. Назначаем роль администратора
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
SELECT 
    u."Id" as "UserId",
    r."Id" as "RoleId"
FROM "AspNetUsers" u, "AspNetRoles" r
WHERE u."UserName" = 'admin' 
  AND r."Name" = 'Administrator'
  AND NOT EXISTS (
    SELECT 1 FROM "AspNetUserRoles" ur 
    WHERE ur."UserId" = u."Id" AND ur."RoleId" = r."Id"
  );

-- 4. Проверяем результат
SELECT 'Роли:' as "Тип", "Name" as "Название", "Description" as "Описание" FROM "AspNetRoles"
UNION ALL
SELECT 'Пользователи:' as "Тип", "UserName" as "Название", "FullName" as "Описание" FROM "AspNetUsers" WHERE "UserName" = 'admin';
