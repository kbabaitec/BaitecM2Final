using System.Drawing;
using System.Windows.Forms;

namespace StoreClient;

public class MainForm : Form
{
    private readonly ItemApiClient _api = new();
    private readonly CategoryApiClient _catApi = new();
    private readonly DashboardApiClient _dashApi = new();
    private readonly DataGridView grid = new();
    private readonly TextBox txtName = new(), txtCode = new(), txtBrand = new();
    private readonly TextBox txtPrice = new(), txtReorder = new(), txtSearch = new();
    private readonly ComboBox cboCategory = new(), cboFilter = new();
    private readonly Button btnAdd = new(), btnUpdate = new(), btnDelete = new();
    private readonly Button btnClear = new(), btnSearch = new();
    private readonly Label lblStats = new(), lblAlert = new();

    public MainForm()
    {
        Text = $"Sari-Sari Store - {Session.CurrentUser?.FullName} ({Session.CurrentUser?.Role})";
        Size = new Size(1300, 720);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = UiHelper.Cream;
        Font = new Font("Segoe UI", 10);

        // ---- Header with buttons ----
        var header = new Label
        {
            Text = "Sari-Sari Store Inventory",
            Dock = DockStyle.Top,
            Height = 55,
            BackColor = UiHelper.Red,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(15, 0, 0, 0)
        };

        // Buttons appear left to right in the order they are created here
        Button HeaderButton(string text, int width)
        {
            var b = new Button
            {
                Text = text,
                Dock = DockStyle.Right,
                Width = width,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10)
            };
            b.FlatAppearance.BorderColor = Color.White;
            header.Controls.Add(b);
            return b;
        }

        if (Session.IsAdmin)
        {
            var btnDashboard = HeaderButton("Dashboard", 95);
            btnDashboard.Click += async (_, _) =>
            {
                new DashboardForm().ShowDialog(this);
                await LoadItems();
            };

            var btnReports = HeaderButton("Reports", 80);
            btnReports.Click += (_, _) => new ReportsForm().ShowDialog(this);
        }

        // Everyone can sell
        var btnSales = HeaderButton("Sales", 70);
        btnSales.Click += async (_, _) =>
        {
            new SalesForm().ShowDialog(this);
            await LoadItems();   // stock changed after sales
        };

        if (Session.IsAdmin)
        {
            var btnSalesLog = HeaderButton("Sales Log", 95);
            btnSalesLog.Click += (_, _) => new SalesHistoryForm().ShowDialog(this);

            var btnStock = HeaderButton("Stock", 70);
            btnStock.Click += async (_, _) =>
            {
                new StockForm().ShowDialog(this);
                await LoadItems();
            };

            var btnHistory = HeaderButton("History", 80);
            btnHistory.Click += (_, _) => new StockHistoryForm().ShowDialog(this);

            var btnCategories = HeaderButton("Categories", 110);
            btnCategories.Click += async (_, _) =>
            {
                new CategoriesForm().ShowDialog(this);
                await LoadCategories();
                await LoadItems();
            };

            var btnSuppliers = HeaderButton("Suppliers", 100);
            btnSuppliers.Click += (_, _) => new SuppliersForm().ShowDialog(this);

            var btnUsers = HeaderButton("Users", 70);
            btnUsers.Click += (_, _) => new UsersForm().ShowDialog(this);
        }

        var btnLogout = HeaderButton("Logout", 80);
        btnLogout.Click += (_, _) =>
        {
            Session.LoggedOut = true;
            Session.CurrentUser = null;
            Close();
        };

        // ---- Low-stock alert banner (Admin only) ----
        lblAlert.Dock = DockStyle.Top;
        lblAlert.Height = 34;
        lblAlert.BackColor = Color.FromArgb(255, 243, 205);
        lblAlert.ForeColor = Color.FromArgb(133, 100, 4);
        lblAlert.TextAlign = ContentAlignment.MiddleLeft;
        lblAlert.Padding = new Padding(15, 0, 0, 0);
        lblAlert.Cursor = Cursors.Hand;
        lblAlert.Visible = false;
        lblAlert.Click += async (_, _) =>
        {
            new StockForm().ShowDialog(this);
            await LoadItems();
        };

