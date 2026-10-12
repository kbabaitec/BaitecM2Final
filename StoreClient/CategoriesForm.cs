using System.Drawing;
using System.Windows.Forms;

namespace StoreClient;

public class CategoriesForm : Form
{
    private readonly CategoryApiClient _api = new();
    private readonly DataGridView grid = new();
    private readonly TextBox txtName = new();
    private readonly Button btnAdd = new(), btnUpdate = new(), btnDelete = new(), btnClear = new();
    private int _selectedId;   

    public CategoriesForm()
    {
        Text = "Manage Categories";
        Size = new Size(700, 440);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = UiHelper.Cream;
        Font = new Font("Segoe UI", 10);

        var panel = new Panel { Dock = DockStyle.Left, Width = 250 };
        panel.Controls.Add(new Label { Text = "Category name", Left = 15, Top = 15, AutoSize = true });
        txtName.SetBounds(15, 38, 215, 28);
        panel.Controls.Add(txtName);

        UiHelper.StyleButton(btnAdd, "Add", Color.SeaGreen, 15, 85, 105);
        UiHelper.StyleButton(btnUpdate, "Update", Color.SteelBlue, 125, 85, 105);
        UiHelper.StyleButton(btnDelete, "Delete", Color.Firebrick, 15, 130, 105);
        UiHelper.StyleButton(btnClear, "Clear", Color.Gray, 125, 130, 105);
        panel.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete, btnClear });

        UiHelper.StyleGrid(grid);
        Controls.Add(grid);
        Controls.Add(panel);

        Load += async (_, _) => await LoadData();
        grid.SelectionChanged += (_, _) => FillFromGrid();
        btnAdd.Click += async (_, _) => await AddCategory();
        btnUpdate.Click += async (_, _) => await UpdateCategory();
        btnDelete.Click += async (_, _) => await DeleteCategory();
        btnClear.Click += (_, _) => ClearForm();
    }

    private async Task LoadData()
    {
        try
        {
            grid.DataSource = await _api.GetAllAsync();
            ClearForm();
        }
        catch (Exception ex) { MessageBox.Show("Cannot load categories.\n\n" + ex.Message); }
    }

    private void FillFromGrid()
    {
        if (grid.CurrentRow?.DataBoundItem is Category c)
        {
            _selectedId = c.Id;
            txtName.Text = c.Name;
        }
    }

    private void ClearForm()
    {
        _selectedId = 0;
        txtName.Clear();
        grid.ClearSelection();
    }

    private async Task AddCategory()
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("Enter a category name.");
            return;
        }

        var error = await _api.AddAsync(new Category { Name = txtName.Text });
        if (error != null) { MessageBox.Show(error, "Error"); return; }
        await LoadData();
    }

    private async Task UpdateCategory()
    {
        if (_selectedId == 0) { MessageBox.Show("Select a category first."); return; }
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("Enter a category name.");
            return;
        }

        var error = await _api.UpdateAsync(new Category { Id = _selectedId, Name = txtName.Text });
        if (error != null) { MessageBox.Show(error, "Error"); return; }
        await LoadData();
    }

    private async Task DeleteCategory()
    {
        if (_selectedId == 0) { MessageBox.Show("Select a category first."); return; }
        if (MessageBox.Show($"Delete category {txtName.Text}?", "Confirm",
                            MessageBoxButtons.YesNo) != DialogResult.Yes) return;

        var error = await _api.DeleteAsync(_selectedId);
        if (error != null) { MessageBox.Show(error, "Error"); return; }
        await LoadData();
    }
}