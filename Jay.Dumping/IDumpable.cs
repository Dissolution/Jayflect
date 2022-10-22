﻿namespace Jay.Dumping;

public interface IDumpable
{
    public string Dump(DumpFormat dumpFormat = default)
    {
        DefStringHandler stringHandler = new();
        DumpTo(ref stringHandler, dumpFormat);
        return stringHandler.ToStringAndClear();
    }

    void DumpTo(ref DefStringHandler stringHandler, DumpFormat dumpFormat = default);
}