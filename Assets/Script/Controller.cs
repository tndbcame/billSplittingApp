using System.Collections;
using System.Collections.Generic;
using Kyub.EmojiSearch.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private ScrollRect scrollRect;
    private GameObject button_ob;//押したボタンを取得するためのGameObject型の変数
    private float longTapTime = 0.4f;//長押しの判定時間
    public static bool EditFlg = true;

    void Start()
    {
        Utility.CalcUserPeyment();
    }

    void Update()
    {
        if (LongTap.isDown && EditFlg)
        {
            LongTap.time += Time.deltaTime;
            if (LongTap.time >= longTapTime)
            {

                Debug.Log("Long Tap");
                EditFlg = false;
                LongTap.isDown = false;
                button_ob = eventSystem.currentSelectedGameObject;
                Utility.OnChangeEditModeListener(button_ob, scrollRect, true);
            }
        }
    }
    /**
    <summary>
        順番を編集する、並び替え終了後に呼び出される
        return : なし
    </summary>
    */
    public void OnEndOrderLisner()
    {
        if(button_ob.tag == "shousaiArea")
        {
            Transform UserAreaActive = GameObject.FindGameObjectWithTag("UserAreaActive").transform;
            foreach (Transform child in UserAreaActive)
            {
                if (child.tag == "userNameArea")
                {
                    child.SetSiblingIndex(1);
                }
                else if (child.tag == "addArea")
                {
                    child.SetSiblingIndex(2);
                }
                else if(child.tag == "ContentbottomLeft")
                {
                    child.SetSiblingIndex(0);
                }
            }
        }
        else if(button_ob.tag == "userNameArea")
        {
            Transform UserNameAreaEdt = GameObject.FindGameObjectWithTag("UserNameAreaEdt").transform;
            foreach (Transform child in UserNameAreaEdt)
            {
                if (child.tag == "ContentbottomLeft")
                {
                    child.SetAsLastSibling();
                }
            }
        }
    }
}
