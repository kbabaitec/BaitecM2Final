namespace StoreClient;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        while (true)
        {
            Session.LoggedOut = false;

            using (var login = new LoginForm())
            {
                if (login.ShowDialog() != DialogResult.OK) break;   // closed the login: exit
            }

            Application.Run(new MainForm());

            if (!Session.LoggedOut) break;   // closed the window: exit, logout: show login again
        }
    }
}