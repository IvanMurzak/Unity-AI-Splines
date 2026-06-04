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
using System.Collections;
using com.IvanMurzak.ReflectorNet.Model;
using AIGD;
using com.IvanMurzak.Unity.MCP.Editor.API;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Splines;

namespace com.IvanMurzak.Unity.MCP.Splines.Editor.Tests
{
    public class TestSplinesGeneric : BaseTest
    {
        [UnityTest]
        public IEnumerator Get_SerializesSplineContainer()
        {
            var go = CreateGameObjectWithSplineContainer(GO_ContainerName);
            var container = go.GetComponent<SplineContainer>();

            var tool = new Tool_Splines();
            var result = tool.GetComponentData(
                gameObjectRef: new GameObjectRef(go.GetInstanceID()),
                componentRef: new ComponentRef(container.GetInstanceID()));

            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsNotNull(result.data, "Serialized data should not be null");
            StringAssert.Contains("SplineContainer", result.componentType, "Component type should be reported");

            yield return null;
        }

        [UnityTest]
        public IEnumerator Get_FirstSplinesComponent_WhenNoComponentRef()
        {
            var go = CreateGameObjectWithSplineContainer(GO_ContainerName);

            var tool = new Tool_Splines();
            var result = tool.GetComponentData(new GameObjectRef(go.GetInstanceID()));

            Assert.IsNotNull(result.data, "Should serialize the first Splines component");
            StringAssert.Contains("Splines", result.componentType, "Resolved component should be a Splines type");

            yield return null;
        }

        [UnityTest]
        public IEnumerator Modify_SplineExtrudeSides_ViaFieldsChannel()
        {
            // SplineExtrude exposes `public int Sides` backed by the private serialized
            // field `m_Sides`. The 'fields' channel resolves FieldInfo (including private
            // serialized fields) — there is no cross-fallback to the 'props' channel, so the
            // member MUST be supplied via AddField, not AddProperty.
            var go = CreateGameObjectWithSplineContainer(GO_ContainerName);
            var extrude = go.AddComponent<SplineExtrude>();
            var reflector = UnityMcpPluginEditor.Instance.Reflector ?? throw new Exception("Reflector not available.");

            const int newSides = 12;
            var diff = SerializedMember.FromValue(
                    reflector: reflector,
                    name: extrude.GetType().Name,
                    type: typeof(SplineExtrude),
                    value: null)
                .AddField(SerializedMember.FromValue(
                    reflector: reflector,
                    name: "m_Sides",
                    value: newSides));

            var tool = new Tool_Splines();
            var result = tool.ModifyComponent(
                gameObjectRef: new GameObjectRef(go.GetInstanceID()),
                data: diff,
                componentRef: new ComponentRef(extrude.GetInstanceID()));

            Assert.IsTrue(result.success, "Modification should succeed");
            Assert.AreEqual(newSides, extrude.Sides, "m_Sides should be modified via the fields channel");

            yield return null;
        }

        [UnityTest]
        public IEnumerator ModifyJson_SplineExtrudeSides_Dispatch()
        {
            var go = CreateGameObjectWithSplineContainer(GO_ContainerName);
            var extrude = go.AddComponent<SplineExtrude>();

            var json = $@"{{
                ""gameObjectRef"": {{ ""instanceID"": {go.GetInstanceID()} }},
                ""componentRef"": {{ ""instanceID"": {extrude.GetInstanceID()} }},
                ""data"": {{
                    ""typeName"": ""UnityEngine.Splines.SplineExtrude"",
                    ""fields"": [
                        {{
                            ""name"": ""m_Sides"",
                            ""typeName"": ""System.Int32"",
                            ""value"": 7
                        }}
                    ]
                }}
            }}";

            var result = RunToolAllowWarnings(Tool_Splines.ModifyToolId, json);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.AreEqual(7, extrude.Sides, "m_Sides should be modified via JSON fields channel");

            yield return null;
        }
    }
}
