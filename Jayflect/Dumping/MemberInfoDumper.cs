using Jay.Dumping;
using Jay.Dumping.Interpolated;

namespace Jayflect.Dumping;

[DumpOptions(10)] // late!
public class MemberInfoDumper : Dumper<MemberInfo>
{
    protected override void DumpImpl(ref DumpStringHandler stringHandler, 
        [DisallowNull] MemberInfo value, DumpFormat format)
    {
        // MemberInfo type not known at find time
        if (value is FieldInfo field)
            DumperCache.GetDumper<FieldInfo>().DumpTo(ref stringHandler, field, format);
        else if (value is PropertyInfo property)
            DumperCache.GetDumper<PropertyInfo>().DumpTo(ref stringHandler, property, format);
        else if (value is EventInfo @event)
            DumperCache.GetDumper<EventInfo>().DumpTo(ref stringHandler, @event, format);
        else if (value is ConstructorInfo ctor)
            DumperCache.GetDumper<ConstructorInfo>().DumpTo(ref stringHandler, ctor, format);
        else if (value is MethodBase method)
            DumperCache.GetDumper<MethodBase>().DumpTo(ref stringHandler, method, format);
        else if (value is Type type)
            DumperCache.GetDumper<Type>().DumpTo(ref stringHandler, type, format);
        else
        {
            throw new NotImplementedException();
        }
    }
}