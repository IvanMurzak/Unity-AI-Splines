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
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Splines
    {
        public const string EvaluateToolId = "splines-evaluate";

        [AiTool
        (
            EvaluateToolId,
            Title = "Splines / Evaluate",
            ReadOnlyHint = true,
            DestructiveHint = false,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Evaluate a spline at normalized parameter `t` in [0,1], returning the local-space and " +
            "world-space position, the tangent (direction of travel), and the up vector. Read-only.")]
        [AiSkillBody("Sample a spline at a point along its length. `t` is normalized: 0 is the first knot, 1 is the " +
            "last knot (or the loop point for a closed spline).\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `SplineContainer` (required).\n" +
            "- `t` — normalized parameter in [0,1] (default 0.5). Values outside are clamped.\n" +
            "- `splineIndex` — which spline in the container (default 0).\n\n" +
            "## Behavior\n\n" +
            "Evaluates local position / tangent / up via `Spline.Evaluate`, transforms the position into world space " +
            "using the container's transform, and returns all of them. The spline must have at least 2 knots. " +
            "Read-only. Runs on the Unity main thread.")]
        [Description("Evaluates a spline at normalized t in [0,1]: position (local+world), tangent, and up. Read-only.")]
        public EvaluateResponse Evaluate
        (
            [Description("Reference to the GameObject containing the SplineContainer component.")]
            GameObjectRef gameObjectRef,
            [Description("Normalized parameter along the spline in [0,1] (clamped). Default 0.5.")]
            float t = 0.5f,
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

                if (spline.Count < 2)
                    throw new Exception("[Error] The spline needs at least 2 knots to be evaluated.");

                float clampedT = Mathf.Clamp01(t);

                spline.Evaluate(clampedT, out float3 localPos, out float3 tangent, out float3 up);

                var localPosition = (Vector3)localPos;
                var worldPosition = container.transform.TransformPoint(localPosition);

                return new EvaluateResponse
                {
                    gameObjectRef = new GameObjectRef(container.gameObject),
                    containerRef = new ComponentRef(container),
                    splineIndex = splineIndex,
                    t = clampedT,
                    localPosition = localPosition,
                    worldPosition = worldPosition,
                    tangent = (Vector3)tangent,
                    up = (Vector3)up
                };
            });
        }

        public class EvaluateResponse
        {
            [Description("Reference to the SplineContainer GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the SplineContainer component.")]
            public ComponentRef? containerRef;

            [Description("Index of the evaluated spline in the container.")]
            public int splineIndex;

            [Description("Normalized parameter actually evaluated (after clamping).")]
            public float t;

            [Description("Local-space position at t.")]
            public Vector3 localPosition;

            [Description("World-space position at t.")]
            public Vector3 worldPosition;

            [Description("Tangent (direction of travel) at t.")]
            public Vector3 tangent;

            [Description("Up vector at t.")]
            public Vector3 up;
        }
    }
}
