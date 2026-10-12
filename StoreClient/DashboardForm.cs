using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace StoreClient;

public class DashboardForm : Form
{
    private readonly DashboardApiClient _api = new();
    private readonly DataGridView gridLow = new(), gridTop = new();
    private readonly Panel chart = new();
    private readonly Label leftTitle = new();
    private readonly Button btnRefresh = new();

    private readonly Label lblToday = new(), lblTodayNote = new();
    private readonly Label lblMonth = new(), lblMonthNote = new();
    private readonly Label lblValue = new(), lblValueNote = new();
    private readonly Label lblItems = new(), lblItemsNote = new();
    private readonly Label lblLow = new(), lblLowNote = new();

    private List<DayTotalInfo> _days = new();

    public DashboardForm()
    {
        Text = "Dashboard";
        Size = new Size(1150, 700);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = UiHelper.Cream;
        Font = new Font("Segoe UI", 10);

        
        var kpiPanel = new Panel { Dock = DockStyle.Top, Height = 125 };

        void AddCard(string caption, Label value, Label note, Color color, int index)
        {
            var card = new Panel
            {
                Left = 10 + index * 207,
                Top = 10,
                Width = 200,
                Height = 100,
                BackColor = Color.White
            };
            card.Controls.Add(new Panel { Dock = DockStyle.Left, Width = 6, BackColor = color });
            card.Controls.Add(new Label
            { Text = caption, Left = 16, Top = 8, AutoSize = true, ForeColor = Color.Gray });

            value.SetBounds(16, 30, 182, 36);
            value.Font = new Font("Segoe UI", 15, FontStyle.Bold);
            value.ForeColor = color;
            note.SetBounds(16, 70, 182, 22);
            note.ForeColor = Color.Gray;

            card.Controls.Add(value);
            card.Controls.Add(note);
            kpiPanel.Controls.Add(card);
        }

        AddCard("Today's sales", lblToday, lblTodayNote, Color.SeaGreen, 0);
        AddCard("This month", lblMonth, lblMonthNote, Color.SteelBlue, 1);
        AddCard("Inventory value", lblValue, lblValueNote, Color.DarkOrange, 2);
        AddCard("Items", lblItems, lblItemsNote, Color.SlateGray, 3);
        AddCard("Low stock", lblLow, lblLowNote, Color.Firebrick, 4);

        UiHelper.StyleButton(btnRefresh, "Refresh", UiHelper.Red, 1048, 10, 80);
        kpiPanel.Controls.Add(btnRefresh);

        
        Label Title(string text) => new()
        {
            Text = text,
            Dock = DockStyle.Top,
            Height = 34,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            TextAlign = ContentAlignment.BottomLeft,
            Padding = new Padding(10, 0, 0, 4)
        };

        var left = new Panel { Dock = DockStyle.Left, Width = 560 };
        UiHelper.StyleGrid(gridLow);
        leftTitle.Dock = DockStyle.Top;
        leftTitle.Height = 34;
        leftTitle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        leftTitle.TextAlign = ContentAlignment.BottomLeft;
        leftTitle.Padding = new Padding(10, 0, 0, 4);
        left.Controls.Add(gridLow);
        left.Controls.Add(leftTitle);

        
        var right = new Panel { Dock = DockStyle.Fill };
        UiHelper.StyleGrid(gridTop);
        chart.Dock = DockStyle.Top;
        chart.Height = 240;
        chart.BackColor = Color.White;

        right.Controls.Add(gridTop);
        right.Controls.Add(Title("Top sellers - last 30 days"));
        right.Controls.Add(chart);
        right.Controls.Add(Title("Sales - last 7 days"));

        var body = new Panel { Dock = DockStyle.Fill };
        body.Controls.Add(right);
        body.Controls.Add(left);

        Controls.Add(body);
        Controls.Add(kpiPanel);

        
        Load += async (_, _) => await LoadData();
        btnRefresh.Click += async (_, _) => await LoadData();
        chart.Paint += (_, e) => DrawChart(e.Graphics, chart.ClientRectangle);
    }

    private async Task LoadData()
    {
        try
        {
            var d = await _api.GetAsync();
            if (d is null) return;

            lblToday.Text = $"₱{d.TodaySales:N2}";
            lblTodayNote.Text = $"{d.TodaySaleCount} sale(s) today";
            lblMonth.Text = $"₱{d.MonthSales:N2}";
            lblMonthNote.Text = "Month to date";
            lblValue.Text = $"₱{d.InventoryValue:N2}";
            lblValueNote.Text = "Stock at selling price";
            lblItems.Text = $"{d.TotalItems} items";
            lblItemsNote.Text = $"{d.TotalUnits} units in stock";
            lblLow.Text = d.LowStockCount.ToString();
            lblLowNote.Text = $"{d.OutOfStockCount} out of stock";

            leftTitle.Text = d.LowStock.Count == 0
                ? "Low stock - everything is well stocked"
                : "Low stock - needs restocking";

            gridLow.DataSource = d.LowStock;
            foreach (DataGridViewColumn col in gridLow.Columns)
            {
                col.HeaderText = col.Name switch
                {
                    "Quantity" => "In stock",
                    "ReorderLevel" => "Reorder at",
                    "Suggested" => "Suggested order",
                    _ => col.HeaderText
                };
            }
            foreach (DataGridViewRow row in gridLow.Rows)
            {
                if (row.DataBoundItem is LowStockInfo item)
                    row.DefaultCellStyle.BackColor =
                        item.Quantity == 0 ? Color.FromArgb(255, 205, 205) : Color.MistyRose;
            }

            gridTop.DataSource = d.TopSellers;
            foreach (DataGridViewColumn col in gridTop.Columns)
                if (col.Name == "QuantitySold") col.HeaderText = "Sold";

            _days = d.Last7Days;
            chart.Invalidate();   
        }
        catch (Exception ex)
        {
            MessageBox.Show("Cannot load the dashboard.\n\n" + ex.Message, "Connection error");
        }
    }

    
    private void DrawChart(Graphics g, Rectangle area)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.White);
        if (_days.Count == 0) return;

        var max = Math.Max(_days.Max(d => d.Total), 1m);
        int left = 20, top = 30, bottom = area.Height - 35;
        int chartHeight = bottom - top;
        int slot = (area.Width - left - 20) / _days.Count;
        int barWidth = slot * 6 / 10;

        using var font = new Font("Segoe UI", 9);
        using var barBrush = new SolidBrush(UiHelper.Red);
        using var axisPen = new Pen(Color.LightGray);

        g.DrawLine(axisPen, left, bottom, area.Width - 10, bottom);

        for (int i = 0; i < _days.Count; i++)
        {
            var day = _days[i];
            int barHeight = (int)(chartHeight * (double)(day.Total / max));
            int x = left + i * slot + (slot - barWidth) / 2;

            g.FillRectangle(barBrush, x, bottom - barHeight, barWidth, barHeight);

            var dateText = day.Date.ToString("MMM d");
            var dateSize = g.MeasureString(dateText, font);
            g.DrawString(dateText, font, Brushes.Black, x + barWidth / 2f - dateSize.Width / 2, bottom + 6);

            var valueText = day.Total.ToString("N0");
            var valueSize = g.MeasureString(valueText, font);
            g.DrawString(valueText, font, Brushes.DimGray,
                         x + barWidth / 2f - valueSize.Width / 2, bottom - barHeight - 18);
        }
    }
}