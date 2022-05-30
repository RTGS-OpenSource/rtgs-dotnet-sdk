using RTGS.DotNetSDK.IntegrationTests.Extensions;
using RTGS.DotNetSDK.IntegrationTests.HttpHandlers;
using RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.Verification.GivenOpenConnection;

public sealed class AndSignaturesAreNotValid : IDisposable, IClassFixture<GrpcServerFixture>
{
	private readonly GrpcServerFixture _grpcServer;
	private readonly ITestCorrelatorContext _serilogContext;

	private StatusCodeHttpHandler _idCryptServiceHttpHandler;
	private IHost _clientHost;
	private FromRtgsSender _fromRtgsSender;
	private IRtgsSubscriber _rtgsSubscriber;

	public AndSignaturesAreNotValid(GrpcServerFixture grpcServer)
	{
		_grpcServer = grpcServer;

		SetupSerilogLogger();

		SetupDependencies();

		_serilogContext = TestCorrelator.CreateContext();
	}

	private static void SetupSerilogLogger() =>
		Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Debug()
			.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
			.Enrich.FromLogContext()
			.WriteTo.Console()
			.WriteTo.TestCorrelator()
			.CreateLogger();

	private void SetupDependencies()
	{
		try
		{
			var rtgsSdkOptions = RtgsSdkOptions.Builder.CreateNew(
					TestData.ValidMessages.RtgsGlobalId,
					_grpcServer.ServerUri,
					new Uri("https://id-crypt-service"))
				.Build();

			_idCryptServiceHttpHandler = StatusCodeHttpHandlerBuilderFactory
				.Create()
				.WithOkResponse(VerifyMessageUnsuccessfully.HttpRequestResponseContext)
				.Build();

			_clientHost = Host.CreateDefaultBuilder()
				.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
				.ConfigureServices((_, services) => services
					.AddRtgsSubscriber(rtgsSdkOptions)
					.AddTestIdCryptServiceHttpClient(_idCryptServiceHttpHandler))
				.UseSerilog()
				.Build();

			_fromRtgsSender = _grpcServer.Services.GetRequiredService<FromRtgsSender>();
			_rtgsSubscriber = _clientHost.Services.GetRequiredService<IRtgsSubscriber>();
		}
		catch (Exception)
		{
			Dispose();

			throw;
		}
	}

	public void Dispose()
	{
		_clientHost?.Dispose();

		_grpcServer.Reset();
	}

	[Theory]
	[ClassData(typeof(SubscriberActionSignedMessagesData))]
	public async Task WhenVerifyingMessage_ThenLogError<TMessage>(SubscriberAction<TMessage> subscriberAction)
	{
		using var exceptionSignal = new ManualResetEventSlim();

		await _rtgsSubscriber.StartAsync(new AllTestHandlers());
		_rtgsSubscriber.OnExceptionOccurred += (_, _) => exceptionSignal.Set();

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message, subscriberAction.AdditionalHeaders);

		exceptionSignal.Wait();

		await _rtgsSubscriber.StopAsync();

		var errorLogs = _serilogContext.LogsFor($"RTGS.DotNetSDK.Subscriber.RtgsSubscriber", LogEventLevel.Error);

		errorLogs.Should().ContainSingle().Which.Should().BeEquivalentTo(new LogEntry(
			$"An error occurred while verifying a message (MessageIdentifier: {subscriberAction.MessageIdentifier})",
			LogEventLevel.Error,
			typeof(VerificationFailedException)));
	}

	[Theory]
	[ClassData(typeof(SubscriberActionSignedMessagesData))]
	public async Task WhenVerifyingMessage_ThenRaiseExceptionEvent<TMessage>(SubscriberAction<TMessage> subscriberAction)
	{
		using var exceptionSignal = new ManualResetEventSlim();

		Exception raisedException = null;

		await _rtgsSubscriber.StartAsync(new AllTestHandlers());
		_rtgsSubscriber.OnExceptionOccurred += (_, args) =>
		{
			raisedException = args.Exception;
			exceptionSignal.Set();
		};

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message, subscriberAction.AdditionalHeaders);

		exceptionSignal.Wait();

		await _rtgsSubscriber.StopAsync();

		raisedException.Should().BeOfType<VerificationFailedException>().Which.Message.Should().Be($"Verification of {subscriberAction.MessageIdentifier} message failed.");
	}

	[Theory]
	[ClassData(typeof(SubscriberActionSignedMessagesData))]
	public async Task WhenVerifyingMessageAndVerifierThrows_ThenLogError<TMessage>(SubscriberAction<TMessage> subscriberAction)
	{
		using var exceptionSignal = new ManualResetEventSlim();

		await _rtgsSubscriber.StartAsync(new AllTestHandlers());
		_rtgsSubscriber.OnExceptionOccurred += (_, _) => exceptionSignal.Set();

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message, subscriberAction.AdditionalHeaders);

		exceptionSignal.Wait();

		await _rtgsSubscriber.StopAsync();

		var errorLogs = _serilogContext.SubscriberLogs(LogEventLevel.Error);
		errorLogs.Should().ContainSingle().Which.Should().BeEquivalentTo(new LogEntry(
			$"An error occurred while verifying a message (MessageIdentifier: {subscriberAction.MessageIdentifier})",
			LogEventLevel.Error,
			typeof(VerificationFailedException)));
	}
}
