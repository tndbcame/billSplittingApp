using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;

public static class SaveManager
{

    // セーブデータの保存先ディレクトリ
    const string SAVE_DIRECTORY = "SaveData";
    // セーブファイルの名前
    const string SAVE_FILE_NAME = "data";
    // セーブファイルの拡張子
    const string SAVE_FILE_TAIL = ".json";
    // セーブデータの一覧
    public static Dictionary<int, Contents> saveDatas
      = new Dictionary<int, Contents>();

    // クラス起動時にSaveファイルを読み取っておく
    static SaveManager()
    {
        Debug.Log("SaveManagerClass起動");
        getSaveData();
    }
    /**
    <summary>
        データをセーブする
        return : なし
    </summary>
    */
    public static void save(int index, Contents contentsSd)
    {
        contentsSd.fileNo = index;
        contentsSd.updateTime = DateTime.Now.ToString();
        contentsSd.name = "セーブデータ" + index.ToString();
        //コンテンツデータを更新
        saveDatas[index] = contentsSd;
        string json = JsonUtility.ToJson(contentsSd);
        //TODObuildするときはここを変更する
        // IOS(クラウドに保存されないような設定が必要)
        //string path = Application.persistentDataPath;
        // unity
        string path = Directory.GetCurrentDirectory();
        path += ("/" + SAVE_DIRECTORY + "/" + SAVE_FILE_NAME + index.ToString() + SAVE_FILE_TAIL);
        createDirectory(Path.GetDirectoryName(path));
        StreamWriter writer = new StreamWriter(path, false, Encoding.GetEncoding("UTF-8"));
        writer.WriteLine(json);
        writer.Flush();
        writer.Close();
    }
    /**
    <summary>
        データを削除する。ただしSaveDataの数が残り一つの場合は削除できない
        return : なし
    </summary>
    */
    public static void delete(int index)
    {
        if (1 >= saveDatas.Count)
            return;
        //TODObuildするときはここを変更する
        // IOS(クラウドに保存されないような設定が必要)
        //string path = Application.persistentDataPath;
        string path = Directory.GetCurrentDirectory();
        path += ("/" + SAVE_DIRECTORY + "/" + SAVE_FILE_NAME + index.ToString() + SAVE_FILE_TAIL);
        File.Delete(path);
        saveDatas = new Dictionary<int, Contents>();
        getSaveData();
    }
    /**
    <summary>
        フォルダごとすべてのセーブデータを削除する
        return : なし
    </summary>
    */
    public static void deleteAllSaveData()
    {
        //string path = Application.persistentDataPath;
        string path = Directory.GetCurrentDirectory();
        path += ("/" + SAVE_DIRECTORY);
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
            //static変数を初期化
            saveDatas = new Dictionary<int, Contents>();
        }
            
    }
    /**
    <summary>
        すべてのセーブデータを取得する
        return : なし
    </summary>
    */
    public static void getSaveData()
    {
        //TODObuildするときはここを変更する
        // プロジェクトディレクトリを取得    
        // IOS(クラウドに保存されないような設定が必要)
        //string path = Application.persistentDataPath;
        // unity
        string path = Directory.GetCurrentDirectory();
        // セーブデータの保存先ディレクトリを取得
        path += ("/" + SAVE_DIRECTORY + "/");
        createDirectory(Path.GetDirectoryName(path));
        string[] names = Directory.GetFiles(path, SAVE_FILE_NAME + "*" + SAVE_FILE_TAIL);
        foreach (string name in names)
        {
            try
            {
                FileInfo info = new FileInfo(name);
                StreamReader reader = new StreamReader(info.OpenRead(), Encoding.GetEncoding("UTF-8"));
                string json = reader.ReadToEnd();
                reader.Close();
                Contents sd = JsonUtility.FromJson<Contents>(json);
                if (!saveDatas.ContainsKey(sd.fileNo))
                    saveDatas.Add(sd.fileNo, sd);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
    }
    /**
    <summary>
        データがセーブデータを使用しない場合の初期値を設定
        return : なし
    </summary>
    */
    private static Contents setInitValue(Contents contentsSd)
    {
        contentsSd.contentsName = "コンテンツ名";
        contentsSd.updateTime = DateTime.Now.ToString();
        contentsSd.user = new List<User>();
        for (int i = 1; i < 3; i++)
        {
            User user = new User();
            user.userName = "ユーザー名" + i;
            user.index = i - 1;
            user.shousai = new List<Shousai>();
            Shousai shousai = new Shousai();
            shousai.ItemName = "支払い名";
            shousai.index = 0;
            shousai.money = 1000;
            shousai.date = DateTime.Today.ToString("yyyy/M/d");
            shousai.targetUser = new List<string>();
            user.shousai.Add(shousai);
            contentsSd.user.Add(user);
        }
        return contentsSd;
    }
    /**
    <summary>
        データをロードする
        return : なし
    </summary>
    */
    public static Contents load(int index)
    {
        Contents sd = new Contents();
        //指定したキーが含まれているかどうか
        if (saveDatas.ContainsKey(index))
        {
            sd = saveDatas[index];
        }
        else
        {
            sd = setInitValue(new Contents());
        }
        return sd;
    }
    /**
    <summary>
        パスが存在しないときにパスを作成する
        return : なし
    </summary>
    */
    public static void createDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}

