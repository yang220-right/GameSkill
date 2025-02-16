using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class JsonTools{
    //写数据
    public static void SaveJson<T>(T data, string path){
        if (data == null) return;
        string js = JsonUtility.ToJson(data, true);
        //获取到项目路径
        string fileUrl = Application.streamingAssetsPath + $"\\{path}";
        //打开或者新建文档
        using (StreamWriter sw = new StreamWriter(fileUrl)){
            //保存数据
            sw.WriteLine(js);
            //关闭文档
            sw.Close();
            sw.Dispose();
            //刷新面板 如果新创建文件则不显示 所以刷新一下
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }
    }

    //读取文件
    public static string ReadJson_String(string path){
        //string类型的数据常量
        string readData;
        //获取到路径
        string fileUrl = Application.streamingAssetsPath + $"\\{path}";
        if (!File.Exists(fileUrl)){
            return "";
        }

        //读取文件
        using (StreamReader sr = File.OpenText(fileUrl)){
            //数据保存
            readData = sr.ReadToEnd();
            sr.Close();
        }

        //返回数据
        return readData;
    }

    public static T ReadJson<T>(string path){
        return JsonUtility.FromJson<T>(ReadJson_String(path));
    }
}