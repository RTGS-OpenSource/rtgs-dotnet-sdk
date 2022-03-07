using System.Text;
using RTGS.DotNetSDK.Subscriber.Handlers;

namespace RTGS.DotNetSDK.Subscriber.Validators;

internal class HandlerValidator : IHandlerValidator
{
	private readonly IEnumerable<Type> _requiredHandlers = new[]
	{
		typeof(IAtomicLockResponseV1Handler),
		typeof(IAtomicTransferFundsV1Handler),
		typeof(IAtomicTransferResponseV1Handler),
		typeof(IBankPartnersResponseV1Handler),
		typeof(IEarmarkCompleteV1Handler),
		typeof(IEarmarkFundsV1Handler),
		typeof(IEarmarkReleaseV1Handler),
		typeof(IIdCryptInvitationConfirmationV1Handler),
		typeof(IMessageRejectV1Handler),
		typeof(IPayawayFundsV1Handler),
		typeof(IPayawayCompleteV1Handler)
	};

	public void Validate(IReadOnlyList<IHandler> handlers)
	{
		var errorsBuilder = new StringBuilder();

		if (handlers.Any(handler => handler is null))
		{
			errorsBuilder.AppendLine("Handlers collection cannot contain null handlers.");
		}

		foreach (Type requiredHandler in _requiredHandlers)
		{
			var configuredHandlers = handlers.Where(requiredHandler.IsInstanceOfType).ToList();
			if (!configuredHandlers.Any())
			{
				errorsBuilder.AppendLine($"No handler of type {requiredHandler.Name} was found.");
			}
			else if (configuredHandlers.Count > 1)
			{
				errorsBuilder.AppendLine($"Multiple handlers of type {requiredHandler.Name} were found.");
			}
		}

		var errorMessage = errorsBuilder.ToString();
		if (!string.IsNullOrWhiteSpace(errorMessage))
		{
			throw new ArgumentException(errorMessage, nameof(handlers));
		}
	}
}
