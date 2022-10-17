

using System.Collections;
using System.Diagnostics;
using System.Reflection;
using ConsoleSandbox.Toys;
using Jayflect;
using Jayflect.Extensions;

PropertyInfo property = Reflect.FindMember(() => 
    typeof(TestClass).GetProperty("Bleed", BindingFlags.Public | BindingFlags.Instance));


 
Debugger.Break();


