using Apps.Apollo.Api;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Authentication;
using Blackbird.Applications.Sdk.Common.Invocation;

namespace Apps.Apollo;

public class Invocable : BaseInvocable
{
    protected AuthenticationCredentialsProvider[] Creds =>
        InvocationContext.AuthenticationCredentialsProviders.ToArray();

    protected ApolloClient Client { get; }

    public Invocable(InvocationContext invocationContext) : base(invocationContext)
    {
        Client = new(Creds);
    }
}
