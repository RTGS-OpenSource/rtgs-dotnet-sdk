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
using RTGS.ISO20022.Messages.Camt_054_001.V09;
using RTGS.ISO20022.Messages.Pacs_008_001.V10;
using RTGS.Public.Payment.V1.Pacs;
using Xunit;
using Xunit.Abstractions;

namespace RTGS.DotNetSDK.Publisher.IntegrationTests
{
	public class GivenOpenConnection : IAsyncLifetime
	{
		private const string BankDid = "test-bank-did";
		private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(0.5);

		private readonly GrpcTestServer _server;
		private IRtgsPublisher _rtgsPublisher;
		private ToRtgsMessageHandler _toRtgsMessageHandler;
		private IHost _clientHost;

		public static readonly IEnumerable<object[]> PublisherActions = new[]
		{
			new object[]
			{
				new PublisherAction<AtomicLockRequest>(
					ValidRequests.AtomicLockRequest,
					"payment.lock.v1",
					(publisher, request) => publisher.SendAtomicLockRequestAsync(request))
			},
			new object[]
			{
				new PublisherAction<AtomicTransferRequest>(
					ValidRequests.AtomicTransferRequest,
					"payment.block.v1",
					(publisher, request) => publisher.SendAtomicTransferRequestAsync(request))
			},
			new object[]
			{
				new PublisherAction<EarmarkConfirmation>(
					ValidRequests.EarmarkConfirmation,
					"payment.earmarkconfirmation.v1",
					(publisher, request) => publisher.SendEarmarkConfirmationAsync(request))
			},
			 new object[]
			 {
			 	new PublisherAction<TransferConfirmation>(
			 		ValidRequests.TransferConfirmation,
			 		"payment.blockconfirmation.v1",
			 		(publisher, request) => publisher.SendTransferConfirmationAsync(request))
			 },
			 new object[]
			 {
			 	new PublisherAction<UpdateLedgerRequest>(
			 		ValidRequests.UpdateLedgerRequest,
			 		"payment.update.ledger.v1",
			 		(publisher, request) => publisher.SendUpdateLedgerRequestAsync(request))
			 },
			 new object[]
			 {
			 	new PublisherAction<FIToFICustomerCreditTransferV10>(
			 		ValidRequests.PayawayCreate,
			 		"payaway.create.v1",
			 		(publisher, request) => publisher.SendPayawayCreateAsync(request))
			 },
			 new object[]
			 {
			 	new PublisherAction<BankToCustomerDebitCreditNotificationV09>(
			 		ValidRequests.PayawayConfirmation,
			 		"payaway.confirmation.v1",
			 		(publisher, request) => publisher.SendPayawayConfirmationAsync(request))
			 }
		};

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

		[Theory]
		[MemberData(nameof(PublisherActions))]
		public async Task ThenCanSendRequestToRtgs<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithSuccess();

			await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			var receiver = _server.Services.GetRequiredService<ToRtgsReceiver>();
			var receivedMessage = receiver.Connections.Should().ContainSingle().Which.Requests.Should().ContainSingle().Subject;
			var receivedRequest = JsonConvert.DeserializeObject<TRequest>(receivedMessage.Data);

			using var _ = new AssertionScope();

			receivedMessage.Header.Should().NotBeNull();
			receivedMessage.Header?.InstructionType.Should().Be(publisherAction.InstructionType);
			receivedMessage.Header?.CorrelationId.Should().NotBeNullOrEmpty();

			receivedRequest.Should().BeEquivalentTo(publisherAction.Request, options => options.ComparingByMembers<TRequest>());
		}

		[Theory]
		[MemberData(nameof(PublisherActions))]
		public async Task WhenBankMessageApiReturnsSuccessfulAcknowledgement_ThenReturnSuccess<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithSuccess();

