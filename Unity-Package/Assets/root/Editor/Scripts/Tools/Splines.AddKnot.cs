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
        public const string AddKnotToolId = "splines-add-knot";

        [AiTool
        (
            AddKnotToolId,
            Title = "Splines / Add Knot",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = false,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Append a knot to a spline in a `SplineContainer`. A knot is a `BezierKnot` with a local " +
            "position plus in/out tangents and a rotation. Returns the index of the appended knot.")]
        [AiSkillBody("Append a new knot to the end of a spline. Knots define the control points the spline curve " +
            "passes through.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `SplineContainer` (required).\n" +
            "- `position` — local-space `Vector3` position of the knot (default zero).\n" +
            "- `tangentIn` — optional in tangent relative to the position (default `(0,0,-1)`).\n" +
            "- `tangentOut` — optional out tangent relative to the position (default `(0,0,1)`).\n" +
            "- `rotation` — optional euler-degrees rotation of the knot (default zero).\n" +
            "- `splineIndex` — which spline in the container to append to (default 0).\n\n" +
            "## Behavior\n\n" +
            "Builds a `BezierKnot` from the inputs, appends it to the chosen spline, marks the scene dirty, repaints, " +
            "and returns the appended knot index plus the spline's new knot count. Runs on the Unity main thread.")]
        [Description("Appends a knot (BezierKnot) to a spline in a SplineContainer. Returns the appended knot index.")]
        public KnotMutationResponse AddKnot
        (
            [Description("Reference to the GameObject containing the SplineContainer component.")]
            GameObjectRef gameObjectRef,
            [Description("Local-space position of the new knot.")]
            Vector3? position = null,
            [Description("In tangent relative to the knot position.")]
            Vector3? tangentIn = null,
            [Description("Out tangent relative to the knot position.")]
            Vector3? tangentOut = null,
            [Description("Knot rotation in euler angles (degrees).")]
            Vector3? rotation = null,
            [Description("Index of the spline inside the container to append to (default 0).")]
            int splineIndex = 0
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var container = ResolveSplineContainer(gameObjectRef, nameof(gameObjectRef));
                var spline = ResolveSpline(container, splineIndex);

                var knot = BuildKnot(position, tangentIn, tangentOut, rotation);
                spline.Add(knot);
                int newIndex = spline.Count - 1;

                MarkDirtyAndRepaint(container, container.gameObject.scene);

                return new KnotMutationResponse
                {
                    gameObjectRef = new GameObjectRef(container.gameObject),
                    containerRef = new ComponentRef(container),
                    splineIndex = splineIndex,
                    knotIndex = newIndex,
                    knotCount = spline.Count,
                    knot = ToKnotInfo(spline[newIndex], newIndex, spline.GetTangentMode(newIndex)),
                    success = true
                };
            });
        }

        public class KnotMutationResponse
        {
            [Description("Reference to the SplineContainer GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the SplineContainer component.")]
            public ComponentRef? containerRef;

            [Description("Index of the affected spline in the container.")]
            public int splineIndex;

            [Description("Index of the affected knot in the spline (-1 when not applicable).")]
            public int knotIndex = -1;

            [Description("Resulting number of knots in the spline.")]
            public int knotCount;

            [Description("Summary of the affected knot, when applicable.")]
            public KnotInfo? knot;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
