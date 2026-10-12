using System.Drawing;
using System.Windows.Forms;

namespace StoreClient;

public class UsersForm : Form
{
    private readonly UserApiClient _api = new();
    private readonly DataGridView grid = new();
    private readonly TextBox txtUsername = new(), txtFullName = new(), txtPassword = new();
    private readonly ComboBox cboRole = new();
    private readonly Button btnAdd = new(), btnDelete = new();

    public UsersForm()
    {
        Text = "Manage Users";
        Size = new Size(780, 460);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(255, 248, 235);
        Font = new Font("Segoe UI", 10);

        var panel = new Panel { Dock = DockStyle.Left, Width = 250 };
        int y = 15;
        void AddField(string label, Control box)
        {
            panel.Controls.Add(new Label { Text = label, Left = 15, Top = y, AutoSize = true });
            box.Left = 15; box.Top = y + 22; box.Width = 215;
            panel.Controls.Add(box);
            y += 60;
        }

        txtPassword.UseSystemPasswordChar = true;
        cboRole.DropDownStyle = ComboBoxStyle.DropDownList;
        cboRole.Items.AddRange(new object[] { "Admin", "Cashier" });
        cboRole.SelectedIndex = 1;

        AddField("Username", txtUsername);
        AddField("Full name", txtFullName);
        AddField("Password", txtPassword);
        AddField("Role", cboRole);

        btnAdd.Text = "Add User";
        btnAdd.SetBounds(15, y, 105, 36);
        btnAdd.BackColor = Color.SeaGreen;
        btnAdd.ForeColor = Color.White;
        btnAdd.FlatStyle = FlatStyle.Flat;

        btnDelete.Text = "Delete";
        btnDelete.SetBounds(125, y, 105, 36);
        btnDelete.BackColor = Color.Firebrick;
        btnDelete.ForeColor = Color.White;
        btnDelete.FlatStyle = FlatStyle.Flat;

        panel.Controls.Add(btnAdd);
        panel.Controls.Add(btnDelete);

        grid.Dock = DockStyle.Fill;
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.BackgroundColor = Color.White;
        grid.RowHeadersVisible = false;

        Controls.Add(grid);
        Controls.Add(panel);

        Load += async (_, _) => await LoadUsers();
        btnAdd.Click += async (_, _) => await AddUser();
        btnDelete.Click += async (_, _) => await RemoveUser();
    }

    private async Task LoadUsers()
    {
        try { grid.DataSource = await _api.GetAllAsync(); }
        catch (Exception ex) { MessageBox.Show("Cannot load users.\n\n" + ex.Message); }
    }

    private async Task AddUser()
    {
        if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
            string.IsNullOrWhiteSpace(txtFullName.Text) ||
            string.IsNullOrWhiteSpace(txtPassword.Text))
        {
            MessageBox.Show("Username, full name and password are required.");
            return;
        }

        var error = await _api.AddAsync(txtUsername.Text, txtFullName.Text,
                                        txtPassword.Text, cboRole.Text);
        if (error != null) { MessageBox.Show(error, "Error"); return; }

        txtUsername.Clear(); txtFullName.Clear(); txtPassword.Clear();
        await LoadUsers();
    }

    private async Task RemoveUser()
    {
        if (grid.CurrentRow?.DataBoundItem is not UserInfo user)
        {
            MessageBox.Show("Select a user first.");
            return;
        }

        if (MessageBox.Show($"Delete user {user.Username}?", "Confirm",
                            MessageBoxButtons.YesNo) != DialogResult.Yes) return;

        var error = await _api.DeleteAsync(user.Id);
        if (error != null) { MessageBox.Show(error, "Error"); return; }

        await LoadUsers();
    }
}