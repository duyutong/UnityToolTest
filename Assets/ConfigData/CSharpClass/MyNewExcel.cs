
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using D.Unity3dTools;
/// <summary>
/// #ClassDes#
/// <summary>
public class MyNewExcel:BaseConfig
{
	
    /// <summary>
    /// 编号
    /// </summary>
    public int ID { get; protected set; }
    /// <summary>
    /// 一维数组
    /// </summary>
    public List<int> Array { get; protected set; }
    /// <summary>
    /// 二维数组
    /// </summary>
    public List<List<int>> Arrays { get; protected set; }
    /// <summary>
    /// 键值对
    /// </summary>
    public Dictionary<int,int> Pair { get; protected set; }

    public MyNewExcel() { }
    public MyNewExcel(Dictionary<string, object> _dataDic)
    {
        ID = _dataDic["ID"].ToInt();
        Array = _dataDic["Array"].ToIntArray();
        Arrays = _dataDic["Arrays"].ToIntArrays();
        Pair = _dataDic["Pair"].ToDictionary();
        id = ID;
    }
} 