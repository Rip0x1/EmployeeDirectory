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
<img width="1902" height="994" alt="{9D5CC7A3-28C3-4315-B9D7-FF0C302DE7B8}" src="https://github.com/user-attachments/assets/86d763cf-42e9-4afc-9702-5aeab26bd809" />
<img width="1920" height="740" alt="{7614CBC9-A60B-4ECA-A881-7383B83744F1}" src="https://github.com/user-attachments/assets/b5ffdd9b-6563-4d2e-a5cb-9b83bd07e33b" />
<img width="1920" height="369" alt="{D9927645-C12E-4DB0-B137-349F7B71F05D}" src="https://github.com/user-attachments/assets/1587d8d2-ad38-4b5a-a427-8a79e399a2df" />
<img width="1920" height="306" alt="{7B370C80-2D66-489C-8047-1A0E5462A984}" src="https://github.com/user-attachments/assets/9fc35fbe-c789-424d-a5c8-13611cce6fa8" />
<img width="1920" height="713" alt="{4A395A62-43E9-485D-BF92-83A1CE9F9503}" src="https://github.com/user-attachments/assets/55d20768-c9ef-49ba-b002-301b446965f8" />




