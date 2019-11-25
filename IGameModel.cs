using System;
using System.Collections.Generic;

namespace peggame
{
    interface IGameModel
    {
        char? ChooseStartingPeg();
        bool PerformNextJump(Dictionary<char, bool> pegs);
        bool PlayAgain(Dictionary<char, bool> pegs);
        void PrintStats();
    }
}
