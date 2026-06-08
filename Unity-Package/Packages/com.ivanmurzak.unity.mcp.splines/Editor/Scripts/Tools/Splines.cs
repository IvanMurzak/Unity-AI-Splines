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
using com.IvanMurzak.McpPlugin;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    [AiToolType]
    public partial class Tool_Splines
    {
        public static class Error
        {
            public static string GameObjectNotFound()
                => "[Error] GameObject not found. Provide a valid reference to an existing GameObject.";

            public static string SplineContainerNotFound()
                => "[Error] SplineContainer component not found on the target GameObject. " +
                   "Make sure the GameObject has a SplineContainer component attached (create one with 'splines-container-create').";

            public static string SplineIndexOutOfRange(int splineIndex, int count)
                => $"[Error] Spline index {splineIndex} is out of range. The SplineContainer has {count} spline(s).";

            public static string KnotIndexOutOfRange(int knotIndex, int count)
                => $"[Error] Knot index {knotIndex} is out of range. The spline has {count} knot(s).";

            public static string EmptySpline()
                => "[Error] The spline has no knots. Add at least one knot before performing this operation.";

            public static string TypeNotFound(string typeName)
                => $"[Error] Type '{typeName}' could not be resolved. Provide a full type name (e.g. 'UnityEngine.Splines.SplineContainer').";

            public static string ReflectorNotAvailable()
                => "[Error] ReflectorNet reflector is not available.";
        }
    }
}
