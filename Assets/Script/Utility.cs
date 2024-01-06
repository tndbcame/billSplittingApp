using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Kyub.EmojiSearch.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Utility : MonoBehaviour
{

    /*
     * ユーザーごとの支出を計算する
     * return : 
     */
    public void CalcUserPeyment()
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
                    transform.GetChild(0).GetComponent<Transform>().
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
                    transform.GetChild(3).GetComponent<TextMeshProUGUI>();

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
                    transform.GetChild(0).GetComponent<Transform>().
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
                    if(userName.text.Equals(calcItem.targetUserProperty[0]) || calcItem.targetUserProperty[0] == "")
                    {
                        UserTotalMoney -= calcItem.incomeProperty / (calcItem.targetUserCountProperty - 1);
                    }
                }
            }
            //ユーザー一人あたりの収支を計算してtextに格納する
            userArea.transform.GetChild(1).GetComponent<Transform>().
                transform.GetChild(0).GetComponent<Transform>().
                transform.GetChild(3).GetComponent<TextMeshProUGUI>().text
                = "¥" + UserTotalMoney.ToString("N0");
        }
    }
}
