using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SimpleGraph
{
    public class NodeEditor : EditorWindow
    {
        public static List<GraphNode> nodes = new List<GraphNode>();

        [MenuItem("Window/Node Editor")]
        static void ShowEditor()
        {
            NodeEditor editor = EditorWindow.GetWindow<NodeEditor>();
            editor.titleContent = new GUIContent("Node Editor");
        }

        void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Node"))
            {
                // Add node creation logic here
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            BeginWindows();
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].windowRect = GUI.Window(i, nodes[i].windowRect, nodes[i].DrawNodeWindow, nodes[i].nodeName);
            }
            EndWindows();
            // Draw connection points after the windows
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].DrawConnectionPoints();
            }
        }

        public static void RemoveNode(GraphNode node)
        {
            nodes.Remove(node);
            DestroyImmediate(node.gameObject);
        }
    }
}