using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Object = UnityEngine.Object;
using Snorlax.EditorUtilities;

namespace Snorlax.Animation.Events
{
    public class AnimationEventEditor : EditorWindow
    {
        #region Variable
        // FBX Controller
        private ModelImporter FBX;
        private GameObject Model;
        private Object FBXObject;

        // Animation Clips and events
        private List<AnimationClip> clips = new List<AnimationClip>();
        private ModelImporterClipAnimation[] animationClips = new ModelImporterClipAnimation[0];
        private ModelImporterClipAnimation[] filtedClips = new ModelImporterClipAnimation[0];
        private List<AnimationEvent> animationEvents = new List<AnimationEvent>();
        private AnimationClip selectedAnimationClip = null;

        // Search strings
        string ClipSearchString = string.Empty;
        string PreviousClipSearchString = string.Empty;
        string SelectedAnimatinClipName = string.Empty;
        string EventSearchString = string.Empty;

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

        private string[] toolbarInfo = { "Events", "Curves", "Settings", "Preview" };
        private int SelectedInfoBar = 0;

        // Animation Frames
        private float clipFrame;
        private float editorDeltaTime = 0f;
        private float lastTimeSinceStartup = 0f;
        private bool repaint = false;
        private float previousFrame;

        // Preivewer
        Editor Previewer = null;
        private static object timeControlField;
        private static FieldInfo FieldInfoFrame;

        // Misc
        private List<float> AnimationValueFloats = new List<float> { 0, 1, -1.5f, -1.2f, 1.2f, 1.5f };
        private List<string> arrayEventMethodName = new List<string>();
        private int selectedEvent = -1;
        private List<AnimationEvent> copiedEvents = new List<AnimationEvent>();
        private PreviewWindow previewWindow;
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

            Wrapper.HorizontalWrapper(() =>
            {
                Section1();

                Section2();
            });
        }

        private void OnSelectionChange()
        {
            NewFBXLoaded(Selection.activeObject);
        }

        private void Update()
        {
            SetEditorDeltaTime();

            if (selectedAnimationClip == null) return;

            #region Clip Frame Logic
            
            clipFrame += editorDeltaTime * Helper.ReturnValue(SelectedAnimationButton, AnimationValueFloats);

            if (clipFrame < 0f)
            {
                clipFrame = selectedAnimationClip.length;
            }
            else if (clipFrame >= selectedAnimationClip.length)
            {
                clipFrame = 0f;
            }
            #endregion

            if (previousFrame != clipFrame)
            {
                SetFrame(clipFrame);
                if (previewWindow != null) previewWindow.SetFrame(clipFrame);
                if (Model) selectedAnimationClip.SampleAnimation(Model, clipFrame);

                previousFrame = clipFrame;
            }

            Repaint();

            void SetEditorDeltaTime()
            {
                if (lastTimeSinceStartup == 0f)
                {
                    lastTimeSinceStartup = (float)EditorApplication.timeSinceStartup;
                }
                editorDeltaTime = (float)EditorApplication.timeSinceStartup - lastTimeSinceStartup;
                lastTimeSinceStartup = (float)EditorApplication.timeSinceStartup;
            }

            void SetFrame(float frame)
            {
                if (Previewer == null) return;
                if ((Previewer.target is AnimationClip clip) || FieldInfoFrame != null) FieldInfoFrame.SetValue(timeControlField, frame);
            }
        }
        #endregion

