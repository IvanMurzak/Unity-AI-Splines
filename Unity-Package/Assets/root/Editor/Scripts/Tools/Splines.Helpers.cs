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
using AIGD;
using com.IvanMurzak.Unity.MCP.Runtime.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Splines
    {
        /// <summary>Resolve a required GameObjectRef to its GameObject (throws on failure).</summary>
        static GameObject ResolveGameObject(GameObjectRef? gameObjectRef, string paramName)
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(paramName);
            if (!gameObjectRef.IsValid(out var validationError))
                throw new ArgumentException(validationError, paramName);

            var go = gameObjectRef.FindGameObject(out var error);
            if (error != null)
                throw new Exception(error);
            if (go == null)
                throw new Exception(Error.GameObjectNotFound());

            return go;
        }

        /// <summary>Resolve a required GameObjectRef to its SplineContainer (throws on failure).</summary>
        static SplineContainer ResolveSplineContainer(GameObjectRef? gameObjectRef, string paramName)
        {
            var go = ResolveGameObject(gameObjectRef, paramName);
            var container = go.GetComponent<SplineContainer>();
            if (container == null)
                throw new Exception(Error.SplineContainerNotFound());
            return container;
        }

        /// <summary>Resolve a Spline at the given index inside a SplineContainer (throws on out-of-range).</summary>
        static Spline ResolveSpline(SplineContainer container, int splineIndex)
        {
            if (splineIndex < 0 || splineIndex >= container.Splines.Count)
                throw new Exception(Error.SplineIndexOutOfRange(splineIndex, container.Splines.Count));
            return container.Splines[splineIndex];
        }

        /// <summary>Mark a scene object dirty and repaint the editor after a mutation.</summary>
        static void MarkDirtyAndRepaint(UnityEngine.Object target, UnityEngine.SceneManagement.Scene scene)
        {
            EditorUtility.SetDirty(target);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            com.IvanMurzak.Unity.MCP.Editor.Utils.EditorUtils.RepaintAllEditorWindows();
        }

        /// <summary>Build a BezierKnot from optional position / tangents / euler-rotation inputs.</summary>
        static BezierKnot BuildKnot(Vector3? position, Vector3? tangentIn, Vector3? tangentOut, Vector3? rotation)
        {
            var knot = new BezierKnot
            {
                Position = position ?? Vector3.zero,
                TangentIn = tangentIn ?? new Vector3(0f, 0f, -1f),
                TangentOut = tangentOut ?? new Vector3(0f, 0f, 1f),
                Rotation = Quaternion.Euler(rotation ?? Vector3.zero)
            };
            return knot;
        }

        /// <summary>Convert a BezierKnot to a serializable summary for tool responses.</summary>
        static KnotInfo ToKnotInfo(BezierKnot knot, int index, TangentMode tangentMode)
        {
            return new KnotInfo
            {
                index = index,
                position = (Vector3)knot.Position,
                tangentIn = (Vector3)knot.TangentIn,
                tangentOut = (Vector3)knot.TangentOut,
                rotationEuler = ((Quaternion)knot.Rotation).eulerAngles,
                tangentMode = tangentMode.ToString()
            };
        }

        public class KnotInfo
        {
            [System.ComponentModel.Description("Index of the knot inside the spline.")]
            public int index;

            [System.ComponentModel.Description("Local-space position of the knot.")]
            public Vector3 position;

            [System.ComponentModel.Description("In tangent (relative to the knot position).")]
            public Vector3 tangentIn;

            [System.ComponentModel.Description("Out tangent (relative to the knot position).")]
            public Vector3 tangentOut;

            [System.ComponentModel.Description("Knot rotation in euler angles (degrees).")]
            public Vector3 rotationEuler;

            [System.ComponentModel.Description("Tangent mode of the knot.")]
            public string tangentMode = string.Empty;
        }
    }
}
