using UnityEditor;
using UnityEngine;

public class PrecisionTimelineWindow : EditorWindow
{
    private float currentTime = 0f;        // 当前时间
    private float minTime = 0f;            // 最小时间
    private float maxTime = 60f;           // 最大时间
    private float timeScale = 1f;          // 时间缩放
    private Vector2 scrollPosition;        // 滚动位置
    private bool isDragging = false;       // 是否正在拖动指针

    [MenuItem("Window/Precision Timeline")]
    public static void ShowWindow()
    {
        GetWindow<PrecisionTimelineWindow>("Timeline");
    }

    private void OnGUI()
    {
        DrawControls();
        DrawTimeline();
        HandleEvents();
    }

    private void DrawControls()
    {
        // 时间范围控制
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Min Time:", GUILayout.Width(60));
        minTime = EditorGUILayout.FloatField(minTime);
        GUILayout.Label("Max Time:", GUILayout.Width(60));
        maxTime = EditorGUILayout.FloatField(maxTime);
        EditorGUILayout.EndHorizontal();

        // 当前时间和缩放
        currentTime = EditorGUILayout.Slider("Current Time", currentTime, minTime, maxTime);
        timeScale = EditorGUILayout.Slider("Time Scale", timeScale, 0.1f, 5f);
    }

    private void DrawTimeline()
    {
        // 开始滚动视图
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(100));

        // 获取时间轴的布局区域
        Rect timelineRect = GUILayoutUtility.GetRect(1000, 20); // 固定高度，宽度根据内容扩展

        // 绘制时间轴背景
        EditorGUI.DrawRect(timelineRect, new Color(0.1f, 0.1f, 0.1f, 1f));

        // 动态计算刻度间隔
        float totalDuration = maxTime - minTime;
        float pixelsPerSecond = timelineRect.width / totalDuration;
        float majorInterval = CalculateMajorInterval(pixelsPerSecond); // 主刻度间隔（秒）
        float minorInterval = majorInterval / 5f;                      // 次刻度间隔（秒）

        // 绘制刻度线
        for (float t = minTime; t <= maxTime; t += minorInterval)
        {
            bool isMajor = (t % majorInterval) < 0.001f; // 判断是否主刻度
            float x = ((t - minTime) / totalDuration) * timelineRect.width;
            DrawTick(timelineRect, x, isMajor ? 15 : 10, isMajor ? Color.white : Color.gray);
            
            if (isMajor)
            {
                // 绘制时间标签
                Rect labelRect = new Rect(timelineRect.x + x - 20, timelineRect.y + 15, 40, 20);
                GUI.Label(labelRect, t.ToString("F1"));
            }
        }

        // 绘制当前时间指针
        float pointerX = ((currentTime - minTime) / totalDuration) * timelineRect.width;
        EditorGUI.DrawRect(new Rect(timelineRect.x + pointerX, timelineRect.y, 2, timelineRect.height), Color.red);

        GUILayout.EndScrollView();
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

    private void DrawTick(Rect timelineRect, float x, float height, Color color)
    {
        EditorGUI.DrawRect(
            new Rect(timelineRect.x + x, timelineRect.y + timelineRect.height - height, 1, height),
            color
        );
    }

    private void HandleEvents()
    {
        Event evt = Event.current;
        Rect timelineRect = GUILayoutUtility.GetLastRect();

        // 鼠标拖动事件
        if (evt.type == EventType.MouseDown && timelineRect.Contains(evt.mousePosition))
        {
            isDragging = true;
            evt.Use();
        }

        if (evt.type == EventType.MouseUp)
        {
            isDragging = false;
        }

        if (isDragging && evt.type == EventType.MouseDrag)
        {
            float clickX = evt.mousePosition.x - timelineRect.x;
            currentTime = minTime + (clickX / timelineRect.width) * (maxTime - minTime);
            currentTime = Mathf.Clamp(currentTime, minTime, maxTime);
            Repaint();
        }
    }
}