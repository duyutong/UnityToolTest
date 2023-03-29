
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using LitJson;
using System;
using D.Unity3dTools.EditorTool;
public class ConfigLoader
{
    public static string jsonPath  
    {
        get 
        {
            PathLibrary pathLibrary = JsonMapper.ToObject<PathLibrary>(File.ReadAllText(Application.dataPath + "/PathLibrary.json"));
            return pathLibrary.jsonPath;
        }
    }
    
    #region MyNewExcel
    private static Dictionary<int, MyNewExcel> configMyNewExcelTable = new Dictionary<int, MyNewExcel>();
    public static MyNewExcel GetMyNewExcelConfig(int _id)
    {
        if (configMyNewExcelTable.Count == 0) configMyNewExcelTable = LoadMyNewExcelConfig();
        if (!configMyNewExcelTable.ContainsKey(_id)) return null;
        return configMyNewExcelTable[_id];
    }
    private static Dictionary<int, MyNewExcel> LoadMyNewExcelConfig()
    {
        Dictionary<int, MyNewExcel> result = new Dictionary<int, MyNewExcel>();
        JsonData _data = JsonMapper.ToObject(File.ReadAllText(jsonPath + "/MyNewExcel.json"));
        for (int i = 0; i<_data.Count; i++)
        {
            int index = i;
            Dictionary<string, object> pairs = new Dictionary<string, object>();
            foreach (string key in _data[index].Keys) pairs.Add(key, _data[index][key]);
            MyNewExcel confItem = new MyNewExcel(pairs);
            result.Add(confItem.id, confItem);
        }
        return result;
    }
    #endregion

}