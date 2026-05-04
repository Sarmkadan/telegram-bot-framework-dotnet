# Deployment Guide

Complete guide for deploying the Telegram Bot Framework to production.

## Pre-Deployment Checklist

- [ ] Bot token obtained from @BotFather
- [ ] Configuration reviewed and set for production
- [ ] SSL certificate configured
- [ ] Logging configured and tested
- [ ] Backup strategy documented
- [ ] Monitoring configured
- [ ] Rate limits reviewed
- [ ] Session timeout appropriate

## Local Development

```bash
git clone https://github.com/Sarmkadan/telegram-bot-framework-dotnet.git
cd telegram-bot-framework-dotnet
dotnet restore
dotnet build
dotnet run
```

**Default Configuration**
- URL: `https://localhost:5001`
- Polling enabled
- In-memory storage
- Debug logging

## Docker Deployment

### Single Container

```bash
docker build -t telegram-bot:latest .
docker run -e TELEGRAM_BOT_TOKEN=your_token \
           -p 5001:5001 \
           --name bot \
           telegram-bot:latest
```

### Docker Compose

```bash
# Set environment variables
export TELEGRAM_BOT_TOKEN=your_token
export TELEGRAM_BOT_USERNAME=your_username

# Start services
docker-compose up -d

# View logs
docker-compose logs -f telegram-bot

# Stop services
docker-compose down
```

**Services Included**
- telegram-bot (main app)
- redis (caching)
- postgres (database - optional)

### Build Configuration

Production build:
```bash
docker build --target=runtime -t telegram-bot:latest .
```

Multi-stage build reduces image size from ~1GB to ~100MB.

## Cloud Deployments

### Azure App Service

```bash
# Create resource group
az group create -n telegram-bot -l eastus

# Create app service plan
az appservice plan create -n bot-plan -g telegram-bot --sku B2

# Create web app
az webapp create -n telegram-bot-prod \
                 -g telegram-bot \
                 -p bot-plan \
                 -r "DOTNETCORE|10.0"

# Set application settings
az webapp config appsettings set -g telegram-bot \
                                 -n telegram-bot-prod \
                                 --settings \
                                   TELEGRAM_BOT_TOKEN=your_token \
                                   ASPNETCORE_ENVIRONMENT=Production

# Deploy from git
az webapp deployment source config-zip \
  -g telegram-bot \
  -n telegram-bot-prod \
  --src ./publish.zip
```

### AWS Lambda

```bash
# Package function
dotnet publish -c Release -o ./bin/release/net10.0

# Create Lambda function
aws lambda create-function \
  --function-name telegram-bot \
  --runtime dotnet10.x \
  --handler TelegramBotFramework::TelegramBotFramework.LambdaEntryPoint::FunctionHandler \
  --zip-file fileb://function.zip
```

### Google Cloud Run

```bash
gcloud run deploy telegram-bot \
  --source . \
  --platform managed \
  --region us-central1 \
  --set-env-vars TELEGRAM_BOT_TOKEN=your_token
```

### DigitalOcean App Platform

```yaml
name: telegram-bot
services:
- name: bot
  github:
    branch: main
    repo: Sarmkadan/telegram-bot-framework-dotnet
  source_dir: src/TelegramBotFramework
  http_port: 5001
  envs:
  - key: TELEGRAM_BOT_TOKEN
    value: ${TELEGRAM_BOT_TOKEN}
```

## Kubernetes Deployment

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: telegram-bot
spec:
  replicas: 3
  selector:
    matchLabels:
      app: telegram-bot
  template:
    metadata:
      labels:
        app: telegram-bot
    spec:
      containers:
      - name: bot
        image: telegram-bot:v1.0.0
        ports:
        - containerPort: 5001
        env:
        - name: TELEGRAM_BOT_TOKEN
          valueFrom:
            secretKeyRef:
              name: bot-secrets
              key: token
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        resources:
          requests:
            memory: "128Mi"
            cpu: "100m"
          limits:
            memory: "256Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /api/bot/health
            port: 5001
          initialDelaySeconds: 30
          periodSeconds: 10
```

## Configuration for Production

Create `appsettings.Production.json`:

```json
{
  "BotConfiguration": {
    "BotToken": "${TELEGRAM_BOT_TOKEN}",
    "BotUsername": "${TELEGRAM_BOT_USERNAME}",
    "WebhookUrl": "https://your-domain.com/api/bot/webhook",
    "UseWebhook": true
  },
  "SessionConfiguration": {
    "SessionTimeoutMinutes": 60,
    "MaxActiveSessions": 10000,
    "SessionCleanupIntervalMinutes": 10
  },
  "RateLimitConfiguration": {
    "EnableRateLimiting": true,
    "DefaultLimitPerMinute": 30,
    "Strategy": "TokenBucket"
  },
  "CacheConfiguration": {
    "Provider": "DistributedCache",
    "DefaultExpirationMinutes": 60
  },
  "LoggingConfiguration": {
    "LogLevel": "Warning",
    "EnableFileOutput": true,
    "LogFilePath": "/var/log/telegram-bot/bot.log"
  }
}
```

## Setting Up Webhooks

### 1. DNS Configuration

Point your domain to your server:
```bash
# Add DNS A record
A: bot.example.com → 1.2.3.4
```

### 2. SSL Certificate

Using Let's Encrypt:
```bash
# Install Certbot
sudo apt install certbot python3-certbot-nginx

