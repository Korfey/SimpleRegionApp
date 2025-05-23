using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;

namespace SimpleRegionApp.API.Core;
public class SnsUtils(IAmazonSimpleNotificationService snsClient, string topicArn)
{
    public async Task<SubscribeResponse> Subscribe(string email)
    {
        return await snsClient.SubscribeAsync(topicArn, "email", email);
    }

    public async Task<UnsubscribeResponse> Unsubscribe(string email)
    {
        var subscriptionArn = await GetSubscriberArn(email);

        return await snsClient.UnsubscribeAsync(subscriptionArn);
    }

    private async Task<string?> GetSubscriberArn(string email)
    {
        var subscriptions = new List<Subscription>();
        string? nextToken = null;

        do
        {
            var response = await snsClient.ListSubscriptionsByTopicAsync(topicArn, nextToken);

            subscriptions.AddRange(response.Subscriptions);
            nextToken = response.NextToken;

        } while (!string.IsNullOrEmpty(nextToken));

        var subscription = subscriptions
            .FirstOrDefault(s => s.Endpoint.Equals(email, StringComparison.OrdinalIgnoreCase));

        if (subscription != null)
        {
           return subscription.SubscriptionArn;
        }

        return null;
    }
}
