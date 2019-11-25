using System;
using System.Collections.Generic;

namespace peggame
{
    interface IGameModel
    {
        bool RemoveStartingPeg(Dictionary<char, bool> pegs);
        bool PerformNextJump(Dictionary<char, bool> pegs);
        bool PlayAgain(Dictionary<char, bool> pegs);
        void PrintStats();
    }
}
