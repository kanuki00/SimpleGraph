using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System;

namespace SimpleGraph {
    [System.Serializable]
    public class GraphManager : MonoBehaviour
    {
        public string projectName;

        void Start()
        {
            foreach (GraphNode node in FindObjectsOfType<GraphNode>())
            {
                if (node.nodeType == NodeType.StartNode)
                {
                    node.UpdateState("isActive", true);
                }
                if (node.nodeType == NodeType.InverterNode)
                {
                    node.UpdateState("isActive", true);
                    node.UpdateState("isComplete", true);
                }
            }
        }

        public void SetNodeCompletion(GraphNode node, bool desiredCompletion)
        {
            if (node.isActive && ArePreviousNodesCompleted(node))
            {
                if (node.isCompleted && !desiredCompletion)
                {
                    node.UpdateState("isRevoke", desiredCompletion);
                } else if (!node.isCompleted && desiredCompletion)
                {
                    node.UpdateState("isComplete", desiredCompletion);
                }
                
                foreach (GraphNode nextNode in node.nextNodes)
                {
                    ChangeNextNodeState(nextNode, desiredCompletion);
                }
            }
            else
            {
                Debug.LogWarning($"[GraphManager] Node '{node.nodeType}' is not active or previous nodes are not completed.");
            }
        }
        
        private bool ArePreviousNodesCompleted(GraphNode node)
        {
            if (node.previousNodes == null || node.previousNodes.Count == 0)
            {
            return true;
            }

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
            switch (node.nodeType)
            {
                case NodeType.InverterNode:
                    node.UpdateState("isComplete", !desiredCompletion);
                    node.UpdateState("isActive", !desiredCompletion);
                    foreach (GraphNode nextNode in node.nextNodes)  {
                        ChangeNextNodeState(nextNode, !desiredCompletion);
                    }
                    break;
                default:
                    if (ArePreviousNodesCompleted(node))
                    {
                        node.UpdateState("isActive", desiredCompletion);
                        break;
                    }
                    else if (!desiredCompletion) {
                        node.UpdateState("isDeactive", desiredCompletion);
                        break;
                    }
                    else  {
                        Debug.LogWarning($"[GraphManager] Node '{node.nodeType}' is not active or previous nodes are not completed.");
                    }
                    break; 
            }
        }


        // Wrapper method(s) for UnityEvents
        public void CompleteNode(GraphNode selectedNode)
        {
            SetNodeCompletion(selectedNode, true);
        }
        public void RevokeNode(GraphNode node)
        {
            SetNodeCompletion(node, false);
        }

    }


    #if UNITY_EDITOR 
    [CustomEditor(typeof(GraphManager))]

    
    public class GraphManagerEditor : Editor
    {
        private string customTextField = "Welcome to the Node Manager! You can find documentation to this project from the project's Github page, and I highly recommend skimming through the source code, as most of it is commented.";

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
            serializedObject.ApplyModifiedProperties();

        }
    }
    #endif
}
