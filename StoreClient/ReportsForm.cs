using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
namespace StoreClient;

public class ReportsForm : Form
{
    private readonly ReportsApiClient _api = new();
    private readonly DashboardApiClient _dashApi = new();
    private readonly DataGridView grid = new();
    private readonly ComboBox cboReport = new();
    private readonly DateTimePicker dtFrom = new(), dtTo = new();
    private readonly Label lblFrom = new(), lblTo = new(), lblSummary = new();
    private readonly Button btnGenerate = new(), btnExport = new();
    public ReportsForm()
    {
        Text = "Reports";
        Size = new Size(1000, 600);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = UiHelper.Cream;
        Font = new Font("Segoe UI", 10);
        var filterPanel = new Panel { Dock = DockStyle.Top, Height = 60 };
        filterPanel.Controls.Add(new Label { Text = "Report", Left = 10, Top = 20, AutoSize = true });
        cboReport.SetBounds(70, 16, 200, 28);
        cboReport.DropDownStyle = ComboBoxStyle.DropDownList;
        cboReport.Items.AddRange(new object[]
            { "Daily sales", "Sales by item", "Inventory valuation", "Reorder list" });
        cboReport.SelectedIndex = 0;
        lblFrom.Text = "From";
        lblFrom.SetBounds(290, 20, 40, 24);
        dtFrom.SetBounds(332, 16, 115, 28);
        dtFrom.Value = DateTime.Today.AddDays(-29);
        lblTo.Text = "To";
        lblTo.SetBounds(460, 20, 25, 24);
        dtTo.SetBounds(487, 16, 115, 28);
        foreach (var dt in new[] { dtFrom, dtTo })
        {
            dt.Format = DateTimePickerFormat.Short;
            dt.ShowCheckBox = true;
            dt.Checked = true;
        }
        UiHelper.StyleButton(btnGenerate, "Generate", UiHelper.Red, 620, 14, 100);
        btnGenerate.Height = 32;
        UiHelper.StyleButton(btnExport, "Export to CSV", Color.SeaGreen, 730, 14, 130);
        btnExport.Height = 32;
        filterPanel.Controls.AddRange(new Control[]
            { cboReport, lblFrom, dtFrom, lblTo, dtTo, btnGenerate, btnExport });

        lblSummary.Dock = DockStyle.Bottom;
        lblSummary.Height = 32;
        lblSummary.TextAlign = ContentAlignment.MiddleLeft;
        lblSummary.Padding = new Padding(10, 0, 0, 0);
        lblSummary.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        UiHelper.StyleGrid(grid);
        Controls.Add(grid);
        Controls.Add(lblSummary);
        Controls.Add(filterPanel);
        Load += async (_, _) => await Generate();
        btnGenerate.Click += async (_, _) => await Generate();
        cboReport.SelectionChangeCommitted += async (_, _) =>
        {
            UpdateFilters();
            await Generate();
        };
        btnExport.Click += (_, _) => ExportCsv();
        UpdateFilters();
    }
    
