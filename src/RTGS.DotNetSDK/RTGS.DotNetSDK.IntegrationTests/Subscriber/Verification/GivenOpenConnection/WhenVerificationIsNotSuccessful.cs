﻿using RTGS.DotNetSDK.IntegrationTests.Extensions;
using RTGS.DotNetSDK.IntegrationTests.HttpHandlers;
using RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;

namespace RTGS.DotNetSDK.IntegrationTests.Subscriber.Verification.GivenOpenConnection;

public class WhenVerificationIsNotSuccessful : IDisposable, IClassFixture<GrpcServerFixture>
{
	private static readonly TimeSpan WaitForExceptionEventDuration = TimeSpan.FromMilliseconds(100);
	private static readonly TimeSpan WaitForReceivedMessageDuration = TimeSpan.FromMilliseconds(500);
	private static readonly TimeSpan WaitForAcknowledgementsDuration = TimeSpan.FromMilliseconds(100);

	private readonly GrpcServerFixture _grpcServer;
	private readonly ITestCorrelatorContext _serilogContext;
	private IHost _clientHost;
	private FromRtgsSender _fromRtgsSender;
	private IRtgsSubscriber _rtgsSubscriber;


	public WhenVerificationIsNotSuccessful(GrpcServerFixture grpcServer)
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
					TestData.ValidMessages.BankDid,
					_grpcServer.ServerUri,
					new Uri("http://id-crypt-cloud-agent-api.com"),
					"id-crypt-api-key",
					new Uri("http://id-crypt-cloud-agent-service-endpoint.com"))
				.Build();

			var idCryptMessageHandler = StatusCodeHttpHandlerBuilderFactory
				.Create()
				.WithOkResponse(GetActiveConnectionWithAlias.HttpRequestResponseContext)
				.WithOkResponse(VerifyPrivateSignatureUnsuccessfully.HttpRequestResponseContext)
				.Build();

			_clientHost = Host.CreateDefaultBuilder()
				.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
				.ConfigureServices((_, services) => services
					.AddRtgsSubscriber(rtgsSdkOptions)
					.AddTestIdCryptHttpClient(idCryptMessageHandler))
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
	public async Task ThenLogError<TMessage>(SubscriberAction<TMessage> subscriberAction)
	{
		await _rtgsSubscriber.StartAsync(subscriberAction.AllTestHandlers);

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message, subscriberAction.AdditionalHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		subscriberAction.Handler.WaitForMessage(WaitForReceivedMessageDuration);

		await _rtgsSubscriber.StopAsync();

		_serilogContext.LogsFor($"RTGS.DotNetSDK.Subscriber.IdCrypt.Verification.{subscriberAction.MessageIdentifier}MessageVerifier", LogEventLevel.Error)
			.Should().ContainSingle().Which.Should().BeEquivalentTo(
				new LogEntry($"Verification of {subscriberAction.MessageIdentifier} message private signature failed", LogEventLevel.Error, typeof(RtgsSubscriberException)));
	}

	[Theory]
	[ClassData(typeof(SubscriberActionSignedMessagesData))]
	public async Task ThenRaiseExceptionEvent<TMessage>(SubscriberAction<TMessage> subscriberAction)
	{
		Exception raisedException = null;

		await _rtgsSubscriber.StartAsync(subscriberAction.AllTestHandlers);
		_rtgsSubscriber.OnExceptionOccurred += (_, args) => raisedException = args.Exception;

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message, subscriberAction.AdditionalHeaders);

		_fromRtgsSender.WaitForAcknowledgements(WaitForAcknowledgementsDuration);

		await _rtgsSubscriber.StopAsync();

		raisedException.Should().BeOfType<RtgsSubscriberException>().Which.Message.Should().Be($"Verification of {subscriberAction.MessageIdentifier} message failed.");
	}

	[Theory]
	[ClassData(typeof(SubscriberActionSignedMessagesData))]
	public async Task AndVerifierThrows_ThenLogError<TMessage>(SubscriberAction<TMessage> subscriberAction)
	{
		using var exceptionSignal = new ManualResetEventSlim();

		await _rtgsSubscriber.StartAsync(subscriberAction.AllTestHandlers);
		_rtgsSubscriber.OnExceptionOccurred += (_, args) => exceptionSignal.Set();

		await _fromRtgsSender.SendAsync(subscriberAction.MessageIdentifier, subscriberAction.Message);

		exceptionSignal.Wait(WaitForExceptionEventDuration);

		var errorLogs = _serilogContext.SubscriberLogs(LogEventLevel.Error);
		errorLogs.Should().BeEquivalentTo(new[]
		{
			new LogEntry(
				$"An error occurred while verifying a message (MessageIdentifier: {subscriberAction.MessageIdentifier})",
				LogEventLevel.Error,
				typeof(RtgsSubscriberException))
		});
	}
}