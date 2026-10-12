namespace StoreClient;

public static class Session
{
    public static UserInfo? CurrentUser { get; set; }
    public static bool LoggedOut { get; set; }
    public static bool IsAdmin => CurrentUser?.Role == "Admin";
}