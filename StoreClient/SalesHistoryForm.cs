using System.Drawing;
using System.Windows.Forms;

namespace StoreClient;

public class SalesHistoryForm : Form
{
    private readonly SalesApiClient _api = new();
    private readonly DataGridView grid = new();
    private readonly DateTimePicker dtFrom = new(), dtTo = new();
    private readonly Button btnFilter = new();
    private readonly Label lblSummary = new();

    public SalesHistoryForm()
    {
        Text = "Sales Log";
        Size = new Size(900, 520);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = UiHelper.Cream;
        Font = new Font("Segoe UI", 10);

        var filterPanel = new Panel { Dock = DockStyle.Top, Height = 55 };

        var lblFrom = new Label { Text = "From", Left = 10, Top = 18, AutoSize = true };
        dtFrom.SetBounds(55, 14, 115, 28);
        var lblTo = new Label { Text = "To", Left = 185, Top = 18, AutoSize = true };
        dtTo.SetBounds(210, 14, 115, 28);
        foreach (var dt in new[] { dtFrom, dtTo })
        {
            dt.Format = DateTimePickerFormat.Short;
            dt.ShowCheckBox = true;
            dt.Checked = true;   
        }

        UiHelper.StyleButton(btnFilter, "Filter", UiHelper.Red, 340, 12, 90);
        btnFilter.Height = 32;
        var lblHint = new Label
        {
            Text = "Double-click a sale to see its receipt",
            Left = 450,
            Top = 18,
            AutoSize = true,
            ForeColor = Color.Gray
        };

        filterPanel.Controls.AddRange(new Control[]
            { lblFrom, dtFrom, lblTo, dtTo, btnFilter, lblHint });

        lblSummary.Dock = DockStyle.Bottom;
        lblSummary.Height = 30;
        lblSummary.TextAlign = ContentAlignment.MiddleLeft;
        lblSummary.Padding = new Padding(10, 0, 0, 0);

        UiHelper.StyleGrid(grid);
        Controls.Add(grid);
        Controls.Add(lblSummary);
        Controls.Add(filterPanel);

        Load += async (_, _) => await LoadSales();
        btnFilter.Click += async (_, _) => await LoadSales();
        grid.CellDoubleClick += async (_, e) =>
        {
            if (e.RowIndex >= 0 && grid.Rows[e.RowIndex].DataBoundItem is SaleSummaryInfo s)
                await ShowReceipt(s.Id);
        };
    }

    private async Task LoadSales()
    {
        try
        {
            DateTime? from = dtFrom.Checked ? dtFrom.Value : null;
            DateTime? to = dtTo.Checked ? dtTo.Value : null;

            var rows = await _api.GetSalesAsync(from, to);
            grid.DataSource = rows;

            foreach (DataGridViewColumn col in grid.Columns)
            {
                if (col.Name == "Date") col.DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";
                if (col.Name is "Total" or "AmountPaid") col.DefaultCellStyle.Format = "N2";
                col.HeaderText = col.Name switch
                {
                    "Id" => "Receipt #",
                    "ItemCount" => "Items",
                    "AmountPaid" => "Paid",
                    "CashierName" => "Cashier",
                    _ => col.HeaderText
                };
            }

            lblSummary.Text = $"Sales: {rows.Count}   |   Total sales: ₱{rows.Sum(r => r.Total):N2}";
        }
        catch (Exception ex)
        {
            MessageBox.Show("Cannot load the sales.\n\n" + ex.Message, "Connection error");
        }
    }

    private async Task ShowReceipt(int id)
    {
        try
        {
            var sale = await _api.GetSaleAsync(id);
            if (sale == null) return;
            using var receipt = new ReceiptForm(sale);
            receipt.ShowDialog(this);
        }
        catch (Exception ex) { MessageBox.Show("Cannot load the receipt.\n\n" + ex.Message); }
    }
}