        // ---- Input panel (left) ----
        var panel = new Panel { Dock = DockStyle.Left, Width = 260, AutoScroll = true };
        int y = 10;
        void AddField(string label, Control box)
        {
            panel.Controls.Add(new Label { Text = label, Left = 15, Top = y, AutoSize = true });
            box.Left = 15; box.Top = y + 22; box.Width = 225;
            panel.Controls.Add(box);
            y += 58;
        }
        AddField("Name", txtName);
        AddField("Code (unique)", txtCode);
        AddField("Brand", txtBrand);
        AddField("Unit Price", txtPrice);

        cboCategory.DropDownStyle = ComboBoxStyle.DropDownList;
        AddField("Category", cboCategory);
        AddField("Reorder level", txtReorder);
        txtReorder.Text = "10";

        UiHelper.StyleButton(btnAdd, "Add", Color.SeaGreen, 15, y, 108);
        UiHelper.StyleButton(btnUpdate, "Update", Color.SteelBlue, 132, y, 108);
        UiHelper.StyleButton(btnDelete, "Delete", Color.Firebrick, 15, y + 45, 108);
        UiHelper.StyleButton(btnClear, "Clear", Color.Gray, 132, y + 45, 108);
        panel.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete, btnClear });

        // ---- Search bar with category filter ----
        var searchPanel = new Panel { Dock = DockStyle.Top, Height = 50 };
        txtSearch.PlaceholderText = "Search by name, brand or code...";
        txtSearch.SetBounds(10, 12, 330, 28);
        cboFilter.DropDownStyle = ComboBoxStyle.DropDownList;
        cboFilter.SetBounds(350, 12, 180, 28);
        UiHelper.StyleButton(btnSearch, "Search", UiHelper.Red, 540, 10, 100);
        btnSearch.Height = 30;
        searchPanel.Controls.AddRange(new Control[] { txtSearch, cboFilter, btnSearch });

        // ---- Grid and status bar ----
        UiHelper.StyleGrid(grid);
        lblStats.Dock = DockStyle.Bottom;
        lblStats.Height = 30;
        lblStats.TextAlign = ContentAlignment.MiddleLeft;
        lblStats.Padding = new Padding(10, 0, 0, 0);

        Controls.Add(grid);
        Controls.Add(lblStats);
        Controls.Add(searchPanel);
        Controls.Add(lblAlert);
        Controls.Add(panel);
        Controls.Add(header);

        // ---- Events ----
        Load += async (_, _) => { await LoadCategories(); await LoadItems(); };
        btnSearch.Click += async (_, _) => await LoadItems();
        cboFilter.SelectionChangeCommitted += async (_, _) => await LoadItems();
        btnClear.Click += (_, _) => ClearForm();
        btnAdd.Click += async (_, _) => await Save(isNew: true);
        btnUpdate.Click += async (_, _) => await Save(isNew: false);
        btnDelete.Click += async (_, _) => await Remove();
        grid.SelectionChanged += (_, _) => FillFormFromGrid();

        // Only Admin can add, update or delete items
        if (!Session.IsAdmin)
        {
            btnAdd.Visible = false;
            btnUpdate.Visible = false;
            btnDelete.Visible = false;
        }
    }

    private async Task LoadCategories()
    {
        try
        {
            var categories = await _catApi.GetAllAsync();

            var forForm = new List<Category> { new() { Id = 0, Name = "(none)" } };
            forForm.AddRange(categories);
            var forFilter = new List<Category> { new() { Id = 0, Name = "All categories" } };
            forFilter.AddRange(categories);

            cboCategory.DisplayMember = "Name"; cboCategory.ValueMember = "Id";
            cboCategory.DataSource = forForm;
            cboFilter.DisplayMember = "Name"; cboFilter.ValueMember = "Id";
            cboFilter.DataSource = forFilter;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Cannot load categories.\n\n" + ex.Message, "Connection error");
        }
    }

    private async Task LoadItems()
    {
        try
        {
            int? categoryId = cboFilter.SelectedValue is int id && id > 0 ? id : null;
            var items = await _api.GetAllAsync(txtSearch.Text, categoryId);
            grid.DataSource = items;
            FormatGrid();

            lblStats.Text = items.Count == 0
                ? "No items found."
                : $"Total items: {items.Count}   |   " +
                  $"Low stock: {items.Count(i => i.Quantity <= i.ReorderLevel)}   |   " +
                  $"Average price: ₱{items.Average(i => i.UnitPrice):N2}   |   " +
                  $"Highest: ₱{items.Max(i => i.UnitPrice):N2}";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Cannot reach the API. Is it running?\n\n" + ex.Message, "Connection error");
        }

        await RefreshAlert();
    }

    // Shows or hides the yellow banner (Admin only)
    private async Task RefreshAlert()
    {
        if (!Session.IsAdmin) return;
        try
        {
            var d = await _dashApi.GetAsync();
            if (d is null || d.LowStockCount == 0)
            {
                lblAlert.Visible = false;
                return;
            }

            lblAlert.Text = $"⚠  {d.LowStockCount} item(s) need restocking"
                + (d.OutOfStockCount > 0 ? $" ({d.OutOfStockCount} out of stock)" : "")
                + ".   Click here to open Stock.";
            lblAlert.Visible = true;
        }
        catch (Exception)
        {
            lblAlert.Visible = false;   // the banner is optional, so ignore errors
        }
    }

    private void FormatGrid()
    {
        foreach (DataGridViewColumn col in grid.Columns)
        {
            if (col.Name is "Id" or "CategoryId" or "Category") col.Visible = false;
            col.HeaderText = col.Name switch
            {
                "UnitPrice" => "Unit Price",
                "ReorderLevel" => "Reorder Level",
                "CategoryName" => "Category",
                _ => col.HeaderText
            };
        }

        // Pink rows = quantity is at or below the reorder level
        foreach (DataGridViewRow row in grid.Rows)
        {
            if (row.DataBoundItem is Item item && item.Quantity <= item.ReorderLevel)
                row.DefaultCellStyle.BackColor = Color.MistyRose;
        }
    }

    private void FillFormFromGrid()
    {
        if (grid.CurrentRow?.DataBoundItem is Item i)
        {
            txtName.Text = i.Name;
            txtCode.Text = i.Code;
            txtBrand.Text = i.Brand;
            txtPrice.Text = i.UnitPrice.ToString("0.00");
            txtReorder.Text = i.ReorderLevel.ToString();
            cboCategory.SelectedValue = i.CategoryId ?? 0;
            txtCode.ReadOnly = true;   // code is the key, can't be edited
        }
    }

    private void ClearForm()
    {
        txtName.Clear(); txtCode.Clear(); txtBrand.Clear(); txtPrice.Clear();
        txtReorder.Text = "10";
        if (cboCategory.Items.Count > 0) cboCategory.SelectedIndex = 0;
        txtCode.ReadOnly = false;
        grid.ClearSelection();
    }

    private async Task Save(bool isNew)
    {
        if (!decimal.TryParse(txtPrice.Text, out var price) || price <= 0)
        { MessageBox.Show("Enter a valid unit price."); return; }
        if (!int.TryParse(txtReorder.Text, out var reorder) || reorder < 0)
        { MessageBox.Show("Enter a valid reorder level (0 or more)."); return; }
        if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtCode.Text)
            || string.IsNullOrWhiteSpace(txtBrand.Text))
        { MessageBox.Show("Name, Code and Brand are required."); return; }

        int? categoryId = cboCategory.SelectedValue is int cid && cid > 0 ? cid : null;

        var item = new Item
        {
            Name = txtName.Text.Trim(),
            Code = txtCode.Text.Trim(),
            Brand = txtBrand.Text.Trim(),
            UnitPrice = price,
            ReorderLevel = reorder,
            CategoryId = categoryId
        };

        var error = isNew ? await _api.AddAsync(item) : await _api.UpdateAsync(item);
        if (error != null) { MessageBox.Show(error, "Error"); return; }

        ClearForm();
        await LoadItems();
    }

    private async Task Remove()
    {
        if (string.IsNullOrWhiteSpace(txtCode.Text))
        { MessageBox.Show("Select an item first."); return; }
        if (MessageBox.Show($"Delete item {txtCode.Text}?", "Confirm",
                            MessageBoxButtons.YesNo) != DialogResult.Yes) return;

        var error = await _api.DeleteAsync(txtCode.Text);
        if (error != null) { MessageBox.Show(error, "Error"); return; }

        ClearForm();
        await LoadItems();
    }
}