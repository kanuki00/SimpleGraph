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
            ActivateStartNode();
        }

        public void CompleteNode(string nodeName)
        {
            Debug.Log($"[GraphManager] CompleteNode called with nodeName: {nodeName}");
            foreach (Transform child in transform)
            {
                GraphNode node = child.GetComponent<GraphNode>();
                if (node != null && node.nodeName == nodeName)
                {
                    Debug.Log($"[GraphManager] Found node: {nodeName}");
                    if (node.isActive)
                    {
                        Debug.Log($"[GraphManager] Node {nodeName} is active.");
                        // Check if all previous nodes are completed
                        if (ArePreviousNodesCompleted(node))
                        {
                            node.UpdateCompletionState(true);
                            Debug.Log($"[GraphManager] Node {nodeName} is now completed.");
                            ActivateNextNodes(node);
                        }
                        else
                        {
                            Debug.LogWarning($"[GraphManager] Not all previous nodes are completed for node {nodeName}.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[GraphManager] Node {nodeName} is not active.");
                    }
                    return;
                }
            }

            Debug.LogWarning($"[GraphManager] Node with name {nodeName} not found.");
        }

        private bool ArePreviousNodesCompleted(GraphNode node)
        {
            Debug.Log($"[GraphManager] Checking if previous nodes are completed for node: {node.nodeName}");
            foreach (GraphNode previousNode in node.previousNodes)
            {
                if (!previousNode.isCompleted)
                {
                    
                    return false;
                }
            }
            Debug.Log($"[GraphManager] All previous nodes are completed for node: {node.nodeName}");
            return true;
        }

        private void ActivateNextNodes(GraphNode node)
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
                    nextNode.isActive = true;
                    Debug.Log("Node " + nextNode.nodeName + " is now active.");
                }
            }
        }

        private void ActivateStartNode()
        {
            foreach (Transform child in transform)
            {
                GraphNode node = child.GetComponent<GraphNode>();
                if (node != null && node.nodeName == "StartNode")
                {
                    node.isActive = true;
                    Debug.Log($"[GraphManager] Start node {node.nodeName} is now active.");
                    return;
                }
            }

            Debug.LogWarning("[GraphManager] Start node with name 'StartNode' not found.");
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
