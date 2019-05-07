# Xelmish - XNA + Elmish!

**Xelmish** is a small project that creates an **X**NA Game loop (via Mono Game) and connects it to the **Elmish** MVU architecure, via a custom setState method in its own version of the classic Elmish.Program module (Xelmish.Program). 

In this way, you can develop games using the excellent Elmish architecture, with all the power of an XNA renderer! You can also convert existing Elmish applications to use Xelmish by rewriting their view functions.

To use Xelmish, the Elmish program must provide a view function that returns a list of 'viewables', functions which take an XNA SpriteBatch. A set of common such functions like colour, image and text are provided in the Xelmish Viewables helper module.

Xelmish is for **2D games** (the SpriteBatch object is for drawing textures, not rendering vertices). Hopefully it allows users to develop such games rapidly using the Elm architecture and F#!

## Simple Example of Usage

The simplest usage of Xelmish is shown in the first sample, [xelmish-first](./samples/xelmish-first/Program.fs). This sample renders a square to the screen, and allows you to move and resize it with key presses. It doesnt have any loaded assets like textures, fonts or sound, and therefore also doesn't require the monogame content pipeline. Nice and simple.

Once you have processed that, see the samples section below for a guide on the other, progressively more involved samples in the project.

## Development Info

Xelmish was developed first with Visual Studio Community 2017, then later with Visual Studio Community 2019, on various Windows 10 machines. A Visual Studio solution file is in the root of the project if you wish to build using these IDEs. However, it should be fully compilable from the command line and other IDEs if that is your preference.

It has been built with pure dotnet core 2.2, and you will need to have this installed to compile it. Xelmish and its samples have been tested on Windows 10, Mac OSX and Ubuntu 18.

### A note for Linux builders

On Linux the Monogame Content Pipeline may not work by default. If you get mono failure errors, try installing mono-complete, e.g. `sudo apt install mono-complete`. I was able to compile and run the samples on Ubuntu 18.04 after this without issue.

Note you also need the dotnet core 2.2 SDK to be installed on Linux in order to compile Xelmish and the samples.

## Samples description

Under [/samples](./samples), there are numerous projects that use Elmish and Xelmish. These are described below, in their order of complexity.

### 0. Xelmish-first

The most basic sample, described above. Just a coloured rectangle on the screen with move/resize commands.

### 1. Simple-Counter

The 'hello world' of Elmish, this sample should be almost identical (except for the Xelmish view) to other counters in other Elmish-* projects

### 2. Sub-Model

An app with two sub components, each containing a counter and a clock. Pretty similar to other samples in Elmish projects, but with Xelmish views

### 3. Tetris-Clone

The game tetris, implemented using several elmish components for screens, with a relatively simple Xelmish view. Much more involved than prior samples, but still simple enough to follow easily I hope.

### 4. Space-Invaders-Clone

A clone of 1979's space invaders, though not a hundred percent accurate to the old version. Compared to Tetris, Space Invaders requires a great deal more events, animations and individual entities, so it serves as a good demonstration of how the bulky (compared to direct imperative style) Elmish eventing model performs in such a context.

This is also the first sample that uses audio, with retro beeps and explosions based on game events. Sounds and music are a little complex to handle in the Elmish/Monogame structure, due to their temporal differences from textures, which makes it worth seeing a real world example.

## History and Reasoning

Xelmish has been built for the **[2019 F# Applied Competition](http://foundation.fsharp.org/applied_fsharp_challenge)**, but also as a replacement architecture for my prior [fsharp-gamecore](https://github.com/ChrisPritchard/fsharp-gamecore) experimental engine. 

While I have successfully built several small games with gamecore, I was finding that as my games increased in complexity the very simplistic model/view architecture in gamecore started to get stretched and warp. Things which were view-specific started to leak into model, and vice versa. 

In contrast the battle-tested Elmish model has, so far, proved a pleasure to work with. Much more elegant, and it has also achieved in a far better way my goal of having games being purely functional (where performance permits) and agnostic of engine. The MVU architecture, and parent-child relationships that the Elm architecture handles so well, mean that a game can be designed and theorised without having the engine get in the way, which is (in my opinion) ideal.

## License

Xelmish is provided under the **MIT** license. PLease contact me if you have issue with this. In addition, many if not all of the sample projects use fonts that are provided under the **SIL Open Font License**, a copy of which is in the root of the solution.
