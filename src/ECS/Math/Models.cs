using System;
using System.Numerics;
using System.Text;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Fliox.Engine.ECS;

// --------------------------- Position ---------------------------
[StructComponent("pos")]
public struct  Position
{
    [Browse(Never)] public Vector3 Value;
    
    public float x { get => Value.X; set => Value.X = value; }
    public float y { get => Value.Y; set => Value.Y = value; }
    public float z { get => Value.Z; set => Value.Z = value; }

    public override string ToString() => $"{x}, {y}, {z}";

    public Position (float x, float y, float z) {
        Value.X = x;
        Value.Y = y;
        Value.Z = z;
    }
}

// --------------------------- Rotation ---------------------------
[StructComponent("rot")]
public struct  Rotation
{
    [Browse(Never)] public Quaternion Value;
    
    public float x { get => Value.X; set => Value.X = value; }
    public float y { get => Value.Y; set => Value.Y = value; }
    public float z { get => Value.Z; set => Value.Z = value; }
    public float w { get => Value.W; set => Value.W = value; }
    
    public override string ToString() => $"{x}, {y}, {z}, {w}";
    
    public Rotation (float x, float y, float z, float w) {
        Value.X = x;
        Value.Y = y;
        Value.Z = z;
        Value.W = w;
    }
}

// --------------------------- Scale3 ---------------------------
[StructComponent("scl3")]
public struct Scale3
{
    [Browse(Never)] public Vector3 Value;
    
    public float x { get => Value.X; set => Value.X = value; }
    public float y { get => Value.Y; set => Value.Y = value; }
    public float z { get => Value.Z; set => Value.Z = value; }

    public override string ToString() => $"{x}, {y}, {z}";

    public Scale3 (float x, float y, float z) {
        Value.X = x;
        Value.Y = y;
        Value.Z = z;
    }
}

// --------------------------- Scale ---------------------------
[StructComponent("name")]
public struct EntityName
{
                    public  string              Value   { get => value; set => SetValue(value); }
    [Browse(Never)] public  ReadOnlySpan<byte>  UTF8    => new (utf8);

    [Browse(Never)] private string              value;
    [Browse(Never)] private byte[]              utf8;
    
    public override         string              ToString() => $"Name: \"{value}\"";

    public EntityName (string value) {
        Value = value;
    }
    
    private void SetValue(string value) {
        this.value  = value;
        utf8        = Encoding.UTF8.GetBytes(value);
    }
}