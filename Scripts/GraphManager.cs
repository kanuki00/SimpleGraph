using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace SimpleGraph {
    public class GraphManager : MonoBehaviour
    {
        public string projectName;
        public AuthenticationType authType; 
        public CloudserviceLoggerType loggerType;
        public bool sendDataToCloud; 
        
        void Start() {

            if (sendDataToCloud)
            {
                string json = JsonUtility.ToJson(this, true);
                string path = Application.dataPath + "/GraphData.json";
                System.IO.File.WriteAllText(path, json);
                Debug.Log($"[GraphManager] Data sent to cloud and saved to {path}");
            }
            
            foreach (Transform child in transform)
            {
                GraphNode node = child.GetComponent<GraphNode>();
                
                if (node.nodeType == GraphNodeType.InverterNode) {
                    node.UpdateState("isActive", true);
                    node.UpdateState("isComplete", true);
                }
            }

            foreach (Transform child in transform)
            {
                GraphNode node = child.GetComponent<GraphNode>();
        

                if (node != null && node.nodeType == GraphNodeType.StartNode)
                {
                    if (node.startOnBegin == true ) {
                        node.UpdateState("isActive", true);
                        CompleteNode(node.nodeName);
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
                            if (node.nodeType == GraphNodeType.InverterNode) {
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
            if (sendDataToCloud) {
                sendDataToCloudMethod(nodeName, "completed");
            }
        }

        public void UnCompleteNode(string nodeName)
        {
            SetNodeCompletion(nodeName, false);
            if (sendDataToCloud) {
                sendDataToCloudMethod(nodeName, "uncompleted");
            }
        }

        public void sendDataToCloudMethod(string nodeName, string nodeState) {
            string json = JsonUtility.ToJson(this, true);
            string path = Application.dataPath + "/GraphData.json";
            System.IO.File.WriteAllText(path, json);
            Debug.Log($"[GraphManager] Data sent to cloud and saved to {path}");
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
                if (nextNode.nodeType == GraphNodeType.InverterNode)
                {
                    if (desiredCompletion) {
                        nextNode.UpdateState("isComplete", false);
                        Debug.Log("Inverter node " + nextNode.nodeName + " state is now FALSE " + nextNode.isCompleted);
                    } else {
                        nextNode.UpdateState("isComplete", true);
                        Debug.Log("Inverter node " + nextNode.nodeName + " state is now TRUE" + nextNode.isCompleted);
                    }
                }
                if (nextNode.nodeType == GraphNodeType.EndNode) {
                    if (desiredCompletion) {
                        if (nextNode.requireAllCompleted) {
                            bool allNodesCompleted = true;
                            foreach (Transform child in transform)
                            {
                                GraphNode checkNode = child.GetComponent<GraphNode>();
                                if (checkNode != null && checkNode.nodeType != GraphNodeType.InverterNode && checkNode.nodeType != GraphNodeType.EndNode && !checkNode.isCompleted)
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

                                //Debug.Log("End node " + nextNode.nodeName + " is now active and completed.");
                            }
                        }
                    } else {
                        nextNode.UpdateState("isActive", false);
                        //Debug.Log("Node " + nextNode.nodeName + " is now not active.");
                    }
                }
                else
                {
                    if (ArePreviousNodesCompleted(nextNode))
                    {
                        nextNode.UpdateState("isActive", true);
                    }
                }
            }
        }
    }
     public enum AuthenticationType
    {
        OAuth,
        APIKey,
        BasicAuth
    }

    public enum CloudserviceLoggerType
    {
        Automatic,
        Manual,
    }

    #if UNITY_EDITOR 
    [CustomEditor(typeof(GraphManager))]

    
    public class GraphManagerEditor : Editor
    {
        private string customTextField = "Welcome to the Node Manager! You can find documentation to this project from the project's Github page, and I highly recommend skimming through the source code, as most of it is commented.";
        private bool showOracleIntegration = false;

        public override void OnInspectorGUI()
        {
            GraphManager graphManager = (GraphManager)target;
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
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();

            // Oracle Integration settings. Modify this section to implement other cloud services.
            showOracleIntegration = EditorGUILayout.Foldout(showOracleIntegration, "Cloud Integration");
            if (showOracleIntegration)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Currently this project only supports the Oracle Cloud integration.", EditorStyles.wordWrappedLabel);
                EditorGUILayout.Space();
                graphManager.sendDataToCloud = EditorGUILayout.Toggle("Send data:", graphManager.sendDataToCloud);
                graphManager.loggerType = (CloudserviceLoggerType)EditorGUILayout.EnumPopup("Logger type:", graphManager.loggerType);
                EditorGUILayout.Space();

                EditorGUILayout.BeginVertical("box");
                graphManager.authType = (AuthenticationType)EditorGUILayout.EnumPopup("Authentication:", graphManager.authType);
                EditorGUILayout.Space();
                if (graphManager.authType == AuthenticationType.BasicAuth)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("Auth link:");
                    graphManager.projectName = EditorGUILayout.TextField(graphManager.projectName);
                    EditorGUI.indentLevel--;
                }
    
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            // Apply changes to the serializedObject
            serializedObject.ApplyModifiedProperties();

        }
    }
    #endif
}
