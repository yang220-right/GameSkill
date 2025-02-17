using UnityEngine;
using UnityEditor;

public class DraggableLabelWindow : EditorWindow
{
    private int selectedRow = -1; // 当前选中的行索引
    private Vector2 labelOffset = Vector2.zero; // 记录 Label 的拖拽偏移
    private bool isDragging = false; // 记录拖拽状态

    [MenuItem("Window/Draggable Label")]
    public static void ShowWindow()
    {
        GetWindow<DraggableLabelWindow>("Draggable Label");
    }

    private void OnGUI()
    {
        Event e = Event.current;

        for (int i = 0; i < 5; i++)
        {
            Rect rowRect = new Rect(10, 20 + i * 30, position.width - 20, 25);

            // 处理鼠标点击，选择行
            if (e.type == EventType.MouseDown && rowRect.Contains(e.mousePosition))
            {
                selectedRow = i;
                isDragging = true;
            }

            // 处理鼠标拖动
            if (e.type == EventType.MouseDrag && isDragging && selectedRow == i)
            {
                labelOffset.x += e.delta.x; // 仅在选中的行上拖动 Label
                e.Use(); // 标记事件已使用，避免影响其他 UI
            }

            // 处理鼠标释放
            if (e.type == EventType.MouseUp)
            {
                isDragging = false;
            }

            // 绘制背景
            EditorGUI.DrawRect(rowRect, selectedRow == i ? new Color(0.2f, 0.6f, 1f, 0.3f) : new Color(0.2f, 0.2f, 0.2f, 0.1f));

            // 绘制可拖动的 Label
            GUI.Label(new Rect(20 + labelOffset.x, rowRect.y, 100, 25), $"Row {i}");
        }
    }
}