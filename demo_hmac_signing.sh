#!/bin/bash
# Demonstration script for HMAC callback data signing feature
# This script shows how to use the CallbackDataSigner class

set -e

echo "=========================================="
echo "HMAC Callback Data Signing Demo"
echo "=========================================="
echo ""

# Navigate to project directory
cd /home/redrocket/task-factory/workdir/telegram-bot-framework-dotnet

echo "1. Building the project..."
dotnet build src/TelegramBotFramework/TelegramBotFramework.csproj -q

echo "✓ Build successful"
echo ""

echo "2. Running CallbackDataSigner tests..."
dotnet test tests/telegram-bot-framework-dotnet.Tests/telegram-bot-framework-dotnet.Tests.csproj \
    --filter CallbackDataSignerTests \
    --nologo \
    -q

echo "✓ All CallbackDataSigner tests passed"
echo ""

echo "3. Example: How HMAC signing works"
echo "-----------------------------------"
echo ""

# Create a simple C# program to demonstrate
cat > /tmp/hmac_demo.cs << 'EOF'
using System;
using TelegramBotFramework.Utilities;

class Program
{
    static void Main()
    {
        Console.WriteLine("HMAC Callback Data Signing Demo");
        Console.WriteLine("==============================\n");

        // Secret key (should be stored securely in production)
        string secret = "my-secret-key-12345";

        // Original callback data
        string originalData = "delete_account:user123";

        Console.WriteLine($"Original data: {originalData}");
        Console.WriteLine($"Secret: {secret}");
        Console.WriteLine();

        // Sign the data
        string signedData = CallbackDataSigner.Sign(originalData, secret);
        Console.WriteLine($"Signed callback data: {signedData}");
        Console.WriteLine($"Total bytes: {System.Text.Encoding.UTF8.GetByteCount(signedData)}");
        Console.WriteLine();

        // Validate the signed data
        bool isValid = CallbackDataSigner.TryValidate(signedData, secret, out string extractedData);

        Console.WriteLine("Validation result:");
        Console.WriteLine($"  Valid: {isValid}");
        Console.WriteLine($"  Extracted data: {extractedData}");
        Console.WriteLine();

        // Test with wrong secret
        Console.WriteLine("Testing with wrong secret:");
        bool invalidResult = CallbackDataSigner.TryValidate(signedData, "wrong-secret", out _);
        Console.WriteLine($"  Valid with wrong secret: {invalidResult}");
        Console.WriteLine();

        // Test tampering
        Console.WriteLine("Testing tampered data:");
        string tampered = signedData.Replace("delete", "update");
        bool tamperedResult = CallbackDataSigner.TryValidate(tampered, secret, out _);
        Console.WriteLine($"  Valid tampered data: {tamperedResult}");
        Console.WriteLine();

        Console.WriteLine("✓ HMAC signing demonstration complete!");
    }
}
EOF

echo "Compiling demo program..."
dotnet new console -n HmacDemo -o /tmp/hmac_demo_proj > /dev/null 2>&1
cp /tmp/hmac_demo.cs /tmp/hmac_demo_proj/Program.cs
cd /tmp/hmac_demo_proj

# Add project reference
dotnet add reference /home/redrocket/task-factory/workdir/telegram-bot-framework-dotnet/src/TelegramBotFramework/TelegramBotFramework.csproj > /dev/null 2>&1

# Run the demo
dotnet run --configuration Release

cd /home/redrocket/task-factory/workdir/telegram-bot-framework-dotnet

echo ""
echo "=========================================="
echo "Demo completed successfully!"
echo "=========================================="
echo ""
echo "Summary:"
echo "  - CallbackDataSigner.Sign(data, secret) creates signed callback data"
echo "  - CallbackDataSigner.TryValidate(signed, secret, out data) validates and extracts"
echo "  - Uses HMAC-SHA256 with 8-byte truncated signature (16 hex chars)"
echo "  - Fits within Telegram's 64-byte callback data limit"
echo "  - Protects against forged callback queries"
echo ""
