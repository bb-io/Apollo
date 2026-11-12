using Apps.Apollo.Actions;
using Apps.Apollo.Models.Requests;
using Tests.Apollo.Base;

namespace Tests.Apollo;

[TestClass]
public class EmailActionsTests : TestBase
{
    [TestMethod]
    public async Task SearchOutreachEmails_WithoutFilters_ReturnsEmails()
    {
        var actions = new EmailActions(InvocationContext);

        var result = await actions.SearchOutreachEmails(new SearchOutreachEmailsRequest { MaximumResults = 5 });

        PrintJsonResult(result);
        Assert.IsTrue(result.ReturnedEmails <= 5);
    }

    [TestMethod]
    public async Task SearchOutreachEmails_WithStatusAndDates_ReturnsEmails()
    {
        var actions = new EmailActions(InvocationContext);

        var result = await actions.SearchOutreachEmails(new SearchOutreachEmailsRequest
        {
            Statuses = ["delivered"],
            StartDate = DateTime.UtcNow.AddYears(-2),
            EndDate = DateTime.UtcNow,
            MaximumResults = 5
        });

        PrintJsonResult(result);
        Assert.IsNotNull(result.Emails);
    }

    [TestMethod]
    public async Task GetEmailContent_FoundAndUnknownIds_KeepsInputOrder()
    {
        var actions = new EmailActions(InvocationContext);
        var emails = await actions.SearchOutreachEmails(new SearchOutreachEmailsRequest
        {
            Statuses = ["delivered"],
            MaximumResults = 2
        });
        var messageIds = emails.Emails.Select(x => x.MessageId).Append("unknown-message-id").ToList();

        var result = await actions.GetEmailContent(new GetEmailContentRequest { EmailMessageIds = messageIds });

        PrintJsonResult(result);
        CollectionAssert.AreEqual(messageIds, result.Results.Select(x => x.MessageId).ToList());
        Assert.IsFalse(result.Results.Last().ContentAvailable);
    }
}
