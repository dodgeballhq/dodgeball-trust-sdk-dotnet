using dotenv.net;
using Dodgeball.TrustServer.Api;

namespace Dodgeball.TrustServer.Api.Tests;

public class DodgeballTests
{
    private IDictionary<string, string> Vars;
    
    [SetUp]
    public void Setup()
    {
        this.Vars = DotEnv.Read();
    }

    [Test]
    public void TestTrackingCall()
    {
        Assert.Pass();
    }
}