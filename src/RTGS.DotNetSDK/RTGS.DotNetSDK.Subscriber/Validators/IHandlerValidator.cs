using System.Collections.Generic;
using RTGS.DotNetSDK.Subscriber.Handlers;

namespace RTGS.DotNetSDK.Subscriber.Validators
{
	public interface IHandlerValidator
	{
		void Validate(IList<IHandler> handlers);
	}
}
