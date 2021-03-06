using System.Net.Http;
using RTGS.DotNetSDK.IntegrationTests.Extensions;
using RTGS.DotNetSDK.IntegrationTests.HttpHandlers;
using RTGS.DotNetSDK.IntegrationTests.Publisher.TestData.IdCrypt;
using RTGS.DotNetSDK.Publisher.Exceptions;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.Signing.GivenOpenConnection;

public sealed class WhenSigningIsNotSuccessful : IDisposable, IClassFixture<GrpcServerFixture>
{
	private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(1);

	private readonly GrpcServerFixture _grpcServer;
	private readonly ITestCorrelatorContext _serilogContext;

	private IHost _clientHost;
	private IRtgsPublisher _rtgsPublisher;
	private ToRtgsMessageHandler _toRtgsMessageHandler;

	public WhenSigningIsNotSuccessful(GrpcServerFixture grpcServer)
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
				.WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
				.KeepAlivePingDelay(TimeSpan.FromSeconds(30))
				.KeepAlivePingTimeout(TimeSpan.FromSeconds(30))
				.EnableMessageSigning()
				.Build();

			var idCryptServiceHttpHandler = StatusCodeHttpHandlerBuilderFactory
				.Create()
				.WithServiceUnavailableResponse(SignMessage.Path)
				.Build();

			_clientHost = Host.CreateDefaultBuilder()
				.ConfigureAppConfiguration(configuration => configuration.Sources.Clear())
				.ConfigureServices(services => services
					.AddRtgsPublisher(rtgsSdkOptions)
					.AddTestIdCryptServiceHttpClient(idCryptServiceHttpHandler))
				.UseSerilog()
				.Build();

			_rtgsPublisher = _clientHost.Services.GetRequiredService<IRtgsPublisher>();
			_toRtgsMessageHandler = _grpcServer.Services.GetRequiredService<ToRtgsMessageHandler>();
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
	[ClassData(typeof(PublisherActionSignedMessagesData))]
	public async Task ThenExceptionIsThrown<TRequest>(PublisherAction<TRequest> publisherAction)
	{
		_toRtgsMessageHandler.SetupForMessage(handler => handler.ReturnExpectedAcknowledgementWithSuccess());

		await FluentActions.Awaiting(() => publisherAction.InvokeSendDelegateAsync(_rtgsPublisher))
		  .Should()
		  .ThrowAsync<RtgsPublisherException>().WithMessage($"Error when signing {typeof(TRequest).Name} message.")
		  .WithInnerException(typeof(HttpRequestException));
	}

	[Theory]
	[ClassData(typeof(PublisherActionSignedMessagesData))]
	public async Task ThenMessageNotSent<TRequest>(PublisherAction<TRequest> publisherAction)
	{
		_toRtgsMessageHandler.SetupForMessage(handler =>
			handler.ReturnExpectedAcknowledgementWithSuccess());

		await FluentActions.Awaiting(() => publisherAction.InvokeSendDelegateAsync(_rtgsPublisher))
		  .Should()
		  .ThrowAsync<Exception>();

		var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

		receiver.Connections.Should().BeEmpty();
	}

	[Theory]
	[ClassData(typeof(PublisherActionSignedMessagesData))]
	public async Task ThenPublisherLogs<TRequest>(PublisherAction<TRequest> publisherAction)
	{
		_toRtgsMessageHandler.SetupForMessage(handler =>
			handler.ReturnExpectedAcknowledgementWithSuccess());

		await FluentActions.Awaiting(() => publisherAction.InvokeSendDelegateAsync(_rtgsPublisher))
		  .Should()
		  .ThrowAsync<Exception>();

		using var _ = new AssertionScope();

		_serilogContext.PublisherLogs(LogEventLevel.Information)
			.Should().ContainSingle().Which.Should().BeEquivalentTo(
				new LogEntry($"Signing {typeof(TRequest).Name} message", LogEventLevel.Information));

		_serilogContext.PublisherLogs(LogEventLevel.Error)
				.Should().ContainSingle().Which.Should().BeEquivalentTo(
					new LogEntry($"Error signing {typeof(TRequest).Name} message", LogEventLevel.Error, typeof(RtgsPublisherException)));
	}

	[Theory]
	[ClassData(typeof(PublisherActionSignedMessagesData))]
	public async Task ThenIdCryptServiceClientLogs<TRequest>(PublisherAction<TRequest> publisherAction)
	{
		_toRtgsMessageHandler.SetupForMessage(handler =>
			handler.ReturnExpectedAcknowledgementWithSuccess());

		await FluentActions.Awaiting(() => publisherAction.InvokeSendDelegateAsync(_rtgsPublisher))
			.Should()
			.ThrowAsync<Exception>();

		using var _ = new AssertionScope();

		_serilogContext.LogsFor("RTGS.DotNetSDK.IdCrypt.IdCryptServiceClient", LogEventLevel.Debug)
			.Should().ContainSingle().Which.Should().BeEquivalentTo(
				new LogEntry("Sending SignMessageForBank request to ID Crypt Service", LogEventLevel.Debug));

		_serilogContext.LogsFor("RTGS.DotNetSDK.IdCrypt.IdCryptServiceClient", LogEventLevel.Error)
			.Should().ContainSingle().Which.Should().BeEquivalentTo(
				new LogEntry("Error occurred when sending SignMessageForBank request to ID Crypt Service", LogEventLevel.Error, typeof(HttpRequestException)));
	}
}
