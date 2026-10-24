namespace TelegramBotFramework.Integration;

using System;
using System.Globalization;
using System.Threading.Tasks;
using TelegramBotFramework.Events;

/// <summary>
/// Circuit breaker that stops calls to the Telegram Bot API after repeated failures.
/// </summary>
/// <remarks>
/// <para>
/// The circuit opens after <see cref="CircuitBreakerOptions.FailureThreshold"/> consecutive failures. While open,
/// calls are rejected without reaching the network. Once <see cref="CircuitBreakerOptions.OpenDuration"/> has elapsed,
/// a single probe call is admitted (half-open): its success closes the circuit and its failure re-opens it.
/// </para>
/// <para>
/// Every transition publishes a <see cref="BotStateChangedEvent"/> through the supplied <see cref="IEventPublisher"/>
/// so hosts can alert on degradation and recovery. Polling consults <see cref="GetPauseDuration"/> to sleep instead of
/// spinning while the circuit is open.
/// </para>
/// </remarks>
public sealed class TelegramCircuitBreaker
{
    private readonly object _gate = new();
    private readonly CircuitBreakerOptions _options;
    private readonly IEventPublisher? _eventPublisher;
    private readonly ILogger<TelegramCircuitBreaker> _logger;
    private CircuitState _state = CircuitState.Closed;
    private int _consecutiveFailures;
    private DateTimeOffset _openedAt;
    private bool _probeInFlight;

    /// <summary>
    /// Initializes a new instance of the <see cref="TelegramCircuitBreaker"/> class.
    /// </summary>
    /// <param name="options">Thresholds and timing for the circuit.</param>
    /// <param name="eventPublisher">Optional publisher used to emit <see cref="BotStateChangedEvent"/> on each transition.</param>
    /// <param name="logger">Optional logger for diagnostic messages.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="options"/> contains invalid values.</exception>
    public TelegramCircuitBreaker(
        CircuitBreakerOptions options,
        IEventPublisher? eventPublisher = null,
        ILogger<TelegramCircuitBreaker>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Validate();
        _options = options;
        _eventPublisher = eventPublisher;
        _logger = logger ?? new ConsoleLogger<TelegramCircuitBreaker>();
    }

    /// <summary>
    /// Gets the current state of the circuit.
    /// </summary>
    public CircuitState State
    {
        get
        {
            lock (_gate)
            {
                return _state;
            }
        }
    }

    /// <summary>
    /// Gets how long a caller should wait before attempting another call.
    /// </summary>
    /// <returns>
    /// <see cref="TimeSpan.Zero"/> when a call may proceed now; otherwise the remaining open time, or a short wait while
    /// a half-open probe is still in flight. This method does not change the circuit state.
    /// </returns>
    public TimeSpan GetPauseDuration()
    {
        lock (_gate)
        {
            return _state switch
            {
                CircuitState.Open => RemainingOpenDuration(),
                CircuitState.HalfOpen when _probeInFlight => TelegramCircuitBreakerConstants.ProbeInFlightWaitDuration,
                _ => TimeSpan.Zero
            };
        }
    }

    /// <summary>
    /// Asks permission to make a Telegram API call. Must be paired with <see cref="RecordSuccessAsync"/> or
    /// <see cref="RecordFailureAsync"/> when the call was admitted.
    /// </summary>
    /// <returns><see langword="true"/> if the call may proceed; <see langword="false"/> if the circuit rejects it.</returns>
    public async Task<bool> TryAcquireAsync()
    {
        var allowed = TryAcquireCore(out var transition);
        await PublishTransitionAsync(transition).ConfigureAwait(false);
        return allowed;
    }

    /// <summary>
    /// Records that an admitted Telegram API call succeeded.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task RecordSuccessAsync()
    {
        var transition = RecordSuccessCore();
        await PublishTransitionAsync(transition).ConfigureAwait(false);
    }

