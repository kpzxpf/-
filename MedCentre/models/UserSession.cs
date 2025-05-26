namespace MedCentre.models;

public sealed class UserSession
{
    private static readonly Lazy<UserSession> _instance =
        new Lazy<UserSession>(() => new UserSession());

    public static UserSession Instance => _instance.Value;
    
    private UserSession() { }
    public User CurrentUser { get; private set; }

    public void SignIn(User user)
    {
        CurrentUser = user;
    }

    public void SignOut()
    {
        CurrentUser = null;
    }

    public bool IsSignedIn => CurrentUser != null;
}