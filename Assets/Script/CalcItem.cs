
public class CalcItem
{
    //ユーザー名
    private string userName;
    //支払ってもらう対象のユーザー
    private string[] targetUser;
    //支払ってもらう対象のユーザーの数
    private int targetUserCount;
    //支出額
    private int peyment;
    //収入額
    private int income;

    public string userNameProperty
    {
        get { return userName; }
        set { this.userName = value; }

    }
    public string[] targetUserProperty
    {
        get { return targetUser; }
        set { this.targetUser = value; }

    }
    public int targetUserCountProperty
    {
        get { return targetUserCount; }
        set { this.targetUserCount = value; }

    }
    public int peymentProperty
    {
        get { return peyment; }
        set { this.peyment = value; }

    }
    public int incomeProperty
    {
        get { return income; }
        set { this.income = value; }

    }
}
