# Tetris Clone

This is the third sample, and unlike the other two prior samples (simple-counter and sub-model), this project is written from scratch for Xelmish. It is more involved and much more 'game-like' rather than user interface heavy.

Each 'screen' in the game is an Elmish component with its own model, messages and update. The 'PlayScreen' is the most complicated, as it encompasses the logic of Tetris, but it is still fairly simple and should be easy to follow. 

The progression of the game is driven through listening to keystrokes and through event methods that run on every update in the core game loop (all nicely abstracted away to fit with the Elmish model).