        #region Sections
        private void Section1()
        {
            GUILayout.BeginVertical("box", GUILayout.Width(250), GUILayout.Height(position.height - 8)); ////

            Wrapper.LabeledField("In Scene Model", () =>
            {
                Wrapper.BeginChecks(
                    () => Model = (GameObject)EditorGUILayout.ObjectField(Model, typeof(GameObject), true),
                    () =>
                    {
                        arrayEventMethodName.Clear();
                        if (Model != null && Model.TryGetComponent<Animator>(out Animator anim))
                            Helper.ListOfEventMethods(anim, ref arrayEventMethodName);
                    });

                Wrapper.SmallButton("Save", () =>
                {
                    if (selectedAnimationClip != null)
                    {
                        SerializedObject so = new SerializedObject(FBX);
                        SerializedProperty SerializedClips = so.FindProperty("m_ClipAnimations");

                        for (int i = 0; i < FBX.clipAnimations.Length; i++)
                        {
                            Helper.SetEvents(SerializedClips.GetArrayElementAtIndex(i), clips);
                        }

                        so.ApplyModifiedProperties();
                        FBX.SaveAndReimport();
                    }
                });
            });

            Wrapper.LabeledField("Animation FBX", () =>
            {
                Wrapper.BeginChecks(
                    () => FBXObject = (Object)EditorGUILayout.ObjectField(FBXObject, typeof(Object), false),
                    () =>
                    {
                        NewFBXLoaded(FBXObject);
                    });

                Wrapper.SmallButton("Info", () =>
                {
                    GetWindow<AnimationEventEditor>("Animation Events");
                });
            });

            Wrapper.SearchBar(ref ClipSearchString);

            Wrapper.ScrollWrapper(ref clipScrollBar, () =>
            {
                Wrapper.IsNotCheck<string>(ClipSearchString, PreviousClipSearchString, () =>
                {
                    PreviousClipSearchString = ClipSearchString;
                    filtedClips = PreviousClipSearchString == string.Empty ? animationClips : animationClips.Where(e => e.name.ToLower().Contains(PreviousClipSearchString.ToLower())).ToArray();
                });

                if(filtedClips != null)
                {
                    foreach (ModelImporterClipAnimation clips in filtedClips)
                    {
                        GUI.backgroundColor = SelectedAnimatinClipName == clips.name ? color_selected : color_default;
                        if (GUILayout.Button(clips.name, leftButton))
                        {
                            repaint = true;
                            SelectedAnimatinClipName = clips.name;
                            clipFrame = 0;
                        }

                        GUI.backgroundColor = color_default;
                    }
                }
            });

            GUILayout.EndVertical(); ////
        }

