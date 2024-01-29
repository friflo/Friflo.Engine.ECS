using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

namespace Internal.ECS;

// ReSharper disable once InconsistentNaming
public static class Test_EventFilter
{
    [Test]
    public static void Test_EventFilter_Init()
    {
        var store       = new EntityStore(PidType.UsePidAsId);
        var recorder    = store.EventRecorder;
        
        IsFalse(recorder.Enabled);
        recorder.Enabled = true;
        IsTrue(recorder.Enabled);
    }
}