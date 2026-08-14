using System;
using System.Collections.Generic;
using TelegramBotFramework.Models;
using Xunit;

namespace TelegramBotFramework.Tests;

public class BotUserExtensionsTests
{
    #region GetDisplayName

    [Fact]
    public void GetDisplayName_ReturnsUsername_WhenUsernameIsPresent()
    {
        var user = new BotUser
        {
            Username = "cool_user",
            FirstName = "John",
            LastName = "Doe"
        };

        var result = user.GetDisplayName();

        Assert.Equal("cool_user", result);
    }

    [Fact]
    public void GetDisplayName_ReturnsFirstAndLastName_WhenUsernameMissing_AndLastNamePresent()
    {
        var user = new BotUser
        {
            Username = null,
            FirstName = "John",
            LastName = "Doe"
        };

        var result = user.GetDisplayName();

        Assert.Equal("John Doe", result);
    }

    [Fact]
    public void GetDisplayName_ReturnsFirstName_WhenUsernameAndLastNameMissing()
    {
        var user = new BotUser
        {
            Username = "   ",
            FirstName = "John",
            LastName = null
        };

        var result = user.GetDisplayName();

        Assert.Equal("John", result);
    }

    [Fact]
    public void GetDisplayName_ThrowsArgumentNullException_WhenUserIsNull()
    {
        BotUser? user = null;

        Assert.Throws<ArgumentNullException>(() => user!.GetDisplayName());
    }

    #endregion

    #region IsActive

    [Fact]
    public void IsActive_ReturnsTrue_WhenLastActivityWithinThreshold()
    {
        var user = new BotUser
        {
            LastActivityAt = DateTime.UtcNow.AddMinutes(-5)
        };

        var result = user.IsActive(TimeSpan.FromMinutes(10));

        Assert.True(result);
    }

    [Fact]
    public void IsActive_ReturnsFalse_WhenLastActivityOutsideThreshold()
    {
        var user = new BotUser
        {
            LastActivityAt = DateTime.UtcNow.AddHours(-2)
        };

        var result = user.IsActive(TimeSpan.FromMinutes(30));

        Assert.False(result);
    }

    [Fact]
    public void IsActive_ReturnsFalse_WhenLastActivityIsNull()
    {
        var user = new BotUser
        {
            LastActivityAt = null
        };

        var result = user.IsActive(TimeSpan.FromMinutes(10));

        Assert.False(result);
    }

    [Fact]
    public void IsActive_ThrowsArgumentNullException_WhenUserIsNull()
    {
        BotUser? user = null;

        Assert.Throws<ArgumentNullException>(() => user!.IsActive(TimeSpan.FromMinutes(10)));
    }

    #endregion

    #region GetMetadataValue

    [Fact]
    public void GetMetadataValue_ReturnsValue_WhenKeyExists()
    {
        var user = new BotUser
        {
            Metadata = new Dictionary<string, string>
            {
                ["role"] = "admin"
            }
        };

        var result = user.GetMetadataValue("role");

        Assert.Equal("admin", result);
    }

    [Fact]
    public void GetMetadataValue_ReturnsNull_WhenKeyDoesNotExist()
    {
        var user = new BotUser
        {
            Metadata = new Dictionary<string, string>
            {
                ["foo"] = "bar"
            }
        };

        var result = user.GetMetadataValue("missing");

        Assert.Null(result);
    }

    [Fact]
    public void GetMetadataValue_ReturnsNull_WhenMetadataDictionaryIsNull()
    {
        var user = new BotUser
        {
            Metadata = null
        };

        var result = user.GetMetadataValue("any");

        Assert.Null(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GetMetadataValue_ThrowsArgumentException_WhenKeyIsNullOrEmpty(string key)
    {
        var user = new BotUser
        {
            Metadata = new Dictionary<string, string>()
        };

        Assert.Throws<ArgumentException>(() => user.GetMetadataValue(key!));
    }

    [Fact]
    public void GetMetadataValue_ThrowsArgumentNullException_WhenUserIsNull()
    {
        BotUser? user = null;

        Assert.Throws<ArgumentNullException>(() => user!.GetMetadataValue("any"));
    }

    #endregion
}
