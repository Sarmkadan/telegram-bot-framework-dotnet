# Frequently Asked Questions

## Installation & Setup

### Q: What are the system requirements?

**A:** 
- .NET 10 SDK or later
- 512MB RAM minimum (1GB+ recommended for production)
- Internet connection for Telegram API
- Modern CPU (any modern processor works)

### Q: How do I get a bot token?

**A:**
1. Open Telegram
2. Search for **@BotFather**
3. Send `/newbot` command
4. Follow prompts to name your bot
5. Copy the token provided

Token looks like: `123456789:ABCDefGHiJKlmnoPQRstuvWXYZ1234567`

### Q: Can I use the same token for multiple bots?

**A:** No, each bot needs its own unique token from @BotFather. However, you can clone the repository and run multiple instances with different tokens.

### Q: What about .NET 9 or .NET 8 compatibility?

**A:** The framework targets .NET 10 specifically. For older versions, you would need to:
1. Change `TargetFramework` in `.csproj` from `net10.0` to `net9.0` or `net8.0`
2. Downgrade NuGet packages to compatible versions
3. Test thoroughly as some APIs may differ

We recommend upgrading to .NET 10 for latest features and performance improvements.

---

## Configuration

### Q: Should I use webhook or polling?

**A:**
| Aspect | Polling | Webhook |
|--------|---------|---------|
| Setup | Simple | More complex (needs domain/SSL) |
| Latency | 1-2 seconds | Instant (<100ms) |
| Resource Usage | Higher CPU/Network | Lower |
| Scalability | ≤100 users | 1000+ users |
| Cloud Friendly | Affordable | Requires public URL |

**Recommendation:** Use polling for development/small bots, webhook for production/large bots.

### Q: How do I set up webhooks?

**A:**
1. Get HTTPS domain with SSL certificate
2. Configure in `appsettings.json`:
   ```json
   {
     "BotConfiguration": {
       "UseWebhook": true,
       "WebhookUrl": "https://your-domain.com/api/bot/webhook"
     }
   }
   ```
3. Register webhook with Telegram:
   ```bash
   curl -X POST https://api.telegram.org/bot<TOKEN>/setWebhook \
     -F url="https://your-domain.com/api/bot/webhook"
   ```
4. Verify: `curl https://api.telegram.org/bot<TOKEN>/getWebhookInfo`

### Q: What's the difference between local and distributed cache?

**A:**
| Aspect | Local | Distributed |
|--------|-------|-------------|
| Storage | In-process memory | Redis/Memcached |
| Instances | Single instance | Multiple instances |
| Persistence | Cleared on restart | Survives restarts |
| Cost | Free | Paid service |
| Latency | Fastest | Slight network latency |

**Recommendation:** Local for development, distributed for production.

### Q: How do I change the rate limit?

**A:**
In `appsettings.json`:
```json
{
  "RateLimitConfiguration": {
    "DefaultLimitPerMinute": 60,
    "Strategy": "TokenBucket",
    "BurstCapacity": 10
  }
}
```

Per-command limits override the default in the Command configuration.

---

## Development

### Q: How do I create a custom command?

**A:**
```csharp
var command = new Command
{
    Name = "/mycommand",
    Description = "My custom command",
    HandlerType = "MyCommandHandler",
    Type = CommandType.Standard,
    IsEnabled = true,
    RequiresAdmin = false,
    RateLimitPerMinute = 30
};

await commandService.RegisterCommandAsync(command);
```

Then implement the handler in your application logic.

### Q: How do I handle user input?

**A:**
```csharp
var session = await sessionService.GetSessionAsync(userId);
session.SetContextData("user_input", userMessage);
await sessionService.UpdateSessionAsync(session);

// Later retrieve
var input = session.GetContextData("user_input");
```

Use `UserSession.ContextData` dictionary to store conversation state.

### Q: How do I access user information?

