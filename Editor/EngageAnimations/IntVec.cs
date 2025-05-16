using UnityEngine;

namespace Combat
{
    public class IntVec
    {
        // for use with Combat.SignalArgsReaderWriter_Hit$$HitSlashDir
        public static Vector3 Decode(int intValue)
        {
            int x = Bit.GetSigned(intValue, 7, 0);
            int y = Bit.GetSigned(intValue, 7, 7);
            int z = Bit.GetSigned(intValue, 7, 14);

            float scaleFactor = 0.015873017f;
            Vector3 result = new Vector3(x * scaleFactor, y * scaleFactor, z * scaleFactor);

            return result;
        }
        
        public static int Encode(Vector3 vector)
        {
            // Extract vector components
            float z = vector.z;
            float y = vector.y;
            float x = vector.x;
    
            // Normalize and scale
            Vector3 normalized = vector.normalized;
            x = normalized.x * 63.0f;
            y = normalized.y * 63.0f;
            z = normalized.z * 63.0f;
    
            // Check for infinity and use actual value (otherwise use original)
            float xVal = x;
            if (float.IsInfinity(x))
                xVal = -x;
        
            float yVal = y;
            if (float.IsInfinity(y))
                yVal = -y;
        
            float zVal = z;
            if (float.IsInfinity(z))
                zVal = -z;
    
            uint srcValue = (uint)Bit.Combine(0, (int)xVal, 7, 0);
            uint uVar2 = (uint)Bit.Combine((int)srcValue, (int)yVal, 7, 7);
            uint uVar3 = (uint)Bit.Combine((int)(uVar2 | srcValue), (int)zVal, 7, 0xe);
    
            return (int)(uVar3 | uVar2 | srcValue);
        }
    }
}
