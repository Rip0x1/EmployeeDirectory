# Employee Directory - Справочник сотрудников

Веб-приложение для управления сотрудниками организации с поддержкой Docker.

## 🚀 Быстрый старт

### Требования
- Docker Desktop (Windows/Mac) или Docker Engine (Linux)
- 2 GB свободной памяти
- Порты 5000 и 7777 должны быть свободны

### Запуск локально

```bash
# Клонировать репозиторий
git clone <repository-url>
cd <project-directory>

# Запустить все контейнеры
docker-compose up -d

# Приложение будет доступно на http://localhost:5000
```

### Учетные данные по умолчанию
- **Логин:** `admin`
- **Пароль:** `admin123`

---

## 📋 Управление контейнерами

### Основные команды

```bash
# Запустить все контейнеры
docker-compose up -d

# Остановить все контейнеры
docker-compose down

# Посмотреть статус
docker ps

# Посмотреть логи
docker logs employee-directory-web
docker logs employee-directory-db

# Пересобрать после изменений в коде
docker-compose up -d --build

# Остановить и удалить все данные (ВНИМАНИЕ: БД будет очищена!)
docker-compose down -v
```

### 🔄 Применение изменений в коде

После внесения изменений в код выполните:

```bash
# Остановить контейнеры
docker-compose down

# Пересобрать и запустить
docker-compose up -d --build
```

**Или через Docker Desktop:**
1. Остановите контейнеры кнопкой "Stop"
2. Нажмите на действие "Build & Run" рядом с `employee-directory-web`
3. Или используйте кнопку "Rebuild" для пересборки

### Полезные команды

```bash
# Перезапустить конкретный контейнер
docker restart employee-directory-web

# Посмотреть логи в реальном времени
docker logs -f employee-directory-web

# Подключиться к базе данных
docker exec -it employee-directory-db psql -U postgres -d EmployeeDirectory

# Проверить использование ресурсов
docker stats
```

### 📊 Подключение через pgAdmin 4

Если установлен pgAdmin 4 локально, можно подключиться к БД контейнера:

**Параметры подключения:**
- **Host:** `localhost` (или `127.0.0.1`)
- **Port:** `7777`
- **Database:** `EmployeeDirectory`
- **Username:** `postgres`
- **Password:** `root`

**Шаги:**
1. Откройте pgAdmin 4
2. Правый клик на "Servers" → "Create" → "Server"
3. На вкладке **General**:
   - Name: `Employee Directory (Docker)`
4. На вкладке **Connection**:
   - Host: `localhost`
   - Port: `7777`
   - Database: `EmployeeDirectory`
   - Username: `postgres`
   - Password: `root`
   - Сохраните пароль (Save password ✓)
5. Нажмите "Save"

**Важно:** Контейнер с БД должен быть запущен (`docker ps`).

---

## 🌐 Фото





