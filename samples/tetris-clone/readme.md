# Tetris Clone

This is the third sample, and unlike the other two prior samples (simple-counter and sub-model), this project is written from scratch for Xelmish. It is more involved, and makes better use of the XNA engine behind the scenes.

Tetris is also paced in such a way that you can use a regular timer for game loop events. Later samples require to be hooked into the loop's update function (via the Program helper) in order to ensure smooth movement.

Each 'screen' in the game is an Elmish component with its own model, messages and update. However the 'PlayScreen' is the most complicated obviously, so this readme will focus on deconstructing that.

## 'PlayScreen' component

## Todo:

- DONE fix drop and tick overlap
  - tick should be on a timer, and reset on drop
  - drop should be on key down, or maybe double tick rate
    - DONE onkeydown, onkeyup changing a 'down flag'?
    - tick controlled by a subscription tied to update?
- present score and lines. maybe level?
  - change tick rate by level to min of 100ms
- DONE present next shape (precalculate next)
- start screen (show high score)
- game over screen (show score vs high score)
- sounds?
- DONE escape to exit game
- DONE side overlaps should not stop a shape, just block move