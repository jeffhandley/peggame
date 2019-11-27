using System;
using System.Collections.Generic;

namespace peggame
{
    class Hints
    {
        public int Possibilities {get; set;}
        public int Wins {get; set;}
        public int BestScore {get; set;}
        public int WorstScore {get; set;}

        public decimal WinRate {
            get {
                if (Possibilities > 0) {
                    return (decimal)Wins / (decimal)Possibilities;
                }

                return (decimal)0;
            }
        }
    }

    class GameHints : Hints
    {
        public Dictionary<int, Hints> JumpHints {get; set;}

        public GameHints()
        {
            JumpHints = new Dictionary<int, Hints>();
        }
    }
}
