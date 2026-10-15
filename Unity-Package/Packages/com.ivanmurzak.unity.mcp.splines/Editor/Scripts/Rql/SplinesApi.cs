/*
┌─────────────────────────────────────────────────────────────────────────────┐
│  Author: Ivan Murzak (https://github.com/IvanMurzak)                        │
│  Repository: GitHub (https://github.com/IvanMurzak/Unity-AI-Splines)        │
│  Copyright (c) 2026 Ivan Murzak                                             │
│  Licensed under the MIT License.                                            │
│  See the LICENSE file in the project root for more information.             │
└─────────────────────────────────────────────────────────────────────────────┘
*/

#nullable enable
using System.Collections.Generic;
using System.Linq;
using Rql;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Splines;

namespace com.IvanMurzak.Unity.MCP.Editor.Rql.Splines
{
    /// <summary>
    /// The <c>splines</c> source: the spline containers of the loaded scenes and the operation without a natural owner,
    /// <c>splines | call CreateContainer("Road")</c>. Everything else lives on SplineContainer (methods) and its
    /// <c>splines[i].knots[j]</c> views (writable members).
    /// </summary>
    [RqlDescription("Splines: containers (every SplineContainer in the loaded scenes) and CreateContainer(name, position, eulerAngles, closed, parent)")]
    public sealed class SplinesApi
    {
        public static readonly SplinesApi Instance = new SplinesApi();

        [RqlDescription("every SplineContainer in the loaded scenes, inactive ones included")]
        public List<SplineContainer> containers =>
            RqlUnity.AllGameObjects().SelectMany(go => go.GetComponents<SplineContainer>()).Where(c => c != null).ToList();

        [RqlCallable("create a GameObject with a SplineContainer holding one empty spline (optionally under a parent, at a local " +
                     "position / rotation, as a closed loop) and return the container; then call AddKnot / AddKnots on it")]
        public SplineContainer CreateContainer(string name = "Spline", Vector3? position = null, Vector3? eulerAngles = null,
            bool closed = false, GameObject? parent = null)
        {
            var go = new GameObject(string.IsNullOrEmpty(name) ? "Spline" : name);
            if (parent != null) go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = position ?? Vector3.zero;
            go.transform.localEulerAngles = eulerAngles ?? Vector3.zero;
            // A new SplineContainer already owns one empty spline.
            var container = go.AddComponent<SplineContainer>();
            container.Spline.Closed = closed;
            Undo.RegisterCreatedObjectUndo(go, "RQL create spline " + go.name);
            if (!EditorApplication.isPlaying && go.scene.IsValid()) EditorSceneManager.MarkSceneDirty(go.scene);
            return container;
        }
    }
}
