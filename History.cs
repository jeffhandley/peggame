using System;
using System.Collections.Generic;

namespace peggame.History
{
    class JumpRecord
    {
        public char From;
        public char Over;
        public char To;
        public Jump Jump;
        public int JumpIndex;

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
        public char StartingPeg;
        public JumpList JumpList;
        public int PegsRemaining;

        public GameRecord(char startingPeg, JumpList jumps, int pegsRemaining)
        {
            this.StartingPeg = startingPeg;
            this.JumpList = jumps;
            this.PegsRemaining = pegsRemaining;
        }
    }

    class GameList : List<GameRecord>
    {
    }
}
