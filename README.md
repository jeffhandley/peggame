# Peg Game

If you've ever been to Cracker Barrel, you've seen and probably played the peg game that sits on every table. There is [plenty of documentation about the game](https://www.google.com/search?q=peg+game+cracker+barrel) if you're interested in learning more.

## Game Setup

The game board is a triangle with 15 holes in it. You have 14 pegs (often golf tees) on the board, and it's up to you to decide which hole to leave open at the start of the game.

In this implementation of the peg game, the holes are labeled using letters and numbers, as seen below.

``` txt
             1
            2 3
           4 5 6
          7 8 9 0
         A B C D E
```

At the start of the game, you will choose which starting peg to remove. To choose a peg, press the key that corresponds with its label of `1`, `2`, `3`, `4`, `5`, `6`, `7`, `8`, `9`, `0`, `A`, `B`, `C`, `D`, or `E`.

This leaves 14 pegs in 15 holes and gameplay begins.

## Gameplay

After the first peg is removed, you begin making jumps with the pegs. You jump one peg over another peg into an empty hole--when you complete the jump, the peg that was jumped over is removed from the game. You continue making more jumps until there are no more possible jumps.

The object of the game is to conduct a series of jumps that leave 1 single peg at the end of the game. There's no way to jump the last peg, so leaving a single peg wins the game.

## Running the Game

This game requires [.NET Core 3.0](https://docs.microsoft.com/en-us/dotnet/core/#download-net-core). After cloning the repository, `dotnet run` will start the game.

You'll be presented with the board and asked to choose your starting peg to remove. Once that peg is removed, you'll be prompted to start making jumps. The possible jumps are listed out for you, and you'll enter the peg to jump with and the peg to jump over.

``` txt
             ∘
            2 3
           4 5 6
          7 8 9 0
         A B C D E

Choose the peg to jump with: 4
Choose the peg to jump over:


Possible Jumps:
  - Jump 4 over 2
  - Jump 6 over 3

Press 'H' for hints
```

During gameplay, you can press `ESC` at any time to cancel the current jump or end the game.

## Hints

During gameplay, you're offered hints. Hints will help you choose jumps that can lead to winning the game by showing you the number of gameplay possibilities, the best and worst scores possible from each jump, how many possible wins exist for the jump, and what the win rate percentage is.

``` txt
             ∘
            2 3
           4 5 6
          7 8 9 0
         A B C D E

Choose the peg to jump with:



Possible Jumps:   (Possibilities: 568,630 - Best/Worst Score: 1/8 - Wins: 29,760 - Win Rate:   5.23%)
  - Jump 4 over 2 (Possibilities: 284,315 - Best/Worst Score: 1/8 - Wins: 14,880 - Win Rate:   5.23%)
  - Jump 6 over 3 (Possibilities: 284,315 - Best/Worst Score: 1/8 - Wins: 14,880 - Win Rate:   5.23%)
```

## Difficulty Levels

The game has 3 difficulty levels:

* Easy
* Medium (default)
* Hard
* Expert

### Easy

To run the game in Easy mode, use `dotnet run -easy`. In Easy mode, the game will automatically show hints for every jump. You can then easily follow along with jumps that lead to winning.

If you choose a jump that would lead to a loss, the game will let you know it might not be the best jump possible.

``` txt
This might not be the best jump. Press ESC to choose another jump or any other key to continue.
```

### Medium (default)

The default difficulty level is Medium, using `dotnet run`. In Medium mode, hints are available for every jump, but they are not displayed automatically. Press `H` to see hints for any jump.

If you choose a jump that would lead to a loss, the game will let you know it might not be the best jump possible.

``` txt
This might not be the best jump. Press ESC to choose another jump or any other key to continue.
```

### Hard

To run the game in Hard mode, use `dotnet run -hard`. In Hard mode, hints are available for every jump, but they are not displayed automatically. Press `H` to see hints for any jump.

In Hard mode, you will not be warned if you choose a jump that leads to a loss.

### Expert

To run the game in Expert mode, use `dotnet run -expert`. In Expert mode, hints are not available and you will not be warned if you choose a jump that leads to a loss.

## Game Stats

The game is winnable from all starting peg positions, but some starting positions have better chances than others. Here are the game statistics.

To calculate all game stats, use `dotnet run -stats`. Here are the stats that get calculated.

``` txt
Peg 1: Possibilities:   568,630 - Best/Worst Score:  1/8  - Wins:  29,760 - Win Rate:   5.23%
Peg 2: Possibilities:   294,543 - Best/Worst Score:  1/8  - Wins:  14,880 - Win Rate:   5.05%
Peg 3: Possibilities:   294,543 - Best/Worst Score:  1/8  - Wins:  14,880 - Win Rate:   5.05%
Peg 4: Possibilities: 1,149,568 - Best/Worst Score:  1/7  - Wins:  85,258 - Win Rate:   7.42%
Peg 5: Possibilities:   137,846 - Best/Worst Score:  1/10 - Wins:   1,550 - Win Rate:   1.12%
Peg 6: Possibilities: 1,149,568 - Best/Worst Score:  1/7  - Wins:  85,258 - Win Rate:   7.42%
Peg 7: Possibilities:   294,543 - Best/Worst Score:  1/8  - Wins:  14,880 - Win Rate:   5.05%
Peg 8: Possibilities:   137,846 - Best/Worst Score:  1/10 - Wins:   1,550 - Win Rate:   1.12%
Peg 9: Possibilities:   137,846 - Best/Worst Score:  1/10 - Wins:   1,550 - Win Rate:   1.12%
Peg 0: Possibilities:   294,543 - Best/Worst Score:  1/8  - Wins:  14,880 - Win Rate:   5.05%
Peg A: Possibilities:   568,630 - Best/Worst Score:  1/8  - Wins:  29,760 - Win Rate:   5.23%
Peg B: Possibilities:   294,543 - Best/Worst Score:  1/8  - Wins:  14,880 - Win Rate:   5.05%
Peg C: Possibilities: 1,149,568 - Best/Worst Score:  1/7  - Wins:  85,258 - Win Rate:   7.42%
Peg D: Possibilities:   294,543 - Best/Worst Score:  1/8  - Wins:  14,880 - Win Rate:   5.05%
Peg E: Possibilities:   568,630 - Best/Worst Score:  1/8  - Wins:  29,760 - Win Rate:   5.23%

Total: Possibilities: 7,335,390 - Best/Worst Score:  0/10 - Wins: 438,984 - Win Rate:   5.98%
```
