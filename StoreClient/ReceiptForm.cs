using System.Drawing;
using System.Text;
using System.Windows.Forms;
namespace StoreClient;

public class ReceiptForm : Form
{
    public ReceiptForm(SaleDetailInfo sale)
    {
        Text = $"Receipt #{sale.Id}";
        ClientSize = new Size(420, 540);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.White;
        var txt = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 10),
            BackColor = Color.White,
            BorderStyle = BorderStyle.None,
            ScrollBars = ScrollBars.Vertical,
            Text = BuildText(sale)
        };
        var btnClose = new Button { Dock = DockStyle.Bottom, Height = 40 };
        UiHelper.StyleButton(btnClose, "Close", UiHelper.Red, 0, 0, 0);
        btnClose.Dock = DockStyle.Bottom;
        btnClose.Height = 40;
        btnClose.Click += (_, _) => Close();
        Controls.Add(txt);
        Controls.Add(btnClose);
        txt.SelectionStart = 0;
        txt.SelectionLength = 0;
    }
    public static string BuildText(SaleDetailInfo s)
    {
        const int w = 40;
        string Center(string t) => t.PadLeft((w + t.Length) / 2);
        string Row(string label, decimal v) =>
            label.PadRight(w - 14) + ("₱" + v.ToString("N2")).PadLeft(14);
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine(Center("SARI-SARI STORE"));
        sb.AppendLine(Center($"Receipt #{s.Id}"));
        sb.AppendLine();
        sb.AppendLine($"Date:    {s.Date:yyyy-MM-dd HH:mm}");
        sb.AppendLine($"Cashier: {s.CashierName}");
        sb.AppendLine(new string('-', w));
        foreach (var l in s.Lines)
        {
            sb.AppendLine(l.ItemName);
            sb.AppendLine($"  {l.Quantity} x {l.UnitPrice:N2}".PadRight(w - 12)
                          + l.LineTotal.ToString("N2").PadLeft(12));
        }
        sb.AppendLine(new string('-', w));
        sb.AppendLine(Row("TOTAL", s.Total));
        sb.AppendLine(Row("PAID", s.AmountPaid));
        sb.AppendLine(Row("CHANGE", s.Change));
        sb.AppendLine();
        sb.AppendLine(Center("Thank you! Come again."));
        return sb.ToString();
    }
}