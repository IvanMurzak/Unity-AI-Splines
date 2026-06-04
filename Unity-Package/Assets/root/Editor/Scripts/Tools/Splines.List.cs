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
using UnityEngine;
using UnityEngine.Splines;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Splines
    {
        public const string ListToolId = "splines-list";

        [AiTool
        (
            ListToolId,
            Title = "Splines / List Containers",
            ReadOnlyHint = true,
            DestructiveHint = false,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("List every `SplineContainer` in the active scene with its name, spline count, and the " +
            "knot count of each spline. Read-only.")]
        [AiSkillBody("Enumerate the `SplineContainer`s in the active scene. For each, returns a reference, the name, " +
            "the number of splines, and per-spline knot counts and closed flags.\n\n" +
            "## Inputs\n\n" +
            "- `includeInactive` (bool, default true) — include containers on inactive/disabled GameObjects.\n\n" +
            "## Behavior\n\n" +
            "Finds all `SplineContainer` instances and summarizes them. Read-only. The whole call runs on the Unity " +
            "main thread.")]
        [Description("Lists all SplineContainers in the active scene with name and per-spline knot counts. Read-only.")]
        public ListResponse ListContainers
        (
            [Description("If true (default), include containers on inactive/disabled GameObjects.")]
            bool includeInactive = true
        )
        {
            return MainThread.Instance.Run(() =>
            {
#if UNITY_2023_1_OR_NEWER
                var containers = UnityEngine.Object.FindObjectsByType<SplineContainer>(
                    includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
                var containers = UnityEngine.Object.FindObjectsOfType<SplineContainer>(includeInactive);
#endif

                var items = new List<ContainerListItem>(containers.Length);
                foreach (var container in containers)
                {
                    if (container == null)
                        continue;

                    var splineSummaries = new List<SplineSummary>(container.Splines.Count);
                    for (int i = 0; i < container.Splines.Count; i++)
                    {
                        var spline = container.Splines[i];
                        splineSummaries.Add(new SplineSummary
                        {
                            splineIndex = i,
                            knotCount = spline.Count,
                            closed = spline.Closed
                        });
                    }

                    items.Add(new ContainerListItem
                    {
                        gameObjectRef = new GameObjectRef(container.gameObject),
                        containerRef = new ComponentRef(container),
                        name = container.name,
                        splineCount = container.Splines.Count,
                        splines = splineSummaries.ToArray()
                    });
                }

                return new ListResponse
                {
                    count = items.Count,
                    containers = items.ToArray()
                };
            });
        }

        public class ListResponse
        {
            [Description("Number of SplineContainers found.")]
            public int count;

            [Description("The SplineContainers in the active scene.")]
            public ContainerListItem[] containers = Array.Empty<ContainerListItem>();
        }

        public class ContainerListItem
        {
            [Description("Reference to the SplineContainer GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the SplineContainer component.")]
            public ComponentRef? containerRef;

            [Description("Name of the container GameObject.")]
            public string name = string.Empty;

            [Description("Number of splines in the container.")]
            public int splineCount;

            [Description("Per-spline summaries.")]
            public SplineSummary[] splines = Array.Empty<SplineSummary>();
        }

        public class SplineSummary
        {
            [Description("Index of the spline in the container.")]
            public int splineIndex;

            [Description("Number of knots in the spline.")]
            public int knotCount;

            [Description("Whether the spline is a closed loop.")]
            public bool closed;
        }
    }
}
