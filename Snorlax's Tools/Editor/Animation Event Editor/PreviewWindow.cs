using Snorlax.Animation.Events;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;

public class PreviewWindow : EditorWindow
{
    // Preivewer
    Editor Previewer = null;
    private object timeControlField;
    private FieldInfo FieldInfoFrame;
    public AnimationClip clip = null;

    // Removes instance from Animation Event Editor
    public System.Action NullableAction = null;

    // Instance
    private static PreviewWindow previewWindow = null;
    public static PreviewWindow InstanceWindow 
    {
        get 
        {
            if (previewWindow == null)
            {
                previewWindow = GetWindow<PreviewWindow>("Animation Events");
                return previewWindow;
            }
            else
            {
                return previewWindow;
            }
        }
    }

    public void Repainting(AnimationClip clip, bool repaint = false)
    {
         if(this.clip != clip) this.clip = clip;

        if (Previewer != null && repaint)
        {
            DestroyImmediate(Previewer);
        }

        if (Previewer == null)
        {
            Previewer = Editor.CreateEditor(this.clip);
            Previewer.HasPreviewGUI();
            InitPreviewer(Previewer);
            Repaint();
        }

        void InitPreviewer(Editor editor)
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
    }

    public void SetFrame(float frame)
    {
        if (Previewer == null) return;
        if ((Previewer.target is AnimationClip clip) || FieldInfoFrame != null) FieldInfoFrame.SetValue(timeControlField, frame);
    }

    private void OnGUI()
    {
        Repainting(clip);

        try
        {
            Previewer.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(position.width, position.height), EditorStyles.whiteLabel);
            Repaint();
        }
        catch (System.Exception)
        {
            Previewer = null;
        }
    }

    private void OnDestroy()
    {
        NullableAction();
        previewWindow = null;
        DestroyImmediate(Previewer);
    }
}