**A:**
```csharp
var userService = serviceProvider.GetRequiredService<IUserService>();

// Get user
var user = await userService.GetUserByIdAsync(userId);

// Or by Telegram ID
var user = await userService.GetUserByTelegramIdAsync(telegramId);

// Update user
user.Username = "newusername";
await userService.UpdateUserAsync(user);
```

### Q: Can I use a database instead of in-memory storage?

**A:** Yes! Phase 2+ will support:
- SQL Server
- PostgreSQL
- MongoDB

For now, implement `IRepository` interface with your database:
```csharp
public class SqlServerRepository : IRepository
{
    // Implement methods with database calls
}
```

### Q: How do I test my bot locally?

**A:**
1. Use polling mode (default)
2. Run locally: `dotnet run`
3. Open Telegram and message your bot
4. Messages processed immediately

No webhook setup needed for local testing.

---

## Deployment

### Q: What's the best hosting option?

**A:**
| Option | Cost | Difficulty | Scalability |
|--------|------|------------|-------------|
| VPS (DigitalOcean) | $4-12/month | Easy | Good |
| Docker | $5+/month | Medium | Excellent |
| Azure App Service | $10-100/month | Easy | Excellent |
| AWS Lambda | Pay per request | Hard | Excellent |
| Kubernetes | $50+/month | Hard | Excellent |

**Recommendation:** Start with Docker on a $5 VPS, scale to Kubernetes as needed.

### Q: How do I deploy with Docker?

**A:**
```bash
docker build -t my-bot:latest .
docker run -e TELEGRAM_BOT_TOKEN=your_token \
           -p 5001:5001 \
           my-bot:latest
```

Or use docker-compose for production setup with Redis.

### Q: How often should I back up data?

**A:**
- Daily for production
- Every 6 hours during active development
- After any major configuration change

Use automated backup scripts with cloud storage (S3, Azure Blob).

### Q: Can I run multiple instances?

**A:** Yes! With distributed cache:
1. Set up shared Redis instance
2. Configure all instances to use it
3. Use load balancer frontend
4. Instances share session/user data

---

## Performance & Optimization

### Q: Why is my bot slow?

**A:** Common causes:
1. **No caching** - Enable LocalCache or Redis
2. **Database queries** - Add indexes, use pagination
3. **Rate limiting** - Check if you're hitting limits
4. **Network latency** - Use webhook instead of polling
5. **Memory pressure** - Monitor with `dotnet-trace`

### Q: How many users can my bot handle?

**A:**
- **Single instance**: 100-500 users
- **Multiple instances**: 500-100,000+ users
- Depends on message frequency and processing logic

### Q: How do I monitor performance?

**A:**
- Health endpoint: `GET /api/bot/health`
- Statistics: `GET /api/admin/statistics`
- Application Insights (Azure)
- Prometheus + Grafana
- ELK stack for logging

### Q: Should I use async/await?

**A:** **Always!** All framework methods are async:
```csharp
var user = await userService.GetUserAsync(userId);
var message = await messageService.ProcessAsync(msg);
```

Synchronous calls block threads and hurt scalability.

---

## Troubleshooting

### Q: Bot not receiving messages

**A:** Check:
1. Telegram token is correct
2. Bot is running (`dotnet run`)
3. Polling is enabled (for local testing)
4. No errors in logs
5. User hasn't blocked the bot

### Q: "Invalid bot token" error

**A:**
1. Copy token directly from @BotFather (no spaces)
2. Set in environment: `export TELEGRAM_BOT_TOKEN=...`
3. Or in appsettings.json
4. Verify token hasn't been revoked

### Q: Rate limiting my own bot

**A:**
1. Check `RateLimitConfiguration` settings
2. Increase `DefaultLimitPerMinute`
3. Disable for development: `"EnableRateLimiting": false`
4. Set per-command limits appropriately

### Q: Port already in use

