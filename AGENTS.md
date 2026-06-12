# AGENTS.md

## Cursor Cloud specific instructions

BlockShy is a single .NET 8.0 console application (a minimal blockchain demo). The entire program lives in `Program.cs`, with the project defined by `blockshy.csproj` (`net8.0`, `OutputType=Exe`).

- Build: `dotnet build`
- Run: `dotnet run` — mines 10 blocks against a difficulty prefix and prints chain validity at each step, ending with `Done!`. Each printed hash starts with `0000` (the 2-byte `0x00 0x00` difficulty), so each block can take a moment to mine; the full run typically finishes in a few seconds.
- Format/lint: `dotnet format --verify-no-changes`. Note: the committed `Program.cs` does not satisfy the default formatter (it reports pre-existing whitespace style deviations), so this command exits non-zero on a clean checkout. Do not treat that as a regression.
- There is no test project in this repository, so there are no automated tests to run.
