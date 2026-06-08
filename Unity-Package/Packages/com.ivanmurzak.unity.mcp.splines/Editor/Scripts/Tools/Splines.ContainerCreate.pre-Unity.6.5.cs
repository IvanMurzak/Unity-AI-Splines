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
#if !UNITY_6000_5_OR_NEWER
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using AIGD;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Splines
    {
        public const string ContainerCreateToolId = "splines-container-create";

        [AiTool
        (
            ContainerCreateToolId,
            Title = "Splines / Create Container",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = false,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Create a new GameObject with a `SplineContainer` (and one initial empty `Spline`) in " +
            "the active scene. Optionally set name, position and rotation. Returns the new GameObject reference and instanceId.")]
        [AiSkillBody("Create a new GameObject hosting a `SplineContainer` component in the active scene. The " +
            "`SplineContainer` is the authoring component that holds one or more `Spline`s; this tool seeds it with " +
            "a single empty spline ready for knots.\n\n" +
            "## Inputs\n\n" +
            "- `name` — optional GameObject name (default `Spline`).\n" +
            "- `position` — optional world `Vector3` position (default zero).\n" +
            "- `rotation` — optional euler-degrees `Vector3` rotation (default zero).\n" +
            "- `closed` — optional; when true the seeded spline is a closed loop (default false).\n\n" +
            "## Behavior\n\n" +
            "Creates the GameObject, adds a `SplineContainer` with one empty spline, applies transform and the " +
            "`Closed` flag, marks the scene dirty, repaints the Editor, and returns the new GameObject reference and " +
            "instanceId. The whole call runs on the Unity main thread.")]
        [Description("Creates a new SplineContainer GameObject (with one empty spline) in the active scene. " +
            "Optionally sets name, transform, and closed-loop flag.")]
        public ContainerCreateResponse CreateContainer
        (
            [Description("Name of the new SplineContainer GameObject.")]
            string? name = null,
            [Description("World-space position of the GameObject.")]
            Vector3? position = null,
            [Description("World-space rotation of the GameObject in euler angles (degrees).")]
            Vector3? rotation = null,
            [Description("If true, the seeded spline is a closed loop.")]
            bool closed = false
        )
        {
            return MainThread.Instance.Run(() =>
            {
                position ??= Vector3.zero;
                rotation ??= Vector3.zero;

                var go = new GameObject(string.IsNullOrEmpty(name) ? "Spline" : name);
                go.transform.position = position.Value;
                go.transform.eulerAngles = rotation.Value;

                var container = go.AddComponent<SplineContainer>();
                // A fresh SplineContainer already owns one empty Spline (container.Spline).
                var spline = container.Spline;
                spline.Closed = closed;

                EditorUtility.SetDirty(go);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(go.scene);
                com.IvanMurzak.Unity.MCP.Editor.Utils.EditorUtils.RepaintAllEditorWindows();

                return new ContainerCreateResponse
                {
                    gameObjectRef = new GameObjectRef(go),
                    containerRef = new ComponentRef(container),
                    instanceId = go.GetInstanceID(),
                    gameObjectName = go.name,
                    splineCount = container.Splines.Count,
                    closed = spline.Closed
                };
            });
        }

        public class ContainerCreateResponse
        {
            [Description("Reference to the created SplineContainer GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the created SplineContainer component.")]
            public ComponentRef? containerRef;

            [Description("Instance id of the created GameObject.")]
            public int instanceId;

            [Description("Name of the created GameObject.")]
            public string gameObjectName = string.Empty;

            [Description("Number of splines in the container.")]
            public int splineCount;

            [Description("Whether the seeded spline is a closed loop.")]
            public bool closed;
        }
    }
}
#endif
