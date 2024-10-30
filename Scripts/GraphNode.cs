using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using SimpleGraph; 

namespace SimpleGraph
{
    public class GraphNode : MonoBehaviour
    {
        public Rect windowRect;
        public string nodeName;
        public List<GraphNode> previousNodes = new List<GraphNode>(); // Add list for previous nodes
        public List<GraphNode> nextNodes = new List<GraphNode>(); // Add list for next nodes

        [HideInInspector]
        public bool isCompleted = false; // Add isCompleted state
        [HideInInspector]
        public bool isActive = false; // Add isActive state

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
                //Debug.Log("In button clicked on node: " + nodeName);
                OnSetStartNode?.Invoke(this);
            }
            
            // "Out" button
            if (GUI.Button(new Rect(outButtonX, outButtonY, buttonWidth, buttonHeight), "•"))
            {
                Debug.Log("Out button clicked on node: " + nodeName);
                OnSetEndNode?.Invoke(this);
                
            }
        }

        public virtual void DrawNodeWindow(int id)
        {
            // The "x" button in the top-right corner
            if (GUI.Button(new Rect(windowRect.width - 20, 0, 20, 20), "x"))
            {
                Debug.Log("Delete button clicked on node: " + nodeName);
                GraphUtility.RemoveNode(this);
            }

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Space(20); 
            GUILayout.BeginVertical();
            GUILayout.Space(10); 

            GUILayout.Label("Node Name:");
            string newNodeName = GUILayout.TextField(nodeName, GUILayout.Width(150));
            if (newNodeName != nodeName)
            {
                nodeName = newNodeName;
                UpdateNodeNameInHierarchy();
            }

            GUILayout.Label("Completed: " + isCompleted);
            GUILayout.Label("Active: " + isActive);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();


            GUILayout.EndVertical();

            DrawConnectionPoints();
            GUI.DragWindow();
        }

        private void UpdateNodeNameInHierarchy()
        {
            this.gameObject.name = nodeName;
        }
    }
}