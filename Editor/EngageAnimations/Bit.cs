namespace Combat
{
    public class Bit
    {
        public static int GetSigned(int srcValue, int bits, int shift)
        {
            return (srcValue << ((0x20 - bits) - shift & 0x1f)) >> (0x20 - bits & 0x1f);
        }
        
        public static int Get(int srcValue, int bits, int shift)
        {
            return (srcValue >> (shift & 0x1f)) & (~(-1 << (bits & 0x1f)));
        }
        
        public static int Combine(int srcValue, int value, int bits, int shift)
        {
            uint mask = (uint)(~(-1 << (bits & 0x1f)) << (shift & 0x1f));
            return (srcValue & (int)~mask) | ((value << (shift & 0x1f)) & (int)mask);
        }
    }
}