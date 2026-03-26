using System.Runtime.CompilerServices;
using VerifyTests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        // This enables the magic that turns a GeneratorDriver 
        // into a set of verified .cs files.
        VerifySourceGenerators.Initialize();
    }
}