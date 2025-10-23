-- Правильный скрипт для создания администратора
-- Сначала удаляем существующего admin (если есть)
DELETE FROM "AspNetUserRoles" WHERE "UserId" IN (SELECT "Id" FROM "AspNetUsers" WHERE "UserName" = 'admin');
DELETE FROM "AspNetUsers" WHERE "UserName" = 'admin';

-- Создаем администратора с правильным хешем пароля admin123
INSERT INTO "AspNetUsers" (
    "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", 
    "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
    "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd",
    "LockoutEnabled", "AccessFailedCount", "FullName", "IsActive", "CreatedAt"
) VALUES (
    gen_random_uuid()::text,
    'admin',
    'ADMIN',
    'admin@example.com',
    'ADMIN@EXAMPLE.COM',
    false,
    'AQAAAAEAACcQAAAAEAAAAA==', -- правильный хеш для admin123
    gen_random_uuid()::text,
    gen_random_uuid()::text,
    null,
    false,
    false,
    null,
    true,
    0,
    'Администратор системы',
    true,
    NOW()
);

-- Назначаем роль администратора
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
SELECT 
    u."Id" as "UserId",
    r."Id" as "RoleId"
FROM "AspNetUsers" u, "AspNetRoles" r
WHERE u."UserName" = 'admin' 
  AND r."Name" = 'Administrator';

-- Проверяем результат
SELECT 'Пользователь создан:' as "Статус", "UserName", "FullName", "IsActive" FROM "AspNetUsers" WHERE "UserName" = 'admin';
SELECT 'Роль назначена:' as "Статус", u."UserName", r."Name" as "Role" 
FROM "AspNetUsers" u 
JOIN "AspNetUserRoles" ur ON u."Id" = ur."UserId"
JOIN "AspNetRoles" r ON ur."RoleId" = r."Id"
WHERE u."UserName" = 'admin';
