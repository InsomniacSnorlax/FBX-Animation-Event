using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Snorlax.Animation.Events
{
    public static class Helper
    {
        public static float ReturnValue(int value, List<float> floats)
        {
            for (int i = 0; i < floats.Count; i++)
            {
                if (value == i) return floats[i];
            }

            return 0;
        }

        public static void SetEvents(SerializedProperty sp, List<AnimationClip> clips)
        {
            var foundClip = clips.ToList().Find(e => e.name == sp.FindPropertyRelative("name").stringValue);

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

        public static void ListOfEventMethods(Animator anim,  ref List<string> arrayEventMethodName)
        {
            MonoBehaviour[] arrayMonoBehaviour = anim.GetComponents<MonoBehaviour>();

            List<string> tmpNames = new List<string>();

            foreach (MonoBehaviour mono in arrayMonoBehaviour)
            {
                #region Gets Animator Methods from components
                Type type = mono.GetType();
                MethodInfo[] arrayMethodInfo = type.GetMethods();

                IEnumerable<MethodInfo> tmpInfos = arrayMethodInfo.Where
                (
                    p =>
                    p.IsPublic &&
                    p.ReturnType == typeof(void) &&
                    (p.GetParameters().Select(q => q.ParameterType).SequenceEqual(new Type[] { }) //||
                                                                                                  //p.GetParameters().Select(q => q.ParameterType).SequenceEqual(new Type[] { typeof(int) }) ||
                                                                                                  //p.GetParameters().Select(q => q.ParameterType.BaseType).SequenceEqual(new Type[] { typeof(Enum) }) ||
                                                                                                  //p.GetParameters().Select(q => q.ParameterType).SequenceEqual(new Type[] { typeof(float) }) ||
                                                                                                  // p.GetParameters().Select(q => q.ParameterType).SequenceEqual(new Type[] { typeof(string) }) ||
                                                                                                  //p.GetParameters().Select(q => q.ParameterType).SequenceEqual(new Type[] { typeof(UnityEngine.Object) }))
                ));
                #endregion

                #region Event Method Names
                foreach (MethodInfo info in tmpInfos)
                {
                    ParameterInfo[] paramInfo = info.GetParameters();
                    if (paramInfo.Length == 0)
                    {
                        tmpNames.Add(type + "." + info.Name + "()");
                    }
                    else
                    {
                        tmpNames.Add(type + "." + info.Name + "(" + paramInfo[0].ParameterType + ")");
                    }
                }
                #endregion
            }

            arrayEventMethodName = tmpNames;
        }

        
    }

    // Property of Yaell. Github enforcer and royal guard to Kiran
    public static class Wrapper
    {
        public static void HorizontalWrapper(Action content)
        {
            EditorGUILayout.BeginHorizontal();

            content();

            EditorGUILayout.EndHorizontal();
        }

        public static void VerticalWrapper(Action content) 
        {
            EditorGUILayout.BeginVertical("box");

            content();

            EditorGUILayout.EndVertical();
        }

        public static void ScrollWrapper(ref Vector2 scroll, Action content) 
        {
            scroll = GUILayout.BeginScrollView(scroll);

            content();

            GUILayout.EndScrollView();
        }

        public static void SearchBar(ref string SearchString)
        {
            EditorGUILayout.BeginHorizontal();

            SearchString = GUILayout.TextField(SearchString, GUI.skin.FindStyle("ToolbarSeachTextField"));

            if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
            {
                SearchString = "";
            }

            EditorGUILayout.EndHorizontal();
        }
        
        // Yes I use this a lot
        public static void SmallButton(string label, Action content, float size = 40f)
        {
            if (GUILayout.Button(label, GUILayout.Width(size)))
            {
                content();
            }
        }

        public static void LabeledField(string label, Action content)
        {
            HorizontalWrapper(() =>
            {
                GUILayout.Label(label);

                content();
            });
        }

        public static void BeginChecks(Action content, Action check)
        {
            EditorGUI.BeginChangeCheck();

            content();

            if(EditorGUI.EndChangeCheck())
            {
                check();
            }
        }

        public static void IsNotCheck<T>(T check1, T check2, Action content)
        {
            if(!check1.Equals(check2))
            {
                content();
            }
        }   
    }
}