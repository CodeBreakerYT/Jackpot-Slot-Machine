# Jackpot Slot Machine

## Game Overview
Jackpot Slot Machine is a 2D interactive casino game built with Unity. It features a fully functional slot machine interface where players can bet in-game currency, spin the reels, and aim for a jackpot matching all symbols. The game includes a robust betting system, smooth reel animations, sound effects, and an exciting win presentation.

## Instructions to Run WebGL Build
1. Locate the `webgl_build` folder in the project directory.
2. You can run the game by uploading the contents of this folder to a local web server (like Python's `http.server` or Node's `http-server`) or hosting it on a platform like Itch.io or GitHub Pages.
   - Using Python 3: Open a terminal in the `webgl_build` directory and run `python -m http.server 8000`. Then open `http://localhost:8000` in your web browser.
3. Once the game loads, you'll start with 1000 in currency.
4. Select your bet amount (500, 1000, or 2000) using the bet buttons.
5. Click the "Spin" button to spin the reels.
6. Match all symbols across the reels to win the jackpot!

## Bonus Features
- **Dynamic Betting System:** Choose from multiple bet amounts (500, 1000, 2000). The UI intelligently hides the spin and betting buttons while the reels are spinning to prevent sequence-breaking.
- **Suspenseful Staggered Stops:** Reels do not stop simultaneously. They stop one after another with an increasing delay to build excitement and suspense.
- **Smooth Snap Animations:** When the reels finish spinning, they use an `easeOutCubic` smoothing function to seamlessly snap the final symbols perfectly into place on the grid.
- **Animated Win Pop-up:** Upon hitting a jackpot, a celebratory pop-up message smoothly fades in and scales up to congratulate the player.
- **Audio Feedback:** The game includes immersive audio, featuring a continuous looping spin sound while active, distinct stop sounds for each reel, and a celebratory sound when you win.
- **Auto-Reset:** After a jackpot is won and the celebration finishes, the game automatically reloads to let you play again.

## Thought Process or Approach
1. **Architecture & Organization:** The project logic is separated into manageable components. The `SlotGameManager` handles the high-level game state, betting mechanics, and UI orchestration, while individual `SlotReel` scripts handle the localized physics and smooth animations of the scrolling symbols. Data structure is kept modular using `SymbolData` objects to define different sprites and payouts.
2. **Infinite Scrolling Reels:** To simulate spinning, the `SlotReel` script continuously translates RectTransforms downwards. When a symbol moves out of the bottom bounds, it is instantaneously teleported to the top of the stack and randomly reassigned a new symbol sprite to create the illusion of an endless reel.
3. **Determining the Outcome:** Instead of relying entirely on physics for the final result, the `SlotGameManager` pre-determines the final stop time for each reel. When a `SlotReel` begins its stopping sequence, it chooses a random final outcome, smoothly interpolates its current symbols to snap exactly onto the grid, and guarantees the assigned outcome appears directly in the center row.
4. **User Experience (UX):** Great care was taken to polish the user experience. Adding staggered delays between reel stops vastly improves the "slot machine feel". Similarly, restricting UI interactions (hiding buttons) while the spin sequence is active ensures the game state cannot be accidentally corrupted or interrupted by the player.