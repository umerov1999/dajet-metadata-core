namespace DaJet.Data
{
    public enum ColumnType
    {
        Pointer,  // _TYPE  0x01 = Неопределено
        Boolean,  // _L     0x02
        Numeric,  // _N     0x03
        DateTime, // _T     0x04
        String,   // _S     0x05
        // ? Binary // _B   0x06 | 0x07 ?
        TypeCode, // _RTRef 0x08
        Object    // _RRRef 0x08
    }
}
