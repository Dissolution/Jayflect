using Jay.Text;

namespace Jay.Reflection.Building.Emission;

public class InstructionStream : LinkedList<Instruction>
{
    public Instruction? FindByOffset(int offset)
    {
        if (offset < 0 || this.Count == 0)
            return null;
        foreach (var node in this)
        {
            if (node.Offset == offset)
                return node;
            if (node.Offset > offset)
                return null;
        }
        return null;
    }

    public override string ToString()
    {
        var text = TextBuilder.Borrow();
        text.AppendDelimit<Instruction>(Environment.NewLine, this);
        return text.ToString();
    }
}