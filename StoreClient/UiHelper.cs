using System.Drawing;
using System.Windows.Forms;
namespace StoreClient;

public static class UiHelper
{
    public static readonly Color Cream = Color.FromArgb(255, 248, 235);
    public static readonly Color Red = Color.FromArgb(214, 69, 65);
    public static void StyleButton(Button b, string text, Color color, int left, int top, int width)
    {
        b.Text = text;
        b.SetBounds(left, top, width, 36);
        b.BackColor = color;
        b.ForeColor = Color.White;
        b.FlatStyle = FlatStyle.Flat;
    }
    public static void StyleGrid(DataGridView g)
    {
        g.Dock = DockStyle.Fill;
        g.ReadOnly = true;
        g.AllowUserToAddRows = false;
        g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        g.MultiSelect = false;
        g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        g.BackgroundColor = Color.White;
        g.RowHeadersVisible = false;
    }
}