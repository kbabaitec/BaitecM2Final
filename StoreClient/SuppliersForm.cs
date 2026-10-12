using System.Drawing;
using System.Windows.Forms;

namespace StoreClient;

public class SuppliersForm : Form
{
    private readonly SupplierApiClient _api = new();
    private readonly DataGridView grid = new();
    private readonly TextBox txtName = new(), txtContact = new(), txtAddress = new();
    private readonly Button btnAdd = new(), btnUpdate = new(), btnDelete = new(), btnClear = new();
    private int _selectedId;   // 0 means nothing is selected

    public SuppliersForm()
    {
        Text = "Manage Suppliers";
        Size = new Size(860, 460);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = UiHelper.Cream;
        Font = new Font("Segoe UI", 10);

        var panel = new Panel { Dock = DockStyle.Left, Width = 250 };
        int y = 15;
        void AddField(string label, TextBox box)
        {
            panel.Controls.Add(new Label { Text = label, Left = 15, Top = y, AutoSize = true });
            box.Left = 15; box.Top = y + 22; box.Width = 215;
            panel.Controls.Add(box);
            y += 60;
        }
        AddField("Supplier name", txtName);
        AddField("Contact number", txtContact);
        AddField("Address", txtAddress);

        UiHelper.StyleButton(btnAdd, "Add", Color.SeaGreen, 15, y, 105);
        UiHelper.StyleButton(btnUpdate, "Update", Color.SteelBlue, 125, y, 105);
        UiHelper.StyleButton(btnDelete, "Delete", Color.Firebrick, 15, y + 45, 105);
        UiHelper.StyleButton(btnClear, "Clear", Color.Gray, 125, y + 45, 105);
        panel.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete, btnClear });

        UiHelper.StyleGrid(grid);
        Controls.Add(grid);
        Controls.Add(panel);

        Load += async (_, _) => await LoadData();
        grid.SelectionChanged += (_, _) => FillFromGrid();
        btnAdd.Click += async (_, _) => await Save(isNew: true);
        btnUpdate.Click += async (_, _) => await Save(isNew: false);
        btnDelete.Click += async (_, _) => await Remove();
        btnClear.Click += (_, _) => ClearForm();
    }

    private async Task LoadData()
    {
        try
        {
            grid.DataSource = await _api.GetAllAsync();
            ClearForm();
        }
        catch (Exception ex) { MessageBox.Show("Cannot load suppliers.\n\n" + ex.Message); }
    }

    private void FillFromGrid()
    {
        if (grid.CurrentRow?.DataBoundItem is Supplier s)
        {
            _selectedId = s.Id;
            txtName.Text = s.Name;
            txtContact.Text = s.ContactNumber;
            txtAddress.Text = s.Address;
        }
    }

    private void ClearForm()
    {
        _selectedId = 0;
        txtName.Clear(); txtContact.Clear(); txtAddress.Clear();
        grid.ClearSelection();
    }

    private async Task Save(bool isNew)
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("Enter a supplier name.");
            return;
        }
        if (!isNew && _selectedId == 0)
        {
            MessageBox.Show("Select a supplier first.");
            return;
        }

        var supplier = new Supplier
        {
            Id = isNew ? 0 : _selectedId,
            Name = txtName.Text,
            ContactNumber = txtContact.Text,
            Address = txtAddress.Text
        };

        var error = isNew ? await _api.AddAsync(supplier) : await _api.UpdateAsync(supplier);
        if (error != null) { MessageBox.Show(error, "Error"); return; }
        await LoadData();
    }

    private async Task Remove()
    {
        if (_selectedId == 0) { MessageBox.Show("Select a supplier first."); return; }
        if (MessageBox.Show($"Delete supplier {txtName.Text}?", "Confirm",
                            MessageBoxButtons.YesNo) != DialogResult.Yes) return;

        var error = await _api.DeleteAsync(_selectedId);
        if (error != null) { MessageBox.Show(error, "Error"); return; }
        await LoadData();
    }
}