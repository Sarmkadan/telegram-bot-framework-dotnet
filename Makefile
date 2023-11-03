.PHONY: help restore build test clean run docker-build docker-up docker-down publish format lint

help:
	@echo "Telegram Bot Framework - Build Commands"
	@echo "========================================"
	@echo "make restore       - Restore NuGet packages"
	@echo "make build         - Build the project"
	@echo "make test          - Run tests"
	@echo "make clean         - Clean build artifacts"
	@echo "make run           - Run the application"
	@echo "make publish       - Publish release build"
	@echo "make format        - Format code style"
	@echo "make lint          - Run code analysis"
	@echo "make docker-build  - Build Docker image"
	@echo "make docker-up     - Start Docker containers"
	@echo "make docker-down   - Stop Docker containers"
	@echo "make docker-logs   - View Docker logs"

restore:
	@echo "Restoring NuGet packages..."
	dotnet restore

build: restore
	@echo "Building project..."
	dotnet build --configuration Release

test: build
	@echo "Running tests..."
	dotnet test --configuration Release --verbosity normal

clean:
	@echo "Cleaning build artifacts..."
	dotnet clean
	rm -rf bin/ obj/ publish/

run: build
	@echo "Starting application..."
	cd src/TelegramBotFramework && dotnet run

publish: clean
	@echo "Publishing release build..."
	dotnet publish -c Release -o ./publish

format:
	@echo "Formatting code..."
	dotnet format

lint:
	@echo "Running code analysis..."
	dotnet build --no-restore /p:EnforceCodeStyleInBuild=true

docker-build: publish
	@echo "Building Docker image..."
	docker build -t telegram-bot:latest .
	docker build -t telegram-bot:$(shell date +%Y%m%d) .

docker-up:
	@echo "Starting Docker containers..."
	docker-compose up -d

docker-down:
	@echo "Stopping Docker containers..."
	docker-compose down

docker-logs:
	@echo "Viewing Docker logs..."
	docker-compose logs -f telegram-bot

docker-clean:
	@echo "Removing Docker containers and images..."
	docker-compose down -v
	docker rmi telegram-bot:latest

all: clean restore build test publish
	@echo "Build complete!"
