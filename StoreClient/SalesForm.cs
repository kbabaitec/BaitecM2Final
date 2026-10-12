using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace StoreClient;

public class SalesForm : Form
{
    private readonly ItemApiClient _itemApi = new();
    private readonly SalesApiClient _salesApi = new();

    private readonly DataGridView gridItems = new(), gridCart = new();
    private readonly TextBox txtSearch = new(), txtPaid = new();
    private readonly NumericUpDown numQty = new();
    private readonly Button btnSearch = new(), btnAddToCart = new();
    private readonly Button btnRemove = new(), btnClearCart = new(), btnCheckout = new();
    private readonly Label lblTotal = new(), lblChange = new();
    private readonly BindingList<CartLine> _cart = new();

    public SalesForm()
    {
        Text = "Sales";
        Size = new Size(1150, 650);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = UiHelper.Cream;
        Font = new Font("Segoe UI", 10);

        
        var left = new Panel { Dock = DockStyle.Left, Width = 520 };

        var searchPanel = new Panel { Dock = DockStyle.Top, Height = 45 };
        txtSearch.PlaceholderText = "Search items...";
        txtSearch.SetBounds(10, 9, 330, 28);
        UiHelper.StyleButton(btnSearch, "Search", UiHelper.Red, 350, 7, 90);
        btnSearch.Height = 30;
        searchPanel.Controls.AddRange(new Control[] { txtSearch, btnSearch });

        var addPanel = new Panel { Dock = DockStyle.Bottom, Height = 60 };
        addPanel.Controls.Add(new Label { Text = "Quantity", Left = 10, Top = 20, AutoSize = true });
        numQty.SetBounds(85, 16, 70, 28);
        numQty.Minimum = 1;
        numQty.Maximum = 1000;
        numQty.Value = 1;
        UiHelper.StyleButton(btnAddToCart, "Add to cart", Color.SeaGreen, 175, 12, 160);
        addPanel.Controls.AddRange(new Control[] { numQty, btnAddToCart });

        UiHelper.StyleGrid(gridItems);
        left.Controls.Add(gridItems);
        left.Controls.Add(addPanel);
        left.Controls.Add(searchPanel);

        
        var right = new Panel { Dock = DockStyle.Fill };

        var totalPanel = new Panel { Dock = DockStyle.Bottom, Height = 200 };
        totalPanel.Controls.Add(new Label { Text = "TOTAL", Left = 15, Top = 10, AutoSize = true });
        lblTotal.SetBounds(15, 32, 360, 50);
        lblTotal.Font = new Font("Segoe UI", 26, FontStyle.Bold);
        lblTotal.Text = "₱0.00";

        totalPanel.Controls.Add(new Label { Text = "Amount paid", Left = 15, Top = 95, AutoSize = true });
        txtPaid.SetBounds(15, 118, 180, 32);
        txtPaid.Font = new Font("Segoe UI", 14);

        totalPanel.Controls.Add(new Label { Text = "Change", Left = 215, Top = 95, AutoSize = true });
        lblChange.SetBounds(215, 118, 190, 32);
        lblChange.Font = new Font("Segoe UI", 14, FontStyle.Bold);
        lblChange.Text = "₱0.00";

        UiHelper.StyleButton(btnCheckout, "Checkout", Color.SeaGreen, 400, 15, 200);
        btnCheckout.Height = 60;
        btnCheckout.Font = new Font("Segoe UI", 14, FontStyle.Bold);
        UiHelper.StyleButton(btnRemove, "Remove line", Color.Firebrick, 400, 95, 95);
        UiHelper.StyleButton(btnClearCart, "Clear cart", Color.Gray, 505, 95, 95);

        totalPanel.Controls.AddRange(new Control[]
            { lblTotal, txtPaid, lblChange, btnCheckout, btnRemove, btnClearCart });

        UiHelper.StyleGrid(gridCart);
        gridCart.DataSource = _cart;
        right.Controls.Add(gridCart);
        right.Controls.Add(totalPanel);

        Controls.Add(right);
        Controls.Add(left);

        
        Load += async (_, _) => { FormatCart(); await LoadItems(); };
        btnSearch.Click += async (_, _) => await LoadItems(txtSearch.Text);
        txtSearch.KeyDown += async (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await LoadItems(txtSearch.Text);
            }
        };
        btnAddToCart.Click += (_, _) => AddToCart();
        gridItems.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) AddToCart(); };
        btnRemove.Click += (_, _) => RemoveLine();
        btnClearCart.Click += (_, _) => { _cart.Clear(); UpdateTotals(); };
        txtPaid.TextChanged += (_, _) => UpdateChange();
        btnCheckout.Click += async (_, _) => await Checkout();
    }

    private async Task LoadItems(string? search = null)
    {
        try
        {
            var items = await _itemApi.GetAllAsync(search);
            gridItems.DataSource = items;

            foreach (DataGridViewColumn col in gridItems.Columns)
            {
                col.Visible = col.Name is "Code" or "Name" or "UnitPrice" or "Quantity";
                col.HeaderText = col.Name switch
                {
                    "UnitPrice" => "Price",
                    "Quantity" => "In stock",
                    _ => col.HeaderText
                };
            }

            foreach (DataGridViewRow row in gridItems.Rows)
            {
                if (row.DataBoundItem is not Item item) continue;
                if (item.Quantity == 0) row.DefaultCellStyle.BackColor = Color.LightGray;
                else if (item.Quantity <= item.ReorderLevel)
                    row.DefaultCellStyle.BackColor = Color.MistyRose;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Cannot load items.\n\n" + ex.Message, "Connection error");
        }
    }

    private void FormatCart()
    {
        foreach (DataGridViewColumn col in gridCart.Columns)
        {
            col.HeaderText = col.Name switch
            {
                "ItemCode" => "Code",
                "ItemName" => "Item",
                "UnitPrice" => "Price",
                "LineTotal" => "Total",
                _ => col.HeaderText
            };
        }
    }

    private void AddToCart()
    {
        if (gridItems.CurrentRow?.DataBoundItem is not Item item)
        {
            MessageBox.Show("Select an item first.");
            return;
        }

        int qty = (int)numQty.Value;
        var line = _cart.FirstOrDefault(l => l.ItemCode == item.Code);
        int inCart = line?.Quantity ?? 0;

        if (inCart + qty > item.Quantity)
        {
            MessageBox.Show($"Not enough stock. {item.Name} has {item.Quantity} in stock"
                + (inCart > 0 ? $" ({inCart} already in the cart)." : "."));
            return;
        }

        if (line == null)
        {
            _cart.Add(new CartLine
            {
                ItemCode = item.Code,
                ItemName = item.Name,
                Quantity = qty,
                UnitPrice = item.UnitPrice
            });
        }
        else
        {
            line.Quantity += qty;
            _cart.ResetBindings();
        }

        numQty.Value = 1;
        UpdateTotals();
    }

    private void RemoveLine()
    {
        if (gridCart.CurrentRow?.DataBoundItem is CartLine line)
        {
            _cart.Remove(line);
            UpdateTotals();
        }
    }

    private void UpdateTotals()
    {
        lblTotal.Text = $"₱{_cart.Sum(l => l.LineTotal):N2}";
        UpdateChange();
    }

    private void UpdateChange()
    {
        var total = _cart.Sum(l => l.LineTotal);
        if (!decimal.TryParse(txtPaid.Text, out var paid))
        {
            lblChange.Text = "₱0.00";
            lblChange.ForeColor = Color.Black;
            return;
        }

        var change = paid - total;
        lblChange.Text = change >= 0 ? $"₱{change:N2}" : $"Short ₱{-change:N2}";
        lblChange.ForeColor = change >= 0 ? Color.SeaGreen : Color.Firebrick;
    }

    private async Task Checkout()
    {
        if (_cart.Count == 0)
        {
            MessageBox.Show("The cart is empty.");
            return;
        }

        var total = _cart.Sum(l => l.LineTotal);
        if (!decimal.TryParse(txtPaid.Text, out var paid) || paid < total)
        {
            MessageBox.Show($"Enter an amount paid of at least ₱{total:N2}.");
            return;
        }

        btnCheckout.Enabled = false;
        try
        {
            var (sale, error) = await _salesApi.CheckoutAsync(_cart, paid);
            if (sale is null)
            {
                MessageBox.Show(error ?? "Checkout failed.", "Error");
                return;
            }

            _cart.Clear();
            txtPaid.Clear();
            UpdateTotals();

            using (var receipt = new ReceiptForm(sale)) receipt.ShowDialog(this);
            await LoadItems(txtSearch.Text);   
        }
        catch (Exception ex)
        {
            MessageBox.Show("Cannot reach the API.\n\n" + ex.Message, "Connection error");
        }
        finally
        {
            btnCheckout.Enabled = true;
        }
    }
}