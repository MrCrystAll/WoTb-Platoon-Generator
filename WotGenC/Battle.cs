using System;
using System.Timers;

namespace WotGenC.Missions
{
    public class Battle
    {
        public GameMode Mode;
        public bool Started { get; set; }
        public TimeSpan RemainingTime { get; set; }
        
        public System.Timers.Timer Clock { get; set; }

        public Battle(GameMode mode)
        {
            Mode = mode;
        }

        public void Start()
        {
            //TODO: Get timer
            Started = true;
            
            //List of requests
            string hp = 
                "return readInteger(readPointer(readPointer(readPointer(readPointer(readPointer(\"THREADSTACK0-0000260\") + 0x1C) + 0x2C) + 0x8) + 0x0) + 0x18C)";
            string timer =
                "return readInteger(readPointer(readPointer(readPointer(readPointer(readPointer(readPointer(readPointer(\"D3DCOMPILER_47.dll+0027DFA4\") + 0x114) + 0x4) + 0x3C) + 0x4C) + 0x4C) + 0x5C) + 0x10)";
            Clock.Start();
        }
    }
}