			var sendResult = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			sendResult.Should().Be(SendResult.Success);
		}

		[Theory]
		[MemberData(nameof(PublisherActions))]
		public async Task WhenBankMessageApiReturnsUnsuccessfulAcknowledgement_ThenReturnServerError<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithFailure();

			var sendResult = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			sendResult.Should().Be(SendResult.ServerError);
		}

		[Theory]
		[MemberData(nameof(PublisherActions))]
		public async Task WhenBankMessageApiReturnsSuccessfulAcknowledgementTooLate_ThenReturnTimeout<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithDelay(TestWaitForAcknowledgementDuration.Add(TimeSpan.FromSeconds(1)));

			var sendResult = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			sendResult.Should().Be(SendResult.Timeout);
		}

		[Theory]
		[MemberData(nameof(PublisherActions))]
		public async Task WhenSendingMultipleMessages_ThenOnlyOneConnection<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithSuccess();
			await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithSuccess();
			await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithSuccess();
			await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			var receiver = _server.Services.GetRequiredService<ToRtgsReceiver>();

			receiver.NumberOfConnections.Should().Be(1);
		}

		[Theory]
		[MemberData(nameof(PublisherActions))]
		public async Task WhenSendingMultipleMessagesAndLastOneTimesOut_ThenDoNotSeePreviousSuccess<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithSuccess();
			var sendResult1 = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithDelay(TestWaitForAcknowledgementDuration.Add(TimeSpan.FromSeconds(1)));
			var sendResult2 = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);
			using var _ = new AssertionScope();

			sendResult1.Should().Be(SendResult.Success);
			sendResult2.Should().Be(SendResult.Timeout);
		}

		[Theory]
		[MemberData(nameof(PublisherActions))]
		public async Task WhenBankMessageApiOnlyReturnsUnexpectedAcknowledgement_ThenReturnTimeout<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.EnqueueUnexpectedAcknowledgementWithSuccess();

			var sendResult = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			sendResult.Should().Be(SendResult.Timeout);
		}

		[Theory]
		[MemberData(nameof(PublisherActions))]
		public async Task WhenBankMessageApiReturnsUnexpectedAcknowledgementBeforeFailureAcknowledgement_ThenReturnServerError<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.EnqueueUnexpectedAcknowledgementWithSuccess();
			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithFailure();

			var sendResult = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			sendResult.Should().Be(SendResult.ServerError);
		}

		[Theory]
		[MemberData(nameof(PublisherActions))]
		public async Task WhenBankMessageApiReturnsFailureAcknowledgementBeforeUnexpectedAcknowledgement_ThenReturnServerError<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithFailure();
			_toRtgsMessageHandler.EnqueueUnexpectedAcknowledgementWithSuccess();

			var sendResult = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

			sendResult.Should().Be(SendResult.ServerError);
		}

		[Theory]
		[MemberData(nameof(PublisherActions))]
		public async Task WhenBankMessageApiReturnsSuccessWrappedByUnexpectedFailureAcknowledgements_ThenReturnServerError<TRequest>(PublisherAction<TRequest> publisherAction)
		{
			_toRtgsMessageHandler.EnqueueUnexpectedAcknowledgementWithFailure();
			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithSuccess();
			_toRtgsMessageHandler.EnqueueUnexpectedAcknowledgementWithFailure();

			var sendResult = await publisherAction.InvokeSendDelegateAsync(_rtgsPublisher);

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
				DbtrToRtgsId = new Public.Payment.V1.Pacs.GenericFinancialIdentification1
				{
					Id = BankDid
				},
				CdtrAmt = new Public.Payment.V1.Pacs.ActiveCurrencyAndAmount
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
					Id = new Public.Payment.V1.Pacs.AccountIdentification4Choice { IBAN = "XX00ULTIMATEDEBTORACCOUNT" }
				},
				UltmtCdtrAcct = new CashAccount38
				{
					Ccy = "GBP",
					Id = new Public.Payment.V1.Pacs.AccountIdentification4Choice { IBAN = "XX00ULTIMATECREDITORACCOUNT" }
				},
				SplmtryData = "some-extra-data",
				EndToEndId = "end-to-end-id"
			};

			public static readonly AtomicTransferRequest AtomicTransferRequest = new()
			{
				DbtrToRtgsId = new Public.Payment.V1.Pacs.GenericFinancialIdentification1
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
				LckId = Guid.NewGuid().ToString()
			};

			public static readonly EarmarkConfirmation EarmarkConfirmation = new()
			{
				LockId = Guid.NewGuid(),
				Success = true
			};

			public static readonly TransferConfirmation TransferConfirmation = new()
			{
				LockId = Guid.NewGuid(),
				Success = true
			};

			public static readonly UpdateLedgerRequest UpdateLedgerRequest = new()
			{
				Amt = new ProtoDecimal()
				{
					Units = 1,
					Nanos = 230_000_000
				},
				BkToRtgsId = new Public.Payment.V1.Pacs.GenericFinancialIdentification1()
				{
					Id = BankDid
				}
			};

			public static readonly FIToFICustomerCreditTransferV10 PayawayCreate = new()
			{
				GrpHdr = new GroupHeader96
				{
					MsgId = "message-id"
				},
				CdtTrfTxInf = new[]
				{
					new CreditTransferTransaction50 { PoolgAdjstmntDt = new DateTime(2021, 1, 1) }
				}
			};

			public static readonly BankToCustomerDebitCreditNotificationV09 PayawayConfirmation = new()
			{
				GrpHdr = new GroupHeader81
				{
					MsgId = "message-id"
				},
				Ntfctn = new[]
				{
					new AccountNotification19
					{
						AddtlNtfctnInf = "additional-notification-info"
					}
				}
			};
		}
	}
}
