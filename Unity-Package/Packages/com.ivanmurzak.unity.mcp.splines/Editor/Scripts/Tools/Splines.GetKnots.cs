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
using System.Collections.Generic;
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using AIGD;
using UnityEngine.Splines;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Splines
    {
        public const string GetKnotsToolId = "splines-get-knots";

        [AiTool
        (
            GetKnotsToolId,
            Title = "Splines / Get Knots",
            ReadOnlyHint = true,
            DestructiveHint = false,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Read all knots of a spline in a `SplineContainer`: per-knot position, in/out tangents, " +
            "rotation, and tangent mode, plus the closed flag. Read-only.")]
        [AiSkillBody("Inspect the knots of a specific spline. Useful before editing to know the indices, positions, " +
            "and tangent modes.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `SplineContainer` (required).\n" +
            "- `splineIndex` — which spline in the container (default 0).\n\n" +
            "## Behavior\n\n" +
            "Enumerates every knot of the chosen spline and returns a per-knot summary plus the spline's `Closed` " +
            "flag. Read-only. Runs on the Unity main thread.")]
        [Description("Reads all knots (position, tangents, rotation, tangent mode) of a spline. Read-only.")]
        public GetKnotsResponse GetKnots
        (
            [Description("Reference to the GameObject containing the SplineContainer component.")]
            GameObjectRef gameObjectRef,
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

                var knots = new List<KnotInfo>(spline.Count);
                for (int i = 0; i < spline.Count; i++)
                    knots.Add(ToKnotInfo(spline[i], i, spline.GetTangentMode(i)));

                return new GetKnotsResponse
                {
                    gameObjectRef = new GameObjectRef(container.gameObject),
                    containerRef = new ComponentRef(container),
                    splineIndex = splineIndex,
                    closed = spline.Closed,
                    knotCount = spline.Count,
                    knots = knots.ToArray()
                };
            });
        }

        public class GetKnotsResponse
        {
            [Description("Reference to the SplineContainer GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the SplineContainer component.")]
            public ComponentRef? containerRef;

            [Description("Index of the spline in the container.")]
            public int splineIndex;

            [Description("Whether the spline is a closed loop.")]
            public bool closed;

            [Description("Number of knots in the spline.")]
            public int knotCount;

            [Description("Per-knot summaries.")]
            public KnotInfo[] knots = Array.Empty<KnotInfo>();
        }
    }
}
