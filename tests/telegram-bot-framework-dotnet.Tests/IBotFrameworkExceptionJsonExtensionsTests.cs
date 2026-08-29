namespace TelegramBotFramework.Tests;

public interface IBotFrameworkExceptionJsonExtensionsTests
{
    void ReturnsValidJsonString_WhenCalledWithValidException();
    void ReturnsIndentedJson_WhenIndentedParameterIsTrue();
    void ReturnsCompactJson_WhenIndentedParameterIsFalse();
    void ThrowsArgumentNullException_WhenValueIsNull();
    void SerializesErrorCodeProperty();
    void SerializesCommandExecutionException();
    void ReturnsNull_WhenJsonIsNull();
    void ReturnsNull_WhenJsonIsEmpty();
    void ReturnsNull_WhenJsonIsWhitespace();
    void ReturnsDeserializedException_WhenJsonIsValid();
    void ReturnsDeserializedException_WhenJsonHasCamelCaseProperties();
    void ReturnsNull_WhenJsonIsMalformed();
    void ReturnsNull_WhenJsonHasInvalidStructure();
    void ReturnsFalse_WhenJsonIsNull();
    void ThrowsArgumentNullException_WhenJsonIsNull();
    void ReturnsFalse_WhenJsonIsEmpty();
    void ReturnsFalse_WhenJsonIsWhitespace();
    void ReturnsTrueAndDeserializedException_WhenJsonIsValid();
    void ReturnsFalseAndNull_WhenJsonIsMalformed();
}
