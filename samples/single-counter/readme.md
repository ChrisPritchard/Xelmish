# Simple Counter Sample

This is a very simple project, the archetypical 'hello world' of Elmish: a counter with adjustable step size.

Like sub model, this sample project also demonstrates Xelmish's compatibility with the general Elmish ecosystem: all the code in this project except the view and program setup is lifted directly from Elmish.WPF's sample code (which in turn is lifted from core Elmish). I literally copy-pasted the model, messages, update and init functions, replaced the 'bindings' function with a function that creates Xelmish viewables, and it was done.

Worth looking at if you would like to see the near minimum architecture you can have with Xelmish/Elmish. However note that this sample and the next are UI heavy, where Xelmish and XNA in general is weaker than a UI-focused framework like Fable and Elmish.WPF. Other samples are more game like.

The font used is 'SourceCodePro-Semibold' which is under the SIL Open Font License.