        private void Section2()
        {
            Wrapper.IsNotCheck<string>(SelectedAnimatinClipName, string.Empty, () =>
            {
                if (selectedAnimationClip == null || selectedAnimationClip.name != SelectedAnimatinClipName)
                {
                    selectedAnimationClip = clips.ToList().Find(e => e.name.ToLower().Replace(" ", "") == SelectedAnimatinClipName.ToLower().Replace(" ", ""));
                    animationEvents = selectedAnimationClip.events.ToList();
                }
            });

            if (!selectedAnimationClip) return;

            GUILayout.BeginVertical(GUILayout.Height(position.height - 8)); ////

            SelectedInfoBar = GUILayout.Toolbar(SelectedInfoBar, toolbarInfo, "toolbarbutton");

            #region Info Bar sections
            if (SelectedInfoBar == 0 || SelectedInfoBar == 3) AnimationControls();

            if(SelectedInfoBar == 0) AnimationEventInfo();

            if(SelectedInfoBar == 1 || SelectedInfoBar == 2) EditorGUILayout.HelpBox("Remind future snorlax to do it. Current me is a mix of tired and lazy", MessageType.Error, true);

            if (SelectedInfoBar == 3) Preview();
            #endregion

            GUILayout.EndVertical(); ////

            void AnimationControls()
            {
                Wrapper.HorizontalWrapper(() =>
                {
                    SelectedAnimationButton = GUILayout.Toolbar(SelectedAnimationButton, toolbarStrings, "toolbarbutton");
                });

                #region Current Frame Slider
                float length = selectedAnimationClip == null ? 0 : selectedAnimationClip.length;
                float frameRate = selectedAnimationClip == null ? 0 : selectedAnimationClip.frameRate;

                Wrapper.HorizontalWrapper(() =>
                {
                    clipFrame = EditorGUILayout.Slider(clipFrame, 0f, length);
                    GUILayout.Label(Mathf.Round(clipFrame * frameRate) + "/" + length * frameRate, GUILayout.Width(50));
                });
                #endregion
            }

            void AnimationEventInfo()
            {
                Wrapper.BeginChecks(
                () =>
                {
                    decimal frameTime = (1.0m / new decimal(selectedAnimationClip.frameRate));
                    float clipDuration = selectedAnimationClip == null ? 0 : selectedAnimationClip.length * selectedAnimationClip.frameRate;

                    Wrapper.LabeledField("Clip Duration: " + clipDuration, () =>
                    {
                        Wrapper.SearchBar(ref EventSearchString);

                        Wrapper.SmallButton("Copy", () =>
                        {
                            copiedEvents = selectedAnimationClip.events.ToList();
                        });

                        Wrapper.SmallButton("Paste", () =>
                        {
                            if (copiedEvents != null) animationEvents = copiedEvents;
                        }, 50f);

                        Wrapper.SmallButton("+", () =>
                        {
                            animationEvents.Add(new AnimationEvent() { time = clipFrame });
                            EventSearchString = string.Empty;
                        });
                    });

                    Wrapper.ScrollWrapper(ref eventScrollBar,  () =>
                    {
                        for (int i = 0; i < animationEvents.Count; i++)
                        {
                            AnimationEvent animEvent = animationEvents[i];

                            if (!string.IsNullOrEmpty(EventSearchString) && !animEvent.functionName.ToLower().Contains(EventSearchString.ToLower())) continue;

                            Wrapper.VerticalWrapper(() =>
                            {
                                int frame = (int)Mathf.Round(animEvent.time * selectedAnimationClip.frameRate);

                                Wrapper.LabeledField("Frame: " + frame, () =>
                                {
                                    Wrapper.SmallButton("-", () => animationEvents.Remove(animationEvents[i]));
                                });

                                Wrapper.LabeledField("Method Name", () =>
                                {
                                    animEvent.functionName = GUILayout.TextField(animEvent.functionName);

                                    if (GUILayout.Button("", EditorStyles.popup, GUILayout.Width(20f)))
                                    {
                                        selectedEvent = i;
                                        SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)),
                                            new SearchTreeEventMethods(arrayEventMethodName.ToArray(), SaveEventName));
                                    }
                                });

                                animEvent.time = decimal.ToSingle(new decimal(EditorGUILayout.IntField("Event Frame", frame)) * frameTime);
                            });
                        }
                    });

                },
                () => AnimationUtility.SetAnimationEvents(selectedAnimationClip, animationEvents.ToArray()));
            }

            void Preview()
            {
                Wrapper.SmallButton("Open Preview", () =>
                {
                    previewWindow = PreviewWindow.InstanceWindow;
                    previewWindow.Repainting(selectedAnimationClip);
                    previewWindow.NullableAction = () => { previewWindow = null; };
                }, 100f);

                if (Previewer != null && repaint)
                {
                    if (previewWindow != null) previewWindow.Repainting(selectedAnimationClip, repaint);
                    DestroyImmediate(Previewer);
                    repaint = false;
                }

                if (Previewer == null)
                {
                    Previewer = Editor.CreateEditor(selectedAnimationClip);
                    Previewer.HasPreviewGUI();
                    InitPreviewer(Previewer);
                    Repaint();
                }

                try
                {
                    var rectPosition = GUILayoutUtility.GetRect(0, 0);
                    rectPosition.Set(rectPosition.x, rectPosition.y, position.width - rectPosition.x, position.height - rectPosition.y);
                    Previewer.OnInteractivePreviewGUI(rectPosition, EditorStyles.whiteLabel);
                    Repaint();
                }
                catch (System.Exception)
                {
                    Previewer = null;
                }
            }
        }
        #endregion

        #region Left over methods
        private void NewFBXLoaded(Object target)
        {
            string path = AssetDatabase.GetAssetPath(target);
            Repaint();
            if (!path.ToLower().Contains(".fbx")) return;

            #region Reset Everything
            clipFrame = 0;
            animationEvents.Clear();
            selectedAnimationClip = null;
            SelectedAnimatinClipName = string.Empty;
            animationClips = null;
            filtedClips = null;
            PreviousClipSearchString = "1";
            clips.Clear();
            #endregion

           
            FBXObject = target;
            #region Set FBX and AnimationClip Info
            FBX = (ModelImporter)AssetImporter.GetAtPath(path);
            animationClips = new ModelImporterClipAnimation[FBX.clipAnimations.Length];
            var Items = AssetDatabase.LoadAllAssetsAtPath(path);
     
            foreach (var item in Items)
            {
                if (item is AnimationClip clip) clips.Add(clip);
            }

            System.Array.Copy(FBX.clipAnimations, animationClips, FBX.clipAnimations.Length);
            Repaint();
            #endregion
        }

        public void SaveEventName(string methodName)
        {
            if (selectedEvent == -1) return;
            animationEvents[selectedEvent].functionName = methodName;
            selectedEvent = -1;
            AnimationUtility.SetAnimationEvents(selectedAnimationClip, animationEvents.ToArray());
        }

        private static void InitPreviewer(Editor editor)
        {
            if (!(editor.target is AnimationClip clip)) return;
            var avatarPreviewFieldInfo = editor.GetType().GetField("m_AvatarPreview", BindingFlags.NonPublic | BindingFlags.Instance);
            if (avatarPreviewFieldInfo == null) return;
            var value = avatarPreviewFieldInfo.GetValue(editor);
            if (value == null) return;
            var timeControl = value.GetType().GetField("timeControl", BindingFlags.Public | BindingFlags.Instance);
            if (timeControl == null) return;
            timeControlField = timeControl.GetValue(value);
            if (timeControlField == null) return;
            var stopTime = timeControlField.GetType().GetField("stopTime", BindingFlags.Public | BindingFlags.Instance);
            if (stopTime == null) return;
            stopTime.SetValue(timeControlField, clip.length);
            FieldInfoFrame = timeControlField.GetType().GetField("currentTime", BindingFlags.Public | BindingFlags.Instance);
        }
        #endregion
    }
}
