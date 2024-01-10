using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

[System.Serializable]
public class Contents
{
    public string name;
    public int fileNo;
    public string updateTime;
    public string contentsName;
    public List<User> user;
}

[Serializable]
public class User
{
    public string userName;
    public int index;
    public List<Shousai> shousai;
}

[Serializable]
public class Shousai
{
    public string ItemName;
    public int index;
    public string money;
    public string date;
}

public static class ContensData
{
    public static Contents contents = new Contents();
}
