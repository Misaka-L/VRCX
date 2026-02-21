using Polly;
using Polly.Retry;
using Serilog;

namespace VRCX.Core.Services.WebApi;

internal sealed class ResilienceHttpHandler : DelegatingHandler
{
    private readonly ILogger _logger = Log.ForContext<ResilienceHttpHandler>();

    private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;

    public ResilienceHttpHandler(HttpMessageHandler innerHandler) : base(innerHandler)
    {
        var options = new RetryStrategyOptions<HttpResponseMessage>
        {
            ShouldHandle = args =>
                ValueTask.FromResult(
                    HttpClientResiliencePredicates.IsTransient(args.Outcome, args.Context.CancellationToken)),
            MaxRetryAttempts = 5,
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            Delay = TimeSpan.FromSeconds(3),
            OnRetry = arguments =>
            {
                _logger.Warning(
                    arguments.Outcome.Exception,
                    "Retrying HTTP request. Attempt {AttemptNumber}. Delay {RetryDelay}.",
                    arguments.AttemptNumber, arguments.RetryDelay.ToString());

                return default;
            }
        };

        var builder = new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(options);

        _pipeline = builder.Build();
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return await _pipeline.ExecuteAsync(
                async token => await base.SendAsync(request, token),
                cancellationToken)
            .AsTask();
    }
}