using System;
using UnityEditor;
using UnityEngine;

namespace Snorlax.EditorUtlities
{
    // Property of Yaell. Github enforcer and royal guard to Kiran
    public static class Wrapper
    {
        public static void HorizontalWrapper(Action content)
        {
            GUILayout.BeginHorizontal();

            content();

            GUILayout.EndHorizontal();
        }

        public static void VerticalWrapper(Action content)
        {
            GUILayout.BeginVertical("box");

            content();

            GUILayout.EndVertical();
        }

        public static void ScrollWrapper(ref Vector2 scroll, Action content)
        {
            scroll = GUILayout.BeginScrollView(scroll);

            content();

            GUILayout.EndScrollView();
        }

        public static void SearchBar(ref string SearchString)
        {
            GUILayout.BeginHorizontal();

            SearchString = GUILayout.TextField(SearchString);

            GUILayout.EndHorizontal();
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

            if (EditorGUI.EndChangeCheck())
            {
                check();
            }
        }

        public static void IsNotCheck<T>(T check1, T check2, Action content)
        {
            if (!check1.Equals(check2))
            {
                content();
            }
        }
    }
}
