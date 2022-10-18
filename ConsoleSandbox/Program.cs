using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using Jay.Dumping;
using Jay.Dumping.Extensions;
using static Jay.Dumping.Extensions.DumperImport;

using Jayflect;

var flags = Jayflect.Reflect.Flags.Instance;
var ctor = Reflect.FindConstructor<Program>();


var k = DumperCache.KnownDumpers;

var str = Dump($"This {typeof(int)} {147} is Lit! {typeof(IList<>)}");
var strb = Dump((ReadOnlySpan<char>)"blah");
var strc = typeof(Program).Dump();


Console.WriteLine(str);
Debugger.Break();


static void DoThing(DumpFormat options)
{
    var str = options.ToString();
    Console.WriteLine(str);
}


