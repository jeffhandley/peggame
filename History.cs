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
}
