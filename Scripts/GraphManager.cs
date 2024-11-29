using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace SimpleGraph {
    public class GraphManager : MonoBehaviour
    {
        public string projectName;

        void Start() {
            foreach (Transform child in transform)
            {
                GraphNode node = child.GetComponent<GraphNode>();
                if (node != null && node.nodeName == "StartNode")
                {
                    node.UpdateState("isActive", true);
                    Debug.Log($"[GraphManager] Start node {node.nodeName} is now active.");
                    return;
                }
            }
            Debug.LogWarning("[GraphManager] Missing 'StartNode'.");
        }

        public void SetNodeCompletion(string nodeName, bool isComplete)
        {
            foreach (Transform child in transform)
            {
                GraphNode node = child.GetComponent<GraphNode>();
                if (node != null && node.nodeName == nodeName)
                {
                    if (node.isActive)
                    {
                        if (isComplete || ArePreviousNodesCompleted(node))
                        {
                            node.UpdateState("isComplete", isComplete);
                            Debug.Log($"[GraphManager] Node '{nodeName}' is {(isComplete ? "completed" : "uncompleted")}!");
                            ChangeNextNodeState(node, isComplete);
                        }
                        else
                        {
                            Debug.LogWarning($"[GraphManager] Previous nodes are not completed for node '{nodeName}'.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[GraphManager] Node '{nodeName}' is not active.");
                    }
                    return;
                }
            }
            Debug.LogWarning($"[GraphManager] SetNodeCompletion on node '{nodeName}' failed. Node not found.");
        }

        // Wrapper method for UnityEvents
        public void CompleteNode(string nodeName)
        {
            SetNodeCompletion(nodeName, true);
        }

        public void UnCompleteNode(string nodeName)
        {
            SetNodeCompletion(nodeName, false);
        }

        private bool ArePreviousNodesCompleted(GraphNode node)
        {
            foreach (GraphNode previousNode in node.previousNodes)
            {
                if (!previousNode.isCompleted)
                {
                    return false;
                }
            }

            return true;
        }

        private void ChangeNextNodeState(GraphNode node, bool isComplete)
        {
            Debug.Log($"[GraphManager] Activating next nodes for node: {node.nodeName}");
            foreach (GraphNode nextNode in node.nextNodes)
            {
                if (nextNode.nodeName == "InverterNode")
                {
                    nextNode.isActive = !node.isActive;
                    Debug.Log("Inverter node " + nextNode.nodeName + " state is now " + nextNode.isActive);
                }
                else
                {
                    nextNode.UpdateState("isActive", true);
                    Debug.Log("Node " + nextNode.nodeName + " is now active.");
                }
            }
        }
    }

    #if UNITY_EDITOR 
    [CustomEditor(typeof(GraphManager))]

    
    public class GraphManagerEditor : Editor
    {
        private string customTextField = "Welcome to the Node Manager! You can find documentation to this project from the project's Github page, and I highly recommend skimming through the source code, as most of it is commented.";
        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(customTextField, EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();

            if (GUILayout.Button("Edit Graph", GUILayout.Height(40)))
            {
                // Use reflection to call GraphEditorWindow.ShowEditor()
                Type graphEditorWindowType = Type.GetType("SimpleGraph.GraphEditorWindow, Assembly-CSharp-Editor");
                if (graphEditorWindowType != null)
                {
                    MethodInfo showEditorMethod = graphEditorWindowType.GetMethod("ShowEditor", BindingFlags.Public | BindingFlags.Static);
                    if (showEditorMethod != null)
                    {
                        showEditorMethod.Invoke(null, new object[] { ((GraphManager)target).gameObject });
                    }
                    else
                    {
                        Debug.LogError("ShowEditor method not found in GraphEditorWindow.");
                    }
                }
                else
                {
                    Debug.LogError("GraphEditorWindow type not found.");
                }
            }
        

            EditorGUILayout.Space();
        }
    }
    #endif
}
