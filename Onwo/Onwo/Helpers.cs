namespace Onwo
{
    public static class Helpers
    {
        public static void Swap<T>(ref T item1, ref T item2)
        {
            T tmp = item1;
            item1 = item2;
            item2 = tmp;
        }
        
    }

}
