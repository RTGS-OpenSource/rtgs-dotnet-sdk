using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

namespace RTGS.DotNetSDK.Publisher.IntegrationTests
{
	public class GivenOpenConnection : IAsyncLifetime, IClassFixture<GrpcServerFixture>
	{
		private const string BankDid = "test-bank-did";
		private static readonly TimeSpan TestWaitForAcknowledgementDuration = TimeSpan.FromSeconds(0.5);

		private readonly GrpcServerFixture _grpcServer;

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
					(publisher, request, cancellationToken) => publisher.SendAtomicLockRequestAsync(request, cancellationToken))
			},
			new object[]
			{
				new PublisherAction<AtomicTransferRequest>(
					ValidRequests.AtomicTransferRequest,
					"payment.block.v1",
					(publisher, request, cancellationToken) => publisher.SendAtomicTransferRequestAsync(request, cancellationToken))
			},
			new object[]
			{
				new PublisherAction<EarmarkConfirmation>(
					ValidRequests.EarmarkConfirmation,
					"payment.earmarkconfirmation.v1",
					(publisher, request, cancellationToken) => publisher.SendEarmarkConfirmationAsync(request, cancellationToken))
			},
			 new object[]
			 {
			 	new PublisherAction<TransferConfirmation>(
			 		ValidRequests.TransferConfirmation,
			 		"payment.blockconfirmation.v1",
			 		(publisher, request, cancellationToken) => publisher.SendTransferConfirmationAsync(request, cancellationToken))
			 },
			 new object[]
			 {
			 	new PublisherAction<UpdateLedgerRequest>(
			 		ValidRequests.UpdateLedgerRequest,
			 		"payment.update.ledger.v1",
			 		(publisher, request, cancellationToken) => publisher.SendUpdateLedgerRequestAsync(request, cancellationToken))
			 },
			 new object[]
			 {
			 	new PublisherAction<FIToFICustomerCreditTransferV10>(
			 		ValidRequests.PayawayCreate,
			 		"payaway.create.v1",
			 		(publisher, request, cancellationToken) => publisher.SendPayawayCreateAsync(request, cancellationToken))
			 },
			 new object[]
			 {
			 	new PublisherAction<BankToCustomerDebitCreditNotificationV09>(
			 		ValidRequests.PayawayConfirmation,
			 		"payaway.confirmation.v1",
			 		(publisher, request, cancellationToken) => publisher.SendPayawayConfirmationAsync(request, cancellationToken))
			 }
		};

		public GivenOpenConnection(GrpcServerFixture grpcServer)
		{
			_grpcServer = grpcServer;
		}

		public async Task InitializeAsync()
		{
			try
			{
				var rtgsClientOptions = RtgsClientOptions.Builder.CreateNew()
					.BankDid(BankDid)
					.RemoteHost(_grpcServer.ServerUri.ToString())
					.WaitForAcknowledgementDuration(TestWaitForAcknowledgementDuration)
					.Build();

				_clientHost = Host.CreateDefaultBuilder()
					.ConfigureServices((_, services) => services.AddRtgsPublisher(rtgsClientOptions))
					.Build();

				_rtgsPublisher = _clientHost.Services.GetRequiredService<IRtgsPublisher>();
				_toRtgsMessageHandler = _grpcServer.Services.GetRequiredService<ToRtgsMessageHandler>();
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

			_clientHost?.Dispose();

			_grpcServer.Reset();
		}

		[Fact]
		public async Task WhenUsingMetadata_ThenSeeBankDidInRequestHeader()
		{
			_toRtgsMessageHandler.EnqueueExpectedAcknowledgementWithSuccess();

			await _rtgsPublisher.SendAtomicLockRequestAsync(ValidRequests.AtomicLockRequest);

			var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

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

			var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();
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

			var receiver = _grpcServer.Services.GetRequiredService<ToRtgsReceiver>();

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

		[Theory]
		[MemberData(nameof(PublisherActions))]
		public async Task WhenCancellationTokenIsCancelled_ThenReturnOperationCancelled<TRequest>(PublisherAction<TRequest> publisherAction) =>
			await FluentActions.Awaiting(() =>
				{
					using var cancellationTokenSource = new CancellationTokenSource(TestWaitForAcknowledgementDuration - TimeSpan.FromMilliseconds(100));
					return publisherAction.InvokeSendDelegateAsync(_rtgsPublisher, cancellationTokenSource.Token);
				})
				.Should().ThrowAsync<OperationCanceledException>();

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
				LckId = "B27C2536-27F8-403F-ABBD-7AC4190FBBD3"
			};

			public static readonly EarmarkConfirmation EarmarkConfirmation = new()
			{
				LockId = new Guid("159C6010-82CB-4775-8C87-05E6EC203E8E"),
				Success = true
			};

			public static readonly TransferConfirmation TransferConfirmation = new()
			{
				LockId = new Guid("B30E15E3-CD54-4FA6-B0EB-B9BAE32976F9"),
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
