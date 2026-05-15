namespace BDOLanguageUpdater.Service.Serializer;

internal readonly record struct LocRecordKey(
    uint StrType,
    uint StrId1,
    ushort StrId2,
    byte StrId3,
    byte StrId4);
