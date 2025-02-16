using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// 自定义界面
/// </summary>
public class CustomWindomsEditor : EditorWindow{
    private const string saveFileName = "NamePath"; //保存的文件名称
    bool isChanged = false; //是否改变过
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
                        Resources.Load<GameObject>(namePathList[nodePrefabIndex].Path +
                                                   namePathList[nodePrefabIndex].Name);
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
        GUILayout.BeginHorizontal();
        //begin 左半部分
        GUILayout.BeginVertical(GUILayout.Width(2) /*最小的时候会被撑开*/);
        SkipSkillScene();

        GUILayout.BeginHorizontal();
        GUILayout.Label("加载对象", GUILayout.Width(Fixedwidth), GUILayout.Height(FixedHeight));
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
        GUILayout.Button("", GUILayout.Height(2));
        GUILayout.BeginVertical();
        Exit();
        SaveButton();
        LoadObj();
        GUILayout.EndVertical();
        GUILayout.EndVertical();
        //end 左半部分

        //begin 中间部分
        GUILayout.Button("", GUILayout.Width(2), GUILayout.Height(1920));
        //end 中间部分

        //begin 右半部分
        GUILayout.BeginVertical(GUILayout.Width(2));
        DrawTime();
        GUILayout.EndVertical();
        //end 右半部分
        GUILayout.EndHorizontal();
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

    private void LoadObj(){
        if (GUILayout.Button("Load", GUILayout.Height(FixedHeight), GUILayout.Width(Fixedwidth))){
            LoadPrefab(NodePrefab, selectedObject);
        }
    }

    private void SkipSkillScene(){
        if (GUILayout.Button("跳转场景", GUILayout.Height(FixedHeight), GUILayout.Width(Fixedwidth * 2 + 25))){
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.path == ScenePath) return;
            EditorSceneManager.OpenScene(ScenePath);
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
                    pathKeep.Add(key, path);
                }
                else{
                    namePathList.RemoveAt(namePathList.FindIndex(data => data.Key == key));
                    pathKeep[key] = path;
                }

