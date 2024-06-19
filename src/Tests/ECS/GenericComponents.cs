using Friflo.Engine.ECS;

// ReSharper disable ClassNeverInstantiated.Global
namespace Tests.ECS {


// --------------------- component field types ignored by serializer
public struct       GenericStruct<T>         { public T value; }
public class        GenericClass<T>          { public T value; }
public interface    IGenericInterface<T>     { public T Value { get; set; } } 

// ------------------------------------------------------------------------------------------------------ 
// [Generic structs in components is not supported] https://github.com/friflo/Friflo.Json.Fliox/issues/45
// Fixed by commit:     [Mapper - Ignore unsupported fields/properties in custom classes, structs and interfaces.]
//                      https://github.com/friflo/Friflo.Json.Fliox/commit/12c4f88f26d86cffd014f00f823d152eede29d36
// Remarks:             Unsupported fields/properties in custom classes, structs and interfaces are now ignored by mapper/serialization.
// Fix published in:    https://www.nuget.org/packages/Friflo.Json.Fliox/1.0.2
public struct ComponentWithGenerics : IComponent
{
    public GenericStruct<int>       genericStruct;
    public GenericClass<int>        genericClass;
    public IGenericInterface<int>   genericInterface;
}

// ------------------------------------------------------------------------------------------------------
// [Generic components and tags types are not supported and throw exception on usage. · Issue #53]
// https://github.com/friflo/Friflo.Json.Fliox/issues/53
[GenericInstanceType(typeof(int))]
public struct GenericComponent<T> : IComponent {
    public T value;
}

[GenericInstanceType(typeof(int))]
[GenericInstanceType(typeof(string))]
// ReSharper disable once UnusedTypeParameter
public struct GenericTag<T> : ITag { }

[GenericInstanceType(typeof(int), typeof(bool))]
// ReSharper disable twice UnusedTypeParameter
public struct GenericTag2<T1, T2> : ITag { }

}