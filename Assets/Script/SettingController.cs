using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingController : MonoBehaviour
{
    [SerializeField] private GameObject worning;
    [SerializeField] private GameObject panel;
    [SerializeField] private Image explanatorImage;
    [SerializeField] private List<Sprite> images;
    /**
    <summary>
        ホーム画面へ繊維
        return : なし
    </summary>
    */
    public void ChengeScreenToHOME()
    {
        //セーブデータを再度取得して画面遷移する
        SaveManager.getSaveData();
        SceneManager.LoadScene("HOME");
    }
    /**
    <summary>
        警告表示のオン/オフを切り替える
        return : なし
    </summary>
    */
    public void SetActiveWorning()
    {
        if (!worning.activeSelf)
        {
            worning.SetActive(true);
        }
        else
        {
            worning.SetActive(false);
        }
        
    }
    /**
    <summary>
        すべてのデータを削除する
        return : なし
    </summary>
    */
    public void DeleteAllSaveData()
    {
        SaveManager.deleteAllSaveData();
        SetActiveWorning();
    }
    /**
    <summary>
        プライバシーポリシーへ画面遷移する
        return : なし
    </summary>
    */
    public void OpenBrowser()
    {
        Application.OpenURL("https://bpro3413privacy.hatenablog.com/");
    }
    /**
    <summary>
        使い方の画像をセットする
        return : なし
    </summary>
    */
    public void SetActivePanel()
    {
        if (!panel.activeSelf)
        {
            panel.SetActive(true);
            //ここで画像をセット
            explanatorImage.sprite = images[0];
        }
        else
        {
            panel.SetActive(false);
        }
    }
    /**
    <summary>
        使い方の説明画像を変更する(右のボタン)
        return : なし
    </summary>
    */
    public void OnChegeSprite1()
    {
        int imagesIndex = images.IndexOf(explanatorImage.sprite);
        if(images.Count == imagesIndex + 1)
        {
            explanatorImage.sprite = images[0];
        }
        else
        {
            explanatorImage.sprite = images[imagesIndex + 1];
        }
    }
    /**
    <summary>
        使い方の説明画像を変更する(左のボタン)
        return : なし
    </summary>
    */
    public void OnChegeSprite2()
    {
        int imagesIndex = images.IndexOf(explanatorImage.sprite);
        if (0 == imagesIndex)
        {
            explanatorImage.sprite = images[images.Count -1];
        }
        else
        {
            explanatorImage.sprite = images[imagesIndex - 1];
        }
    }
}
