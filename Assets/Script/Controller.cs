using System;
using System.Collections.Generic;
using Kyub.EmojiSearch.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GameObject inputFiled;
    [SerializeField] private GameObject returnObject;
    [SerializeField] private GameObject covers;
    [SerializeField] private GameObject addition;
    [SerializeField] private GameObject dustBox;
    [SerializeField] private Toggle Tgl1;
    [SerializeField] private Toggle Tgl2;
    [SerializeField] private Image cv;
    [SerializeField] private ReOrderableList reOrderableList;
    //キャッシュ用のオブジェクト
    private Transform inputArea;
    //新しい項目を追加をするボタン
    private GameObject addItemBtn;
    //新しいユーザーを追加をするボタン
    private GameObject addUserBtn;
    //コンテンツ追加ボタン
    private GameObject addContentBtn;
    //ユーザーネームボタン
    private GameObject userNameBtn;
    //ユーザーネームチェックボタン
    private GameObject userNameCheckBtn;
    //詳細項目ボタン
    private GameObject shousaiBtn;
    //詳細項目チェックボタン
    private GameObject shousaiCheckBtn;
    //コンテンツボタン
    private GameObject contentBtn;
    //矢印ボタン
    private GameObject arrowBtn;
    //支払い対象のユーザーのチェックマーク
    private Image targetUserCheckMark;
    //押したボタンの状態を管理
    private int btnStatus;
    //詳細エリアの削除できるかどうかの状態管理
    private bool shousaiDeleteBtnStatus = false;
    //ユーザーの削除できるかどうかの状態管理
    private bool userDeleteBtnStatus = false;
    //押したボタンを取得するためのGameObject型の変数
    private GameObject button_ob;
    //長押しの判定時間
    private float longTapTime = 0.4f;
    //ユーザーエリアの編集モード時のオブジェクトリスト
    private List<Transform> userNamesList;
    //詳細エリアの編集モード時のオブジェクトリスト
    private List<Transform> shousaiList;
    //編集モード用フラグ
    public static bool editFlg = true;
    //編集モードの状態を管理(通常:0, ユーザーエリア:1, 詳細エリア:2)
    public static int editStatus = 0;
    //コンテンツの状態を管理
    public static int contentsStatus;
    //並べ替えるオブジェクトのインデックス
    private int oldElementIndex = 0;
    //並べ替えられるオブジェクトのインデックス
    private int updateElementIndex = 0;
    //ボタンのタグを格納する
    private string buttonObTag;

    void Start()
    {
        //初めてロードする
        Utility.loadContentAtFirstTime();
        //支出を計算する
        Utility.calcUserPeyment();
        //InputFiledからテキストを取得しやすいようにキャッシュ
        inputArea = inputFiled.transform.GetChild(0).GetComponent<Transform>();
    }

    void Update()
    {
        if (LongTap.isDown && editFlg)
        {
            LongTap.time += Time.deltaTime;
            if (LongTap.time >= longTapTime)
            {

                Debug.Log("Long Tap");
                editFlg = false;
                LongTap.isDown = false;
                button_ob = eventSystem.currentSelectedGameObject;
                buttonObTag = button_ob.tag;
                returnObject.SetActive(true);
                covers.SetActive(true);
                addition.SetActive(false);
                dustBox.SetActive(false);
                reOrderableList.Interactable = true;
                reOrderableList.Orderable = true;
                Utility.onChangeEditMode(button_ob, scrollRect, buttonObTag);
                if (buttonObTag == "userNameArea")
                {
                    userNamesList = Utility.getNewUserNames();
                }
                else if (buttonObTag == "shousaiArea")
                {
                    shousaiList = Utility.getNewShousai();
                }
            }
        }
    }
    public void OnBeginOrderListener(int index)
    {
        oldElementIndex = index;
        Debug.Log($"開始: {index}");
    }

    public void OnUpdateOrderListener(int index)
    {
        if (buttonObTag == "userNameArea")
        {
            if (index == 0)
                return;
        }
        else if (buttonObTag == "shousaiArea")
        {
            if (index == 0 || index == 1 || index == 2)
                return;
        }
        updateElementIndex = index;
        Debug.Log($"更新: {oldElementIndex} ⇒ {updateElementIndex}");
    }
    /**
    <summary>
        順番を編集する、並び替え終了後に呼び出される
        return : なし
    </summary>
    */
    public void OnEndOrderLisner()
    {
        Debug.Log($"完了: {oldElementIndex} ⇒ {updateElementIndex}");
        if (buttonObTag == "shousaiArea")
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
                else if (child.tag == "ContentbottomLeft")
                {
                    child.SetSiblingIndex(0);
                }
            }
            if (updateElementIndex == 0 || updateElementIndex == 1 || updateElementIndex == 2 || updateElementIndex == oldElementIndex)
                return;
            for (int i = 0; i < shousaiList.Count; i++)
            {
                shousaiList[i].GetComponent<ElementIndex>().Index = shousaiList[i].transform.GetSiblingIndex();
            }
        }
        else if (buttonObTag == "userNameArea")
        {
            Transform UserNameAreaEdt = GameObject.FindGameObjectWithTag("UserNameAreaEdt").transform;
            foreach (Transform child in UserNameAreaEdt)
            {
                if (child.tag == "ContentbottomLeft")
                {
                    child.SetAsFirstSibling();
                }
            }
            if (updateElementIndex == 0 || updateElementIndex == oldElementIndex)
                return;
            for (int i = 0; i < userNamesList.Count; i++)
            {
                userNamesList[i].GetComponent<ElementIndex>().Index = userNamesList[i].transform.GetSiblingIndex();
            }
        }
    }
    /**
    <summary>
        新たな支払いを追加が押されたときの処理
        return : なし
    </summary>
    */
    public void OnAddOrUpdateShousaiArea(int status)
    {
        if (!editFlg)
            return;
        btnStatus = status;
        inputFiled.SetActive(true);

        //子を取得して関係ないオブジェクトをfalseにする
        int childCount = inputArea.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child =
                    inputArea.transform.GetChild(i).GetComponent<Transform>();
            if (child.tag == "AddUser" || child.tag == "AddContent" || child.tag == "DeleteContent" || child.tag == "Hozon")
            {
                child.gameObject.SetActive(false);
            }
            else if (child.tag == "AddItem")
            {
                child.gameObject.SetActive(true);
            }
        }

        GameObject parentObj = new GameObject();

        //日付のプレースホルダーは更新するため
        Transform datePlaceholder = GameObject.FindGameObjectWithTag("DatePlaceholder").transform;

        //押したボタンを取得
        if (btnStatus == 1)
        {
            addItemBtn = eventSystem.currentSelectedGameObject;
            parentObj = addItemBtn.transform.parent.gameObject;

            //日付プレースホルダーに本日の日付を入力する
            datePlaceholder.GetComponent<TMP_EmojiTextUGUI>().text
                = DateTime.Today.ToString("yyyy/M/d");
        }
        else if (btnStatus == 6)
        {
            shousaiBtn = eventSystem.currentSelectedGameObject;
            parentObj = shousaiBtn.transform.parent.gameObject;
            string date = "";
            string shiharai = "";
            //セーブデータ内のユーザーエリアと詳細エリアのIdを検索して日付と支払い名を取得
            int UserAreaId = shousaiBtn.transform.parent.GetComponent<Id>().id;
            int shousaiAreaId = shousaiBtn.transform.GetComponent<Id>().id;
            for (int i = 0; i < ContensData.contents.user.Count; i++)
            {
                if (ContensData.contents.user[i].index == UserAreaId)
                {
                    for (int j = 0; j < ContensData.contents.user[i].shousai.Count; j++)
                    {
                        if (ContensData.contents.user[i].shousai[j].index == shousaiAreaId)
                        {
                            date = ContensData.contents.user[i].shousai[j].date;
                            shiharai = ContensData.contents.user[i].shousai[j].ItemName;
                            break;
                        }
                    }
                }
            }

            //支払い名
            inputArea.GetChild(0).GetComponent<TMP_InputField>().text
                = shiharai;

            //金額
            inputArea.GetChild(1).GetComponent<TMP_InputField>().text
                = shousaiBtn.transform.GetChild(1).GetComponent<Transform>().
                    GetChild(3).GetComponent<TMP_EmojiTextUGUI>().text;

            //日付は形式が異なるためセーブデータから直接取得する
            //入力値
            inputArea.GetChild(2).GetComponent<TMP_InputField>().text
                = date;
            //プレースホルダー
            datePlaceholder.GetComponent<TMP_EmojiTextUGUI>().text
                = date;
        }

        //自分以外のユーザーの名前を格納
        string currentUserName = parentObj.transform.GetChild(0).GetComponent<Transform>().
            GetChild(0).GetComponent<Transform>().
            GetChild(1).GetComponent<TMP_EmojiTextUGUI>().text;
        GameObject[] userNames = GameObject.FindGameObjectsWithTag("userNameArea");
        GameObject targetUserContent = GameObject.FindGameObjectWithTag("TargetUserContent");
        foreach (GameObject username in userNames)
        {
            string _username = username.transform.GetChild(0).GetComponent<Transform>().
                GetChild(1).GetComponent<TMP_EmojiTextUGUI>().text;
            if (currentUserName == _username)
                continue;
            Transform targetUser = Utility.getTargetUser(targetUserContent);
            targetUser.GetChild(1).GetComponent<TMP_EmojiTextUGUI>().text = _username;
            //ここでユーザー名が含まれている場合はtrueにする
            if (btnStatus == 6 && shousaiBtn.transform.GetChild(0).GetComponent<TargetUserList>().targetUserList.Contains(_username))
                targetUser.GetChild(0).GetComponent<Transform>().GetChild(0).GetComponent<Image>().enabled = true;
        }
        if (btnStatus == 6 && shousaiBtn.transform.GetChild(0).GetComponent<TargetUserList>().targetUserList.Contains("指定なしfdhksjhfkjshdgakjdshfkjh"))
        {
            Tgl2.isOn = true;
        }
        else if (btnStatus == 6 && shousaiBtn.transform.GetChild(0).GetComponent<TargetUserList>().targetUserList.Count < 1)
        {
            Tgl1.isOn = true;
        }
        else
        {
            Tgl1.isOn = false; Tgl2.isOn = false;
        }
    }
    /**
    <summary>
        戻るボタンが押されたときの処理
        return : なし
    </summary>
    */
    public void OnReturnButton()
    {
        //エディットフラグをもとに
        editFlg = true;
        returnObject.SetActive(false);
        covers.SetActive(false);
        addition.SetActive(true);
        dustBox.SetActive(true);
        reOrderableList.Interactable = false;
        reOrderableList.Orderable = false;
        if (editStatus == 1)
        {
            Utility.onChangeNormalModeListener(userNamesList, ContensData.contents, scrollRect);
        }
        else if (editStatus == 2)
        {
            Utility.onChangeNormalModeListener(shousaiList, ContensData.contents, scrollRect);
        }
        Utility.calcUserPeyment();
    }
    /**
    <summary>
        ユーザー追加/編集ボタンが押されたときの処理
        return : なし
    </summary>
    */
    public void OnAddOrUpdateUserArea(int status)
    {
        if (!editFlg)
            return;
        inputFiled.SetActive(true);
        btnStatus = status;

        //子を取得して関係ないオブジェクトをfalseにする
        int childCount = inputArea.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child =
                    inputArea.transform.GetChild(i).GetComponent<Transform>();
            if (child.tag == "AddUser" || child.tag == "Hozon")
            {
                child.gameObject.SetActive(true);
            }
            else if (child.tag == "AddItem" || child.tag == "AddContent" || child.tag == "DeleteContent")
            {
                child.gameObject.SetActive(false);
            }
        }

        //押したボタンを取得
        if (btnStatus == 2)
        {
            addUserBtn = eventSystem.currentSelectedGameObject;
        }
        else if (btnStatus == 5)
        {
            userNameBtn = eventSystem.currentSelectedGameObject;

            inputArea.GetChild(7).
                transform.GetComponent<TMP_InputField>().text
                = userNameBtn.transform.GetChild(0).GetComponent<Transform>().
                    GetChild(1).GetComponent<TMP_EmojiTextUGUI>().text;
        }
    }
    /**
    <summary>
        コンテンツの追加/編集ボタンが押されたときの処理
        return : なし
    </summary>
    */
    public void OnAddOrUpdateContent(int status)
    {
        if (!editFlg)
            return;

        btnStatus = status;
        if (btnStatus == 7)
        {
            addContentBtn = eventSystem.currentSelectedGameObject;
        }
        else if (btnStatus == 8)
        {
            contentBtn = eventSystem.currentSelectedGameObject;

            //選択されていない場合は終了
            if (contentBtn.GetComponent<Id>().id != contentsStatus)
                return;

            inputArea.GetChild(10).
                transform.GetComponent<TMP_InputField>().text
                = Utility.removeContentNameFormat(contentBtn.transform.GetChild(0).GetComponent<TMP_EmojiTextUGUI>().text);
        }
        inputFiled.SetActive(true);
        //子を取得して関係ないオブジェクトをfalseにする
        int childCount = inputArea.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child =
                    inputArea.transform.GetChild(i).GetComponent<Transform>();
            if (child.tag == "AddContent" || child.tag == "Hozon")
            {
                child.gameObject.SetActive(true);
            }
            else if (child.tag == "AddItem" || child.tag == "AddUser")
            {
                child.gameObject.SetActive(false);
            }
            else if (child.tag == "DeleteContent" && btnStatus == 8)
            {
                child.gameObject.SetActive(true);
            }
            else if (child.tag == "DeleteContent" && btnStatus == 7)
            {
                child.gameObject.SetActive(false);
            }
        }
    }
    /**
    <summary>
        詳細エリアのチェックボタンが押されたときの処理
        return : なし
    </summary>
    */
    public void OnShousaiCheckButton()
    {
        shousaiCheckBtn = eventSystem.currentSelectedGameObject;
        bool shousaiCheckBtnStatus = shousaiCheckBtn.transform.GetChild(0).GetComponent<Image>().enabled;


        if (shousaiCheckBtnStatus)
        {
            shousaiCheckBtn.transform.GetChild(0).GetComponent<Image>().enabled = false;
            shousaiCheckBtn.transform.GetChild(1).GetComponent<Image>().enabled = false;
            shousaiDeleteBtnStatus = false;
        }
        else
        {
            shousaiCheckBtn.transform.GetChild(0).GetComponent<Image>().enabled = true;
            shousaiCheckBtn.transform.GetChild(1).GetComponent<Image>().enabled = true;
            shousaiDeleteBtnStatus = true;
        }
        Utility.calcUserPeyment();
    }
    /**
<summary>
    ユーザーエリアのチェックボタンが押されたときの処理
    return : なし
</summary>
*/
    public void OnUserCheckButton()
    {
        //ユーザーネームエリア
        userNameCheckBtn = eventSystem.currentSelectedGameObject;
        bool userNameCheckBtntatus = userNameCheckBtn.transform.GetChild(0).GetComponent<Image>().enabled;

        //ユーザーエリア取得
        Transform userArea = userNameCheckBtn.transform.parent.parent.parent;

        //追加エリア
        Transform addArea =
        userArea.GetChild(1).GetComponent<Transform>().
            GetChild(0).GetComponent<Transform>().
            GetChild(1).GetComponent<Transform>();


        if (userNameCheckBtntatus)
        {
            userNameCheckBtn.transform.parent.GetChild(4).GetComponent<Transform>().
            GetChild(0).GetComponent<Image>().enabled = false;
            userNameCheckBtn.transform.GetChild(0).GetComponent<Image>().enabled = false;
            addArea.transform.GetChild(0).GetComponent<Image>().enabled = false;
            //詳細エリアは複数のため
            for (int i = 2; i < userArea.childCount; i++)
            {
                Transform shousaiArea =
                    userArea.GetChild(i).GetComponent<Transform>().
                    GetChild(1).GetComponent<Transform>().
                    GetChild(0).GetComponent<Transform>();
                shousaiArea.transform.GetChild(0).GetComponent<Image>().enabled = false;
                shousaiArea.transform.GetChild(1).GetComponent<Image>().enabled = false;
            }
            userDeleteBtnStatus = false;
        }
        else
        {
            userNameCheckBtn.transform.parent.GetChild(4).GetComponent<Transform>().
            GetChild(0).GetComponent<Image>().enabled = true;
            userNameCheckBtn.transform.GetChild(0).GetComponent<Image>().enabled = true;
            addArea.transform.GetChild(0).GetComponent<Image>().enabled = true;
            for (int i = 2; i < userArea.childCount; i++)
            {
                Transform shousaiArea =
                    userArea.GetChild(i).GetComponent<Transform>().
                    GetChild(1).GetComponent<Transform>().
                    GetChild(0).GetComponent<Transform>();
                shousaiArea.transform.GetChild(0).GetComponent<Image>().enabled = true;
                shousaiArea.transform.GetChild(1).GetComponent<Image>().enabled = true;
            }
            userDeleteBtnStatus = true;
        }
        Utility.calcUserPeyment();
    }
    /**
    <summary>
        チェックされてる詳細エリアを削除する
        return : なし
    </summary>
    */
    public void OnDeleteArea()
    {
        //TODOデータが存在しないときにここで削除するとエラーになる問題を解決する
        ContensData.contents = SaveManager.saveDatas[contentsStatus];
        if (shousaiDeleteBtnStatus)
        {
            Utility.deleteShousaiArea(ContensData.contents);
        }
        if (userDeleteBtnStatus)
        {
            Utility.deleteUserArea(ContensData.contents);
        }
    }
    /**
    <summary>
        チェックされてる詳細エリアを削除する
        return : なし
    </summary>
    */
    public void OnDeleteContent()
    {
        if (1 >= SaveManager.saveDatas.Count)
            return;

        SaveManager.delete(contentsStatus);
        Utility.allDeleteUI();
        Utility.loadContent();
        Utility.calcUserPeyment();
        inputFiled.SetActive(false);
    }
    /**
    <summary>
        保存ボタンが押されたときの処理
        return : なし
    </summary>
    */
    public void OnHozon()
    {
        //セーブデータを呼び出して該当のコンテンツを取得する
        SaveManager.getSaveData();
        ContensData.contents = SaveManager.saveDatas[contentsStatus];

        switch (btnStatus)
        {
            case 1:
                //押したボタンの親オブジェクトを取得
                Transform UserArea = addItemBtn.transform.parent;
                Utility.onNewItemHozon(inputArea, ContensData.contents, UserArea, Tgl1, Tgl2);
                break;
            case 2:
                Utility.onNewUserHozon(inputArea, ContensData.contents);
                break;
            case 5:
                Utility.editUserNameAreaHozon(inputArea, ContensData.contents, userNameBtn.transform);
                break;
            case 6:
                Utility.editShousaiAreaHozon(inputArea, ContensData.contents, shousaiBtn.transform, Tgl1, Tgl2);
                break;
            case 7:
                Utility.onNewContentHozon(inputArea, addContentBtn.transform.parent);
                break;
            case 8:
                Utility.editContentHozon(inputArea, contentBtn.transform);
                break;
        }
        Utility.calcUserPeyment();
        inputFiled.SetActive(false);
    }
    /**
    <summary>
        バツボタンを押したときの処理
        return : なし
    </summary>
    */
    public void OnBatuButton()
    {
        if (btnStatus == 1 || btnStatus == 6)
            Utility.deleteTargetUser();
        inputFiled.SetActive(false);
    }
    /**
    <summary>
        コンテンツを変更するときの処理
        return : なし
    </summary>
    */
    public void OnNotSelectContent()
    {

        //コンテンツを取得(選択されているコンテンツは対象外)
        contentBtn = eventSystem.currentSelectedGameObject;
        int contentId = contentBtn.transform.GetComponent<Id>().id;
        if (contentId != contentsStatus)
        {
            Utility.loadNoSelectContent(contentId, contentBtn, contentsStatus);
            Utility.calcUserPeyment();
        }
    }
    /**
    <summary>
        矢印ボタンが押されたときの処理
        return : なし
    </summary>
    */
    public void OnArrowBtn()
    {
        if (!editFlg)
            return;

        //矢印ボタンを取得
        arrowBtn = eventSystem.currentSelectedGameObject;
        Transform arrow = arrowBtn.transform.GetChild(1).GetComponent<Transform>();
        Transform userArea = arrowBtn.transform.parent.parent.parent;
        int childCount = userArea.childCount;

        //矢印フラグの判定
        if (arrow.GetComponent<ArrowFlg>().arrowFlg)
        {
            arrow.GetComponent<ArrowFlg>().arrowFlg = false;
            arrow.rotation = Quaternion.Euler(0.0f, 0.0f, -90f);
            //ユーザーエリア以外のオブジェクトを非アクティブにする
            for (int i = 0; i < childCount; i++)
            {
                if (userArea.GetChild(i).tag == "userNameArea")
                    continue;
                userArea.GetChild(i).gameObject.SetActive(false);
            }
        }
        else
        {
            arrow.GetComponent<ArrowFlg>().arrowFlg = true;
            arrow.rotation = Quaternion.Euler(0.0f, 0.0f, -180f);
            //ユーザーエリア以外のオブジェクトを非アクティブにする
            for (int i = 0; i < childCount; i++)
            {
                if (userArea.GetChild(i).tag == "userNameArea")
                    continue;
                userArea.GetChild(i).gameObject.SetActive(true);
            }
        }
        //リフレッシュ
        userArea.gameObject.SetActive(false);
        Canvas.ForceUpdateCanvases();
        userArea.gameObject.SetActive(true);
    }
    /**
    <summary>
        Toggleを変更されたときの処理
        return : なし
    </summary>
    */
    public void ChengeToggle()
    {
        if (!Tgl1.isOn && !Tgl2.isOn)
        {
            cv.enabled = false;
        }
        else
        {
            cv.enabled = true;
        }
    }
    /**
    <summary>
        支払い対象のチェックマークが押されたときの処理
        return : なし
    </summary>
    */
    public void ChengeTargetUserCheck()
    {
        targetUserCheckMark = eventSystem.currentSelectedGameObject.transform.GetChild(0).GetComponent<Image>();
        if (targetUserCheckMark.enabled)
        {
            targetUserCheckMark.enabled = false;
        }
        else
        {
            targetUserCheckMark.enabled = true;
        }
    }
    /**
    <summary>
        設定画面へ繊維
        return : なし
    </summary>
    */
    public void ChengeScreenToSetting()
    {
        SceneManager.LoadScene("Setting");
    }
}
