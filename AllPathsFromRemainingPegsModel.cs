using System;
using System.Collections.Generic;

namespace peggame
{
    class AllPathsFromRemainingPegsModel : IGameModel
    {
        char startingPeg = '1';

        public char? ChooseStartingPeg(Dictionary<char, bool> pegs)
        {
            return startingPeg;
        }

        public Jump? ChooseNextJump(Dictionary<char, bool> pegs)
        {
            return null;
            // var jumps = GameInterface.GetPossibleJumps(pegs);
        }

        public bool PlayAgain(Dictionary<char, bool> pegs)
        {
            return false;
        }

        public void PrintStats()
        {

        }
    }
}
