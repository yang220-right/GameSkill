using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class OpenSkillEditor : Editor
{
    [MenuItem("Skill/Open")]
    public static void OpenSkill(){
        EditorWindow window = EditorWindow.GetWindow<CustomWindomsEditor>();
        window.position = new Rect(300f, 300f, 800f, 600f);
        window.Show();
    }
    [MenuItem("Skill/OpenTime")]
    public static void OpenTime(){
        DraggableLabelWindow.ShowWindow();
    }
}
