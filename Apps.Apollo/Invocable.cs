using Blackbird.Applications.Sdk.Common;
using Apps.Apollo.Api;
using Blackbird.Applications.Sdk.Common.Invocation;
namespace Apps.Apollo;
public class Invocable : BaseInvocable
{
    protected ApolloClient Client { get; }
    public Invocable(InvocationContext context) : base(context) => Client = new(context.AuthenticationCredentialsProviders);
}
