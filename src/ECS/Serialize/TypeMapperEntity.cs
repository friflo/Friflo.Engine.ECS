using Friflo.Json.Burst;
using Friflo.Json.Fliox.Mapper.Map;

namespace Friflo.Engine.ECS.Serialize;


internal class TypeMapperEntity : TypeMapper<Entity>
{
    public TypeMapperEntity() : base (null, typeof(Entity), false, true) {
        
    }
    
    public override bool    IsNull(ref Entity value)  => value.Id == 0;
    
    public override void Write(ref Writer writer, Entity value) {
        if (value.IsNull) {
            writer.AppendNull();
            return;
        }
        writer.format.AppendInt(ref writer.bytes, value.Id);
    }
    
    public override Entity Read(ref Reader reader, Entity slot, out bool success)
    {
        switch (reader.parser.Event) {
            case JsonEvent.ValueNull:
                success = true;
                return default;
            case JsonEvent.ValueNumber:
                long key = reader.parser.ValueAsLong(out success);
                if (success) {
                    var context = reader.GetMapperContext<MapperContextEntityStore>();
                    return context.store.CreateEntityReference(key);
                }
                return reader.ErrorMsg<Entity>("Invalid entity id", reader.parser.value, out success);
            default:
                return reader.HandleEvent(this, out success);
        }
    }
    

}

