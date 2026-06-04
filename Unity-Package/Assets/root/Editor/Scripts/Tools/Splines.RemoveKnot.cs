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
        public const string RemoveKnotToolId = "splines-remove-knot";

        [AiTool
        (
            RemoveKnotToolId,
            Title = "Splines / Remove Knot",
            ReadOnlyHint = false,
            DestructiveHint = true,
            IdempotentHint = false,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Remove the knot at a given index from a spline, shifting later knots back. Destructive: " +
            "the removed control point is gone.")]
        [AiSkillBody("Remove a knot at a specific index from a spline. Knots after the index shift back by one.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `SplineContainer` (required).\n" +
            "- `knotIndex` — the index of the knot to remove (0..knotCount-1; required).\n" +
            "- `splineIndex` — which spline in the container (default 0).\n\n" +
            "## Behavior\n\n" +
            "Validates the index range, removes the knot via `Spline.RemoveAt`, marks the scene dirty, repaints, and " +
            "returns the spline's new knot count. Runs on the Unity main thread.")]
        [Description("Removes the knot at the given index from a spline, shifting later knots back. Destructive.")]
        public KnotMutationResponse RemoveKnot
        (
            [Description("Reference to the GameObject containing the SplineContainer component.")]
            GameObjectRef gameObjectRef,
            [Description("Index of the knot to remove (0..knotCount-1).")]
            int knotIndex,
            [Description("Index of the spline inside the container (default 0).")]
            int splineIndex = 0
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var container = ResolveSplineContainer(gameObjectRef, nameof(gameObjectRef));
                var spline = ResolveSpline(container, splineIndex);

                if (spline.Count == 0)
                    throw new Exception(Error.EmptySpline());
                if (knotIndex < 0 || knotIndex >= spline.Count)
                    throw new Exception(Error.KnotIndexOutOfRange(knotIndex, spline.Count));

                spline.RemoveAt(knotIndex);

                MarkDirtyAndRepaint(container, container.gameObject.scene);

                return new KnotMutationResponse
                {
                    gameObjectRef = new GameObjectRef(container.gameObject),
                    containerRef = new ComponentRef(container),
                    splineIndex = splineIndex,
                    knotIndex = knotIndex,
                    knotCount = spline.Count,
                    knot = null,
                    success = true
                };
            });
        }
    }
}
