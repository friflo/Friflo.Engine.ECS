namespace Friflo.Fliox.Editor.UI.Models;

public interface ITest
{
    void Foo();
}


public class TestClass : ITest
{
    void ITest.Foo() {
        
    }
}

public static class UseClass
{
    private static void Bar() {
        var testClass = new TestClass();
        ITest testInterface = testClass;
        testInterface.Foo();
    }
}