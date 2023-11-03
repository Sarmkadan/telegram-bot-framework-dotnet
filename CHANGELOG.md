# Changelog

All notable changes to the Telegram Bot Framework for .NET are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.0] - 2026-05-04

### Added
- Comprehensive documentation suite (getting-started.md, architecture.md, api-reference.md, deployment.md, faq.md)
- 5 complete example applications demonstrating core features
- Docker support with multi-stage builds and health checks
- Docker Compose orchestration with Redis and PostgreSQL
- CI/CD pipeline with GitHub Actions
- Makefile for build automation
- .editorconfig for consistent code style
- API endpoint for cache statistics
- Webhook signature validation (HMAC-SHA256)
- Support for bulk user operations
- Request correlation IDs for distributed tracing

### Changed
- Improved error messages with more context
- Enhanced logging with structured fields
- Optimized cache expiration handling
- Better rate limiting precision with sliding window
- Updated documentation with production examples

### Fixed
- Memory leak in event subscriber cleanup
- Race condition in session expiration
- Incorrect cache TTL calculation
- WebSocket disconnect handling in webhook mode

### Security
- Added input validation for all API endpoints
- Implemented request size limits
- Enhanced password hashing with PBKDF2-SHA256
- API key rotation support

## [1.1.0] - 2026-04-15

### Added
- Event bus with pub-sub pattern for decoupled communication
- Background task worker for long-running operations
- Scheduled task manager for recurring tasks
- Distributed cache provider abstraction
- Multiple rate limiting strategies (TokenBucket, SlidingWindow, FixedWindow)
- Message formatting support (JSON, CSV, XML)
- Webhook handler for update processing
- PollingStrategy for alternative update fetching
- HttpClientFactory for connection pooling
- Session context data storage

### Changed
- Refactored middleware pipeline for better composition
- Improved CommandService with parameter validation
- Enhanced UserService with bulk operations
- Optimized repository access patterns

### Fixed
- Session timeout not properly enforced
- Menu button rendering order
- Command parameter parsing with special characters

## [1.0.0] - 2026-03-01

### Added
- Core bot framework with command system
- User management with role-based access control (User, Moderator, Admin, Owner)
- Interactive menu system with inline keyboards
- Session management with automatic timeout
- State machine for complex user flows
- Middleware pipeline (Logging, Error Handling, Authentication, RateLimiting, Validation)
- REST API endpoints for bot management
- In-memory repository for data storage
- User ban/suspend functionality
- Message processing pipeline with lifecycle tracking
- Telegram API client integration
- Local caching provider with TTL
- Configuration management via appsettings.json
- Dependency injection setup
- Comprehensive error handling
- Extension methods and utilities
- Built-in constants and helpers

### Documentation
- README with features and basic usage
- CONTRIBUTING guidelines
- LICENSE (MIT)

---

## Release Dates & Version Status

| Version | Status | Released | Notes |
|---------|--------|----------|-------|
| 0.1.0 | Alpha | 2026-02-01 | Initial development |
| 0.2.0 | Alpha | 2026-02-15 | Core features complete |
| 0.3.0 | Beta | 2026-03-01 | First stable release (1.0.0) |
| 1.0.0 | Stable | 2026-03-01 | Production ready |
| 1.1.0 | Stable | 2026-04-15 | Infrastructure & events |
| 1.2.0 | Latest | 2026-05-04 | Documentation & examples |

---

## Upgrading Guide

### From 0.x to 1.0.0
- Update all NuGet packages
- Namespace changes: Full compatibility maintained
- Configuration: Update appsettings.json structure
- API: Most breaking changes in internal implementation only

### From 1.0.0 to 1.1.0
- EventBus now available - optional, non-breaking
- New caching options - can migrate gradually
- Middleware pipeline extended - custom middleware compatible

### From 1.1.0 to 1.2.0
- Full backward compatibility
- New documentation doesn't affect code
- New examples for reference only

---

## Future Roadmap

### Version 2.0.0 (Planned Q3 2026)
- Database persistence (EF Core)
- SQL Server support
- PostgreSQL support
- MongoDB support
- Plugin system architecture
- Advanced state machine patterns
- Message templating engine

### Version 2.1.0 (Planned Q4 2026)
- GraphQL API layer
- WebSocket support for real-time updates
- Message queue integration (RabbitMQ, Azure Service Bus)
- Distributed tracing (OpenTelemetry, Jaeger)
- Metrics export (Prometheus, Grafana)

### Version 3.0.0 (Planned 2027)
- Multi-language support
- Advanced AI integration (NLP, sentiment analysis)
- Payment integration (Stripe, PayPal)
- File storage backends (S3, Azure Blob)
- Custom command middleware system
- Performance optimizations (caching strategies)

---

## Known Issues

### Version 1.2.0
- [None currently] - Please report issues on GitHub

### Version 1.1.0
- EventBus may accumulate subscribers if not unsubscribed properly
- Scheduled tasks may drift slightly in timing under high load

### Version 1.0.0
- In-memory storage lost on restart
- Session cleanup not automatic (manual call required)

---

## Contributors

Special thanks to:
- [Vladyslav Zaiets](https://github.com/Sarmkadan) - Creator & Maintainer
- Community contributors and issue reporters

---

## Support & Contact

- 📖 [Documentation](docs/)
- 🐛 [Report Issues](https://github.com/Sarmkadan/telegram-bot-framework-dotnet/issues)
- 💬 [GitHub Discussions](https://github.com/Sarmkadan/telegram-bot-framework-dotnet/discussions)
- 📧 Email: rutova2@gmail.com
- 🌐 Website: https://sarmkadan.com

---

## License

MIT License - See [LICENSE](LICENSE) file for details.

Copyright (c) 2026 Vladyslav Zaiets
