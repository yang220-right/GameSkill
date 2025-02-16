using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class OpenSkillEditor : Editor
{
    [MenuItem("Skill/Open")]
    public static void OpenSkill(){
        EditorWindow window = EditorWindow.GetWindow<CustomWindomsEditor>();
        window.position = new Rect(50f, 50f, 800f, 600f);
        window.Show();
    }
}
