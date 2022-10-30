using Jay.Dumping;
using Jay.Dumping.Extensions;
using Jayflect.Extensions;

namespace Jayflect.Dumping;

public sealed class EventInfoDumper : Dumper<EventInfo>
{
    protected override void DumpImpl(ref DumpStringHandler stringHandler, [NotNull] EventInfo eventInfo, DumpFormat format)
    {
        stringHandler.Dump(eventInfo.EventHandlerType, format);
        stringHandler.Write(' ');
        if (format > DumpFormat.View)
        {
            stringHandler.Dump(eventInfo.OwnerType(), format);
            stringHandler.Write('.');
        }
        stringHandler.Write(eventInfo.Name);
    }
}