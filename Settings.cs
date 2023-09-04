using ThunderRoad;
using UnityEngine;

namespace XESuitPod
{
    public class BatmanArkhamLoader : ThunderScript
    {
        public static ModOptionFloat[] pointIncrement()
        {
            ModOptionFloat[] options = new ModOptionFloat[101];
            float val = 0f;
            for (int i = 0; i < options.Length; i++)
            {
                options[i] = new ModOptionFloat(val.ToString("0.0"), val);
                val += 0.05f;
            }
            return options;
        }
        public static ModOptionFloat[] zeroToOneHundred()
        {
            ModOptionFloat[] options = new ModOptionFloat[101];
            float val = 0f;
            for (int i = 0; i < options.Length; i++)
            {
                options[i] = new ModOptionFloat(val.ToString("0.0"), val);
                val += 1f;
            }
            return options;
        }

        [ModOption("Cape Height", "How high the cape sits on you", valueSourceName = nameof(pointIncrement), defaultValueIndex = 27)]
        public static float CapeHeight;
        [ModOption("Gliding Speed", "How quickly you go forward", valueSourceName = nameof(zeroToOneHundred), defaultValueIndex = 35)]
        public static float GlideSpeed;

    }
}