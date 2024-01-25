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
        アプリ開始時に呼び出されデータをロードする
        return : なし
    </summary>
    */
    public static void loadContentAtFirstTime()
    {
        SaveManager.getSaveData();

        if (0 == SaveManager.saveDatas.Count)
        {
            //クローン用コンテンツ取得
            GameObject contentArea = getContentArea();
            //Idを取得
            Id ContentAreaIndex = contentArea.GetComponent<Id>();
            //データを取得
            ContensData.contents = SaveManager.load(1);
            //コンテンツのIDを格納
            ContentAreaIndex.id = 1;
            //順番を整える
            contentArea.transform.SetSiblingIndex(ContentAreaIndex.id - 1);
            //コンテンツ名を格納
            contentArea.transform.GetChild(0).GetComponent<TMP_EmojiTextUGUI>().text = ContensData.contents.contentsName;
            //コンテンツを生成
            generateContent(contentArea, ContensData.contents);
            //コンテンツステータスを更新
            Controller.contentsStatus = ContentAreaIndex.id;
            //セーブする
            SaveManager.save(ContentAreaIndex.id, ContensData.contents);
        }
        else
        {
            loadContent();
        }
    }
    /**
    <summary>
        データが存在する場合データをロードする
        return : なし
    </summary>
    */
    public static void loadContent()
    {
        List<int> keyList = getSaveDataKeyList();
        for (int i = 0; i < keyList.Count; i++)
        {
            //クローン用コンテンツ取得
            GameObject contentArea = getContentArea();
            //Idを取得
            Id ContentAreaIndex = contentArea.GetComponent<Id>();
            //データを取得
            ContensData.contents = SaveManager.saveDatas[keyList[i]];
            //コンテンツのIDを格納
            ContentAreaIndex.id = ContensData.contents.fileNo;
            //順番を整える
            contentArea.transform.SetSiblingIndex(ContentAreaIndex.id - 1);
            Transform addContentArea = GameObject.FindGameObjectWithTag("AddContentArea").transform;
            addContentArea.SetAsLastSibling();
            //コンテンツ名を格納
            contentArea.transform.GetChild(0).GetComponent<TMP_EmojiTextUGUI>().text = ContensData.contents.contentsName;
            //一番最初のセーブデータを生成
            if (i == 0)
            {
                generateContent(contentArea, ContensData.contents);

                //コンテンツステータスを更新
                Controller.contentsStatus = ContentAreaIndex.id;
            }
            Canvas.ForceUpdateCanvases();
        }
    }
    /**
    <summary>
        コンテンツデータをロードする
        return : なし
    </summary>
    */
    public static void loadNoSelectContent(int id, GameObject contentArea, int oldContentStatus)
    {
        ContensData.contents = SaveManager.load(id);

        //ユーザーエリアを削除
        GameObject[] allUserArea = GameObject.FindGameObjectsWithTag("UserArea");
        foreach (GameObject userArea in allUserArea)
        {
            //ここをfalseにしないとFindGameObjectsWithTagでヒットしてしまうため
            userArea.SetActive(false);
            Destroy(userArea);
        }
        //他のコンテンツエリアの表示を変更
        GameObject[] contentsArea = GameObject.FindGameObjectsWithTag("ContentArea");
        foreach (GameObject contentArea_ in contentsArea)
        {
            if (contentArea_.transform.GetComponent<Id>().id == oldContentStatus)
            {
                //フォーマットを元に戻す
                processContentName2(contentArea_);
                break;
            }
        }
        contentArea.SetActive(false);
        //UIを生成
        generateContent(contentArea, ContensData.contents);
        //コンテンツステータスを更新
        Controller.contentsStatus = id;
        Canvas.ForceUpdateCanvases();
    }
    /**
    <summary>
        ユーザーごとの支出を計算する
        return : なし
    </summary>
    */
    public static void calcUserPeyment()
    {
        //すべてのUserAreaを取得
        GameObject[] allUserArea = GameObject.FindGameObjectsWithTag("UserArea");

        //ユーザーの支出リスト
        var calcPeymentList = new List<CalcItem>();

        //ユーザーが払ったすべてのお金の合計値
        int totalPayment = 0;

        //消されるユーザーがチェックする
        int userAreadeleteCount = 0;
        foreach (GameObject userArea in allUserArea)
        {
            if (userArea.transform.GetChild(0).GetComponent<Transform>().
              GetChild(0).GetComponent<Transform>().
              GetChild(0).GetComponent<Transform>().
              GetChild(0).GetComponent<Image>().enabled)
            {
                userAreadeleteCount++;
            }
        }

        //ユーザーごとに支出と収入を計算する
        foreach (GameObject userArea in allUserArea)
        {
            //ユーザー名(テキスト)を取得
            TMP_EmojiTextUGUI userNameTxt =
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
                calcItem.userNameProperty = userNameTxt.text;

                //支払ってもらう対象ユーザーを格納
                calcItem.targetUserProperty = peymentTarget;

                //ユーザー数を判定
                if (peymentTarget[0] == "")
                {
                    calcItem.targetUserCountProperty = allUserArea.Length - userAreadeleteCount;
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
            //消されるユーザーの場合はスキップ
            if (userArea.transform.GetChild(0).GetComponent<Transform>().
              GetChild(0).GetComponent<Transform>().
              GetChild(0).GetComponent<Transform>().
              GetChild(0).GetComponent<Image>().enabled)
                continue;

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
    public static void onChangeEditMode(GameObject obj, ScrollRect scrollRect, string tag)
    {
        Transform parentArea;
        GameObject ContentbottomLeft = GameObject.FindGameObjectWithTag("ContentbottomLeft");

        //長押ししたボタンに応じて編集モード用にオブジェクトを編集する
        if (tag == "userNameArea")
        {
            Controller.editStatus = 1;
            parentArea = GameObject.FindGameObjectWithTag("UserNameAreaEdt").transform;
            GameObject[] allUserNameArea = GameObject.FindGameObjectsWithTag("userNameArea");
            scrollRect.content = parentArea.GetComponent<RectTransform>();

            //対象を移動してコンポーネントをアタッチしオブジェクトを移動させる
            foreach (GameObject userNameArea in allUserNameArea)
            {
                GameObject userNameAreaClone = getUserNameArea(parentArea);
                userNameAreaClone.transform.GetComponent<Id>().id =
                userNameArea.transform.GetComponent<Id>().id;
                userNameAreaClone.transform.GetChild(0).GetComponent<Transform>().
                    GetChild(1).GetComponent<TMP_EmojiTextUGUI>().text =
                userNameArea.transform.GetChild(0).GetComponent<Transform>().
                    GetChild(1).GetComponent<TMP_EmojiTextUGUI>().text;
                userNameAreaClone.transform.GetChild(0).GetComponent<Transform>().
                    GetChild(2).GetComponent<TMP_EmojiTextUGUI>().text =
                userNameArea.transform.GetChild(0).GetComponent<Transform>().
                    GetChild(2).GetComponent<TMP_EmojiTextUGUI>().text;
                userNameAreaClone.transform.GetChild(0).GetComponent<Transform>().
                    GetChild(3).GetComponent<TMP_EmojiTextUGUI>().text =
                userNameArea.transform.GetChild(0).GetComponent<Transform>().
                    GetChild(3).GetComponent<TMP_EmojiTextUGUI>().text;
                userNameAreaClone.tag = "userNameAreaClone";
            }
            List<int> idList = new List<int>();
            //位置情報を取得して並び替え
            for (int i = 0; i < parentArea.childCount; i++)
            {
                idList.Add(parentArea.GetChild(i).GetComponent<Id>().id);
            }
            idList.Sort();

            while (idList.Count > 0)
            {
                int id = idList[0];
                idList.RemoveAt(0);
                for (int i = 0; i < parentArea.childCount; i++)
                {
                    if (id == parentArea.GetChild(i).GetComponent<Id>().id)
                    {
                        parentArea.GetChild(i).SetSiblingIndex(id);
                        break;
                    }
                }
            }
            for (int i = 0; i < parentArea.childCount; i++)
            {
                parentArea.GetChild(i).GetComponent<ElementIndex>().Index = i + 1;
            }
            //マーカーを再設定
            ContentbottomLeft.transform.SetParent(parentArea);
            ContentbottomLeft.transform.localPosition = new Vector3(0, 0, 0);
            ContentbottomLeft.transform.SetAsFirstSibling();
            ContentbottomLeft.SetActive(false);
        }
        else if (obj.tag == "shousaiArea")
        {
            Controller.editStatus = 2;
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

                //indexを付与
                child.gameObject.GetComponent<ElementIndex>().Index = i + 1;

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
        //Content配下を削除
        GameObject[] allUserArea = GameObject.FindGameObjectsWithTag("UserArea");
        foreach (GameObject userArea in allUserArea)
            Destroy(userArea);
    }

    /**
    <summary>
        編集モードから通常モードへ戻るときの処理
        return : なし
    </summary>
    */
    public static void onChangeNormalModeListener(List<Transform> objList, Contents contents, ScrollRect scrollRect)
    {
        //ユーザーエリアの場合
        if (Controller.editStatus == 1)
        {
            GameObject[] allUserNameArea = GameObject.FindGameObjectsWithTag("userNameAreaClone");

            //セーブデータのユーザーIndexとIdが一致するセーブデータのインデックスを更新
            for (int i = 0; i < contents.user.Count; i++)
            {
                for (int j = 0; j < objList.Count; j++)
                {
                    if (contents.user[i].index == objList[j].GetComponent<Id>().id)
                    {
                        contents.user[i].index = objList[j].GetComponent<ElementIndex>().Index;
                        break;
                    }
                }
            }
            contents.user.Sort((a, b) => a.index - b.index);

            //ユーザーネームエリアを削除
            foreach (GameObject userNameArea in allUserNameArea)
                Destroy(userNameArea);

            //マーカーをもとに戻す
            GameObject UserNameAreaEdt = GameObject.FindGameObjectWithTag("UserNameAreaEdt");
            Transform ContentbottomLeft = UserNameAreaEdt.transform.GetChild(0).GetComponent<Transform>();
            ContentbottomLeft.SetParent(UserNameAreaEdt.transform.parent);
            ContentbottomLeft.gameObject.SetActive(true);
        }
        //詳細エリアの場合
        else if (Controller.editStatus == 2)
        {
            //セーブデータの詳細IndexとIdが一致するセーブデータのインデックスを更新
            GameObject userAreaActive = GameObject.FindGameObjectWithTag("UserAreaActive");
            for (int i = 0; i < contents.user.Count; i++)
            {
                if (contents.user[i].index == userAreaActive.GetComponent<Id>().id)
                {
                    for (int j = 0; j < contents.user[i].shousai.Count; j++)
                    {
                        for (int l = 0; l < objList.Count; l++)
                        {
                            if (contents.user[i].shousai[j].index == objList[l].GetComponent<Id>().id)
                            {
                                contents.user[i].shousai[j].index = objList[l].GetComponent<ElementIndex>().Index;
                                break;
                            }
                        }

                    }
                    contents.user[i].shousai.Sort((a, b) => a.index - b.index);
                    break;
                }
            }

            //マーカーをもとに戻す
            Transform ContentbottomLeft = userAreaActive.transform.GetChild(0).GetComponent<Transform>();
            ContentbottomLeft.SetParent(userAreaActive.transform.parent);
            ContentbottomLeft.gameObject.SetActive(true);

            //UserAreaActiveを削除
            Destroy(userAreaActive);
        }

        //UIコンテンツを取得
        GameObject[] allContentArea = GameObject.FindGameObjectsWithTag("ContentArea");
        GameObject UIContent = new GameObject();
        foreach (GameObject contentArea in allContentArea)
        {
            if (contentArea.transform.GetComponent<Id>().id == Controller.contentsStatus)
                UIContent = contentArea;
        }
        generateContent(UIContent, contents);

        //スクロールレクトをもとに戻す
        Transform parentArea = GameObject.FindGameObjectWithTag("Content").transform;
        scrollRect.content = parentArea.GetComponent<RectTransform>();

        SaveManager.save(Controller.contentsStatus, contents);
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
            if (contents.user[i].shousai.Count == 0)
            {
                userArea = getUserArea(true);
            }
            else
            {
                userArea = getUserArea(false);
            }
            //Idを設定
            userArea.GetComponent<Id>().id = contents.user[i].index;

            //ユーザー名を取得
            userArea.transform.GetChild(0).GetComponent<Transform>().
            GetChild(0).GetComponent<Transform>().
            GetChild(1).GetComponent<TMP_EmojiTextUGUI>().text =
                contents.user[i].userName;
            //ユーザー名にIdを設定
            userArea.transform.GetChild(0).GetComponent<Transform>().
                gameObject.GetComponent<Id>().id = contents.user[i].index;
            userArea.transform.GetChild(0).GetComponent<Transform>().
                gameObject.GetComponent<ElementIndex>().Index = contents.user[i].index;

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
        コンテンツ名を加工する(選択状態にする)(引数：1個目が加工するコンテンツオブジェクト、2個目が加工に使われるデータ)
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
        //リフレッシュ
        contentArea.SetActive(false);
        Canvas.ForceUpdateCanvases();
        contentArea.SetActive(true);

    }
    /**
    <summary>
        コンテンツ名を加工する(選択状態でないやつに戻す)(引数：1個目が加工するコンテンツオブジェクト、2個目が加工に使われるデータ)
        return : なし
    </summary>
    */
    public static void processContentName2(GameObject contentArea)
    {
        Vector2 ca = contentArea.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta;
        ca.y = 80;
        contentArea.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = ca;
        contentArea.transform.GetChild(0).GetComponent<TMP_EmojiTextUGUI>().text =
            removeContentNameFormat(contentArea.transform.GetChild(0).GetComponent<TMP_EmojiTextUGUI>().text);
        contentArea.transform.GetChild(0).GetComponent<TMP_EmojiTextUGUI>().alignment = TextAlignmentOptions.Bottom;
        contentArea.transform.GetChild(0).GetComponent<TMP_EmojiTextUGUI>().fontSize = 30;
        contentArea.transform.GetChild(0).GetComponent<TMP_EmojiTextUGUI>().color
            = new Color(0.5849056f, 0.5849056f, 0.5849056f, 1.0f);
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
        GameObject UIcontent = GameObject.FindGameObjectWithTag("Content");
        GameObject userArea =
            Instantiate(UserAreaClone, Vector3.zero, Quaternion.identity, UIcontent.transform);
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
    public static GameObject getUserNameArea(Transform parentUserArea)
    {
        GameObject UserAreaClone = GameObject.FindGameObjectWithTag("UserAreaClone");
        //子供を取得してアクティブにする
        for (int i = 0; i < UserAreaClone.transform.childCount; i++)
        {
            UserAreaClone.transform.GetChild(i).GetComponent<Transform>().gameObject.SetActive(true);
        }
        GameObject _userNameArea = UserAreaClone.transform.GetChild(0).GetComponent<Transform>().gameObject;
        GameObject userNameArea = Instantiate(_userNameArea, Vector3.zero, Quaternion.identity, parentUserArea);
        //クローン々の子供を取得して非アクティブにする
        for (int i = 0; i < UserAreaClone.transform.childCount; i++)
        {
            UserAreaClone.transform.GetChild(i).GetComponent<Transform>().gameObject.SetActive(false);
        }

        return userNameArea;
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
        ユーザーネームリストを取得する
        return : List<Transform>　ユーザーネームリスト
    </summary>
    */
    public static List<Transform> getNewUserNames()
    {
        GameObject UserNameAreaEdt = GameObject.FindGameObjectWithTag("UserNameAreaEdt");
        List<Transform> userNames = new List<Transform>();
        Debug.Log(userNames);
        for (int i = 1; i < UserNameAreaEdt.transform.childCount; i++)
        {
            userNames.Add(UserNameAreaEdt.transform.GetChild(i).GetComponent<Transform>());
        }
        return userNames;
    }
    /**
    <summary>
        詳細リストを取得する
        return : List<Transform>　ユーザーネームリスト
    </summary>
    */
    public static List<Transform> getNewShousai()
    {
        GameObject UserAreaActive = GameObject.FindGameObjectWithTag("UserAreaActive");
        List<Transform> shousaiList = new List<Transform>();
        Debug.Log(shousaiList);
        for (int i = 3; i < UserAreaActive.transform.childCount; i++)
        {
            shousaiList.Add(UserAreaActive.transform.GetChild(i).GetComponent<Transform>());
        }
        return shousaiList;
    }
    /**
    <summary>
        セーブデータのキーリストを取得する
        return : List<int>　セーブデータのキーリスト
    </summary>
    */
    public static List<int> getSaveDataKeyList()
    {
        List<int> keyList = new List<int>();
        foreach (int key in SaveManager.saveDatas.Keys)
        {
            keyList.Add(key);
        }
        return keyList;
    }
    /**
    <summary>
        新しい項目を追加をボタンが押されたときの処理(引数：inputArea, contents, userArea)
        return : なし
    </summary>
    */
    public static void onNewItemHozon(Transform inputArea, Contents contents, Transform UserArea)
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
                SaveManager.save(Controller.contentsStatus, contents);
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
    public static void onNewUserHozon(Transform inputArea, Contents contents)
    {
        //インプットエリア取得
        Transform userName =
            inputArea.GetChild(7).GetComponent<Transform>().
            GetChild(0).GetComponent<Transform>();

        //テキスト取得
        string userNameTxt;

        //ユーザー取得
        GameObject userArea = getUserArea(false);

        //ユーザーインスタンス取得
        User user = new User();

        //ユーザーエリア、ユーザーネームエリア、詳細エリアIdに反映させるTODOここがうまく行かない件をなんとかする
        int id = generateUserAreaIndex();
        userArea.GetComponent<Id>().id = id;
        userArea.transform.GetChild(0).GetComponent<Id>().id = id;
        userArea.transform.GetChild(2).GetComponent<Id>().id = contents.user.Count;
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
        userNameTxt = generateNotDuplicates(userNameTxt, contents);

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
        SaveManager.save(Controller.contentsStatus, contents);
    }
    /**
    <summary>
        コンテンツ追加ボタンが押されたときの処理(引数：inputArea,)
        return : なし
    </summary>
    */
    public static void onNewContentHozon(Transform inputArea, Transform addBtn)
    {
        Transform contentName =
            inputArea.GetChild(10).GetComponent<Transform>().
            GetChild(0).GetComponent<Transform>();
        //新しいコンテンツを取得
        GameObject contentArea = getContentArea();

        string contentNameTxt = "";

        //ファイルNoの最大値+1を新しいファイルNoとして追加する
        List<int> nums = new List<int>();
        foreach (Contents content in SaveManager.saveDatas.Values)
        {
            nums.Add(content.fileNo);
        }
        ContensData.contents = SaveManager.load(nums.Max() + 1);

        if ("​".Equals(contentName.GetChild(2).GetComponent<TMP_EmojiTextUGUI>().text))
        {
            //プレースホルダー
            contentNameTxt = contentName.GetChild(1).GetComponent<TMP_EmojiTextUGUI>().text;
        }
        else
        {
            contentNameTxt = contentName.GetChild(2).GetComponent<TMP_EmojiTextUGUI>().text;
        }

        //UIに格納
        contentArea.transform.GetChild(0).GetComponent<TMP_EmojiTextUGUI>().text = contentNameTxt;
        contentArea.transform.GetComponent<Id>().id = nums.Max() + 1;

        //セーブデータに格納してセーブする
        ContensData.contents.contentsName = contentNameTxt;
        SaveManager.save(nums.Max() + 1, ContensData.contents);
        //セーブしたデータを使えるように保持する
        SaveManager.getSaveData();

        Canvas.ForceUpdateCanvases();
        addBtn.SetAsLastSibling();
    }
    /**
    <summary>
        ユーザー名が重複しないように加工(引数：ユーザー名の文字列, contents)
        return : なし
    </summary>
    */
    public static string generateNotDuplicates(string userNameTxt, Contents contents)
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
        ユーザーエリアの重複しない最大値 + 1のindexを探して返却
        return : int index
    </summary>
    */
    public static int generateUserAreaIndex()
    {
        int index = 0;
        List<int> nums = new List<int>();
        GameObject[] allUserArea = GameObject.FindGameObjectsWithTag("UserArea");
        foreach (GameObject UserArea in allUserArea)
        {
            nums.Add(UserArea.transform.GetComponent<Id>().id);
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
                for (int i = 0; i < contents.user.Count; i++)
                {
                    if (contents.user[i].index == UserAreaId)
                    {
                        for (int j = 0; j < contents.user[i].shousai.Count; j++)
                        {
                            if (contents.user[i].shousai[j].index == shousaiAreaId)
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
                SaveManager.save(Controller.contentsStatus, contents);
            }
        }
    }
    /**
    <summary>
        ユーザーエリアの削除をする
        return : なし
    </summary>
    */
    public static void deleteUserArea(Contents contents)
    {
        GameObject[] allUserArea = GameObject.FindGameObjectsWithTag("UserArea");
        foreach (GameObject userArea in allUserArea)
        {
            //チェックボタンの状態を取得して判定
            bool userNameAreaCheckBtnStatus =
                 userArea.transform.GetChild(0).GetComponent<Transform>().
                 GetChild(0).GetComponent<Transform>().
                 GetChild(0).GetComponent<Transform>().
                 GetChild(0).GetComponent<Image>().enabled;
            if (userNameAreaCheckBtnStatus)
            {
                //セーブデータ内のユーザーエリアのIdを検索して削除
                int UserAreaId = userArea.transform.GetComponent<Id>().id;
                for (int i = 0; i < contents.user.Count; i++)
                {
                    if (contents.user[i].index == UserAreaId)
                    {
                        contents.user.Remove(contents.user[i]);
                    }
                }
                //UIを削除
                Destroy(userArea);
                userArea.SetActive(false);
                Canvas.ForceUpdateCanvases();
                userArea.SetActive(true);
                //セーブする
                SaveManager.save(Controller.contentsStatus, contents);
            }
        }
    }
    /**
    <summary>
        ユーザーネームを編集して保存する
        return : なし
    </summary>
    */
    public static void editUserNameAreaHozon(Transform inputArea, Contents contents, Transform userName)
    {

        int userNameId = userName.GetComponent<Id>().id;

        string userNameTxt =
        inputArea.transform.GetChild(7).GetComponent<Transform>().
                GetChild(0).GetComponent<Transform>().
                GetChild(2).GetComponent<TMP_EmojiTextUGUI>().text;

        //UIに反映
        userName.transform.GetChild(0).GetComponent<Transform>().
            GetChild(1).GetComponent<TMP_EmojiTextUGUI>().text
            = userNameTxt;

        //セーブデータに反映
        for (int i = 0; i < contents.user.Count; i++)
        {
            if (contents.user[i].index == userNameId)
            {
                contents.user[i].userName = userNameTxt;
            }
        }

        //セーブする
        SaveManager.save(Controller.contentsStatus, contents);
    }
    /**
    <summary>
        詳細エリアを編集して保存する
        return : なし
    </summary>
    */
    public static void editShousaiAreaHozon(Transform inputArea, Contents contents, Transform shousai)
    {

        int shousaiId = shousai.GetComponent<Id>().id;
        int userNameId = shousai.parent.GetComponent<Id>().id;

        string itemTxt =
        inputArea.GetChild(0).GetComponent<Transform>().
                GetChild(0).GetComponent<Transform>().
                GetChild(2).GetComponent<TMP_EmojiTextUGUI>().text;
        string moneyTxt_ =
        inputArea.GetChild(1).GetComponent<Transform>().
                GetChild(0).GetComponent<Transform>().
                GetChild(2).GetComponent<TMP_EmojiTextUGUI>().text;
        int moneyTxt = int.Parse(Regex.Replace(moneyTxt_, @"[^0-9]", ""));
        string dateTxt =
        inputArea.GetChild(2).GetComponent<Transform>().
                GetChild(0).GetComponent<Transform>().
                GetChild(2).GetComponent<TMP_EmojiTextUGUI>().text;

        //UIに反映
        shousai.transform.GetChild(1).GetComponent<Transform>().
            GetChild(1).GetComponent<TMP_EmojiTextUGUI>().text
            = itemTxt;
        shousai.transform.GetChild(1).GetComponent<Transform>().
            GetChild(2).GetComponent<TMP_EmojiTextUGUI>().text
            = dateTxt.Substring(5);
        shousai.transform.GetChild(1).GetComponent<Transform>().
            GetChild(3).GetComponent<TMP_EmojiTextUGUI>().text
            = "¥" + moneyTxt.ToString("N0");

        //セーブデータに反映
        for (int i = 0; i < contents.user.Count; i++)
        {
            if (contents.user[i].index == userNameId)
            {
                for (int j = 0; j < contents.user[i].shousai.Count; j++)
                {
                    if (contents.user[i].shousai[j].index == shousaiId)
                    {
                        contents.user[i].shousai[j].ItemName = itemTxt;
                        contents.user[i].shousai[j].money = moneyTxt;
                        contents.user[i].shousai[j].date = dateTxt;
                    }
                }
            }
        }

        //セーブする
        SaveManager.save(Controller.contentsStatus, contents);
    }
    /**
    <summary>
        コンテンツ編集して保存する
        return : なし
    </summary>
    */
    public static void editContentHozon(Transform inputArea, Transform contentArea)
    {
        Transform contentName =
            inputArea.GetChild(10).GetComponent<Transform>().
            GetChild(0).GetComponent<Transform>();

        string contentNameTxt = "";

        //ファイルNoの最大値+1を新しいファイルNoとして追加する
        int contentAreaId = contentArea.GetComponent<Id>().id;
        foreach (Contents content in SaveManager.saveDatas.Values)
        {
            if (contentAreaId == content.fileNo)
            {
                if ("​".Equals(contentName.GetChild(2).GetComponent<TMP_EmojiTextUGUI>().text))
                {
                    //プレースホルダー
                    contentNameTxt = contentName.GetChild(1).GetComponent<TMP_EmojiTextUGUI>().text;
                }
                else
                {
                    contentNameTxt = contentName.GetChild(2).GetComponent<TMP_EmojiTextUGUI>().text;
                }
                content.contentsName = contentNameTxt;
                contentArea.GetChild(0).GetComponent<TMP_EmojiTextUGUI>().text = contentNameTxt;
                processContentName(contentArea.gameObject, content);
                contentArea.gameObject.SetActive(false);
                Canvas.ForceUpdateCanvases();
                contentArea.gameObject.SetActive(true);
                break;
            }
        }

        //セーブデータに格納してセーブする
        ContensData.contents.contentsName = contentNameTxt;
        SaveManager.save(contentAreaId, ContensData.contents);
    }
    /**
    <summary>
        選択されていないコンテンツ名を編集する
        return : なし
    </summary>
    */
    public static string removeContentNameFormat(string text)
    {
        var target_str = "</u>\n<size=28><color=#CFCBC5><i>-select-</i>";
        var target_str2 = "<u color=#CB8652>";
        string text_ = text.Substring(0, text.IndexOf(target_str));
        text_ = text_.Substring(target_str2.Length);
        return text_;
    }
    /**
    <summary>
        すべてのコンテンツとユーザーエリアのUIを削除する
        return : なし
    </summary>
    */
    public static void allDeleteUI()
    {
        GameObject[] contents = GameObject.FindGameObjectsWithTag("ContentArea");
        GameObject[] allUserArea = GameObject.FindGameObjectsWithTag("UserArea");
        foreach(GameObject content in contents)
            Destroy(content);
        foreach (GameObject userArea in allUserArea)
        {
            userArea.SetActive(false);
            Destroy(userArea);
        }
    }
}