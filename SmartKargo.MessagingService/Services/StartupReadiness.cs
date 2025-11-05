// File: Services/StartupReadiness.cs
namespace SmartKargo.MessagingService.Services
{
    /// <summary>
    /// Signals when startup warmup is complete. Functions call WaitForReadyAsync before using ConfigCache.
    /// Implementation uses TaskCompletionSource so waiters don't block threadpool.
    /// </summary>
    public class StartupReadiness
    {
        private readonly TaskCompletionSource<bool> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>
        /// Called by the warmup function when initialization completes (success or final fallback).
        /// </summary>
        public void SignalReady() => _tcs.TrySetResult(true);

        /// <summary>
        /// Optional: signal failure (rare). Waiters will see exception.
        /// </summary>
        public void SignalFailed(Exception ex) => _tcs.TrySetException(ex);

        /// <summary>
        /// Wait for readiness. Throws OperationCanceledException if timeout elapses.
        /// </summary>
        public Task WaitForReadyAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            // Use Task with cancellation/timeout.
            var ct = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            ct.CancelAfter(timeout);

            return _tcs.Task.WaitAsync(ct.Token);
        }

        /// <summary>
        /// Non-blocking check.
        /// </summary>
        public bool IsReady => _tcs.Task.IsCompleted;
    }
}
