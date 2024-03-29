using System;
using System.Collections.Generic;

namespace peggame.History
{
    class JumpRecord
    {
        public char From {get; set;}
        public char Over {get; set;}
        public char To {get; set;}
        public Jump Jump;
        public int JumpIndex {get; set;}

        public JumpRecord(Jump jump, int index)
        {
            this.From = jump.From;
            this.Over = jump.Over;
            this.To = jump.To;
            this.Jump = jump;
            this.JumpIndex = index;
        }
    }

    class JumpList : List<JumpRecord>
    {
    }

    class GameRecord
    {
        public JumpList JumpList {get; set;}
        public char[] PegsRemaining {get; set;}

        public GameRecord(JumpList jumps, char[] pegsRemaining)
        {
            this.JumpList = jumps;
            this.PegsRemaining = pegsRemaining;
        }
    }
}
