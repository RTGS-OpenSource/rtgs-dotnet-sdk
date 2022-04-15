using System.Collections;

namespace RTGS.DotNetSDK.IntegrationTests.Publisher.TestData;

public abstract class BaseActionData : IEnumerable<object[]>
{
	private IEnumerator<object[]> GetActions() =>
	GetType().GetProperties()
		.Select(propertyInfo => new[] { propertyInfo.GetValue(this) })
		.GetEnumerator();

	public IEnumerator<object[]> GetEnumerator() =>
		GetActions();

	IEnumerator IEnumerable.GetEnumerator() =>
		GetEnumerator();
}
