-- Добавление ролей в базу данных
-- Выполните этот скрипт в PostgreSQL для создания необходимых ролей

-- Проверяем и добавляем роли, если они не существуют
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

-- Проверяем результат
SELECT "Name", "Description", "CreatedAt" FROM "AspNetRoles" ORDER BY "Name";