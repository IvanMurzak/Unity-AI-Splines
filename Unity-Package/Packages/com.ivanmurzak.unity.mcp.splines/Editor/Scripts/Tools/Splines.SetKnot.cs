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
using UnityEngine;
using UnityEngine.Splines;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Splines
    {
        public const string SetKnotToolId = "splines-set-knot";

        [AiTool
        (
            SetKnotToolId,
            Title = "Splines / Set Knot",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Set position, in/out tangents, and/or rotation of an existing knot in a spline. Only the " +
            "fields you pass are changed; the rest are preserved.")]
        [AiSkillBody("Edit an existing knot in place. `BezierKnot` is a value type, so this reads the current knot, " +
            "applies only the provided fields, and writes it back through the spline indexer (which rebuilds the " +
            "cached curve).\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `SplineContainer` (required).\n" +
            "- `knotIndex` — the index of the knot to edit (required).\n" +
            "- `position` — optional new local-space position.\n" +
            "- `tangentIn` / `tangentOut` — optional new tangents.\n" +
            "- `rotation` — optional new euler-degrees rotation.\n" +
            "- `splineIndex` — which spline in the container (default 0).\n\n" +
            "## Behavior\n\n" +
            "Reads the knot, overrides only the supplied fields, writes it back, marks the scene dirty, repaints, and " +
            "returns the resulting knot summary. Runs on the Unity main thread.")]
        [Description("Sets position/tangents/rotation of an existing knot in a spline. Only provided fields change.")]
        public KnotMutationResponse SetKnot
        (
            [Description("Reference to the GameObject containing the SplineContainer component.")]
            GameObjectRef gameObjectRef,
            [Description("Index of the knot to edit (0..knotCount-1).")]
            int knotIndex,
            [Description("New local-space position of the knot.")]
            Vector3? position = null,
            [Description("New in tangent relative to the knot position.")]
            Vector3? tangentIn = null,
            [Description("New out tangent relative to the knot position.")]
            Vector3? tangentOut = null,
            [Description("New knot rotation in euler angles (degrees).")]
            Vector3? rotation = null,
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

                if (knotIndex < 0 || knotIndex >= spline.Count)
                    throw new Exception(Error.KnotIndexOutOfRange(knotIndex, spline.Count));

                var knot = spline[knotIndex];
                if (position.HasValue) knot.Position = position.Value;
                if (tangentIn.HasValue) knot.TangentIn = tangentIn.Value;
                if (tangentOut.HasValue) knot.TangentOut = tangentOut.Value;
                if (rotation.HasValue) knot.Rotation = Quaternion.Euler(rotation.Value);
                spline[knotIndex] = knot;

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
