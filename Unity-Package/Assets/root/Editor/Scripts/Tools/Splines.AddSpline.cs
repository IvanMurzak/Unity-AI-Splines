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
        public const string AddSplineToolId = "splines-add-spline";

        [AiTool
        (
            AddSplineToolId,
            Title = "Splines / Add Spline",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = false,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Add an additional empty `Spline` to an existing `SplineContainer`. A container can hold " +
            "many splines; this appends one and returns its new index.")]
        [AiSkillBody("Add a new empty `Spline` to a `SplineContainer`. Use this when you need more than one spline " +
            "on the same GameObject (e.g. a multi-path container).\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `SplineContainer` (required).\n" +
            "- `closed` — optional; when true the new spline is a closed loop (default false).\n\n" +
            "## Behavior\n\n" +
            "Appends a new empty spline to the container, applies the `Closed` flag, marks the scene dirty, repaints, " +
            "and returns the index of the new spline and the resulting spline count. Runs on the Unity main thread.")]
        [Description("Adds a new empty Spline to an existing SplineContainer. Returns the new spline index.")]
        public AddSplineResponse AddSpline
        (
            [Description("Reference to the GameObject containing the SplineContainer component.")]
            GameObjectRef gameObjectRef,
            [Description("If true, the new spline is a closed loop.")]
            bool closed = false
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var container = ResolveSplineContainer(gameObjectRef, nameof(gameObjectRef));

                // SplineContainer has no AddSpline() in this Splines version; rebuild the
                // Splines list with one extra empty spline appended.
                var spline = new Spline { Closed = closed };
                var list = new List<Spline>(container.Splines) { spline };
                container.Splines = list;
                int newIndex = container.Splines.Count - 1;

                MarkDirtyAndRepaint(container, container.gameObject.scene);

                return new AddSplineResponse
                {
                    gameObjectRef = new GameObjectRef(container.gameObject),
                    containerRef = new ComponentRef(container),
                    splineIndex = newIndex,
                    splineCount = container.Splines.Count,
                    closed = spline.Closed,
                    success = true
                };
            });
        }

        public class AddSplineResponse
        {
            [Description("Reference to the SplineContainer GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the SplineContainer component.")]
            public ComponentRef? containerRef;

            [Description("Index of the newly added spline.")]
            public int splineIndex;

            [Description("Resulting number of splines in the container.")]
            public int splineCount;

            [Description("Whether the new spline is a closed loop.")]
            public bool closed;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
