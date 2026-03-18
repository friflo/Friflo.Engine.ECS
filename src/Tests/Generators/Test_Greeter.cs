using NUnit.Framework;

// ReSharper disable CheckNamespace
namespace GeneratedCode;

public partial class Greeter 
{
    // The Hello() method is being injected here by the generator!
}

public static class TestGreeter
{
    [Test]
    public static void Test_Entity_new_EntityStore_Perf()
    {
        var myGreeter = new Greeter();
        myGreeter.Hello();
    }
}