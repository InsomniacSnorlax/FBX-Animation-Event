using UnityEditor;
using UnityEngine;

namespace Snorlax.Animation.Events
{
    public class EditorSettings : EditorWindow
    {
        private void OnGUI()
        {
            GUILayout.Label("General", "LargeLabel");
            EditorGUILayout.HelpBox("Remember to click save in order to apply animation events", MessageType.Info, true);
            EditorGUILayout.HelpBox("Models have to be in scene as it will sample the animation with that model. Models can be changed at any time", MessageType.Info, true);
            EditorGUILayout.HelpBox("To load an fbx file's animations, select it from project files or place it in the Animation FBX field", MessageType.Info, true);
            EditorGUILayout.HelpBox("To enable the event method search tree, you have to put a gameObject with the animator component in the 'In Scene Model' field where it will detect all the scripts found and display public methods", MessageType.Info, true);
            EditorGUILayout.HelpBox("Can't get rid of the second slider for the previewer without actual work. Luckily using it instead will provide root motion movement so accidental feature", MessageType.Info, true);
        }
    }
}