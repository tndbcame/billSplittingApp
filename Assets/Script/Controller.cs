using System;
using Kyub.EmojiSearch.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    [SerializeField] private EventSystem eventSystem;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GameObject InputFiled;
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
    //押したボタンの状態を管理
    private int btnStatus;
    //押したボタンを取得するためのGameObject型の変数
    private GameObject button_ob;
    //長押しの判定時間
    private float longTapTime = 0.4f;
    //編集モート用フラグ
    public static bool EditFlg = true;
    //コンテンツの状態を管理
    public static int ContentsStatus;

    void Start()
    {
        //初めてロードする
        Utility.loadContentAtFirstTime();
        //支出を計算する
        Utility.calcUserPeyment();
        //InputFiledからテキストを取得しやすいようにキャッシュ
        inputArea = InputFiled.transform.GetChild(0).GetComponent<Transform>();
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
                Utility.onChangeEditModeListener(button_ob, scrollRect, true);
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
    /**
    <summary>
        新たな項目を追加が押されたときの処理
        return : なし
    </summary>
    */
    public void OnAddOrUpdateItemArea(int status)
    {
        if (!EditFlg)
            return;
        btnStatus = status;
        InputFiled.SetActive(true);

        //プレースホルダーに本日の日付を入力する
        inputArea.GetChild(2).GetComponent<Transform>().
            GetChild(0).GetComponent<Transform>().
            GetChild(1).GetComponent<TMP_EmojiTextUGUI>().text
                = DateTime.Today.ToString("yyyy/M/d");
        //押したボタンを取得
        if(btnStatus == 1)
        {
            addItemBtn = eventSystem.currentSelectedGameObject;
        }
        else if(btnStatus == 6)
        {
            shousaiBtn = eventSystem.currentSelectedGameObject;
            string date = "";
            //セーブデータ内のユーザーエリアと詳細エリアのIdを検索して日付を取得
            int UserAreaId = shousaiBtn.transform.parent.GetComponent<Id>().id;
            int shousaiAreaId = shousaiBtn.transform.GetComponent<Id>().id;
            for (int i = 0; i < ContensData.contents.user.Count; i++)
            {
                if (ContensData.contents.user[i].index == UserAreaId)
                {
                    for (int j = 0; j < ContensData.contents.user[UserAreaId].shousai.Count; j++)
                    {
                        if (ContensData.contents.user[i].shousai[j].index == shousaiAreaId)
                        {
                            date = ContensData.contents.user[i].shousai[j].date;
                            break;
                        }
                    }
                }
            }

            inputArea.GetChild(0).GetComponent<TMP_InputField>().text
                = shousaiBtn.transform.GetChild(1).GetComponent<Transform>().
                    GetChild(1).GetComponent<TMP_EmojiTextUGUI>().text;
            inputArea.GetChild(1).GetComponent<TMP_InputField>().text
                = shousaiBtn.transform.GetChild(1).GetComponent<Transform>().
                    GetChild(3).GetComponent<TMP_EmojiTextUGUI>().text;
            //日付は形式が異なるためセーブデータから直接取得する
            inputArea.GetChild(2).GetComponent<TMP_InputField>().text
                = date;
        }

        //子を取得して関係ないオブジェクトをfalseにする
        int childCount = inputArea.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child =
                    inputArea.transform.GetChild(i).GetComponent<Transform>();
            if (child.tag == "AddUser" || child.tag == "AddContent")
            {
                child.gameObject.SetActive(false);
            }
            else if(child.tag == "AddItem")
            {
                child.gameObject.SetActive(true);
            }
        }
    }
    /**
    <summary>
        ユーザー追加/編集ボタンが押されたときの処理
        return : なし
    </summary>
    */
    public void OnAddOrUpdateUserArea(int status)
    {
        if (!EditFlg)
            return;
        InputFiled.SetActive(true);
        btnStatus = status;

        //押したボタンを取得
        if(btnStatus == 2)
        {
            addUserBtn = eventSystem.currentSelectedGameObject;
        }
        else if(btnStatus == 5)
        {
            userNameBtn = eventSystem.currentSelectedGameObject;

            inputArea.GetChild(7).
                transform.GetComponent<TMP_InputField>().text
                = userNameBtn.transform.GetChild(0).GetComponent<Transform>().
                    GetChild(1).GetComponent<TMP_EmojiTextUGUI>().text;
        }

        //子を取得して関係ないオブジェクトをfalseにする
        int childCount = inputArea.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child =
                    inputArea.transform.GetChild(i).GetComponent<Transform>();
            if (child.tag == "AddUser")
            {
                child.gameObject.SetActive(true);
            }
            else if (child.tag == "AddItem" || child.tag == "AddContent")
            {
                child.gameObject.SetActive(false);
            }
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
        if (!EditFlg)
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
            if (contentBtn.GetComponent<Id>().id != ContentsStatus)
                return;

            inputArea.GetChild(10).
                transform.GetComponent<TMP_InputField>().text
                = Utility.removeContentNameFormat(contentBtn.transform.GetChild(0).GetComponent<TMP_EmojiTextUGUI>().text);
        }
        InputFiled.SetActive(true);
        //子を取得して関係ないオブジェクトをfalseにする
        int childCount = inputArea.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child =
                    inputArea.transform.GetChild(i).GetComponent<Transform>();
            if (child.tag == "AddContent")
            {
                child.gameObject.SetActive(true);
            }
            else if (child.tag == "AddItem" || child.tag == "AddUser")
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
            btnStatus = 0;
        }
        else
        {
            shousaiCheckBtn.transform.GetChild(0).GetComponent<Image>().enabled = true;
            shousaiCheckBtn.transform.GetChild(1).GetComponent<Image>().enabled = true;
            btnStatus = 3;
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
            btnStatus = 0;
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
            btnStatus = 4;
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
        ContensData.contents = SaveManager.saveDatas[ContentsStatus];
        if(btnStatus == 3)
        {
            Utility.deleteShousaiArea(ContensData.contents);
        }
        else if(btnStatus == 4)
        {
            Utility.deleteUserArea(ContensData.contents);
        }
        
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
        ContensData.contents = SaveManager.saveDatas[ContentsStatus];

        switch (btnStatus)
        {
            case 1:
                //押したボタンの親オブジェクトを取得
                Transform UserArea = addItemBtn.transform.parent;
                Utility.onNewItemHozon(inputArea, ContensData.contents, UserArea);
                break;
            case 2:
                Utility.onNewUserHozon(inputArea, ContensData.contents);
                break;
            case 5:
                Utility.editUserNameAreaHozon(inputArea, ContensData.contents, userNameBtn.transform);
                break;
            case 6:
                Utility.editShousaiAreaHozon(inputArea, ContensData.contents, shousaiBtn.transform);
                break;
            case 7:
                Utility.onNewContentHozon(inputArea, addContentBtn.transform.parent);
                break;
            case 8:
                Utility.editContentHozon(inputArea, contentBtn.transform);
                break;

        }
        Utility.calcUserPeyment();
        InputFiled.SetActive(false);
    }
    /**
    <summary>
        バツボタンを押したときの処理
        return : なし
    </summary>
    */
    public void OnBatuButton()
    {
        InputFiled.SetActive(false);
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
        if(contentId != ContentsStatus)
        {
            Utility.loadContent(contentId, contentBtn, ContentsStatus);
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
        //矢印ボタンを取得
        arrowBtn = eventSystem.currentSelectedGameObject;
        Transform arrow = arrowBtn.transform.GetChild(1).GetComponent<Transform>();
        Transform userArea = arrowBtn.transform.parent.parent.parent;

        //矢印フラグの判定
        if (arrow.GetComponent<ArrowFlg>().arrowFlg)
        {
            arrow.GetComponent<ArrowFlg>().arrowFlg = false;
            arrow.rotation = Quaternion.Euler(0.0f, 0.0f, -90f);
            int childCount = userArea.childCount;
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
            int childCount = userArea.childCount;
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
}
