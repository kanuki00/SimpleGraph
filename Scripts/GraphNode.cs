using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using SimpleGraph; 
using UnityEditor;

namespace SimpleGraph
{
    public class GraphNode : MonoBehaviour
    {
        public Rect windowRect;
        public string nodeName;
        public List<GraphNode> previousNodes = new List<GraphNode>(); // Add list for previous nodes
        public List<GraphNode> nextNodes = new List<GraphNode>(); // Add list for next nodes

        [HideInInspector]
        public bool isCompleted = true; // Add isCompleted state
        [HideInInspector]
        public bool isActive = true; // Add isActive state

        public delegate void NodeEventHandler(GraphNode node);
        public static event NodeEventHandler OnSetStartNode;
        public static event NodeEventHandler OnSetEndNode;

        [System.Serializable]
        public class NodeEvent : UnityEvent<GraphNode> { }

        public NodeEvent onActiveEvent;
        public NodeEvent onCompletionEvent;

        public void TriggerActiveEvent()
        {
            onActiveEvent?.Invoke(this);
        }

        public void TriggerCompletionEvent()
        {
            onCompletionEvent?.Invoke(this);
        }

        public void UpdateCompletionState(bool completed)
        {
            isCompleted = completed;
            TriggerCompletionEvent();
        }

        public virtual void DrawConnectionPoints()
        {
            float buttonWidth = 20;
            float buttonHeight = 20;
            float inButtonX = windowRect.width - buttonWidth; // 10 pixels from the right edge
            float inButtonY = (windowRect.height / 2) - (buttonHeight / 2); // Centered vertically
            float outButtonX = 0; 
            float outButtonY = (windowRect.height / 2) - (buttonHeight / 2);

            // "In" button
            if (GUI.Button(new Rect(inButtonX, inButtonY, buttonWidth, buttonHeight), "•"))
            {
                OnSetStartNode?.Invoke(this);
            }
            
            // "Out" button
            if (GUI.Button(new Rect(outButtonX, outButtonY, buttonWidth, buttonHeight), "•"))
            {
                OnSetEndNode?.Invoke(this);
                
            }
        }

        public virtual void DrawNodeWindow(int id)
        {
            // The "x" button in the top-right corner
            if (GUI.Button(new Rect(windowRect.width - 20, 0, 20, 20), "x"))
            {
                OnSetEndNode?.Invoke(this); // Remove connections
                GraphUtility.RemoveNode(this);
            }

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Space(20); 
            GUILayout.BeginVertical();
            GUILayout.Space(10); 

            if (nodeName == "StartNode" || nodeName == "EndNode" || nodeName == "InverterNode") {

                GUILayout.Label("Node Type: " + nodeName);
                GUILayout.Space(10); 
                if (GUILayout.Button("Edit in Hierarchy", GUILayout.Height(40),  GUILayout.Width(150)))
                {
                    #if UNITY_EDITOR
                    UnityEditor.Selection.activeGameObject = this.gameObject;
                    #endif
                }


            } else {

                GUILayout.Label("Node Name:");
                string newNodeName = GUILayout.TextField(nodeName, GUILayout.Width(150));
                if (newNodeName != nodeName)
                {
                    nodeName = newNodeName;
                    UpdateNodeNameInHierarchy();
                }

                GUILayout.Space(10);

                // Add the "Edit in Hierarchy" button
                if (GUILayout.Button("Edit in Hierarchy", GUILayout.Height(40),  GUILayout.Width(150)))
                {
                    #if UNITY_EDITOR
                    UnityEditor.Selection.activeGameObject = this.gameObject;
                    #endif
                }
            }

            GUILayout.Space(10);

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            Color colorOriginal = GUI.color;

            GUI.color = isActive ? Color.green : Color.red;
            GUILayout.Label("Active: " + isActive);

            GUI.color = isCompleted ? Color.green : Color.red;
            GUILayout.Label("Completed: " + isCompleted);

            GUI.color = colorOriginal;
            GUILayout.Space(10);

            GUILayout.EndVertical();            
            DrawConnectionPoints();
            GUI.DragWindow();
        }

        private void UpdateNodeNameInHierarchy()
        {
            this.gameObject.name = nodeName;
        }
    }

    #if UNITY_EDITOR

    [CustomEditor(typeof(GraphNode))]
    public class GraphNodeEditor : Editor
    {
        private string customTextField = "Add the task node functionality below using UnityEvents.";
        private bool showDefaultInspector = false;

        private bool startOnBegin = false;

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(customTextField, EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();

            GraphNode graphNode = (GraphNode)target;

            if (graphNode.nodeName == "StartNode") {
                startOnBegin = EditorGUILayout.Toggle("Start On Begin", startOnBegin);
                EditorGUILayout.Space(10);
            }

            EditorGUILayout.LabelField("Node Events", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("When node is Activated");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onActiveEvent"), new GUIContent("onActiveEvent"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("When node is Completed");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onCompletionEvent"), new GUIContent("onCompletionEvent"));

            serializedObject.ApplyModifiedProperties();
            
            showDefaultInspector = EditorGUILayout.Foldout(showDefaultInspector, "Default Inspector");
            if (showDefaultInspector)
            {
                DrawDefaultInspector();
            }
        }
    }
    #endif
}