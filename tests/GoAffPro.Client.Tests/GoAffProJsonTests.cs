using System.Text.Json;
using GoAffPro.Client.Generated.Models;

namespace GoAffPro.Client.Tests;

[TestClass]
public sealed class GoAffProJsonTests
{
    [TestMethod]
    public void ToJsonElement_Order_WritesGoAffProFieldNames()
    {
        var order = new UserOrderFeedItem()
        {
            Number = "1001",
            Total = "49.90",
            Currency = "CHF",
        };

        var json = GoAffProJson.ToJsonElement(order);

        Assert.AreEqual("1001", json.GetProperty("number").GetString());
        Assert.AreEqual("49.90", json.GetProperty("total").GetString());
        Assert.AreEqual("CHF", json.GetProperty("currency").GetString());
    }
}
