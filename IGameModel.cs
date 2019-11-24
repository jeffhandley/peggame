using System;
using System.Collections.Generic;

namespace peggame
{
    interface IGameModel
    {
        char? ChooseStartingPeg(Dictionary<char, bool> pegs);
        Jump? ChooseNextJump(Dictionary<char, bool> pegs);
        bool PlayAgain(Dictionary<char, bool> pegs);
        void PrintStats();
    }
}