**A:**
```bash
# Linux/Mac - find and kill process
lsof -i :5001
kill -9 <PID>

# Windows
netstat -ano | findstr :5001
taskkill /PID <PID> /F

# Or change port in launchSettings.json
```

### Q: Out of memory errors

**A:**
1. Increase available memory (deploy with more RAM)
2. Reduce `MaxActiveSessions`
3. Lower cache `DefaultExpirationMinutes`
4. Enable session cleanup

### Q: Webhook not receiving updates

**A:**
1. Verify domain/SSL is working: `curl https://your-domain.com`
2. Check webhook registered: `curl https://api.telegram.org/bot<TOKEN>/getWebhookInfo`
3. Verify port 443 is open (not 8080, 5001, etc.)
4. Check application logs for errors
5. Telegram might rate limit - add delays between requests

### Q: Database connection issues

**A:**
1. Verify connection string in config
2. Check database server is running
3. Verify username/password
4. Check firewall allows connection
5. Test with: `dotnet user-secrets` for credentials

---

## Security

### Q: How do I secure my bot token?

**A:**
1. **Never** commit token to git
2. Use environment variables:
   ```bash
   export TELEGRAM_BOT_TOKEN=...
   ```
3. Or use .NET User Secrets:
   ```bash
   dotnet user-secrets set "BotConfiguration:BotToken" "..."
   ```
4. For production, use cloud secret management (Azure Key Vault, AWS Secrets Manager)

### Q: How do I validate webhook signatures?

**A:** Framework validates automatically via HMAC-SHA256. The WebhookHandler verifies authenticity before processing.

### Q: Should I rate limit my bot?

**A:** **Yes!** Always enable rate limiting in production to prevent abuse and resource exhaustion.

### Q: How do I protect sensitive user data?

**A:**
1. Don't log sensitive information
2. Hash passwords with PBKDF2-SHA256
3. Use encrypted channels
4. Follow GDPR/privacy regulations
5. Implement user data deletion

### Q: Can users see my bot token?

**A:** **No.** The token is server-side only. Users can't access it through the API. Never expose it in client-side code or public repositories.

---

## Contributing & Support

### Q: How can I contribute?

**A:** See [CONTRIBUTING.md](../CONTRIBUTING.md) for guidelines. We welcome:
- Bug reports and fixes
- Feature requests and implementations
- Documentation improvements
- Example bots
- Performance optimizations

### Q: Where can I get help?

**A:**
- 📖 [README](../README.md)
- 📚 [Documentation](.)
- 💬 [GitHub Issues](https://github.com/Sarmkadan/telegram-bot-framework-dotnet/issues)
- 📧 Email: rutova2@gmail.com
- 🌐 Website: https://sarmkadan.com

### Q: How do I report a bug?

**A:** 
1. Search [existing issues](https://github.com/Sarmkadan/telegram-bot-framework-dotnet/issues)
2. Create new issue with:
   - Steps to reproduce
   - Expected vs actual behavior
   - Environment (.NET version, OS)
   - Error logs
   - Minimal code example

### Q: What's the roadmap?

**A:**
- **Phase 1** ✅ Core features (done)
- **Phase 2** ✅ Infrastructure (done)
- **Phase 3** 🚀 Docs & examples (current)
- **Phase 4+** Database adapters, plugin system, advanced features

---

## License & Legal

### Q: Can I use this commercially?

**A:** Yes! MIT license allows commercial use. See [LICENSE](../LICENSE).

### Q: Do I need to attribute the framework?

**A:** Not required by license, but appreciated! Link to the GitHub repo or website.

### Q: Can I modify and redistribute?

**A:** Yes, as long as you include the original license and copyright notice.

### Q: Is there a warranty?

**A:** No, provided "as-is" without warranty. See license for full terms.

---

## More Questions?

Can't find the answer?
- 📧 Email: rutova2@gmail.com
- 💬 Open an issue on GitHub
- 🌐 Visit: https://sarmkadan.com
