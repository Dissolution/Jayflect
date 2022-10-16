using System.Reflection.Emit;

namespace Jay.Reflection.Building.Backing;

public sealed record class ConstructorImpl(FieldBuilder InstanceField, ConstructorBuilder Constructor);