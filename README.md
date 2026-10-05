# HADecompiled

HADecompiled (short for Hybrid Animals Decompiled) is a decompilation of the Hybrid Animals mobile game, starting specifically at version v200613.

Game versions above v185 were compiled with IL2CPP, which leaves the C# scripts as empty dummy stubs. To get them working again, the project required reverse engineering `libil2cpp.so`. Most of the conversion of the decompiled C code back into proper C# was done by Claude using [Claude Code](https://claude.com/product/claude-code), which I wholeheartedly endorse.

## Getting Started

### Prerequisites

- Unity 2021.3.45f1

> **Note:** This Unity version has a [known security vulnerability](https://unity.com/security/sept-2025-01), but it is the engine version the game uses.

### Building

1. Add the project to your Unity Editor.
2. Build to your platform of choice.

## License

This project has no license. The code is shared solely for interoperability purposes with the [HAModLoader](https://github.com/eris-webserv/HAModHelper) project.

All game assets and code belong to © [Abstract Software Inc.](https://www.abstractsoftwares.com/)

This project is not affiliated with or endorsed by Abstract Software Inc.
