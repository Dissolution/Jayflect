using System.Diagnostics;
using System.Runtime.CompilerServices;
using ConsoleSandbox;
using Jay.Validation;
using Jayflect;
using Jayflect.Extensions;

#pragma warning disable

// Use something from Reflection to get its dumpers!
RuntimeHelpers.RunClassConstructor(typeof(Reflect).TypeHandle);

try
{

    // var field = typeof(Entity).GetProperty("Id", Reflect.Flags.Instance).GetBackingField().ValidateNotNull();
    //
    // var entity = new Entity(13)
    // {
    //     Name = "Thirteen Isn't Lucky",
    // };
    //
    // var reflection = DynamicReflection.Of(entity);
    // reflection.UpdateName("joe's");
    // var name = reflection.Name;
    // Console.WriteLine(name);
    // name = reflection.get_Name();
    // Console.WriteLine(name);
    // reflection.Name = "Billy Rae";
    // name = reflection.Name();
    // Console.WriteLine(name);

    var tc = new TestClass();
    //var reflection = DynamicReflection.Of(tc);
    var reflection = DynamicReflection.Of(tc);

    bool isFour = reflection == 4;
    
    
    Debugger.Break();


}
catch (Exception mainException)
{
    Debugger.Break();
}

namespace ConsoleSandbox
{
    public class TestClass
    {
        public static bool operator false(TestClass testClass) => false;
        public static bool operator true(TestClass testClass) => true;

        public static bool operator &(TestClass left, bool right) => right;
        public static bool operator |(TestClass left, bool right) => true;
        public static bool operator ^(TestClass left, bool right) => !right;

        public static bool operator ==(TestClass left, TestClass right) => left.Equals(right);
        public static bool operator !=(TestClass left, TestClass right) => !left.Equals(right);
        
        private readonly Dictionary<string, object?> _dict = new();

        public object? this[string key]
        {
            get => _dict[key];
            set => _dict[key] = value;
        }
}
    
    public static class TestStaticClass
    {
        private static readonly Dictionary<string, object?> _dict = new();
        
        private static object? get_Item(string key)
        {
            return _dict[key];
        }
        private static void set_Item(string key, object? value)
        {
            _dict[key] = value;
        }
        
        public static Entity NewEntity() => new Entity(Random.Shared.Next(), Guid.NewGuid().ToString("N"));

        public static void WriteNonsense()
        {
            Console.WriteLine(Guid.NewGuid().ToString("N"));
        }

        public static object? Invoke(params object?[] args)
        {
            Debugger.Break();
            return null;
        }
    }
    
    public class Entity : IEquatable<Entity>
    {

        public int Id { get; init; }

        public string Name { get; set; } = "";

        public Entity(int id)
        {
            this.Id = id;
        }
        public Entity(int id, string name)
        {
            this.Id = id;
            this.Name = name;
        }

        public void UpdateName(string? name)
        {
            this.Name = name ?? "";
        }
    
        public override string ToString()
        {
            return $"{Name} #{Id}";
        }
        public bool Equals(Entity? entity)
        {
            return this.Id == entity.Id;
        }
        public override bool Equals(object? obj)
        {
            return obj is Entity entity && Equals(entity);
        }
        public override int GetHashCode()
        {
            return Id;
        }
    }
}


