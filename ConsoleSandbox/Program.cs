using System.Diagnostics;
using Jay.Validation;
using Jayflect;
using Jayflect.Extensions;

#pragma warning disable

var field = typeof(Entity).GetProperty("Id", Reflect.Flags.Instance).GetBackingField().ValidateNotNull();

var entity = new Entity(13);
field.SetValue(ref entity, 147);


Debugger.Break();



public class Entity
{
    public int Id { get; }

    public string Name { get; set; } = "";

    public Entity(int id)
    {
        this.Id = id;
    }

    public override string ToString()
    {
        return $"{Name} #{Id}";
    }
}


