using Jay.Dumping;
using Jay.Dumping.Extensions;
using Jayflect.Extensions;

namespace Jayflect.Dumping;

public sealed class EventInfoDumper : Dumper<EventInfo>
{
    protected override void DumpValueImpl(ref DefaultInterpolatedStringHandler stringHandler, [NotNull] EventInfo eventInfo, DumpFormat dumpFormat)
    {
        stringHandler.Dump(eventInfo.EventHandlerType, dumpFormat);
        stringHandler.Write(' ');
        if (dumpFormat > DumpFormat.View)
        {
            stringHandler.Dump(eventInfo.OwnerType(), dumpFormat);
            stringHandler.Write('.');
        }
        stringHandler.Write(eventInfo.Name);
    }
}