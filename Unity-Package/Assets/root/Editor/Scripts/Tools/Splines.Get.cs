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
using Microsoft.Extensions.Logging;
using com.IvanMurzak.ReflectorNet.Model;
using com.IvanMurzak.ReflectorNet.Utils;
using AIGD;
using com.IvanMurzak.Unity.MCP.Runtime.Extensions;
using com.IvanMurzak.Unity.MCP.Utils;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Splines
    {
        public const string GetToolId = "splines-get";

        [AiTool
        (
            GetToolId,
            Title = "Splines / Get Component",
            ReadOnlyHint = true,
            DestructiveHint = false,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Generic read: serialize any Splines `Component` (a type in the `UnityEngine.Splines` " +
            "namespace, e.g. `SplineContainer`, `SplineInstantiate`, `SplineAnimate`) on a GameObject via ReflectorNet. " +
            "Pair with 'splines-modify' to write changes back. Read-only.")]
        [AiSkillBody("Serialize any Splines component on a GameObject using ReflectorNet. This is the generic escape " +
            "hatch for fields not covered by the dedicated tools.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the component (required).\n" +
            "- `componentRef` — optional. Resolves a specific component when the GameObject has more than one " +
            "Splines component; otherwise the first component whose type lives in the `UnityEngine.Splines` namespace " +
            "is used.\n" +
            "- `deepSerialization` — when `true`, recurses through nested objects; otherwise only top-level members.\n\n" +
            "## Behavior\n\n" +
            "Finds the target Splines component, serializes it via ReflectorNet, and returns the serialized member " +
            "plus the resolved component type name. Read-only. Runs on the Unity main thread.")]
        [Description("Generic: serialize any Splines Component on a GameObject via ReflectorNet. Read-only. " +
            "Use splines-modify to write changes back.")]
        public SplinesGetResponse GetComponentData
        (
            [Description("Reference to the GameObject containing the Splines component.")]
            GameObjectRef gameObjectRef,
            [Description("Optional reference to a specific Splines component if the GameObject has multiple.")]
            ComponentRef? componentRef = null,
            [Description("Performs deep serialization including nested objects. Otherwise only top-level members.")]
            bool deepSerialization = false
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));
            if (!gameObjectRef.IsValid(out var validationError))
                throw new ArgumentException(validationError, nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var go = ResolveGameObject(gameObjectRef, nameof(gameObjectRef));
                var (component, index) = FindSplinesComponent(go, componentRef);
                if (component == null)
                    throw new Exception("[Error] No Splines component found on the specified GameObject.");

                var reflector = UnityMcpPluginEditor.Instance.Reflector ?? throw new Exception(Error.ReflectorNotAvailable());
                var logger = UnityLoggerFactory.LoggerFactory.CreateLogger<Tool_Splines>();

                return new SplinesGetResponse
                {
                    gameObjectRef = new GameObjectRef(go),
                    componentRef = new ComponentRef(component),
                    componentIndex = index,
                    componentType = component.GetType().FullName ?? component.GetType().Name,
                    data = reflector.Serialize(
                        obj: component,
                        name: component.GetType().Name,
                        recursive: deepSerialization,
                        logger: logger)
                };
            });
        }

        /// <summary>
        /// Locate a Splines component on the GameObject. When componentRef resolves, returns the matching component;
        /// otherwise returns the first component whose type lives in the UnityEngine.Splines namespace.
        /// </summary>
        static (UnityEngine.Component? component, int index) FindSplinesComponent(GameObject go, ComponentRef? componentRef)
        {
            var all = go.GetComponents<UnityEngine.Component>();
            for (int i = 0; i < all.Length; i++)
            {
                var comp = all[i];
                if (comp == null)
                    continue;

                if (componentRef != null && componentRef.IsValid(out _))
                {
                    if (componentRef.Matches(comp, i))
                        return (comp, i);
                }
                else
                {
                    var ns = comp.GetType().Namespace;
                    if (ns != null && ns.StartsWith("UnityEngine.Splines", StringComparison.Ordinal))
                        return (comp, i);
                }
            }
            return (null, -1);
        }

        public class SplinesGetResponse
        {
            [Description("Reference to the GameObject containing the component.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the serialized component.")]
            public ComponentRef? componentRef;

            [Description("Index of the component in the GameObject's component list.")]
            public int componentIndex = -1;

            [Description("Full type name of the serialized component.")]
            public string componentType = string.Empty;

            [Description("Serialized component data.")]
            public SerializedMember? data;
        }
    }
}
