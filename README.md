# Xelmish - XNA + Elmish!

**Xelmish** is a small project that creates an **X**NA Game loop (via Mono Game) and connects it to the **Elmish** MVU architecure, via a custom setState method in its own version of the classic Elmish.Program module (Xelmish.Program). 

In this way, you can develop games using the excellent Elmish architecture, with all the power of an XNA renderer! You can also convert existing Elmish applications to use Xelmish by rewriting their view functions.

## Simple Example of Usage

## Development Info

Xelmish was developed first with Visual Studio Community 2017, then later with Visual Studio Community 2019, on various Windows 10 machines. A Visual Studio solution file is in the root of the project if you wish to build using these IDEs. However, it should be fully compilable from the command line and other IDEs if that is your preference.

It has been built with pure dotnet core 2.2, and you will need to have this installed to compile it. Xelmish and its samples have been tested on Windows 10 only.

## Samples description

## History and Reasoning

Xelmish has been built for the **[2019 F# Applied Competition](http://foundation.fsharp.org/applied_fsharp_challenge)**, but also as a replacement architecture for my prior [fsharp-gamecore](https://github.com/ChrisPritchard/fsharp-gamecore) experimental engine. 

While I have successfully built several small games with gamecore, I was finding that as my games increased in complexity the very simplistic model/view architecture in gamecore started to get stretched and warp. Things which were view-specific started to leak into model, and vice versa. 

In contrast the battle-tested Elmish model has, so far, proved a pleasure to work with. Much more elegant, and it has also achieved in a far better way my goal of having games being purely functional (where performance permits) and agnostic of engine. The MVU architecture, and parent-child relationships that the Elm architecture handles so well, mean that a game can be designed and theorised without having the engine get in the way, which is (in my opinion) ideal.

## License

Xelmish is provided under the **MIT** license. PLease contact me if you have issue with this. In addition, many if not all of the sample projects use fonts that are provided under the **SIL Open Font License**, a copy of which is in the root of the solution.