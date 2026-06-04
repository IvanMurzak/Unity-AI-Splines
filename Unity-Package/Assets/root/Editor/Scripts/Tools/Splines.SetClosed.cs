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
        public const string SetClosedToolId = "splines-set-closed";

        [AiTool
        (
            SetClosedToolId,
            Title = "Splines / Set Closed",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Toggle whether a spline is a closed loop. A closed spline connects its last knot back to " +
            "its first, forming a continuous loop.")]
        [AiSkillBody("Set the `Closed` flag of a spline.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `SplineContainer` (required).\n" +
            "- `closed` — true to close the loop, false to open it (default true).\n" +
            "- `splineIndex` — which spline in the container (default 0).\n\n" +
            "## Behavior\n\n" +
            "Assigns `Spline.Closed`, marks the scene dirty, repaints, and returns the resulting state. Runs on the " +
            "Unity main thread.")]
        [Description("Toggles whether a spline is a closed loop (last knot connects back to the first).")]
        public SetClosedResponse SetClosed
        (
            [Description("Reference to the GameObject containing the SplineContainer component.")]
            GameObjectRef gameObjectRef,
            [Description("True to close the loop, false to open it.")]
            bool closed = true,
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

                spline.Closed = closed;

                MarkDirtyAndRepaint(container, container.gameObject.scene);

                return new SetClosedResponse
                {
                    gameObjectRef = new GameObjectRef(container.gameObject),
                    containerRef = new ComponentRef(container),
                    splineIndex = splineIndex,
                    closed = spline.Closed,
                    knotCount = spline.Count,
                    success = true
                };
            });
        }

        public class SetClosedResponse
        {
            [Description("Reference to the SplineContainer GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the SplineContainer component.")]
            public ComponentRef? containerRef;

            [Description("Index of the affected spline in the container.")]
            public int splineIndex;

            [Description("Resulting closed-loop state.")]
            public bool closed;

            [Description("Number of knots in the spline.")]
            public int knotCount;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
