using D.Unity3dTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Transform move;
    public HyperlinkText hyperlinkText;
    // Start is called before the first frame update
    void Start()
    {
        hyperlinkText.hyperlinkInfo = "xxxxx<a href=信息1><color=#1b932c>信息1</color></a>xxxxxxx<a href=信息1><color=#1b932c>信息1</color></a>xxxxxxxx";
        hyperlinkText.OnClick.RemoveAllListeners();
        hyperlinkText.OnClick.AddListener((str, pos) =>
        {
            move.localPosition = pos;
            Debug.Log("超链接信息：" + str + " 屏幕坐标 " + pos);
        });
    }

    // Update is called once per frame
    void Update()
    {

    }
}
