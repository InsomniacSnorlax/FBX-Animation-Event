using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Snorlax.Animation.Events
{
    public class AnimationEventEditor : EditorWindow
    {
        #region Variable
        // FBX Controller
        private ModelImporter FBX;
        private GameObject Model;
        private UnityEngine.Object FBXObject;

        // Animation Clips and events
        private List<AnimationClip> clips = new List<AnimationClip>();
        private ModelImporterClipAnimation[] animationClips = new ModelImporterClipAnimation[0];
        private ModelImporterClipAnimation[] filtedClips = new ModelImporterClipAnimation[0];
        private List<AnimationEvent> animationEvents = new List<AnimationEvent>();
        private AnimationClip selectedAnimationClip = null;

        // Search strings
        string ClipSearchString = String.Empty;
        string PreviousClipSearchString = String.Empty;
        string selectedString = String.Empty;
        string EventSearchString = String.Empty;

        // Styles
        private Color color_selected = Color.grey;
        private Color color_default;
        private GUIStyle leftButton;

        // Scroll bar
        private Vector2 clipScrollBar = Vector2.zero;
        private Vector2 eventScrollBar = Vector2.zero;

        // Tool Bar 
        private string[] toolbarStrings = { "||", "Play", "<<", "<", ">", ">>" };
        private int SelectedAnimationButton = 0;
        private float AnimationValue;

        // Animation Frames
        private float clipFrame;
        private float previousClipFrame;
        private float editorDeltaTime = 0f;
        private float lastTimeSinceStartup = 0f;

        #endregion

        #region Default Methods
        [MenuItem("Snorlax's Tools/Animation Event Editor")]
        public static void ShowWindow()
        {
            GetWindow<AnimationEventEditor>("Animation Events");
        }

        private void OnGUI()
        {
            if (leftButton == null)
            {
                leftButton = new GUIStyle("toolbarbutton");
                leftButton.alignment = TextAnchor.MiddleLeft;
                color_default = GUI.backgroundColor;
            }

            GUILayout.BeginHorizontal();
            {
                Section1();

                Section2();
            }
            GUILayout.EndHorizontal();
        }

        private void OnSelectionChange()
        {
            FBXObject = Selection.activeObject;
            NewFBXLoaded();
        }

        private void Update()
        {
            SetEditorDeltaTime();
            if (selectedAnimationClip == null) return;

            if (SelectedAnimationButton == 0 && previousClipFrame == clipFrame) return;
            Repaint();

            if (Model) selectedAnimationClip.SampleAnimation(Model, clipFrame);

            clipFrame += editorDeltaTime * AnimationValue;


            if (clipFrame < 0f)
            {
                clipFrame = selectedAnimationClip.length;
            }
            else if (clipFrame >= selectedAnimationClip.length)
            {
                clipFrame = 0f;
            }

            previousClipFrame = clipFrame;

            void SetEditorDeltaTime()
            {
                if (lastTimeSinceStartup == 0f)
                {
                    lastTimeSinceStartup = (float)EditorApplication.timeSinceStartup;
                }
                editorDeltaTime = (float)EditorApplication.timeSinceStartup - lastTimeSinceStartup;
                lastTimeSinceStartup = (float)EditorApplication.timeSinceStartup;
            }
        }
        #endregion

        #region Sections
        private void Section1()
        {
            GUILayout.BeginVertical("box", GUILayout.Width(250), GUILayout.Height(position.height - 8));

            Settingbuttons();

            Animation();

            SearchBar(ref ClipSearchString);
            clipScrollBar = GUILayout.BeginScrollView(clipScrollBar);
            {
                if (ClipSearchString != PreviousClipSearchString)
                {
                    PreviousClipSearchString = ClipSearchString;
                    filtedClips = PreviousClipSearchString == String.Empty ? animationClips : animationClips.Where(e => e.name.ToLower().Contains(PreviousClipSearchString.ToLower())).ToArray();
                }

                if (filtedClips != null) foreach (ModelImporterClipAnimation clips in filtedClips)
                {
                    GUI.backgroundColor = selectedString == clips.name ? color_selected : color_default;
                    if (GUILayout.Button(clips.name, leftButton))
                    {
                        selectedString = clips.name;
                        clipFrame = 0;
                    }

                    GUI.backgroundColor = color_default;
                }
            }
            GUILayout.EndScrollView();

            GUILayout.EndVertical();

            void Settingbuttons()
            {
                GUILayout.BeginHorizontal();

                GUILayout.Label("In Scene Model");
                Model = (GameObject)EditorGUILayout.ObjectField(Model, typeof(GameObject), true);
                if (GUILayout.Button("Save", GUILayout.Width(40)))
                {
                    if (selectedAnimationClip != null)
                    {
                        SerializedObject so = new SerializedObject(FBX);

                        SerializedProperty clips = so.FindProperty("m_ClipAnimations");
                        List<AnimationEvent[]> animationEvents = new List<AnimationEvent[]>(FBX.clipAnimations.Length);

                        for (int i = 0; i < FBX.clipAnimations.Length; i++)
                        {
                            SetEvents(clips.GetArrayElementAtIndex(i));
                        }

                        so.ApplyModifiedProperties();
                        FBX.SaveAndReimport();
                    }
                }

                GUILayout.EndHorizontal();
            }

            void Animation()
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Animation FBX");

                EditorGUI.BeginChangeCheck();
                FBXObject = (UnityEngine.Object)EditorGUILayout.ObjectField(FBXObject, typeof(UnityEngine.Object), false);
                if (EditorGUI.EndChangeCheck())
                {
                    NewFBXLoaded();
                }

                if (GUILayout.Button("Info", GUILayout.Width(40)))
                {
                    GetWindow<Settings>("Information");
                }

                GUILayout.EndHorizontal();
            }

            void SetEvents(SerializedProperty sp)
            {
                var foundClip = this.clips.ToList().Find(e => e.name == sp.FindPropertyRelative("name").stringValue);

                SerializedProperty serializedProperty = sp.FindPropertyRelative("events");
                serializedProperty.ClearArray();
                if (serializedProperty != null && serializedProperty.isArray && foundClip.events != null && foundClip.events.Length > 0)
                {

                    for (int i = 0; i < foundClip.events.Length; i++)
                    {
                        AnimationEvent animationEvent = foundClip.events[i];
                        serializedProperty.InsertArrayElementAtIndex(serializedProperty.arraySize);

                        SerializedProperty eventProperty = serializedProperty.GetArrayElementAtIndex(i);
                        eventProperty.FindPropertyRelative("floatParameter").floatValue = animationEvent.floatParameter;
                        eventProperty.FindPropertyRelative("functionName").stringValue = animationEvent.functionName;
                        eventProperty.FindPropertyRelative("intParameter").intValue = animationEvent.intParameter;
                        eventProperty.FindPropertyRelative("objectReferenceParameter").objectReferenceValue = animationEvent.objectReferenceParameter;
                        eventProperty.FindPropertyRelative("data").stringValue = animationEvent.stringParameter;

                        int frame = (int)Mathf.Round(foundClip.events[i].time * foundClip.frameRate);
                        eventProperty.FindPropertyRelative("time").floatValue = Decimal.ToSingle(new Decimal(frame) / new decimal(foundClip.frameRate * foundClip.length));
                    }
                }
            }
        }

        private void Section2()
        {
            #region Animation Controls and check
            if (selectedString != String.Empty)
            {
                if (selectedAnimationClip == null || selectedAnimationClip.name != selectedString)
                {
                    selectedAnimationClip = clips.ToList().Find(e => e.name.ToLower().Replace(" ", "") == selectedString.ToLower().Replace(" ", ""));
                    animationEvents = selectedAnimationClip.events.ToList();
                    //clipFrame = animationEvents.First().time;
                }
            }
            #endregion

            if (!selectedAnimationClip) return;

            GUILayout.BeginVertical(GUILayout.Height(position.height - 8));
            EditorGUI.BeginChangeCheck();
            AnimationControls();

            #region Frame time logic
            decimal frameTime = (1.0m / new Decimal(selectedAnimationClip.frameRate));
            float clipDuration = selectedAnimationClip == null ? 0 : selectedAnimationClip.length * selectedAnimationClip.frameRate;

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Clip Duration: " + clipDuration);

                SearchBar(ref EventSearchString);

                if (GUILayout.Button("+", GUILayout.Width(40f)))
                {
                    animationEvents.Add(new AnimationEvent() { time = clipFrame });
                    EventSearchString = String.Empty;
                }
            }
            GUILayout.EndHorizontal();
            #endregion

            #region Animation Events
            eventScrollBar = EditorGUILayout.BeginScrollView(eventScrollBar, "box");
            
            for (int i = 0; i < animationEvents.Count; i++)
            {
                AnimationEvent animEvent = animationEvents[i];

                if (!String.IsNullOrEmpty(EventSearchString) && !animEvent.functionName.ToLower().Contains(EventSearchString.ToLower())) continue;

                int frame = (int)Mathf.Round(animEvent.time * selectedAnimationClip.frameRate);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Frame: " + frame);

                    if (GUILayout.Button("-", GUILayout.Width(40f)))
                    {
                        animationEvents.Remove(animationEvents[i]);
                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Method Name");

                    animEvent.functionName = GUILayout.TextField(animEvent.functionName);
                }
                GUILayout.EndHorizontal();

                animEvent.time = Decimal.ToSingle(new Decimal(EditorGUILayout.IntField("Event Frame", frame)) * frameTime);
            }
            EditorGUILayout.EndScrollView();

            if (EditorGUI.EndChangeCheck())
            {
                AnimationUtility.SetAnimationEvents(selectedAnimationClip, animationEvents.ToArray());
            }
            #endregion

            GUILayout.EndVertical();

            void AnimationControls()
            {
                #region Animation Controls
                GUILayout.BeginHorizontal();
                {
                    SelectedAnimationButton = GUILayout.Toolbar(SelectedAnimationButton, toolbarStrings, "toolbarbutton");

                    switch (SelectedAnimationButton)
                    {
                        case (int)1:
                            AnimationValue = 1;
                            break;
                        case (int)2:
                            AnimationValue = -1.5f;
                            break;
                        case (int)3:
                            AnimationValue = -1.2f;
                            break;
                        case (int)4:
                            AnimationValue = 1.2f;
                            break;
                        case (int)5:
                            AnimationValue = 1.5f;
                            break;
                    }
                }
                GUILayout.EndHorizontal();
                #endregion

                #region Current Frame Slider
                float length = selectedAnimationClip == null ? 0 : selectedAnimationClip.length;
                float frameRate = selectedAnimationClip == null ? 0 : selectedAnimationClip.frameRate;
                #endregion

                #region Camera Views
                GUILayout.BeginHorizontal();
                {
                    clipFrame = EditorGUILayout.Slider(clipFrame, 0f, length);
                    GUILayout.Label(Mathf.Round(clipFrame * frameRate) + "/" + length * frameRate, GUILayout.Width(50));
                }
                GUILayout.EndHorizontal();
                #endregion
            }
        }
        #endregion

        private void NewFBXLoaded()
        {
            clipFrame = 0;
            animationEvents.Clear();
            selectedAnimationClip = null;
            selectedString = string.Empty;
            animationClips = null;
            filtedClips = null;
            clips.Clear();
            string path = AssetDatabase.GetAssetPath(FBXObject);
            Repaint();
            if (!path.ToLower().Contains(".fbx")) return;

            FBX = (ModelImporter)AssetImporter.GetAtPath(path);
            animationClips = new ModelImporterClipAnimation[FBX.clipAnimations.Length];
            var Items = AssetDatabase.LoadAllAssetsAtPath(path);
            clips.Clear();
            foreach (var item in Items)
            {
                if (item is AnimationClip clip) clips.Add(clip);

            }

            PreviousClipSearchString = "1";
            Array.Copy(FBX.clipAnimations, animationClips, FBX.clipAnimations.Length);
            Repaint();
        }

        private void SearchBar(ref string SearchString)
        {
            EditorGUILayout.BeginHorizontal();

            SearchString = GUILayout.TextField(SearchString, GUI.skin.FindStyle("ToolbarSeachTextField"));

            if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
            {
                SearchString = "";
            }

            EditorGUILayout.EndHorizontal();
        }

        private class Settings : EditorWindow
        {
            private void OnGUI()
            {
                GUILayout.Label("General", "LargeLabel");
                EditorGUILayout.HelpBox("Remember to click save in order to apply animation events", MessageType.Info, true);
                EditorGUILayout.HelpBox("Models have to be in scene as it will sample the animation with that model. Models can be changed at any time", MessageType.Info, true);
                EditorGUILayout.HelpBox("To load an fbx file's animations, select it from project files or place it in the Animation FBX field", MessageType.Info, true);
            }
        }
    }
}
