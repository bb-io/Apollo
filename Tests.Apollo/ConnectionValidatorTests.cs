using Apps.Apollo.Connections;
using Blackbird.Applications.Sdk.Common.Authentication;
using Tests.Apollo.Base;

namespace Tests.Apollo;

[TestClass]
public class ConnectionValidatorTests : TestBase
{
    [TestMethod]
    public async Task ValidateConnection_ValidData_ShouldBeSuccessful()
    {
        var validator = new ConnectionValidator(InvocationContext);

        var result = await validator.ValidateConnection(Creds, CancellationToken.None);

        Console.WriteLine(result.Message);
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task ValidateConnection_InvalidData_ShouldFail()
    {
        var validator = new ConnectionValidator(InvocationContext);
        var invalidCreds = Creds.Select(x => new AuthenticationCredentialsProvider(x.KeyName, x.Value + "_incorrect"));

        var result = await validator.ValidateConnection(invalidCreds, CancellationToken.None);

        Console.WriteLine(result.Message);
        Assert.IsFalse(result.IsValid);
    }
}
