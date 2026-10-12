using System.Drawing;
using System.Windows.Forms;

namespace StoreClient;

public class StockHistoryForm : Form
{
    private readonly ItemApiClient _itemApi = new();
    private readonly StockApiClient _stockApi = new();

    private readonly DataGridView grid = new();
    private readonly ComboBox cboItem = new(), cboType = new();
    private readonly DateTimePicker dtFrom = new(), dtTo = new();
    private readonly Button btnFilter = new();
    private readonly Label lblSummary = new();

    public StockHistoryForm()
    {
        Text = "Stock History";
        Size = new Size(1100, 560);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = UiHelper.Cream;
        Font = new Font("Segoe UI", 10);

        var filterPanel = new Panel { Dock = DockStyle.Top, Height = 55 };

        cboItem.SetBounds(10, 14, 230, 28);
        cboItem.DropDownStyle = ComboBoxStyle.DropDownList;

        cboType.SetBounds(250, 14, 130, 28);
        cboType.DropDownStyle = ComboBoxStyle.DropDownList;
        cboType.Items.AddRange(new object[] { "All types", "Stock In", "Stock Out", "Adjustment", "Sale" });
        cboType.SelectedIndex = 0;

        var lblFrom = new Label { Text = "From", Left = 392, Top = 18, AutoSize = true };
        dtFrom.SetBounds(430, 14, 115, 28);
        var lblTo = new Label { Text = "To", Left = 555, Top = 18, AutoSize = true };
        dtTo.SetBounds(580, 14, 115, 28);
        foreach (var dt in new[] { dtFrom, dtTo })
        {
            dt.Format = DateTimePickerFormat.Short;
            dt.ShowCheckBox = true;
            dt.Checked = false;   
        }

        UiHelper.StyleButton(btnFilter, "Filter", UiHelper.Red, 710, 12, 90);
        btnFilter.Height = 32;

        filterPanel.Controls.AddRange(new Control[]
            { cboItem, cboType, lblFrom, dtFrom, lblTo, dtTo, btnFilter });

        lblSummary.Dock = DockStyle.Bottom;
        lblSummary.Height = 30;
        lblSummary.TextAlign = ContentAlignment.MiddleLeft;
        lblSummary.Padding = new Padding(10, 0, 0, 0);

        UiHelper.StyleGrid(grid);
        Controls.Add(grid);
        Controls.Add(lblSummary);
        Controls.Add(filterPanel);

        Load += async (_, _) => { await LoadItems(); await LoadHistory(); };
        btnFilter.Click += async (_, _) => await LoadHistory();
    }

    private async Task LoadItems()
    {
        try
        {
            var items = await _itemApi.GetAllAsync();
            cboItem.Items.Clear();
            cboItem.Items.Add("All items");
            foreach (var i in items) cboItem.Items.Add($"{i.Code} - {i.Name}");
            cboItem.SelectedIndex = 0;
        }
        catch (Exception ex) { MessageBox.Show("Cannot load items.\n\n" + ex.Message); }
    }

    private async Task LoadHistory()
    {
        try
        {
            string? code = cboItem.SelectedIndex > 0 ? cboItem.Text.Split(' ')[0] : null;
            string? type = cboType.SelectedIndex > 0 ? cboType.Text : null;
            DateTime? from = dtFrom.Checked ? dtFrom.Value : null;
            DateTime? to = dtTo.Checked ? dtTo.Value : null;

            var rows = await _stockApi.GetMovementsAsync(code, type, from, to);
            grid.DataSource = rows;
            FormatGrid();

            var totalIn = rows.Where(r => r.QuantityChange > 0).Sum(r => r.QuantityChange);
            var totalOut = rows.Where(r => r.QuantityChange < 0).Sum(r => r.QuantityChange);
            lblSummary.Text = $"Movements: {rows.Count}   |   Total in: +{totalIn}   |   Total out: {totalOut}";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Cannot load the history.\n\n" + ex.Message, "Connection error");
        }
    }

    private void FormatGrid()
    {
        foreach (DataGridViewColumn col in grid.Columns)
        {
            if (col.Name == "Id") col.Visible = false;
            if (col.Name == "Date") col.DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";
            col.HeaderText = col.Name switch
            {
                "ItemCode" => "Code",
                "ItemName" => "Item",
                "QuantityChange" => "Change",
                "SupplierName" => "Supplier",
                "UserName" => "By",
                _ => col.HeaderText
            };
        }

        
        foreach (DataGridViewRow row in grid.Rows)
        {
            if (row.DataBoundItem is StockMovementInfo m)
                row.Cells["QuantityChange"].Style.ForeColor =
                    m.QuantityChange > 0 ? Color.SeaGreen : Color.Firebrick;
        }
    }
}