    /// <summary>
    /// Records that an admitted Telegram API call failed due to a transient problem (network error, timeout,
    /// HTTP 429 or HTTP 5xx).
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task RecordFailureAsync()
    {
        var transition = RecordFailureCore();
        await PublishTransitionAsync(transition).ConfigureAwait(false);
    }

    /// <summary>
    /// Records that an admitted call ended without a verdict on the API's health, for example because the caller
    /// cancelled it. Frees a half-open probe slot so another call can probe, without changing the circuit state.
    /// </summary>
    public void RecordAbandoned()
    {
        lock (_gate)
        {
            _probeInFlight = false;
        }
    }

    private TimeSpan RemainingOpenDuration()
    {
        var remaining = _openedAt + _options.OpenDuration - DateTimeOffset.UtcNow;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    private bool TryAcquireCore(out StateTransition? transition)
    {
        lock (_gate)
        {
            transition = null;

            switch (_state)
            {
                case CircuitState.Closed:
                    return true;

                case CircuitState.Open when DateTimeOffset.UtcNow - _openedAt >= _options.OpenDuration:
                    transition = ChangeState(CircuitState.HalfOpen, TelegramCircuitBreakerConstants.OpenDurationElapsedReason);
                    _probeInFlight = true;
                    return true;

                case CircuitState.HalfOpen when !_probeInFlight:
                    _probeInFlight = true;
                    return true;

                default:
                    return false;
            }
        }
    }

    private StateTransition? RecordSuccessCore()
    {
        lock (_gate)
        {
            _consecutiveFailures = 0;

            if (_state != CircuitState.HalfOpen)
            {
                return null;
            }

            _probeInFlight = false;
            return ChangeState(CircuitState.Closed, TelegramCircuitBreakerConstants.ProbeSucceededReason);
        }
    }

    private StateTransition? RecordFailureCore()
    {
        lock (_gate)
        {
            switch (_state)
            {
                case CircuitState.Closed:
                    _consecutiveFailures++;

                    if (_consecutiveFailures < _options.FailureThreshold)
                    {
                        return null;
                    }

                    _openedAt = DateTimeOffset.UtcNow;
                    return ChangeState(
                        CircuitState.Open,
                        string.Format(
                            CultureInfo.InvariantCulture,
                            TelegramCircuitBreakerConstants.ConsecutiveFailuresReasonFormat,
                            _consecutiveFailures));

                case CircuitState.HalfOpen:
                    _probeInFlight = false;
                    _openedAt = DateTimeOffset.UtcNow;
                    return ChangeState(CircuitState.Open, TelegramCircuitBreakerConstants.ProbeFailedReason);

                default:
                    // Failures reported by calls admitted before the circuit opened do not change its state.
                    return null;
            }
        }
    }

    private StateTransition ChangeState(CircuitState next, string reason)
    {
        var transition = new StateTransition(_state, next, reason);
        _state = next;
        return transition;
    }

    private async Task PublishTransitionAsync(StateTransition? transition)
    {
        if (transition is null)
        {
            return;
        }

        if (transition.To == CircuitState.Open)
        {
            _logger.LogWarning(
                "Telegram API circuit changed from {PreviousState} to {NewState}: {Reason}",
                transition.From,
                transition.To,
                transition.Reason);
        }
        else
        {
            _logger.LogInformation(
                "Telegram API circuit changed from {PreviousState} to {NewState}: {Reason}",
                transition.From,
                transition.To,
                transition.Reason);
        }

        if (_eventPublisher is null)
        {
            return;
        }

        try
        {
            await _eventPublisher
                .PublishBotStateChangedAsync(transition.From.ToString(), transition.To.ToString(), transition.Reason)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Event delivery must not turn a circuit decision into an API failure.
            _logger.LogError(ex, "Failed to publish Telegram API circuit state change to {NewState}", transition.To);
        }
    }

    private sealed record StateTransition(CircuitState From, CircuitState To, string Reason);
}