                namePathList.Add(new NamePath{
                    Key = key,
                    FullPath = path,
                    Path = path.Replace("Assets/Resources/", "").Split(obj.name)[0],
                    Name = obj.name,
                });
            }
        }
        else{
            var isExists = pathKeep.ContainsKey(key);
            if (isExists){
                isChanged = true;
                pathKeep.Remove(key);
                namePathList.RemoveAt(namePathList.FindIndex(data => data.Key == key));
            }
        }
    }

    private const string ScenePath = "Assets/Scenes/Skill.unity";

    private void LoadPrefab(string pathName, GameObject obj){ //GameObject pre,GameObject prefab
        var activeScene = EditorSceneManager.GetActiveScene();
        if (activeScene.path != ScenePath || obj == null) return;
        var objs = activeScene.GetRootGameObjects();
        bool desFlag = false;
        foreach (var gameObject in objs){
            if (gameObject.name == pathName){
                DestroyImmediate(gameObject);
                desFlag = true;
                break;
            }
        }

        var parentObj = new GameObject(pathName);
        if (desFlag){ }

        //生成预制体
        var g = Instantiate(obj, parentObj.transform);
    }


    private float currentTime = 0f; // 当前时间
    private float minTime = 0f; // 最小时间
    private float maxTime = 10f; // 最大时间
    private float timeScale = 1f; // 时间缩放
    private Vector2 scrollPosition; // 滚动位置
    private bool isDragging = false; // 是否正在拖动指针

    #region time

    private void DrawTime(){
        DrawControls();
        DrawTimeline();
        HandleEvents();
    }
    private void DrawControls(){
        // 时间范围控制
        EditorGUILayout.BeginHorizontal(GUILayout.Width(2));
        GUILayout.Label("Min Time:", GUILayout.Width(60));
        minTime = EditorGUILayout.FloatField(minTime);
        GUILayout.Label("Max Time:", GUILayout.Width(60));
        maxTime = EditorGUILayout.FloatField(maxTime);
        GUILayout.Label("Current Time:", GUILayout.Width(60));
        currentTime = EditorGUILayout.FloatField(currentTime);
        EditorGUILayout.EndHorizontal();
        // 当前时间和缩放
        // GUILayout.Label($"CurrentTime: {currentTime:F2}");
        // currentTime = EditorGUILayout.Slider("Current Time", currentTime, minTime, maxTime);
        // timeScale = EditorGUILayout.Slider("Time Scale", timeScale, 0.1f, 5f);
    }

    private void DrawTimeline(){
        // 开始滚动视图
        // scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(100));

        // 获取时间轴的布局区域
        Rect timelineRect = GUILayoutUtility.GetRect(1000, 20); // 固定高度，宽度根据内容扩展

        // 绘制时间轴背景
        EditorGUI.DrawRect(timelineRect, new Color(0.1f, 0.1f, 0.1f, 1f));

        // 动态计算刻度间隔
        float totalDuration = maxTime - minTime;
        float pixelsPerSecond = timelineRect.width / totalDuration;
        float majorInterval =CalculateMajorInterval(pixelsPerSecond); // 主刻度间隔（秒）
        float minorInterval = majorInterval / 5f; // 次刻度间隔（秒）
        // 绘制刻度线
        int currentIndex = 0;
        for (float t = minTime; t <= maxTime; t += minorInterval){
            bool isMajor = currentIndex % 5 == 0; // 判断是否主刻度
            float x = ((t - minTime) / totalDuration) * timelineRect.width;
            DrawTick(timelineRect, x, isMajor ? 15 : 10, isMajor ? Color.white : Color.gray);

            if (isMajor){
                // 绘制时间标签
                Rect labelRect = new Rect(timelineRect.x + x - 5 , timelineRect.y + 15, 40, 20);
                GUI.Label(labelRect, t.ToString("F1"));
            }
            ++currentIndex;
        }

        // 绘制当前时间指针
        float pointerX = ((currentTime - minTime) / totalDuration) * timelineRect.width;
        EditorGUI.DrawRect(new Rect(timelineRect.x + pointerX, timelineRect.y, 2, 1080), Color.red);

        // GUILayout.EndScrollView();
    }

    /// <summary>
    /// 绘制刻度
    /// </summary>
    private void DrawTick(Rect timelineRect, float x, float height, Color color){
        EditorGUI.DrawRect(new Rect(timelineRect.x + x, timelineRect.y + timelineRect.height - height, 1, height),
            color);
    }
    private float CalculateMajorInterval(float pixelsPerSecond)
    {
        // 动态调整主刻度间隔，确保标签不重叠
        float[] possibleIntervals = { 1f, 2f, 5f, 10f, 30f, 60f };
        foreach (float interval in possibleIntervals)
        {
            if (interval * pixelsPerSecond > 50) // 间隔至少50像素
                return interval;
        }
        return possibleIntervals[possibleIntervals.Length - 1];
    }
    /// <summary>
    /// 鼠标事件
    /// </summary>
    private void HandleEvents(){
        Event evt = Event.current;
        Rect timelineRect = GUILayoutUtility.GetLastRect();

        // 鼠标拖动事件
        if (evt.type == EventType.MouseDown && timelineRect.Contains(evt.mousePosition)){
            isDragging = true;
            evt.Use();
        }

        if (evt.type == EventType.MouseUp){
            isDragging = false;
        }

        if (isDragging && evt.type == EventType.MouseDrag){
            float clickX = evt.mousePosition.x - timelineRect.x;
            currentTime = minTime + (clickX / timelineRect.width) * (maxTime - minTime);
            currentTime = Mathf.Clamp(currentTime, minTime, maxTime);
            Repaint();
        }
    }

    #endregion
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