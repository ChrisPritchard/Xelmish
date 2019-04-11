# Tetris Clone

This is the third sample, and unlike the other two prior samples (simple-counter and sub-model), this project is written from scratch for Xelmish. It is more involved, and makes better use of the XNA engine behind the scenes.

Each 'screen' in the game is an Elmish component with its own model, messages and update. However the 'Playing' screen is the most complicated obviously, so this readme will focus on deconstructing that.

## 'Playing' component

## Todo:

- fix drop and tick overlap
  - tick should be on a timer, and reset on drop
  - drop should be on key down, or maybe double tick rate
    - onkeydown, onkeyup changing a 'down flag'?
    - tick controlled by a subscription tied to update?
- present score and lines. maybe level?
  - change tick rate by level to min of 100ms
- present next shape (precalculate next)
- start screen (show high score)
- game over screen (show score vs high score)
- sounds?