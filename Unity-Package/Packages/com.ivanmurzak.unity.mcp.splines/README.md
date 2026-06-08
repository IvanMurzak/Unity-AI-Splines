<h1 align="center"><a href="https://github.com/IvanMurzak/Unity-AI-Splines?tab=readme-ov-file#unity-ai-splines">Unity AI Splines</a></h1>

<div align="center" width="100%">

[![MCP](https://badge.mcpx.dev 'MCP Server')](https://modelcontextprotocol.io/introduction)
[![OpenUPM](https://img.shields.io/npm/v/com.ivanmurzak.unity.mcp.splines?label=OpenUPM&registry_uri=https://package.openupm.com&labelColor=333A41 'OpenUPM package')](https://openupm.com/packages/com.ivanmurzak.unity.mcp.splines/)
[![Unity Editor](https://img.shields.io/badge/Editor-X?style=flat&logo=unity&labelColor=333A41&color=2A2A2A 'Unity Editor supported')](https://unity.com/releases/editor/archive)
[![r](https://github.com/IvanMurzak/Unity-AI-Splines/workflows/release/badge.svg 'Tests Passed')](https://github.com/IvanMurzak/Unity-AI-Splines/actions/workflows/release.yml)</br>
[![Discord](https://img.shields.io/badge/Discord-Join-7289da?logo=discord&logoColor=white&labelColor=333A41 'Join')](https://discord.gg/cfbdMZX99G)
[![Stars](https://img.shields.io/github/stars/IvanMurzak/Unity-AI-Splines 'Stars')](https://github.com/IvanMurzak/Unity-AI-Splines/stargazers)
[![License](https://img.shields.io/github/license/IvanMurzak/Unity-AI-Splines?label=License&labelColor=333A41)](https://github.com/IvanMurzak/Unity-AI-Splines/blob/main/LICENSE)
[![Stand With Ukraine](https://raw.githubusercontent.com/vshymanskyy/StandWithUkraine/main/badges/StandWithUkraine.svg)](https://stand-with-ukraine.pp.ua)

</div>

**AI Splines** is an extension for [AI Game Developer (Unity-MCP)](https://github.com/IvanMurzak/Unity-MCP) that exposes [Unity Splines](https://docs.unity3d.com/Packages/com.unity.splines@2.8/manual/index.html) authoring to AI assistants through the [Model Context Protocol](https://modelcontextprotocol.io/introduction). Create and edit splines, knots, and tangents with natural language.

It wraps `com.unity.splines` **2.8.4** and requires Unity **2022.3** or newer.

## Installation

1. Install [Unity-MCP](https://github.com/IvanMurzak/Unity-MCP) in your Unity project.
2. Install this package via OpenUPM:

```bash
openupm add com.ivanmurzak.unity.mcp.splines
```

Or add it through the Unity-MCP plugin window's **Extensions** list.

## Tools

| Tool id | Description |
| --- | --- |
| `splines-container-create` | Create a GameObject with a `SplineContainer` (one empty spline) |
| `splines-add-spline` | Add another empty spline to a container |
| `splines-add-knot` | Append a knot to a spline |
| `splines-insert-knot` | Insert a knot at an index |
| `splines-remove-knot` | Remove a knot at an index |
| `splines-set-knot` | Set position / tangents / rotation of a knot |
| `splines-set-tangent-mode` | Set a knot's tangent mode (Linear/Continuous/Broken/AutoSmooth/Mirrored) |
| `splines-set-closed` | Toggle a spline's closed-loop flag |
| `splines-evaluate` | Evaluate position / tangent / up at normalized `t` |
| `splines-list` | List all `SplineContainer`s in the scene |
| `splines-get-knots` | Read all knots of a spline |
| `splines-get` | Generic: serialize any Splines component via ReflectorNet |
| `splines-modify` | Generic: apply a `SerializedMember` diff to any Splines component |

## License

MIT © [Ivan Murzak](https://github.com/IvanMurzak)
