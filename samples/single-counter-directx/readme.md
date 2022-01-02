# Simple Counter - DirectX Sample

This project is identical to the other single counter project, except it uses Windows DX APIS.

Note the changes:

- in the fsproj the output type is set to `WinExe`
- the targetframework is set to `net5.0-windows`
- there is a new property `<UseWindowsForms>true</UseWindowsForms>`
- and the nuget package reference references windowsdx rather than desktop gl

Obviously this project will not work on non-windows platforms however.

I have also added an [app.manifest](./app.manifest) file with a corresponding entry in the project file. This can be used to control how aware of dpi settings the app is on windows for example (e.g. this sample looks tiny on my high dpi surface book)