using System;

namespace ParkingFeeControl
{
    public static class ModLogger
    {
        public static void Info(string message)
        {
            Mod.Log?.Info(message);
        }

        public static void Warn(string message)
        {
            Mod.Log?.Warn(message);
        }

        public static void Error(string message)
        {
            Mod.Log?.Error(message);
        }

        public static void Debug(string message)
        {
            if (Mod.Settings?.DebugLogging == true)
            {
                Mod.Log?.Info(message);
            }
        }

        public static void Debug(Func<string> messageFactory)
        {
            if (Mod.Settings?.DebugLogging == true)
            {
                Mod.Log?.Info(messageFactory());
            }
        }
    }
}
