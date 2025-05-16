using UnityEngine;

namespace Combat
{
    public class Quantizer
    {
        public static Vector3 FItoVec3(float magnitude, int packedData)
        {
            float xComponent;
            float zComponent;
            float yComponent;
            float tempValue;
            float swapValue;
        
            int axisConfig = packedData & 3;
            xComponent = (float)(packedData >> 0x11) / 16383.0f;
            yComponent = (float)((packedData << 0xf) >> 0x11) / 16383.0f;
            swapValue = xComponent;
            tempValue = 1.0f;
        
            if (axisConfig == 1)
            {
                swapValue = 1.0f;
                tempValue = xComponent;
            }
        
            zComponent = yComponent;
            if (axisConfig != 2)
            {
                zComponent = swapValue;
                xComponent = tempValue;
            }
        
            tempValue = 1.0f;
            if (axisConfig != 2)
            {
                tempValue = yComponent;
            }
        
            return new Vector3(xComponent * magnitude, zComponent * magnitude, tempValue * magnitude);
        }
        
        public static (float, int) Vec3toFI(Vector3 v)
        {
            uint uVar1;
            float fVar2;
            bool bVar3;
            uint uVar4;
            float fVar5;
            float fVar6;
            float fVar7;
            float fVar8;
            float fVar9;
            float fVar10;
            float fVar11;
            float fVar12;
            float fVar13;
            (float, int) local_18;

            fVar5 = v.x;
            fVar7 = v.y;
            fVar11 = Mathf.Abs(fVar7);
            fVar12 = v.z;
            local_18.Item1 = 0.0f;
            local_18.Item2 = 0;
            fVar13 = Mathf.Abs(fVar12);
            fVar9 = Mathf.Abs(fVar5);
            fVar6 = fVar5;
            fVar2 = fVar7;
            fVar10 = fVar12;
            if (fVar9 <= fVar13) {
                fVar6 = fVar12;
                fVar2 = fVar5;
                fVar10 = fVar7;
            }
            fVar8 = fVar12;
            if (fVar13 <= fVar11) {
                fVar8 = fVar7;
                fVar7 = fVar12;
            }
            uVar4 = 2;
            if (fVar13 <= fVar11) {
                uVar4 = 1;
            }
            bVar3 = fVar9 < fVar11;
            if (bVar3) {
                fVar6 = fVar8;
                fVar2 = fVar5;
            }
            if (bVar3) {
                fVar10 = fVar7;
            }

            var isBigger = 0;
            if (fVar9 <= fVar13) {
                isBigger = 1;
            }
            uVar1 = (uint) isBigger << 1;
            if (bVar3) {
                uVar1 = uVar4;
            }
            fVar7 = (fVar2 / fVar6) * 16383.0f;
            fVar10 = (fVar10 / fVar6) * 16383.0f;
            fVar2 = -fVar7;
            if (!float.IsInfinity(fVar7)) {
                fVar2 = fVar7;
            }
            
            fVar7 = -fVar10;
            if (!float.IsInfinity(fVar10)) {
                fVar7 = fVar10;
            }
            return (fVar6, (int)fVar2 << 17 | (int)uVar1 | ((int)fVar7 & 0x7FFF) << 2);
        }
    }
}