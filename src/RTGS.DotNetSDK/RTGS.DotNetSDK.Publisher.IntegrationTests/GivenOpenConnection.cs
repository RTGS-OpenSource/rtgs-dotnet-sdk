using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RTGS.DotNetSDK.Publisher.Extensions;
using RTGS.DotNetSDK.Publisher.IntegrationTests.TestServer;
using RTGS.DotNetSDK.Publisher.Messages;
using RTGS.Public.Payment.V1.Pacs;
using Xunit;
using Xunit.Abstractions;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests
{
	public class GivenOpenConnection : IAsyncLifetime
	{
		private const string BankDid = "test-bank-did";
		private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(1);

		private readonly GrpcTestServer _server;
		private IRtgsPublisher _rtgsPublisher;
		private ToRtgsMessageHandler _toRtgsMessageHandler;
		private IHost _clientHost;

		public GivenOpenConnection(ITestOutputHelper outputHelper)
		{
			_server = new GrpcTestServer(outputHelper);
		}

		[Fact]
		public async Task WhenUsingMetadata_ThenSeeBankDidInRequestHeader()
		{
			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithSuccess();

			await _rtgsPublisher.SendAtomicLockRequestAsync(ValidRequests.AtomicLockRequest);

			var receiver = _server.Services.GetRequiredService<ToRtgsReceiver>();

			var connection = receiver.Connections.SingleOrDefault();

			connection.Should().NotBeNull();
			connection!.Headers.Should().ContainSingle(header => header.Key == "bankdid" && header.Value == BankDid);
		}

		[Fact]
		public async Task ThenCanSendAtomicLockRequestToRtgs()
		{
			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithSuccess();

			await _rtgsPublisher.SendAtomicLockRequestAsync(ValidRequests.AtomicLockRequest);

			var receiver = _server.Services.GetRequiredService<ToRtgsReceiver>();
			var receivedMessage = receiver.Connections.Should().ContainSingle().Which.Requests.Should().ContainSingle().Subject;
			var receivedAtomicLockRequest = JsonConvert.DeserializeObject<AtomicLockRequest>(receivedMessage.Data);

			using var _ = new AssertionScope();

			receivedMessage.Header.Should().NotBeNull();
			receivedMessage.Header?.InstructionType.Should().Be("payment.lock.v1");
			receivedMessage.Header?.CorrelationId.Should().NotBeNullOrEmpty();

			receivedAtomicLockRequest.Should().BeEquivalentTo(ValidRequests.AtomicLockRequest, options => options.ComparingByMembers<AtomicLockRequest>());
		}

		[Fact]
		public async Task ThenCanSendAtomicTransferRequestToRtgs()
		{
			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithSuccess();

			await _rtgsPublisher.SendAtomicTransferRequestAsync(ValidRequests.AtomicTransferRequest);

			var receiver = _server.Services.GetRequiredService<ToRtgsReceiver>();
			var receivedMessage = receiver.Connections.Should().ContainSingle().Which.Requests.Should().ContainSingle().Subject;
			var receivedAtomicTransferRequest = JsonConvert.DeserializeObject<AtomicTransferRequest>(receivedMessage.Data);

			using var _ = new AssertionScope();

			receivedMessage.Header.Should().NotBeNull();
			receivedMessage.Header?.InstructionType.Should().Be("payment.block.v1");
			receivedMessage.Header?.CorrelationId.Should().NotBeNullOrEmpty();

			receivedAtomicTransferRequest.Should().BeEquivalentTo(ValidRequests.AtomicTransferRequest, options => options.ComparingByMembers<AtomicTransferRequest>());
		}

		[Theory]
		[MemberData(nameof(PublisherActions))]
		public async Task WhenBankMessageApiReturnsSuccessfulAcknowledgement_ThenReturnSuccess<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithSuccess();

			var sendResult = await publisherAction.SendDelegate(_rtgsPublisher, publisherAction.Request);

			sendResult.Should().Be(SendResult.Success);
		}

		// TODO: does this need to be public?
		public static readonly IEnumerable<object[]> PublisherActions = new[]
		{
			new object[] 
			{
				new PublisherAction<AtomicLockRequest>
				{
					SendDelegate = (publisher, request) => publisher.SendAtomicLockRequestAsync(request),
					InstructionType = "payment.lock.v1",
					Request = ValidRequests.AtomicLockRequest
				}
			},
			new object[] 
			{
				new PublisherAction<AtomicTransferRequest>
				{
					SendDelegate = (publisher, request) => publisher.SendAtomicTransferRequestAsync(request),
					InstructionType = "payment.block.v1",
					Request = ValidRequests.AtomicTransferRequest
				}
			}
		};

		public class PublisherAction<TRequest>
		{
			public Func<IRtgsPublisher, TRequest, Task<SendResult>> SendDelegate { get; init; }

			public string InstructionType { get; init; }

			public TRequest Request { get; init; }
		}

		[Fact]
		public async Task WhenBankMessageApiReturnsUnsuccessfulAcknowledgement_ThenReturnServerError()
		{
			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithFailure();

			var sendResult = await _rtgsPublisher.SendAtomicLockRequestAsync(ValidRequests.AtomicLockRequest);

			sendResult.Should().Be(SendResult.ServerError);
		}

		[Fact]
		public async Task WhenBankMessageApiReturnsSuccessfulAcknowledgementTooLate_ThenReturnTimeout()
		{
			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithDelay(TestWaitForAcknowledgementDuration.Add(TimeSpan.FromSeconds(1)));

			var sendResult = await _rtgsPublisher.SendAtomicLockRequestAsync(ValidRequests.AtomicLockRequest);

			sendResult.Should().Be(SendResult.Timeout);
		}

		[Fact]
		public async Task WhenSendingMultipleMessages_ThenOnlyOneConnection()
		{
			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithSuccess();
			await _rtgsPublisher.SendAtomicLockRequestAsync(ValidRequests.AtomicLockRequest);

			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithSuccess();
			await _rtgsPublisher.SendAtomicLockRequestAsync(ValidRequests.AtomicLockRequest);

			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithSuccess();
			await _rtgsPublisher.SendAtomicLockRequestAsync(ValidRequests.AtomicLockRequest);

			var receiver = _server.Services.GetRequiredService<ToRtgsReceiver>();

			receiver.NumberOfConnections.Should().Be(1);
		}

		[Fact]
		public async Task WhenSendingMultipleMessagesAndLastOneTimesOut_ThenDoNotSeePreviousSuccess()
		{
			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithSuccess();
			var sendResult1 = await _rtgsPublisher.SendAtomicLockRequestAsync(ValidRequests.AtomicLockRequest);

			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithDelay(TestWaitForAcknowledgementDuration.Add(TimeSpan.FromSeconds(1)));
			var sendResult2 = await _rtgsPublisher.SendAtomicLockRequestAsync(ValidRequests.AtomicLockRequest);

			using var _ = new AssertionScope();

			sendResult1.Should().Be(SendResult.Success);
			sendResult2.Should().Be(SendResult.Timeout);
		}

		[Fact]
		public async Task WhenBankMessageApiOnlyReturnsUnexpectedAcknowledgement_ThenReturnTimeout()
		{
			_toRtgsMessageHandler.EnqueueUnexpectedAcknowledgementWithSuccess();

			var sendResult = await _rtgsPublisher.SendAtomicLockRequestAsync(ValidRequests.AtomicLockRequest);

			sendResult.Should().Be(SendResult.Timeout);
		}

		[Fact]
		public async Task WhenBankMessageApiReturnsUnexpectedAcknowledgementBeforeFailureAcknowledgement_ThenReturnServerError()
		{
			_toRtgsMessageHandler.EnqueueUnexpectedAcknowledgementWithSuccess();
			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithFailure();

			var sendResult = await _rtgsPublisher.SendAtomicLockRequestAsync(ValidRequests.AtomicLockRequest);

			sendResult.Should().Be(SendResult.ServerError);
		}

		[Fact]
		public async Task WhenBankMessageApiReturnsFailureAcknowledgementBeforeUnexpectedAcknowledgement_ThenReturnServerError()
		{
			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithFailure();
			_toRtgsMessageHandler.EnqueueUnexpectedAcknowledgementWithSuccess();

			var sendResult = await _rtgsPublisher.SendAtomicLockRequestAsync(ValidRequests.AtomicLockRequest);

			sendResult.Should().Be(SendResult.ServerError);
		}

		[Fact]
		public async Task WhenBankMessageApiReturnsSuccessWrappedByUnexpectedFailureAcknowledgements_ThenReturnServerError()
		{
			_toRtgsMessageHandler.EnqueueUnexpectedAcknowledgementWithFailure();
			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithSuccess();
			_toRtgsMessageHandler.EnqueueUnexpectedAcknowledgementWithFailure();

			var sendResult = await _rtgsPublisher.SendAtomicLockRequestAsync(ValidRequests.AtomicLockRequest);

			sendResult.Should().Be(SendResult.Success);
		}

		public async Task InitializeAsync()
		{
			try
			{
				var address = await _server.StartAsync();

				var rtgsClientOptions = RtgsClientOptions.Builder.CreateNew()
					.BankDid(BankDid)
					.RemoteHost(address.ToString())
					.WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
					.Build();

				_clientHost = Host.CreateDefaultBuilder()
					.ConfigureServices((_, services) => services.AddRtgsPublisher(rtgsClientOptions))
					.Build();

				_rtgsPublisher = _clientHost.Services.GetRequiredService<IRtgsPublisher>();
				_toRtgsMessageHandler = _server.Services.GetRequiredService<ToRtgsMessageHandler>();
			}
			catch (Exception)
			{
				// If an exception occurs then manually clean up as IAsyncLifetime.DisposeAsync is not called.
				// See https://github.com/xunit/xunit/discussions/2313 for further details.
				await DisposeAsync();

				throw;
			}
		}

		public async Task DisposeAsync()
		{
			if (_rtgsPublisher is not null)
			{
				await _rtgsPublisher.DisposeAsync();
			}

			_server?.Dispose();
			_clientHost?.Dispose();
		}

		private static class ValidRequests
		{
			public static readonly AtomicLockRequest AtomicLockRequest = new()
			{
				DbtrToRtgsId = new GenericFinancialIdentification1
				{
					Id = BankDid
				},
				CdtrAmt = new ActiveCurrencyAndAmount
				{
					Ccy = "GBP",
					Amt = new ProtoDecimal
					{
						Units = 1,
						Nanos = 230_000_000
					}
				},
				UltmtDbtrAcct = new CashAccount38
				{
					Ccy = "USD",
					Id = new AccountIdentification4Choice { IBAN = "XX00ULTIMATEDEBTORACCOUNT" }
				},
				UltmtCdtrAcct = new CashAccount38
				{
					Ccy = "GBP",
					Id = new AccountIdentification4Choice { IBAN = "XX00ULTIMATECREDITORACCOUNT" }
				},
				SplmtryData = "some-extra-data",
				EndToEndId = "end-to-end-id"
			};

			public static readonly AtomicTransferRequest AtomicTransferRequest = new()
			{
				DbtrToRtgsId = new GenericFinancialIdentification1
				{
					Id = BankDid
				},
				FIToFICstmrCdtTrf = new FinancialInstitutionToFinancialInstitutionCustomerCreditTransfer()
				{
					GrpHdr = new GroupHeader93
					{
						MsgId = "message-id"
					},
					CdtTrfTxInf = 
					{
						{
							new CreditTransferTransaction39 { PoolgAdjstmntDt = "2021-01-01" }
						}
					}
				},
				LckId = "7C10048C-18E8-4A4B-B006-99B4E6C9002B"
			};
		}
	}
}
