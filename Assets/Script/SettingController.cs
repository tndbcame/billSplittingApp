using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingController : MonoBehaviour
{
    [SerializeField] private GameObject worning;
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
}
