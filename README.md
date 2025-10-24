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

## 🌐 Развертывание на сервере

### Вариант 1: VPS/VDS с Linux

#### Требования
- Ubuntu 20.04+ или Debian 11+
- Минимум 2 GB RAM
- Открытые порты 80, 443 (для веб)
- Доступ по SSH

#### Шаги развертывания

1. **Подключитесь к серверу:**
```bash
ssh user@your-server-ip
```

2. **Установите Docker:**
```bash
# Обновить систему
sudo apt update && sudo apt upgrade -y

# Установить Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Добавить пользователя в группу docker
sudo usermod -aG docker $USER
newgrp docker

# Установить Docker Compose
sudo apt install docker-compose-plugin -y
```

3. **Загрузите проект на сервер:**
```bash
# Вариант A: через Git
git clone <repository-url>
cd <project-directory>

# Вариант B: через SCP
scp -r <local-project-directory> user@server-ip:/home/user/
```

4. **Создайте docker-compose.production.yml:**

Создайте файл `docker-compose.production.yml`:

```yaml
name: employee-directory

services:
  postgres:
    image: postgres:16-alpine
    container_name: employee-directory-db
    environment:
      POSTGRES_DB: EmployeeDirectory
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: YOUR_STRONG_PASSWORD_HERE
    volumes:
      - postgres_data:/var/lib/postgresql/data
    restart: unless-stopped
    networks:
      - app-network

  web:
    build:
      context: ./EmployeeDirectory.Web/EmployeeDirectory
      dockerfile: Dockerfile
    container_name: employee-directory-web
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8080
      - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=EmployeeDirectory;Username=postgres;Password=YOUR_STRONG_PASSWORD_HERE
    ports:
      - "8080:8080"
    depends_on:
      postgres:
        condition: service_healthy
    restart: unless-stopped
    networks:
      - app-network

  nginx:
    image: nginx:alpine
    container_name: employee-directory-nginx
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf:ro
      - ./ssl:/etc/nginx/ssl:ro
    depends_on:
      - web
    restart: unless-stopped
    networks:
      - app-network

volumes:
  postgres_data:

networks:
  app-network:
    driver: bridge
```

5. **Создайте nginx.conf:**

```nginx
events {
    worker_connections 1024;
}

http {
    upstream app {
        server web:8080;
    }

    server {
        listen 80;
        server_name your-domain.com;

        location / {
            proxy_pass http://app;
            proxy_http_version 1.1;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection keep-alive;
            proxy_set_header Host $host;
            proxy_cache_bypass $http_upgrade;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }
    }
}
```