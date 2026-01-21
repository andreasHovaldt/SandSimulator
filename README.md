# Sand Simulator

A falling sand simulation built in C# with Raylib. Made as a learning project to get familiar with C# and experiment with GitHub Actions for CI/CD.

## Download

Pre-built binaries for Windows and Linux are available on the [Releases](https://github.com/andreasHovaldt/SandSimulator/releases) page.

## Building from Source

Requires .NET 10 SDK.

```bash
git clone git@github.com:andreasHovaldt/SandSimulator.git
cd SandSimulator
dotnet run
```

For a release build with better performance:

```bash
dotnet run -c Release
```

## Controls

- Left click: Spawn sand
- Right click: Spawn water
- Middle click: Spawn rock
- C: Clear scene
- B: Run benchmark