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
        public const string InsertKnotToolId = "splines-insert-knot";

        [AiTool
        (
            InsertKnotToolId,
            Title = "Splines / Insert Knot",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = false,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Insert a knot at a specific index in a spline, shifting later knots forward. Use this " +
            "to add a control point in the middle of an existing path.")]
        [AiSkillBody("Insert a new knot at a given index in a spline. Knots at and after the index shift forward by " +
            "one.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `SplineContainer` (required).\n" +
            "- `knotIndex` — the index to insert at (0..knotCount; required).\n" +
            "- `position` — local-space `Vector3` position of the knot (default zero).\n" +
            "- `tangentIn` / `tangentOut` — optional tangents (defaults `(0,0,-1)` / `(0,0,1)`).\n" +
            "- `rotation` — optional euler-degrees rotation (default zero).\n" +
            "- `splineIndex` — which spline in the container (default 0).\n\n" +
            "## Behavior\n\n" +
            "Validates the index range (0..count inclusive), builds a `BezierKnot`, inserts it, marks dirty, repaints, " +
            "and returns the inserted knot index and the spline's new knot count. Runs on the Unity main thread.")]
        [Description("Inserts a knot at a specific index in a spline, shifting later knots forward.")]
        public KnotMutationResponse InsertKnot
        (
            [Description("Reference to the GameObject containing the SplineContainer component.")]
            GameObjectRef gameObjectRef,
            [Description("Index at which to insert the knot (0..knotCount).")]
            int knotIndex,
            [Description("Local-space position of the new knot.")]
            Vector3? position = null,
            [Description("In tangent relative to the knot position.")]
            Vector3? tangentIn = null,
            [Description("Out tangent relative to the knot position.")]
            Vector3? tangentOut = null,
            [Description("Knot rotation in euler angles (degrees).")]
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

                if (knotIndex < 0 || knotIndex > spline.Count)
                    throw new Exception(Error.KnotIndexOutOfRange(knotIndex, spline.Count));

                var knot = BuildKnot(position, tangentIn, tangentOut, rotation);
                spline.Insert(knotIndex, knot);

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
