using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 自定义界面
/// </summary>
public class CustomWindomsEditor : EditorWindow{
    private const string saveFileName = "NamePath";//保存的文件名称
    bool isChanged = false;//是否改变过
    private bool isClickExit = false;

    private List<string> fullPathList = new List<string>();
    private List<string> tempPathList = new List<string>();
    private List<NamePath> namePathList = new List<NamePath>();
    private Dictionary<string, string> pathKeep = new();

    private const string NodePrefab = "GamePrefab";

    private GameObject selectedObject; // 选择的对象

    void OnEnable(){
        fullPathList.Clear();
        tempPathList.Clear();
        namePathList.Clear();
        pathKeep.Clear();
        titleContent = new GUIContent("Skill");
        //加载路径
        if (JsonTools.ReadJson<NamePathWrapper>(saveFileName) != null){
            namePathList = JsonTools.ReadJson<NamePathWrapper>(saveFileName).DatasList;
            foreach (var namePath in namePathList){
                fullPathList.Add(namePath.Key);
                pathKeep.Add(namePath.Key, namePath.FullPath);
            }
            if (!string.IsNullOrEmpty(saveFileName)){
                var nodePrefabIndex = -1;
                nodePrefabIndex = fullPathList.FindIndex(d => d == NodePrefab);
                if (nodePrefabIndex != -1){
                    selectedObject =
                        Resources.Load<GameObject>(namePathList[nodePrefabIndex].Path + namePathList[nodePrefabIndex].Name);
                }
            }
        }
    }

    private void OnDisable(){
        if (!isClickExit){
            TipRecordAndSave();
        }
        fullPathList.Clear();
        tempPathList.Clear();
        namePathList.Clear();
        pathKeep.Clear();
        isChanged = false;
        isClickExit = false;
    }

    const int FixedHeight = 35;
    const int Fixedwidth = 80;

    // private ScriptableObject selectedScriptableObject; // 选择的 ScriptableObject
    // private Material selectedMaterial; // 选择的 Material
    private void OnGUI(){
        GUI.color = Color.white;
        GUILayout.BeginHorizontal();
        GUILayout.Label("选择对象", GUILayout.Width(Fixedwidth), GUILayout.Height(FixedHeight));

        selectedObject = (GameObject)EditorGUILayout.ObjectField(
            selectedObject,
            typeof(GameObject),
            false,
            GUILayout.Width(100), // 固定宽度
            GUILayout.Height(FixedHeight) // 固定高度
        );
        // selectedScriptableObject = (ScriptableObject)EditorGUILayout.ObjectField("SelectSO", selectedScriptableObject, typeof(ScriptableObject), true);
        // selectedMaterial = (Material)EditorGUILayout.ObjectField("Select Material", selectedMaterial, typeof(Material), true);

        GUILayout.EndHorizontal();

        GUILayout.BeginVertical();
        Exit();
        SaveButton();
        GUILayout.EndVertical();
    }

    private void Exit(){
        if (GUILayout.Button("Exit", GUILayout.Height(FixedHeight), GUILayout.Width(Fixedwidth))){
            isClickExit = true;
            TipRecordAndSave();
            Close();
        }
    }

    private void SaveButton(){
        if (GUILayout.Button("Save", GUILayout.Height(FixedHeight), GUILayout.Width(Fixedwidth))){
            RecordAndSave();
        }
    }
    /// <summary>
    /// 记录数据
    /// </summary>
    private void RecordData(){
        SaveObj(NodePrefab, selectedObject);
    }
    /// <summary>
    /// 保存数据
    /// </summary>
    private void Save(){
        JsonTools.SaveJson(new NamePathWrapper(){ DatasList = namePathList }, saveFileName);
    }

    private void RecordAndSave(){
        RecordData();
        Save();
    }

    private void TipRecordAndSave(){
        RecordData();
        if (isChanged && EditorUtility.DisplayDialog("", "是否保存?", "是", "否")){
            Save();
        }
    }
    private void SaveObj(string key, GameObject obj){
        if (obj != null){
            var path = AssetDatabase.GetAssetPath(obj);
            var isExists = pathKeep.ContainsKey(key);
            if (!isExists || isExists && pathKeep[key] != path){
                isChanged = true;
                //更新path
                if (!isExists){
                    pathKeep.Add(key,path);
                }
                else{
                    namePathList.RemoveAt(namePathList.FindIndex(data=>data.Key == key));
                    pathKeep[key] = path;
                }
                namePathList.Add(new NamePath{
                    Key = key,
                    FullPath = path,
                    Path = path.Replace("Assets/Resources/", "").Split(obj.name)[0],
                    Name = obj.name,
                });
            }
        }else{
            var isExists = pathKeep.ContainsKey(key);
            if (isExists){
                isChanged = true;
                pathKeep.Remove(key);
                namePathList.RemoveAt(namePathList.FindIndex(data=>data.Key == key));
            }
        }
    }
}

[System.Serializable]
public class NamePath{
    public string Key; //key值
    public string FullPath;
    public string Path;
    public string Name;
}

[System.Serializable]
public class NamePathWrapper{
    public List<NamePath> DatasList;
}