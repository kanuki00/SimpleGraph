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
                
                if (node.nodeName == "InverterNode") {
                    node.UpdateState("isActive", true);
                    node.UpdateState("isComplete", true);
                }
            }

            foreach (Transform child in transform)
            {
                GraphNode node = child.GetComponent<GraphNode>();
        

                if (node != null && node.nodeName == "StartNode")
                {
                    if (node.startOnBegin == true ) {
                        node.UpdateState("isActive", true);
                        CompleteNode(node.nodeName);
                        Debug.Log($"[GraphManager] Start node {node.nodeName} is now active.");
                        return;
                    } else {
                        Debug.Log("[GraphManager] 'StartNode' waiting for activation.");
                        return;
                    }
                }
            }
            Debug.LogWarning("[GraphManager] Missing 'StartNode'.");
        }

        public void SetNodeCompletion(string nodeName, bool desiredCompletion)
        {
            foreach (Transform child in transform)
            {
                GraphNode node = child.GetComponent<GraphNode>();
                if (node != null && node.nodeName == nodeName)
                {
                    if (node.isActive)
                    {
                        if (ArePreviousNodesCompleted(node))
                        {
                            node.UpdateState("isComplete", desiredCompletion);
                            Debug.Log($"[GraphManager] Node '{nodeName}' is {(desiredCompletion ? "completed" : "uncompleted")}!");
                            ChangeNextNodeState(node, desiredCompletion);
                        }
                        else
                        {
                            if (nodeName == "InverterNode") {
                                ChangeNextNodeState(node, desiredCompletion);
                                break;
                            }
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

        private void ChangeNextNodeState(GraphNode node, bool desiredCompletion)
        {
            //Debug.Log($"[GraphManager] Activating next nodes for node: {node.nodeName}");
            foreach (GraphNode nextNode in node.nextNodes)
            {
                if (nextNode.nodeName == "InverterNode")
                {
                    if (desiredCompletion) {
                        nextNode.UpdateState("isComplete", false);
                        Debug.Log("Inverter node " + nextNode.nodeName + " state is now FALSE " + nextNode.isCompleted);
                    } else {
                        nextNode.UpdateState("isComplete", true);
                        Debug.Log("Inverter node " + nextNode.nodeName + " state is now TRUE" + nextNode.isCompleted);
                    }
                }
                if (nextNode.nodeName == "EndNode") {
                    if (desiredCompletion) {
                        if (nextNode.requireAllCompleted) {
                            bool allNodesCompleted = true;
                            foreach (Transform child in transform)
                            {
                                GraphNode checkNode = child.GetComponent<GraphNode>();
                                if (checkNode != null && checkNode.nodeName != "InverterNode" && checkNode.nodeName != "EndNode" && !checkNode.isCompleted)
                                {
                                    allNodesCompleted = false;
                                    Debug.Log("Not all nodes are completed!");
                                    break;
                                }
                            }

                            if (allNodesCompleted)
                            {
                                nextNode.UpdateState("isActive", true);
                                CompleteNode(nextNode.nodeName);

                                Debug.Log("End node " + nextNode.nodeName + " is now active and completed.");
                            }
                        }
                    } else {
                        nextNode.UpdateState("isActive", false);
                        Debug.Log("Node " + nextNode.nodeName + " is now not active.");
                    }
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