    private void UpdateFilters()
    {
        bool usesDates = cboReport.SelectedIndex < 2;
        lblFrom.Visible = lblTo.Visible = dtFrom.Visible = dtTo.Visible = usesDates;
    }
    private async Task Generate()
    {
        if (cboReport.SelectedIndex < 2 && dtFrom.Checked && dtTo.Checked
            && dtTo.Value.Date < dtFrom.Value.Date)
        {
            MessageBox.Show("The 'To' date is before the 'From' date.");
            return;
        }
        DateTime? from = dtFrom.Checked ? dtFrom.Value : null;
        DateTime? to = dtTo.Checked ? dtTo.Value : null;
        try
        {

            switch (cboReport.SelectedIndex)
            {
                case 0: await ShowDailySales(from, to); break;
                case 1: await ShowSalesByItem(from, to); break;
                case 2: await ShowInventory(); break;
                default: await ShowReorderList(); break;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Cannot load the report.\n\n" + ex.Message, "Connection error");
        }
    }
    private async Task ShowDailySales(DateTime? from, DateTime? to)
    {
        var rows = await _api.GetDailySalesAsync(from, to);
        grid.DataSource = rows;
        FormatGrid(new Dictionary<string, string>
        {
            ["SalesCount"] = "Sales",
            ["ItemsSold"] = "Items sold",
            ["Total"] = "Total sales"
        }, "Total");

        lblSummary.Text = $"Days with sales: {rows.Count}   |   Sales: {rows.Sum(r => r.SalesCount)}"
            + $"   |   Items sold: {rows.Sum(r => r.ItemsSold)}   |   Total: ₱{rows.Sum(r =>
                r.Total):N2}";
    }
    private async Task ShowSalesByItem(DateTime? from, DateTime? to)
    {
        var rows = await _api.GetSalesByItemAsync(from, to);
        grid.DataSource = rows;
        FormatGrid(new Dictionary<string, string>
        {
            ["Name"] = "Item",
            ["QuantitySold"] = "Qty sold",
            ["Revenue"] = "Revenue"
        }, "Revenue");
        lblSummary.Text = $"Items sold: {rows.Count}   |   Units sold: {rows.Sum(r => r.QuantitySold)}"
            + $"   |   Revenue: ₱{rows.Sum(r => r.Revenue):N2}";
    }
    private async Task ShowInventory()
    {
        var rows = await _api.GetInventoryAsync();
        grid.DataSource = rows;
        FormatGrid(new Dictionary<string, string>
        {
            ["UnitPrice"] = "Unit price",
            ["Value"] = "Stock value"
        }, "UnitPrice", "Value");
        lblSummary.Text = $"Items: {rows.Count}   |   Units in stock: {rows.Sum(r => r.Quantity)}"
            + $"   |   Total stock value: ₱{rows.Sum(r => r.Value):N2}";
    }
    private async Task ShowReorderList()
    {
        var d = await _dashApi.GetAsync();
        var rows = d?.LowStock ?? new List<LowStockInfo>();
        grid.DataSource = rows;
        FormatGrid(new Dictionary<string, string>
        {
            ["Name"] = "Item",
            ["Quantity"] = "In stock",
            ["ReorderLevel"] = "Reorder at",
            ["Suggested"] = "Suggested order"
        });
        lblSummary.Text = $"Items to reorder: {rows.Count}   |   Units to order: {rows.Sum(r =>
            r.Suggested)}";
    }
    
    private void FormatGrid(Dictionary<string, string> headers, params string[] moneyColumns)
    {
        foreach (DataGridViewColumn col in grid.Columns)
        {
            if (headers.TryGetValue(col.Name, out var header)) col.HeaderText = header;
            if (col.Name == "Date") col.DefaultCellStyle.Format = "yyyy-MM-dd";
            if (moneyColumns.Contains(col.Name))
            {
                col.DefaultCellStyle.Format = "N2";
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }
    }
    
    private void ExportCsv()
    {

        if (grid.Rows.Count == 0)
        {
            MessageBox.Show("There is nothing to export. Generate a report first.");
            return;
        }
        using var dialog = new SaveFileDialog
        {
            Filter = "CSV file (*.csv)|*.csv",
            FileName = $"{cboReport.Text.ToLower().Replace(' ', '-')}-{DateTime.Today:yyyy-MM-dd}.csv"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        var columns = grid.Columns.Cast<DataGridViewColumn>().Where(c => c.Visible).ToList();
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", columns.Select(c => Csv(c.HeaderText))));
        foreach (DataGridViewRow row in grid.Rows)
            sb.AppendLine(string.Join(",", columns.Select(c => Csv(row.Cells[c.Index].Value))));
        try
        {
            File.WriteAllText(dialog.FileName, sb.ToString(), new UTF8Encoding(true));

        }    
        catch (Exception ex)
        {
            MessageBox.Show("Cannot save the file.\n\n" + ex.Message, "Error");
            return;
        }

        if (MessageBox.Show("Saved. Open the file now?", "Export",
                            MessageBoxButtons.YesNo) == DialogResult.Yes)
            Process.Start(new ProcessStartInfo(dialog.FileName) { UseShellExecute = true });
    }
    
    private static string Csv(object? value)
    {
        string text = value switch
        {
            null => "",
            decimal d => d.ToString("0.00", CultureInfo.InvariantCulture),
            DateTime dt => dt.ToString("yyyy-MM-dd"),
            _ => value.ToString() ?? ""
        };
        return "\"" + text.Replace("\"", "\"\"") + "\"";
    }
}