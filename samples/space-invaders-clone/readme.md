# Space Invaders Clone

A partial recreation of the 1979 classic. In contrast to Tetris, there are many more things going on with the game at the same time, pushing Elmish-architecture a little further.

This sample also uses artwork, specifically a sprite sheet for the player and invader graphics. The invaders and projectiles are animated, so you can see a way that can be done with Xelmish.

## Sound System

One of the interesting elements of this sample is how sounds are handled. A sound is played due to a specific event, which means in the Elmish model it best fits to be set in either the update function, or in the view function when an event is dispatched. However, the way 'view's work, is they are set and re-rendered on every view of the XNA loop until the model is changed; a sound played ondraw will therefore get played hundreds of times a second (on fast machines) - not ideal: you want the option to usually play a sound once.

The solution is not built in to Xelmish, though a helper provides one option which is what is used in this sample: add a Queue to your model, and enqueue the keys of sounds to play. Then use a dequeue on each draw to play the next sound, ensuring each enqueued sound is played just once.

While this is a mutable solution using a .NET collections class that feels a bit wonky to use from F# (compared to the rest of the nearly purely functional code), it does work perfectly. So if that solution works for you and you don't mind a slightly hybrid model type, then go for it!

## Credits

Space Invader sprites by [GooperBlooper22](https://www.deviantart.com/gooperblooper22), acquired from [here](https://www.deviantart.com/gooperblooper22/art/Space-Invaders-Sprite-Sheet-135338373). These have been provided under no specific license, but credit given anyway.

The game itself, for mechanic reference, was played via the [Internet Archive](https://archive.org) where it is published [here](https://archive.org/details/Space_Invaders_1985_Sega_Taito). Space Invaders was a bit before my time (i.e. published years before I was born) so having a reference to recreate was very useful. [This article](http://www.classicgaming.cc/classics/space-invaders/play-guide) was also very useful.

The font used is "Press Start 2P" from [here](https://fontlibrary.org/en/font/press-start-2p). It is available under the SIL Open Font License, a copy of which is in the root of this solution.

The sounds are from user [Krial](https://opengameart.org/users/krial) on [OpenGameArt.Org](https://opengameart.org), specifically the [siclone sound effects pack](https://opengameart.org/content/siclone-sound-effects), under the [CCO 1.0 license](http://creativecommons.org/publicdomain/zero/1.0/). I didn't use the sounds as they are named - e.g. I use menu for the 'beep' sound on shuffle. Just my preference.