# Get certificate
sudo certbot certonly -d bot.example.com

# Copy to app
sudo cp /etc/letsencrypt/live/bot.example.com/fullchain.pem ./certs/
sudo cp /etc/letsencrypt/live/bot.example.com/privkey.pem ./certs/
```

### 3. Register Webhook with Telegram

```bash
curl -X POST https://api.telegram.org/bot<TOKEN>/setWebhook \
  -F url="https://bot.example.com/api/bot/webhook" \
  -F certificate=@/path/to/cert.pem
```

### 4. Verify Setup

```bash
curl -X GET https://api.telegram.org/bot<TOKEN>/getWebhookInfo
```

## Monitoring & Logging

### Application Insights (Azure)

```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

### ELK Stack (Elasticsearch, Logstash, Kibana)

Configure Serilog:
```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "Elasticsearch",
        "Args": {
          "nodeUris": "http://elasticsearch:9200"
        }
      }
    ]
  }
}
```

### Prometheus Metrics

Add Prometheus exporter:
```csharp
builder.Services.AddPrometheusMetrics();
app.MapPrometheusScrapingEndpoint();
```

## Scaling

### Vertical Scaling

Increase machine resources:
- CPU: 2 cores → 4 cores
- Memory: 4GB → 8GB

### Horizontal Scaling

Multiple instances with load balancer:

```
        Load Balancer
              │
    ┌─────────┼─────────┐
    ▼         ▼         ▼
  Bot-1    Bot-2    Bot-3
    │         │         │
    └─────────┼─────────┘
              │
        Shared Redis Cache
              │
        Shared Database
```

Configuration:
```bash
export CACHE_PROVIDER=DistributedCache
export REDIS_CONNECTION=redis://redis:6379
```

## Database Setup

### SQL Server

```sql
-- Create database
CREATE DATABASE TelegramBotDb;

-- Run migrations
dotnet ef database update --configuration Release
```

### PostgreSQL

```bash
# Create database
createdb telegram_bot

# Apply migrations
dotnet ef database update
```

## Backup Strategy

### Automated Backups

```bash
# Daily backup script
#!/bin/bash
BACKUP_DIR="/backups/telegram-bot"
DATE=$(date +%Y%m%d)

# Backup database
pg_dump telegram_bot > "$BACKUP_DIR/db_$DATE.sql"

# Backup application files
tar -czf "$BACKUP_DIR/app_$DATE.tar.gz" /app

# Upload to cloud storage
aws s3 cp "$BACKUP_DIR" s3://backups/telegram-bot --recursive
```

## Performance Tuning

### Connection Pool Size

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Min Pool Size=10;Max Pool Size=100;"
  }
}
```

### Cache Optimization

```json
{
  "CacheConfiguration": {
    "DefaultExpirationMinutes": 120,
    "MaxMemoryMB": 512
  }
}
```

### Rate Limit Tuning

```json
{
  "RateLimitConfiguration": {
    "DefaultLimitPerMinute": 60,
    "BurstCapacity": 10,
    "CleanupIntervalSeconds": 60
  }
}
```

## Troubleshooting

### Bot not receiving webhooks

Check webhook status:
```bash
curl https://api.telegram.org/bot<TOKEN>/getWebhookInfo
```

### High CPU usage

- Enable caching
- Increase connection pool
- Optimize database queries

### Memory leaks

- Monitor memory growth
- Check for event handler cleanup
- Verify session cleanup

### Database connection issues

- Check connection string
- Verify network access
- Review firewall rules
- Check database server status

## Rollback Procedure

```bash
# Keep previous version
docker tag telegram-bot:latest telegram-bot:previous

# Deploy new version
docker pull telegram-bot:v2.0.0
docker tag telegram-bot:v2.0.0 telegram-bot:latest
docker-compose up -d

# If issues, rollback
docker-compose stop
docker tag telegram-bot:previous telegram-bot:latest
docker-compose up -d
```

## Health Checks

Regular monitoring:

```bash
# Check bot health
curl https://bot.example.com/api/bot/health

# Check webhook status
curl https://api.telegram.org/bot<TOKEN>/getWebhookInfo

# Check database
curl https://bot.example.com/api/admin/config
```

## Support & Troubleshooting

- 📖 [Getting Started](getting-started.md)
- 🏗️ [Architecture](architecture.md)
- 📚 [API Reference](api-reference.md)
- 💬 [GitHub Issues](https://github.com/Sarmkadan/telegram-bot-framework-dotnet/issues)
