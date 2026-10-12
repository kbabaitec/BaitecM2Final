using System.Drawing;
using System.Windows.Forms;

namespace StoreClient;

public class LoginForm : Form
{
    private readonly AuthApiClient _auth = new();
    private readonly TextBox txtUser = new(), txtPass = new();
    private readonly Button btnLogin = new();
    private readonly Label lblError = new();

    public LoginForm()
    {
        Text = "Login";
        ClientSize = new Size(400, 380);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BackColor = Color.FromArgb(255, 248, 235);
        Font = new Font("Segoe UI", 10);

        var header = new Label
        {
            Text = "Sari-Sari Store",
            Dock = DockStyle.Top,
            Height = 70,
            BackColor = Color.FromArgb(214, 69, 65),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var lblSub = new Label { Text = "Please log in to continue", Left = 40, Top = 90, AutoSize = true };
        var lblUser = new Label { Text = "Username", Left = 40, Top = 125, AutoSize = true };
        txtUser.SetBounds(40, 148, 320, 28);
        var lblPass = new Label { Text = "Password", Left = 40, Top = 190, AutoSize = true };
        txtPass.SetBounds(40, 213, 320, 28);
        txtPass.UseSystemPasswordChar = true;

        lblError.SetBounds(40, 252, 320, 40);
        lblError.ForeColor = Color.Firebrick;

        btnLogin.Text = "Login";
        btnLogin.SetBounds(40, 305, 320, 40);
        btnLogin.BackColor = Color.FromArgb(214, 69, 65);
        btnLogin.ForeColor = Color.White;
        btnLogin.FlatStyle = FlatStyle.Flat;

        Controls.AddRange(new Control[]
            { header, lblSub, lblUser, txtUser, lblPass, txtPass, lblError, btnLogin });

        AcceptButton = btnLogin;   
        btnLogin.Click += async (_, _) => await DoLogin();
    }

    private async Task DoLogin()
    {
        lblError.Text = "";
        if (string.IsNullOrWhiteSpace(txtUser.Text) || string.IsNullOrWhiteSpace(txtPass.Text))
        {
            lblError.Text = "Enter your username and password.";
            return;
        }

        btnLogin.Enabled = false;
        try
        {
            var (user, error) = await _auth.LoginAsync(txtUser.Text.Trim(), txtPass.Text);
            if (user is null)
            {
                lblError.Text = error ?? "Login failed.";
                txtPass.Clear();
                return;
            }

            Session.CurrentUser = user;
            DialogResult = DialogResult.OK;   
        }
        catch (Exception)
        {
            lblError.Text = "Cannot reach the API. Is it running?";
        }
        finally
        {
            btnLogin.Enabled = true;
        }
    }
}
