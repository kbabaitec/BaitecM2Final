using System.Drawing;
using System.Windows.Forms;
namespace StoreClient;

public class StockForm : Form
{
    private readonly ItemApiClient _itemApi = new();
    private readonly SupplierApiClient _supplierApi = new();
    private readonly StockApiClient _stockApi = new();
    private readonly DataGridView grid = new();
    private readonly ComboBox cboAction = new(), cboItem = new();
    private readonly ComboBox cboSupplier = new(), cboReason = new();
    private readonly TextBox txtQty = new(), txtNote = new();
    private readonly Label lblQty = new(), lblSupplier = new(), lblReason = new();
    private readonly Label lblCurrent = new();
    private readonly Button btnSave = new();
    private List<Item> _items = new();
    public StockForm()
    {
        Text = "Stock Management";
        Size = new Size(960, 520);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = UiHelper.Cream;
        Font = new Font("Segoe UI", 10);
        var panel = new Panel { Dock = DockStyle.Left, Width = 300 };
        panel.Controls.Add(new Label { Text = "Action", Left = 15, Top = 15, AutoSize = true });
        cboAction.SetBounds(15, 38, 265, 28);
        cboAction.DropDownStyle = ComboBoxStyle.DropDownList;
        cboAction.Items.AddRange(new object[] { "Stock In", "Stock Out", "Adjustment" });
        cboAction.SelectedIndex = 0;
        panel.Controls.Add(new Label { Text = "Item", Left = 15, Top = 75, AutoSize = true });
        cboItem.SetBounds(15, 98, 265, 28);
        cboItem.DropDownStyle = ComboBoxStyle.DropDownList;
        cboItem.FormattingEnabled = true;
        cboItem.Format += (_, e) =>
        {
            if (e.ListItem is Item i) e.Value = $"{i.Code} - {i.Name}";
        };
        lblCurrent.SetBounds(15, 132, 265, 24);
        lblCurrent.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        lblCurrent.ForeColor = Color.SteelBlue;
        lblQty.SetBounds(15, 165, 265, 22);
        txtQty.SetBounds(15, 188, 265, 28);
        lblSupplier.Text = "Supplier (optional)";
        lblSupplier.SetBounds(15, 225, 265, 22);
        cboSupplier.SetBounds(15, 248, 265, 28);
        cboSupplier.DropDownStyle = ComboBoxStyle.DropDownList;
        lblReason.Text = "Reason";
        lblReason.SetBounds(15, 225, 265, 22);
        cboReason.SetBounds(15, 248, 265, 28);
        cboReason.DropDownStyle = ComboBoxStyle.DropDownList;
        cboReason.Items.AddRange(new object[] { "Damaged", "Expired", "Lost", "Other" });
        cboReason.SelectedIndex = 0;
        panel.Controls.Add(new Label { Text = "Note", Left = 15, Top = 285, AutoSize = true });
        txtNote.SetBounds(15, 308, 265, 28);
        UiHelper.StyleButton(btnSave, "Stock In", UiHelper.Red, 15, 355, 265);
        btnSave.Height = 40;
        panel.Controls.AddRange(new Control[]
        {
            cboAction, cboItem, lblCurrent, lblQty, txtQty,
            lblSupplier, cboSupplier, lblReason, cboReason, txtNote, btnSave
        });
        UiHelper.StyleGrid(grid);
        Controls.Add(grid);
        Controls.Add(panel);
        Load += async (_, _) => { await LoadSuppliers(); await LoadData(); };
        cboAction.SelectedIndexChanged += (_, _) => UpdateFields();
        cboItem.SelectedIndexChanged += (_, _) => UpdateCurrent();
        grid.SelectionChanged += (_, _) => SyncComboFromGrid();
        btnSave.Click += async (_, _) => await Submit();
        UpdateFields();
    }
    private void UpdateFields()
    {
        var action = cboAction.Text;
        lblQty.Text = action == "Adjustment" ? "New quantity (counted)" : "Quantity";
        lblSupplier.Visible = cboSupplier.Visible = action == "Stock In";
        lblReason.Visible = cboReason.Visible = action == "Stock Out";
        btnSave.Text = action == "Adjustment" ? "Save adjustment" : action;
    }
    private void UpdateCurrent()
    {
        lblCurrent.Text = cboItem.SelectedItem is Item i ? $"Current stock: {i.Quantity}" : "";
    }
    private void SyncComboFromGrid()
    {
        if (grid.CurrentRow?.DataBoundItem is Item i) cboItem.SelectedItem = i;
    }
    private async Task LoadSuppliers()
    {
        try
        {

            var list = new List<Supplier> { new() { Id = 0, Name = "(none)" } };
            list.AddRange(await _supplierApi.GetAllAsync());
            cboSupplier.DisplayMember = "Name";
            cboSupplier.ValueMember = "Id";
            cboSupplier.DataSource = list;
        }
        catch (Exception ex) { MessageBox.Show("Cannot load suppliers.\n\n" + ex.Message); }
    }
    private async Task LoadData(string? selectCode = null)
    {
        try
        {
            _items = await _itemApi.GetAllAsync();
            grid.DataSource = _items;

            FormatGrid();
            cboItem.DataSource = new List<Item>(_items);
            if (selectCode != null)
                cboItem.SelectedItem = _items.FirstOrDefault(i => i.Code == selectCode);
            UpdateCurrent();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Cannot load items.\n\n" + ex.Message, "Connection error");
        }
    }
    private void FormatGrid()
    {
        foreach (DataGridViewColumn col in grid.Columns)
        {
            col.Visible = col.Name is "Code" or "Name" or "Quantity" or "ReorderLevel";
            if (col.Name == "ReorderLevel") col.HeaderText = "Reorder Level";
        }
        foreach (DataGridViewRow row in grid.Rows)
        {
            if (row.DataBoundItem is Item item && item.Quantity <= item.ReorderLevel)
                row.DefaultCellStyle.BackColor = Color.MistyRose;
        }
    }
    private async Task Submit()
    {
        if (cboItem.SelectedItem is not Item item)
        {
            MessageBox.Show("Select an item.");
            return;
        }
        if (!int.TryParse(txtQty.Text, out var qty))
        {
            MessageBox.Show("Enter a whole number for the quantity.");
            return;
        }
        string? error;
        switch (cboAction.Text)
        {
            case "Stock In":
                if (qty <= 0) { MessageBox.Show("Quantity must be more than 0."); return; }
                int? supplierId = cboSupplier.SelectedValue is int sid && sid > 0 ? sid : null;
                error = await _stockApi.StockInAsync(item.Code, qty, supplierId, txtNote.Text);
                break;
            case "Stock Out":
                if (qty <= 0) { MessageBox.Show("Quantity must be more than 0."); return; }
                error = await _stockApi.StockOutAsync(item.Code, qty, cboReason.Text, txtNote.Text);
                break;
            default:   
                if (qty < 0) { MessageBox.Show("Quantity cannot be negative."); return; }
                if (string.IsNullOrWhiteSpace(txtNote.Text))
                {
                    MessageBox.Show("Please write the reason in the Note box.");
                    return;
                }
                error = await _stockApi.AdjustAsync(item.Code, qty, txtNote.Text);
                break;
        }
        if (error != null) { MessageBox.Show(error, "Error"); return; }
        MessageBox.Show("Stock updated.", "Done");
        txtQty.Clear();
        txtNote.Clear();
        await LoadData(item.Code);
    }
}