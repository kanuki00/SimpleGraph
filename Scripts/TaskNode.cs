using UnityEngine;
using UnityEngine.Events;
using UnityEditor;

namespace SimpleGraph {
   public class TaskNode : GraphNode    {
        public override void DrawNodeWindow(int id)
        {
            // Custom appearance for TaskNode
            GUILayout.BeginVertical();
            GUILayout.Label("Task Node", EditorStyles.label);
            GUILayout.EndVertical();

            base.DrawNodeWindow(id);
        }
    }
}