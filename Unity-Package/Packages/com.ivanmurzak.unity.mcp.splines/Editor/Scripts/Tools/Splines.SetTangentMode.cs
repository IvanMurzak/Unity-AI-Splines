/*
┌─────────────────────────────────────────────────────────────────────────────┐
│  Author: Ivan Murzak (https://github.com/IvanMurzak)                        │
│  Repository: GitHub (https://github.com/IvanMurzak/Unity-AI-Splines)        │
│  Copyright (c) 2025 Ivan Murzak                                             │
│  Licensed under the MIT License.                                            │
│  See the LICENSE file in the project root for more information.             │
└─────────────────────────────────────────────────────────────────────────────┘
*/

#nullable enable
using System;
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using AIGD;
using UnityEngine.Splines;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Splines
    {
        public const string SetTangentModeToolId = "splines-set-tangent-mode";

        [AiTool
        (
            SetTangentModeToolId,
            Title = "Splines / Set Tangent Mode",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Set the `TangentMode` of a knot in a spline. Valid modes: Linear, Continuous, Broken, " +
            "AutoSmooth, Mirrored. The mode controls how the knot's in/out tangents are computed.")]
        [AiSkillBody("Change how a knot's tangents behave by setting its `TangentMode`.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `SplineContainer` (required).\n" +
            "- `knotIndex` — the index of the knot (required).\n" +
            "- `tangentMode` — one of `Linear`, `Continuous`, `Broken`, `AutoSmooth`, `Mirrored` (case-insensitive, required).\n" +
            "- `splineIndex` — which spline in the container (default 0).\n\n" +
            "## Behavior\n\n" +
            "Parses the mode, applies it via `Spline.SetTangentMode(index, mode)`, marks the scene dirty, repaints, " +
            "and returns the resulting knot summary. Runs on the Unity main thread.")]
        [Description("Sets the TangentMode (Linear/Continuous/Broken/AutoSmooth/Mirrored) of a knot in a spline.")]
        public KnotMutationResponse SetTangentMode
        (
            [Description("Reference to the GameObject containing the SplineContainer component.")]
            GameObjectRef gameObjectRef,
            [Description("Index of the knot to edit (0..knotCount-1).")]
            int knotIndex,
            [Description("Tangent mode: Linear, Continuous, Broken, AutoSmooth, or Mirrored (case-insensitive).")]
            string tangentMode,
            [Description("Index of the spline inside the container (default 0).")]
            int splineIndex = 0
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));
            if (string.IsNullOrWhiteSpace(tangentMode))
                throw new ArgumentException("tangentMode must be provided.", nameof(tangentMode));

            if (!Enum.TryParse<TangentMode>(tangentMode, ignoreCase: true, out var mode))
                throw new ArgumentException(
                    $"[Error] Invalid tangent mode '{tangentMode}'. Valid values: Linear, Continuous, Broken, AutoSmooth, Mirrored.",
                    nameof(tangentMode));

            return MainThread.Instance.Run(() =>
            {
                var container = ResolveSplineContainer(gameObjectRef, nameof(gameObjectRef));
                var spline = ResolveSpline(container, splineIndex);

                if (knotIndex < 0 || knotIndex >= spline.Count)
                    throw new Exception(Error.KnotIndexOutOfRange(knotIndex, spline.Count));

                spline.SetTangentMode(knotIndex, mode);

                MarkDirtyAndRepaint(container, container.gameObject.scene);

                return new KnotMutationResponse
                {
                    gameObjectRef = new GameObjectRef(container.gameObject),
                    containerRef = new ComponentRef(container),
                    splineIndex = splineIndex,
                    knotIndex = knotIndex,
                    knotCount = spline.Count,
                    knot = ToKnotInfo(spline[knotIndex], knotIndex, spline.GetTangentMode(knotIndex)),
                    success = true
                };
            });
        }
    }
}
