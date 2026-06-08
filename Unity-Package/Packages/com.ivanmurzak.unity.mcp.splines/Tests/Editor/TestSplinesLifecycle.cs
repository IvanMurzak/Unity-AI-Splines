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
using System.Collections;
using AIGD;
using com.IvanMurzak.Unity.MCP.Editor.API;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Splines;

namespace com.IvanMurzak.Unity.MCP.Splines.Editor.Tests
{
    public class TestSplinesLifecycle : BaseTest
    {
        [UnityTest]
        public IEnumerator CreateContainer_AddsSplineContainerWithOneSpline()
        {
            var tool = new Tool_Splines();
            var result = tool.CreateContainer(name: "MySpline", position: new Vector3(1, 2, 3), closed: false);

            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsNotNull(result.gameObjectRef, "GameObject reference should be set");
            Assert.AreEqual("MySpline", result.gameObjectName, "Name should be applied");
            Assert.AreEqual(1, result.splineCount, "A fresh container should have exactly one spline");

            var go = GameObject.Find("MySpline");
            Assert.IsNotNull(go, "The created GameObject should exist in the scene");
            Assert.IsNotNull(go!.GetComponent<SplineContainer>(), "SplineContainer should be attached");

            yield return null;
        }

        [UnityTest]
        public IEnumerator AddKnot_AppendsToSpline()
        {
            var go = CreateGameObjectWithSplineContainer(GO_ContainerName);

            var tool = new Tool_Splines();
            var r0 = tool.AddKnot(new GameObjectRef(go.GetInstanceID()), position: new Vector3(0, 0, 0));
            var r1 = tool.AddKnot(new GameObjectRef(go.GetInstanceID()), position: new Vector3(5, 0, 0));

            Assert.IsTrue(r0.success && r1.success, "Both AddKnot calls should succeed");
            Assert.AreEqual(0, r0.knotIndex, "First knot should be index 0");
            Assert.AreEqual(1, r1.knotIndex, "Second knot should be index 1");
            Assert.AreEqual(2, go.GetComponent<SplineContainer>().Spline.Count, "Spline should have 2 knots");

            yield return null;
        }

        [UnityTest]
        public IEnumerator InsertAndRemoveKnot_ShiftKnots()
        {
            var go = CreateGameObjectWithKnots(2, GO_ContainerName);
            var spline = go.GetComponent<SplineContainer>().Spline;

            var tool = new Tool_Splines();
            var ins = tool.InsertKnot(new GameObjectRef(go.GetInstanceID()), knotIndex: 1, position: new Vector3(2, 2, 2));
            Assert.IsTrue(ins.success, "Insert should succeed");
            Assert.AreEqual(3, spline.Count, "Spline should have 3 knots after insert");
            Assert.AreEqual(new Vector3(2, 2, 2), (Vector3)spline[1].Position, "Inserted knot should be at index 1");

            var rem = tool.RemoveKnot(new GameObjectRef(go.GetInstanceID()), knotIndex: 1);
            Assert.IsTrue(rem.success, "Remove should succeed");
            Assert.AreEqual(2, spline.Count, "Spline should have 2 knots after remove");

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetKnot_OverridesProvidedFields()
        {
            var go = CreateGameObjectWithKnots(1, GO_ContainerName);
            var spline = go.GetComponent<SplineContainer>().Spline;

            var tool = new Tool_Splines();
            var r = tool.SetKnot(new GameObjectRef(go.GetInstanceID()), knotIndex: 0,
                position: new Vector3(9, 8, 7), tangentOut: new Vector3(0, 0, 2));

            Assert.IsTrue(r.success, "SetKnot should succeed");
            Assert.AreEqual(new Vector3(9, 8, 7), (Vector3)spline[0].Position, "Position should be updated");
            Assert.AreEqual(new Vector3(0, 0, 2), (Vector3)spline[0].TangentOut, "TangentOut should be updated");

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetTangentMode_AppliesMode()
        {
            var go = CreateGameObjectWithKnots(2, GO_ContainerName);
            var spline = go.GetComponent<SplineContainer>().Spline;

            var tool = new Tool_Splines();
            var r = tool.SetTangentMode(new GameObjectRef(go.GetInstanceID()), knotIndex: 0, tangentMode: "Linear");

            Assert.IsTrue(r.success, "SetTangentMode should succeed");
            Assert.AreEqual(TangentMode.Linear, spline.GetTangentMode(0), "Tangent mode should be Linear");

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetClosed_TogglesLoop()
        {
            var go = CreateGameObjectWithKnots(3, GO_ContainerName);
            var spline = go.GetComponent<SplineContainer>().Spline;

            var tool = new Tool_Splines();
            var r = tool.SetClosed(new GameObjectRef(go.GetInstanceID()), closed: true);

            Assert.IsTrue(r.success, "SetClosed should succeed");
            Assert.IsTrue(spline.Closed, "Spline should be closed");
            Assert.IsTrue(r.closed, "Response should report closed=true");

            yield return null;
        }

        [UnityTest]
        public IEnumerator AddSpline_AppendsSecondSpline()
        {
            var go = CreateGameObjectWithSplineContainer(GO_ContainerName);

            var tool = new Tool_Splines();
            var r = tool.AddSpline(new GameObjectRef(go.GetInstanceID()), closed: true);

            Assert.IsTrue(r.success, "AddSpline should succeed");
            Assert.AreEqual(1, r.splineIndex, "Second spline should be index 1");
            Assert.AreEqual(2, go.GetComponent<SplineContainer>().Splines.Count, "Container should have 2 splines");

            yield return null;
        }

        [UnityTest]
        public IEnumerator Evaluate_ReturnsPointAlongSpline()
        {
            var go = CreateGameObjectWithKnots(2, GO_ContainerName);
            // knots at (0,0,0) and (1,0,0); evaluate at t=0.5 should land near the middle.

            var tool = new Tool_Splines();
            var r = tool.Evaluate(new GameObjectRef(go.GetInstanceID()), t: 0.5f);

            Assert.AreEqual(0.5f, r.t, "t should be echoed back");
            Assert.Greater(r.localPosition.x, 0f, "Midpoint should advance along +X");
            Assert.Less(r.localPosition.x, 1f, "Midpoint should be before the last knot");

            yield return null;
        }

        [UnityTest]
        public IEnumerator GetKnots_ReturnsAllKnots()
        {
            var go = CreateGameObjectWithKnots(3, GO_ContainerName);

            var tool = new Tool_Splines();
            var r = tool.GetKnots(new GameObjectRef(go.GetInstanceID()));

            Assert.AreEqual(3, r.knotCount, "Should report 3 knots");
            Assert.AreEqual(3, r.knots.Length, "Should return 3 knot summaries");

            yield return null;
        }

        [UnityTest]
        public IEnumerator ListContainers_FindsCreatedContainer()
        {
            CreateGameObjectWithKnots(2, "ListedSpline");

            var tool = new Tool_Splines();
            var r = tool.ListContainers(includeInactive: true);

            Assert.GreaterOrEqual(r.count, 1, "Should find at least one SplineContainer");

            yield return null;
        }
    }
}
