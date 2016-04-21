namespace Onwo.IO
{
    public enum DirectoryCompareResult
    {
        NoResult = 0,
        Equal = 1,
        SubDirectory = 2,
        ParentDirectory = 4,
        Unrelated = 8
    }
}
