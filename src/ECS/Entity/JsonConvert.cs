// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Fliox.Engine.ECS.Serialize;
using Friflo.Json.Fliox.Mapper;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

internal class JsonConvert
{
    private readonly    EntityConverter converter;
    private readonly    DataEntity      dataEntity;      
    private readonly    ObjectWriter    writer;
    
    internal JsonConvert()
    {
        converter   = new EntityConverter();
        dataEntity  = new DataEntity();      
        writer      = new (new TypeStore()) {  // todo use global TypeStore
            Pretty = true, WriteNullMembers = false
        };
    }
    
    internal string EntityToJSON(Entity entity)
    {
        converter.EntityToDataEntity(entity, dataEntity, true);
        return writer.Write(dataEntity);
    }
    
    internal string DataEntityToJSON(DataEntity dataEntity) {
        return writer.Write(dataEntity);
    }
}