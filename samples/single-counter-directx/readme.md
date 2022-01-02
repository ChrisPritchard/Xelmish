# Simple Counter - DirectX Sample

This project is identical to the other single counter project, except it uses Windows DX APIS.

Note the changes:

- in the fsproj the output type is set to `WinExe`
- the targetframework is set to `net5.0-windows`
- there is a new property `<UseWindowsForms>true</UseWindowsForms>`
- and the nuget package reference references windowsdx rather than desktop gl

Obviously this project will not work on non-windows platforms however.