# Tetris Clone

This is the third sample, and unlike the other two prior samples (simple-counter and sub-model), this project is written from scratch for Xelmish. It is more involved, and makes better use of the XNA engine behind the scenes.

Tetris is also paced in such a way that you can use a regular timer for game loop events. Later samples require to be hooked into the loop's update function (via the Program helper) in order to ensure smooth movement.

Each 'screen' in the game is an Elmish component with its own model, messages and update. The 'PlayScreen' is the most complicated, as it encompasses the logic of Tetris, but it is still fairly simple and should be easy to follow.