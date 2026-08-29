#nullable enable

namespace TelegramBotFramework.Tests;

/// <summary>
/// Interface for unit tests of <see cref="CallbackDataSigner"/>.
/// </summary>
public interface ICallbackDataSignerTests
{
    void Sign_WithValidDataAndSecret_ReturnsSignedData();
    void Sign_WithNullData_ThrowsArgumentNullException();
    void Sign_WithNullSecret_ThrowsArgumentNullException();
    void Sign_WithEmptyData_ThrowsArgumentException();
    void Sign_WithEmptySecret_ThrowsArgumentException();
    void Sign_WithWhitespaceData_ThrowsArgumentException();
    void Sign_WithWhitespaceSecret_ThrowsArgumentException();
    void TryValidate_WithValidSignedData_ReturnsTrueAndExtractsOriginalData();
    void TryValidate_WithInvalidSecret_ReturnsFalse();
    void TryValidate_WithTamperedSignedData_ReturnsFalse();
    void TryValidate_WithNullSignedData_ReturnsFalse();
    void TryValidate_WithEmptySignedData_ReturnsFalse();
    void TryValidate_WithNullSecret_ReturnsFalse();
    void TryValidate_WithEmptySecret_ReturnsFalse();
    void TryValidate_WithMissingSeparator_ReturnsFalse();
    void TryValidate_WithSeparatorAtEnd_ReturnsFalse();
    void Sign_ProducesDifferentOutputForSameInputWithDifferentSecrets();
    void Sign_ProducesSameOutputForSameInputAndSecret();
    void TryValidate_WithLongData_FitsWithinTelegramLimit();
    void Sign_WithDataThatWouldExceedLimit_ThrowsArgumentException();
}