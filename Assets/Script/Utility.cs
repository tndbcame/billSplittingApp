using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Kyub.EmojiSearch.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Utility : MonoBehaviour
{
    /**
    <summary>
        ユーザーごとの支出を計算する
        return : なし
    </summary>
    */
    public static void CalcUserPeyment()             
    {
        //すべてのUserAreaを取得
        GameObject[] allUserArea = GameObject.FindGameObjectsWithTag("UserArea");

        //ユーザーごとの支出/収入をdictで管理
        var UserTotalMoneyDict = new Dictionary<GameObject, int>();

        //ユーザーの支出リスト
        var calcPeymentList = new List<CalcItem>();

        //ユーザーごとに支出と収入を計算する
        foreach (GameObject userArea in allUserArea)
        {
            //ユーザー名を取得
            TMP_EmojiTextUGUI userName =
                    userArea.transform.GetChild(1).GetComponent<Transform>().
                    GetChild(0).GetComponent<Transform>().
                    GetChild(1).GetComponent<TMP_EmojiTextUGUI>();

            //最終的な自分の支払い
            int ownPeyment = 0;

            //最終的な自分の収入
            int ownIncome = 0;

            int childCount = userArea.transform.childCount;

            //ユーザーが作成した支出をリストにまとめる
            for (int i = 0; i < childCount; i++)
            {
                if (i == 0 || i == 1 || i == 2)
                    continue;

                //詳細範囲を取得
                Transform shousaiAreaTransform =
                    userArea.transform.GetChild(i).GetComponent<Transform>();

                //詳細範囲から支出のtextMeshProを取得
                TextMeshProUGUI peymentTxt =
                    shousaiAreaTransform.transform.GetChild(1).GetComponent<Transform>().
                    GetChild(3).GetComponent<TextMeshProUGUI>();

                //詳細範囲から支出する対象を取得
                TextMeshProUGUI peymentTargetTxt =
                    shousaiAreaTransform.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

                //文字列から数値へ変換
               　int peyment = int.Parse(Regex.Replace(peymentTxt.text, @"[^0-9]", ""));

                //支出対象を取得
                string[] peymentTarget = new string[1] { peymentTargetTxt.text };

                //計算情報を格納する
                //TODO一旦全員か対象の一人だけ指定できるようするが複数人にも対応できるように調整する必要あり
                CalcItem calcItem = new CalcItem();

                //ユーザー名を格納
                calcItem.userNameProperty = userName.text;

                //支払ってもらう対象ユーザーを格納
                calcItem.targetUserProperty = peymentTarget;

                //ユーザー数を判定
                if (peymentTarget[0] == "")
                {
                    calcItem.targetUserCountProperty = allUserArea.Length;
                }
                else
                {
                    calcItem.targetUserCountProperty = 2;
                }

                //ユーザー数で割って支出を算出して格納
                ownPeyment = peyment / calcItem.targetUserCountProperty;
                ownPeyment += peyment - ownPeyment * calcItem.targetUserCountProperty;
                calcItem.peymentProperty = ownPeyment;

                //支払ってもらう金額を格納
                ownIncome = peyment - ownPeyment;
                calcItem.incomeProperty = ownIncome;

                calcPeymentList.Add(calcItem);
            }
        }
        //ユーザーごとにlistにまとめた情報を整理して最終的な計算結果を算出する
        foreach (GameObject userArea in allUserArea)
        {
            int UserTotalMoney = 0;

            //ユーザー名を取得
            TextMeshProUGUI userName =
                    userArea.transform.GetChild(1).GetComponent<Transform>().
                    GetChild(0).GetComponent<Transform>().
                    GetChild(1).GetComponent<TextMeshProUGUI>();

            //リストから計算した情報を取得して最終的なユーザーが支払う金額を算出する
            foreach (var calcItem in calcPeymentList)
            {
                //ユーザー名が同じ場合の計算
                if (userName.text.Equals(calcItem.userNameProperty))
                {
                    UserTotalMoney += calcItem.incomeProperty;
                }

                //ユーザー名が同じでない場合の計算
                if (!userName.text.Equals(calcItem.userNameProperty))
                {
                    //支払う対象がユーザー名と同じまたは""の場合
                    if (userName.text.Equals(calcItem.targetUserProperty[0]) || calcItem.targetUserProperty[0] == "")
                    {
                        UserTotalMoney -= calcItem.incomeProperty / (calcItem.targetUserCountProperty - 1);
                    }
                }
            }
            //ユーザー一人あたりの収支を計算してtextに反映する
            userArea.transform.GetChild(0).GetComponent<Transform>().
                GetChild(0).GetComponent<Transform>().
                GetChild(3).GetComponent<TextMeshProUGUI>().text
                = "¥" + UserTotalMoney.ToString("N0");
        }
    }
    /**
    <summary>
        編集モードに切り替え
        return : なし
    </summary>
    */
    public static void OnChangeEditModeListener(GameObject obj,ScrollRect scrollRect, bool orderable)
    {
        Transform parentArea;
        GameObject ContentbottomLeft = GameObject.FindGameObjectWithTag("ContentbottomLeft");

        //長押ししたボタンに応じて編集モード用にオブジェクトを編集する
        if (obj.tag == "userNameArea")
        {
            parentArea = GameObject.FindGameObjectWithTag("UserNameAreaEdt").transform;
            GameObject[] allUserNameArea = GameObject.FindGameObjectsWithTag("userNameArea");
            scrollRect.content = parentArea.GetComponent<RectTransform>();

            //対象を移動してコンポーネントをアタッチしオブジェクトを移動させる
            for ( int i = 0; i < allUserNameArea.Length; i++)
            {
                GameObject userNameArea = allUserNameArea[i];
                userNameArea.AddComponent<ListElement>();
                userNameArea.transform.SetParent(parentArea);
                userNameArea.transform.SetSiblingIndex(userNameArea.GetComponent<ElementIndex>().Index);
            }
            //マーカーを再設定
            ContentbottomLeft.transform.SetParent(parentArea);
            ContentbottomLeft.transform.localPosition = new Vector3(0, 0, 0);
            ContentbottomLeft.SetActive(false);

            //位置情報を取得して並び替え
            for (int i = 0; i < allUserNameArea.Length; i++)
            {
                Transform child = parentArea.transform.GetChild(i).GetComponent<Transform>();
                child.SetSiblingIndex(child.gameObject.GetComponent<ElementIndex>().Index);
            }
        }
        else if (obj.tag == "shousaiArea")
        {
            //親オブジェクトを変更して編集する
            Transform grandParentArea = GameObject.FindGameObjectWithTag("Viewport").transform;
            parentArea = obj.transform.parent;
            parentArea.transform.SetParent(grandParentArea.transform);
            parentArea.transform.tag = "UserAreaActive";
            parentArea.localPosition = new Vector3(0, 0, 0);
            int childCount = parentArea.transform.childCount;
            scrollRect.content = parentArea.GetComponent<RectTransform>();

            //対象を移動してコンポーネントをアタッチする
            for (int i = 0; i < childCount; i++)
            {
                Transform child =
                    parentArea.transform.GetChild(i).GetComponent<Transform>();
                if (child.tag == "userNameArea")
                {
                    ElementIndex UserNameAreaIndex =
                        child.GetComponent<ElementIndex>();
                    Destroy(UserNameAreaIndex);
                }
                child.gameObject.AddComponent<ListElement>();
                child.gameObject.AddComponent<ElementIndex>();
                child.gameObject.GetComponent<ElementIndex>().Index = i;

                if (i == (childCount - 1))
                {
                    //マーカーを再設定
                    ContentbottomLeft.transform.SetParent(child, false);
                    ContentbottomLeft.transform.localPosition = new Vector3(-340, -40, 0);
                    ContentbottomLeft.transform.SetParent(parentArea, false);
                    ContentbottomLeft.transform.SetAsFirstSibling();
                    ContentbottomLeft.SetActive(false);

                }
            }
        }
        //Content配下を非アクティブへ変更
        GameObject Content = GameObject.FindGameObjectWithTag("Content");
        Content.SetActive(false);
        Debug.Log(ContentbottomLeft.transform.localPosition);
    }
}
