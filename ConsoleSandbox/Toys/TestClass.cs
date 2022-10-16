namespace ConsoleSandbox.Toys;

public class TestClass
{
    public int Id { get; set; }

    public string Name { get; set; } = "";
    
    public Guid Guid { get; }
    
    public TestClass() {}
    public TestClass(int id, string name)
    {
        this.Id = id;
        this.Name = name;
        this.Guid = Guid.NewGuid();
    }
}