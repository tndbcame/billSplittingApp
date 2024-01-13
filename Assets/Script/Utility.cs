using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        //ユーザーの支出リスト
        var calcPeymentList = new List<CalcItem>();

        //ユーザーが払ったすべてのお金の合計値
        int totalPayment = 0;
            
        //ユーザーごとに支出と収入を計算する
        foreach (GameObject userArea in allUserArea)
        {
            //ユーザー名を取得
            TMP_EmojiTextUGUI userName =
                    userArea.transform.GetChild(0).GetComponent<Transform>().
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
                if (i == 0 || i == 1)
                    continue;

                //詳細範囲を取得
                Transform shousaiAreaTransform =
                    userArea.transform.GetChild(i).GetComponent<Transform>();

                //チェックボタンが押されていた場合は計算に含めない
                if (shousaiAreaTransform.transform.GetChild(1).GetComponent<Transform>().
                    GetChild(0).GetComponent<Transform>().
                    GetChild(0).GetComponent<Image>().enabled)
                    continue;

                //詳細範囲から支出のtextMeshProを取得
                TextMeshProUGUI peymentTxt =
                    shousaiAreaTransform.transform.GetChild(1).GetComponent<Transform>().
                    GetChild(3).GetComponent<TextMeshProUGUI>();

                //詳細範囲から支出する対象を取得
                TextMeshProUGUI peymentTargetTxt =
                    shousaiAreaTransform.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

                //文字列から数値へ変換
                int peyment = int.Parse(Regex.Replace(peymentTxt.text, @"[^0-9]", ""));

                //総支出額の合計値
                totalPayment += peyment;

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

                //ユーザー数で割って支出を算出し格納
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
            Transform userName =
                userArea.transform.GetChild(0).GetComponent<Transform>().
                GetChild(0).GetComponent<Transform>();


            TextMeshProUGUI userNameText =
                    userName.GetChild(1).GetComponent<TextMeshProUGUI>();

            //リストから計算した情報を取得して最終的なユーザーが支払う金額を算出する
            foreach (var calcItem in calcPeymentList)
            {
                //ユーザー名が同じ場合の計算
                if (userNameText.text.Equals(calcItem.userNameProperty))
                {
                    UserTotalMoney += calcItem.incomeProperty;
                }

                //ユーザー名が同じでない場合の計算
                if (!userNameText.text.Equals(calcItem.userNameProperty))
                {
                    //支払う対象がユーザー名と同じまたは""の場合
                    if (userNameText.text.Equals(calcItem.targetUserProperty[0]) || calcItem.targetUserProperty[0] == "")
                    {
                        UserTotalMoney -= calcItem.incomeProperty / (calcItem.targetUserCountProperty - 1);
                    }
                }
            }
            //ユーザー一人あたりの収支を計算してtextに反映する
            if (0 < UserTotalMoney)
            {
                userName.GetChild(2).GetComponent<TextMeshProUGUI>().color
                    = new Color(0.0744927f, 0.509434f, 0.176375f, 1.0f);

                userName.GetChild(2).GetComponent<TextMeshProUGUI>().text
                    = "収入";

                userName.GetChild(3).GetComponent<TextMeshProUGUI>().color
                    = new Color(0.0744927f, 0.509434f, 0.176375f, 1.0f);

                userName.GetChild(3).GetComponent<TextMeshProUGUI>().text
                    = "¥" + UserTotalMoney.ToString("N0");
            }
            else
            {
                userName.GetChild(2).GetComponent<TextMeshProUGUI>().color
                    = new Color(0.7019608f, 0.1294118f, 0.1294118f, 1.0f);

                userName.GetChild(2).GetComponent<TextMeshProUGUI>().text
                    = "支出";

                userName.GetChild(3).GetComponent<TextMeshProUGUI>().color
                    = new Color(0.7019608f, 0.1294118f, 0.1294118f, 1.0f);

                userName.GetChild(3).GetComponent<TextMeshProUGUI>().text
                    = "¥" + (-(UserTotalMoney)).ToString("N0");
            }
        }
        //合計値へ格納する
        GameObject.FindGameObjectWithTag("Goukei").GetComponent<TextMeshProUGUI>().text
            = totalPayment.ToString("N0");
    }
    /**
    <summary>
        編集モードに切り替え
        return : なし
    </summary>
    */
    public static void OnChangeEditModeListener(GameObject obj, ScrollRect scrollRect, bool orderable)
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
            for (int i = 0; i < allUserNameArea.Length; i++)
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
    }
    /**
    <summary>
        アプリ開始時に呼び出されデータをロードする
        return : なし
    </summary>
    */
    public static void loadDataAtFirstTime()
    {
        for (int n = 1; n == 1 || SaveManager.saveDatas.ContainsKey(n); n++)
        {
            ContensData.contents = SaveManager.load(n);
            //クローン用コンテンツ取得
            GameObject contentArea = getContentArea();

            ElementIndex ContentAreaIndex = contentArea.GetComponent<ElementIndex>();

            if (SaveManager.saveDatas.ContainsKey(n))
            {
                ContentAreaIndex.Index = ContensData.contents.fileNo;
            }
            //データが存在しないときにこっちの分岐に入る
            else
            {
                ContentAreaIndex.Index = 1;
            }
            //順番を整える
            contentArea.transform.SetSiblingIndex(ContentAreaIndex.Index - 1);

            //セーブデータ1が初期表示
            if (1 == ContentAreaIndex.Index)
            {
                generateContent(contentArea, ContensData.contents);

                //コンテンツステータスを更新
                Controller.ContentsStatus = ContentAreaIndex.Index;
            }

            //セーブデータが存在しない場合はセーブする
            if (!SaveManager.saveDatas.ContainsKey(n))
            {
                SaveManager.save(ContentAreaIndex.Index, ContensData.contents);
            }
        }
    }
    public static void chengeContentArea(GameObject contentArea)
    {
        //渡されたコンテンツのIndexもとにセーブデータを検索する
        ElementIndex ContentAreaIndex = contentArea.GetComponent<ElementIndex>();
        ContensData.contents = SaveManager.load(ContentAreaIndex.Index);
    }
    /**
    <summary>
        コンテンツのセーブデータからオブジェクトを生成する(引数：1 コンテンツエリア、2 生成するコンテンツ)
        return : なし
    </summary>
    */
    public static void generateContent(GameObject contentArea, Contents contents)
    {
        //コンテンツ名を加工
        processContentName(contentArea, contents);

        for (int i = 0; i < contents.user.Count; i++)
        {
            //ユーザー取得
            GameObject userArea;

            //セーブデータに詳細エリアがない場合で分岐
            if (ContensData.contents.user[i].shousai.Count == 0)
            {
                userArea = getUserArea(true);
            }
            else
            {
                userArea = getUserArea(false);
            }

            //Idを設定
            userArea.GetComponent<Id>().id = i;
            //ユーザー名を取得
            userArea.transform.GetChild(0).GetComponent<Transform>().
            GetChild(0).GetComponent<Transform>().
            GetChild(1).GetComponent<TMP_EmojiTextUGUI>().text =
                contents.user[i].userName;
            //ユーザー名にIdを設定
            userArea.transform.GetChild(0).GetComponent<Transform>().
                gameObject.GetComponent<Id>().id = i;

            //詳細エリアの処理
            for (int j = 0; j < contents.user[i].shousai.Count; j++)
            {
                //詳細エリア取得
                GameObject shousai;

                if (j == 0)
                {
                    shousai = userArea.transform.GetChild(2).GetComponent<Transform>().gameObject;
                }
                else
                {
                    shousai = getShousaiArea(userArea.transform);
                }

                //項目名を格納
                shousai.transform.GetChild(1).GetComponent<Transform>().
                    GetChild(1).GetComponent<TMP_EmojiTextUGUI>().text =
                    contents.user[i].shousai[j].ItemName;

                //日付を格納
                shousai.transform.GetChild(1).GetComponent<Transform>().
                    GetChild(2).GetComponent<TMP_EmojiTextUGUI>().text =
                    contents.user[i].shousai[j].date.Substring(5);

                //金額を格納
                shousai.transform.GetChild(1).GetComponent<Transform>().
                    GetChild(3).GetComponent<TMP_EmojiTextUGUI>().text =
                    "¥" + contents.user[i].shousai[j].money.ToString("N0");

                //Idを付与
                shousai.GetComponent<Id>().id = contents.user[i].shousai[j].index;
            }
        }
    }
    /**
    <summary>
        コンテンツ名を加工する(引数：1個目が加工するコンテンツオブジェクト、2個目が加工に使われるデータ)
        return : なし
    </summary>
    */
    public static void processContentName(GameObject contentArea, Contents contents)
    {
        string contentsName = string.Format("<u color=#CB8652>{0}</u>\n<size=28><color=#CFCBC5><i>-select-</i>",
            contents.contentsName);
        Vector2 ca = contentArea.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta;
        ca.y = 150;
        contentArea.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = ca;
        contentArea.transform.GetChild(0).GetComponent<TMP_EmojiTextUGUI>().text = contentsName;
        contentArea.transform.GetChild(0).GetComponent<TMP_EmojiTextUGUI>().alignment = TextAlignmentOptions.Center;
        contentArea.transform.GetChild(0).GetComponent<TMP_EmojiTextUGUI>().fontSize = 40;
        contentArea.transform.GetChild(0).GetComponent<TMP_EmojiTextUGUI>().color
            = new Color(0.8235294f, 0.6431373f, 0.3882353f, 1.0f);
    }
    /**
    <summary>
        コンテンツを取得する
        return : GameObject コンテンツエリア
    </summary>
    */
    public static GameObject getContentArea()
    {
        GameObject _contentsAreaClone = GameObject.FindGameObjectWithTag("ContentAreaClone");

        GameObject contentAreaParent = GameObject.FindGameObjectWithTag("ContentAreaParent");
        GameObject contentArea =
            Instantiate(_contentsAreaClone, Vector3.zero, Quaternion.identity, contentAreaParent.transform);
        contentArea.tag = "ContentArea";
        return contentArea;
    }
    /**
    <summary>
        ユーザーエリアを取得する
        return : GameObject ユーザーエリア
    </summary>
    */
    public static GameObject getUserArea(bool NoShousaiFlg)
    {
        GameObject UserAreaClone;
        if (NoShousaiFlg)
        {
            UserAreaClone = GameObject.FindGameObjectWithTag("UserAreaCloneNoShousaiArea");
        }
        else
        {
            UserAreaClone = GameObject.FindGameObjectWithTag("UserAreaClone");
        }
        //子供を取得してアクティブにする
        for (int i = 0; i < UserAreaClone.transform.childCount; i++)
        {
            UserAreaClone.transform.GetChild(i).GetComponent<Transform>().gameObject.SetActive(true);
        }
        GameObject Content = GameObject.FindGameObjectWithTag("Content");
        GameObject userArea =
            Instantiate(UserAreaClone, Vector3.zero, Quaternion.identity, Content.transform);
        userArea.tag = "UserArea";
        //クローン元の子供を取得して非アクティブにする
        for (int i = 0; i < UserAreaClone.transform.childCount; i++)
        {
            UserAreaClone.transform.GetChild(i).GetComponent<Transform>().gameObject.SetActive(false);
        }
        return userArea;
    }
    /**
    <summary>
        詳細エリアを取得する(引数で親にするObjectのTransformを渡す)
        return : GameObject 詳細オブジェクト
    </summary>
    */
    public static GameObject getShousaiArea(Transform parentUserArea)
    {
        GameObject UserAreaClone = GameObject.FindGameObjectWithTag("UserAreaClone");
        //子供を取得してアクティブにする
        for (int i = 0; i < UserAreaClone.transform.childCount; i++)
        {
            UserAreaClone.transform.GetChild(i).GetComponent<Transform>().gameObject.SetActive(true);
        }
        GameObject _shousai = UserAreaClone.transform.GetChild(2).GetComponent<Transform>().gameObject;
        GameObject shousai = Instantiate(_shousai, Vector3.zero, Quaternion.identity, parentUserArea);
        //クローン々の子供を取得して非アクティブにする
        for (int i = 0; i < UserAreaClone.transform.childCount; i++)
        {
            UserAreaClone.transform.GetChild(i).GetComponent<Transform>().gameObject.SetActive(false);
        }

        return shousai;
    }
    /**
    <summary>
        新しい項目を追加をボタンが押されたときの処理(引数：inputArea, contents, userArea)
        return : なし
    </summary>
    */
    public static void OnNewItemHozon(Transform inputArea, Contents contents, Transform UserArea)
    {
        //項目
        Transform item =
            inputArea.GetChild(0).GetComponent<Transform>().
            GetChild(0).GetComponent<Transform>();
        string ItemTxt = item.GetChild(2).GetComponent<TMP_EmojiTextUGUI>().text;

        //金額
        Transform money =
            inputArea.GetChild(1).GetComponent<Transform>().
            GetChild(0).GetComponent<Transform>();
        string moneyTxt = money.GetChild(2).GetComponent<TMP_EmojiTextUGUI>().text;

        //日付
        Transform Date =
            inputArea.GetChild(2).GetComponent<Transform>().
            GetChild(0).GetComponent<Transform>();
        string dateTxt = Date.GetChild(2).GetComponent<TMP_EmojiTextUGUI>().text;

        //子オブジェクトから同じ識別子のオブジェクトを取得
        for (int i = 0; i < contents.user.Count; i++)
        {
            if (contents.user[i].index == UserArea.GetComponent<Id>().id)
            {

                GameObject shousaiArea = getShousaiArea(UserArea);
                Shousai shousai = new Shousai();
                //TODOここuserareaに合わせてリファクタする
                //項目
                if ("​".Equals(ItemTxt))
                {
                    shousai.ItemName = item.GetChild(1).GetComponent<TMP_EmojiTextUGUI>().text;
                    shousaiArea.transform.GetChild(1).GetComponent<Transform>().
                        GetChild(1).GetComponent<TMP_EmojiTextUGUI>().text
                        = shousai.ItemName;
                }
                else
                {
                    shousai.ItemName = item.GetChild(2).GetComponent<TMP_EmojiTextUGUI>().text;
                    shousaiArea.transform.GetChild(1).GetComponent<Transform>().
                        GetChild(1).GetComponent<TMP_EmojiTextUGUI>().text
                        = shousai.ItemName;
                }
                //金額
                if ("​".Equals(moneyTxt))
                {
                    shousai.money = int.Parse(Regex.Replace(money.GetChild(1).GetComponent<TMP_EmojiTextUGUI>().text, @"[^0-9]", ""));
                    shousaiArea.transform.GetChild(1).GetComponent<Transform>().
                            GetChild(3).GetComponent<TMP_EmojiTextUGUI>().text
                            = money.GetChild(1).GetComponent<TMP_EmojiTextUGUI>().text;
                }
                else
                {
                    Debug.Log(moneyTxt);
                    shousai.money = int.Parse(Regex.Replace(money.GetChild(2).GetComponent<TMP_EmojiTextUGUI>().text, @"[^0-9]", ""));
                    shousaiArea.transform.GetChild(1).GetComponent<Transform>().
                            GetChild(3).GetComponent<TMP_EmojiTextUGUI>().text
                            = "¥" + shousai.money.ToString("N0");
                }
                //日付
                if ("​".Equals(dateTxt))
                {
                    shousai.date = Date.GetChild(1).GetComponent<TMP_EmojiTextUGUI>().text;
                    shousaiArea.transform.GetChild(1).GetComponent<Transform>().
                        GetChild(2).GetComponent<TMP_EmojiTextUGUI>().text
                        = shousai.date.Substring(5);
                }
                else
                {
                    shousai.date = Date.GetChild(2).GetComponent<TMP_EmojiTextUGUI>().text;
                    shousaiArea.transform.GetChild(1).GetComponent<Transform>().
                        GetChild(2).GetComponent<TMP_EmojiTextUGUI>().text
                        = shousai.date.Substring(5);
                }

                //Idを付与
                shousai.index = generateShousaiIndex(UserArea, contents.user[i].shousai.Count);
                shousaiArea.GetComponent<Id>().id = shousai.index;

                //キャンバス更新
                UserArea.gameObject.SetActive(false);
                Canvas.ForceUpdateCanvases();
                UserArea.gameObject.SetActive(true);
                shousaiArea.transform.SetAsLastSibling();

                //セーブする
                contents.user[i].shousai.Add(shousai);
                SaveManager.save(Controller.ContentsStatus, contents);
                break;
            }
        }
    }
    /**
    <summary>
        ユーザー追加をボタンが押されたときの処理(引数：inputArea, contents, userArea)
        return : なし
    </summary>
    */
    public static void OnNewUserHozon(Transform inputArea, Contents contents, Transform UserArea)
    {
        //インプットエリア取得
        Transform userName =
            inputArea.GetChild(7).GetComponent<Transform>().
            GetChild(0).GetComponent<Transform>();
        //テキスト取得
        string userNameTxt;

        //ユーザー取得
        GameObject userArea = getUserArea(false);

        //Idを取得
        int id = contents.user.Count;

        //ユーザーインスタンス取得
        User user = new User();

        //ユーザーエリア、ユーザーネームエリア、詳細エリアIdに反映させる
        userArea.GetComponent<Id>().id = id;
        userArea.transform.GetChild(0).GetComponent<Id>().id = id;
        userArea.transform.GetChild(2).GetComponent<Id>().id = id;
        user.index = id;

        //ユーザー名をUIに反映
        if ("​".Equals(userName.GetChild(2).GetComponent<TMP_EmojiTextUGUI>().text))
        {
            //プレースホルダー
            userNameTxt = userName.GetChild(1).GetComponent<TMP_EmojiTextUGUI>().text;
        }
        else
        {
            userNameTxt = userName.GetChild(2).GetComponent<TMP_EmojiTextUGUI>().text;
        }
        //重複しないように編集
        userNameTxt = generateDuplicates(userNameTxt, contents);

        userArea.transform.GetChild(0).GetComponent<Transform>().
        GetChild(0).GetComponent<Transform>().
        GetChild(1).GetComponent<TMP_EmojiTextUGUI>().text
        = userNameTxt;

        //セーブデータを反映
        user.userName = userNameTxt;
        user.shousai = new List<Shousai>();
        Shousai shousai = new Shousai();
        shousai.ItemName = "項目名";
        shousai.index = 0;
        shousai.money = 1000;
        shousai.date = DateTime.Today.ToString("yyyy/M/d");
        user.shousai.Add(shousai);
        contents.user.Add(user);

        //詳細エリアのUIは初期値を反映
        Transform shousaiTxt = userArea.transform.GetChild(2).GetComponent<Transform>().
            GetChild(1).GetComponent<Transform>();

        shousaiTxt.GetChild(1).GetComponent<TMP_EmojiTextUGUI>().text = shousai.ItemName;
        shousaiTxt.GetChild(2).GetComponent<TMP_EmojiTextUGUI>().text = shousai.date.Substring(5);
        shousaiTxt.GetChild(3).GetComponent<TMP_EmojiTextUGUI>().text = "¥" + shousai.money.ToString("N0");

        //データをセーブする
        SaveManager.save(Controller.ContentsStatus, contents);
    }
    /**
    <summary>
        ユーザー名が重複しないように加工(引数：ユーザー名の文字列, contents)
        return : なし
    </summary>
    */
    public static string generateDuplicates(string userNameTxt, Contents contents)
    {
        int j = 0;
        string _userNameTxt = userNameTxt;
        for (int i = 0; i < contents.user.Count; i++)
        {
            if (userNameTxt == contents.user[i].userName)
            {
                j++;
                userNameTxt = _userNameTxt + j;
                i = 0;
            }
        }
        return userNameTxt;
    }
    /**
    <summary>
        詳細エリアの重複しない最大値 + 1のindexを探して返却
        return : int index
    </summary>
    */
    public static int generateShousaiIndex(Transform UserArea, int _index)
    {
        int index = 0;
        Transform tfm;
        List<int> nums = new List<int>();
        Debug.Log(UserArea.childCount);
        for (int i = 0; i < UserArea.childCount; i++)
        {
            tfm = UserArea.GetChild(i).GetComponent<Transform>();
            if (tfm.tag == "shousaiArea")
            {
                nums.Add(tfm.GetComponent<Id>().id);
            }
        }
        index = nums.Max() + 1;
        return index;
    }
    /**
    <summary>
        詳細エリアの削除をする
        return : なし
    </summary>
    */
    public static void deleteShousaiArea(Contents contents)
    {
        GameObject[] allshousaiArea = GameObject.FindGameObjectsWithTag("shousaiArea");
        foreach (GameObject shousaiArea in allshousaiArea)
        {
            //チェックボタンの状態を取得して判定
            bool shousaiCheckBtnStatus =
                 shousaiArea.transform.GetChild(1).GetComponent<Transform>().
                 GetChild(0).GetComponent<Transform>().
                 GetChild(0).GetComponent<Image>().enabled;
            if (shousaiCheckBtnStatus)
            {
                //セーブデータ内のユーザーエリアと詳細エリアのIdを検索して削除
                int UserAreaId = shousaiArea.transform.parent.GetComponent<Id>().id;
                int shousaiAreaId = shousaiArea.transform.GetComponent<Id>().id;
                for(int i = 0; i < contents.user.Count; i++)
                {
                    if(contents.user[i].index == UserAreaId)
                    {
                        for (int j = 0; j < contents.user[UserAreaId].shousai.Count; j++)
                        {
                            if(contents.user[i].shousai[j].index == shousaiAreaId)
                            {
                                contents.user[i].shousai.Remove(contents.user[i].shousai[j]);
                            }
                        }
                    }
                }
                //UIを削除
                Destroy(shousaiArea);
                shousaiArea.SetActive(false);
                Canvas.ForceUpdateCanvases();
                shousaiArea.SetActive(true);
                //セーブする
                SaveManager.save(Controller.ContentsStatus, contents);
            }
        }
    }
}