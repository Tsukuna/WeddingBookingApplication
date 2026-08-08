using WeddingBookingApplication.Database.AppDbContextModels;
using WeddingBookingApplication.Domain.Features.Venue;
using WeddingBookingApplication.Domain.Features.Vendor;
using WeddingBookingApplication.Domain.Models.Venue;
using WeddingBookingApplication.WindowForm.Helpers;

namespace WeddingBookingApplication.WindowForm.Forms.Venues;

/// <summary>
/// Full CRUD panel for Venues. Includes a Vendor selector for the FK requirement.
///
/// Responsive changes:
/// • Header buttons use UITheme.AddHeaderButton (Anchor=Right|Top).
/// • Detail right-side now uses BuildDetailForm → scrollable Panel + TableLayoutPanel.
/// • Action buttons use BuildActionButtonRow (FlowLayoutPanel, Dock=Bottom).
/// </summary>
public class VenueForm : UserControl
{
    private readonly AppDbContext  _db         = DbContextFactory.Create();
    private VenueService           _service    = null!;
    private VendorService          _vendorSvc  = null!;

    private int? _selectedId;

    private DataGridView  _grid          = null!;
    private Label         _detailHeading = null!;

    private ComboBox      _cbVendor    = null!;
    private TextBox       _txtName     = null!;
    private TextBox       _txtLocation = null!;
    private NumericUpDown _nudCapacity = null!;
    private NumericUpDown _nudPrice    = null!;
    private TextBox       _txtDesc     = null!;
    private CheckBox      _chkActive   = null!;

    private Button _btnSave   = null!;
    private Button _btnDelete = null!;
    private Button _btnClear  = null!;
    private Label  _lblStatus = null!;

    public VenueForm()
    {
        _service   = new VenueService(_db);
        _vendorSvc = new VendorService(_db);
        BuildUI();
        LoadVendorCombo();
        LoadGrid();
    }

    private void BuildUI()
    {
        SuspendLayout();
        BackColor      = UITheme.Background;
        DoubleBuffered = true;

        // ── Header ──────────────────────────────────────────────────────
        var header     = UITheme.BuildPageHeader("🏛", "Venues", "Manage event venues");
        var btnAdd     = UITheme.AddHeaderButton(header, "+  Add New",  UITheme.Accent,       130);
        var btnRefresh = UITheme.AddHeaderButton(header, "↺  Refresh",  UITheme.SurfaceLight, 244);
        btnAdd.Click     += (_, _) => ClearDetail(isNew: true);
        btnRefresh.Click += (_, _) => LoadGrid();

        // ── Split ────────────────────────────────────────────────────────
        var split = UITheme.BuildSplit(out var left, out var right);

        // Grid (left panel)
        _grid = new DataGridView { Dock = DockStyle.Fill };
        UITheme.StyleDataGridView(_grid);
        _grid.Columns.AddRange(
            Col("VenueId",   "ID",       55,  false),
            Col("VendorId",  "Vendor ID", 75, false),
            Col("VenueName", "Name",      160),
            Col("Location",  "Location",  140),
            Col("Capacity",  "Capacity",  70,  false),
            Col("Price",     "Price",     90,  false),
            Col("IsActive",  "Active",    55,  false)
        );
        _grid.SelectionChanged += Grid_SelectionChanged;
        left.Controls.Add(_grid);

        // Detail (right panel) — scrollable + TableLayoutPanel
        var (scrollPanel, tbl) = UITheme.BuildDetailForm(right);

        _detailHeading = new Label
        {
            Text      = "Select a venue",
            Font      = UITheme.FontSubHeading,
            ForeColor = UITheme.AccentLight,
            AutoSize  = true,
            Dock      = DockStyle.Top,
            Padding   = new Padding(20, 16, 0, 8)
        };
        scrollPanel.Controls.Add(_detailHeading);

        _cbVendor = new ComboBox();
        UITheme.StyleComboBox(_cbVendor);

        _txtName     = TB();
        _txtLocation = TB();

        _nudCapacity = new NumericUpDown { Maximum = 10000, Height = 28 };
        UITheme.StyleNumericUpDown(_nudCapacity);

        _nudPrice = new NumericUpDown { Maximum = 999999999, DecimalPlaces = 2, Height = 28 };
        UITheme.StyleNumericUpDown(_nudPrice);

        _txtDesc = TB(multiLine: true);

        _chkActive = new CheckBox
        {
            Text      = "Active",
            Font      = UITheme.FontBody,
            ForeColor = UITheme.TextPrimary,
            BackColor = Color.Transparent,
            Checked   = true,
            Dock      = DockStyle.Fill,
            Height    = 28
        };

        UITheme.AddDetailRow(tbl, "Vendor *",      _cbVendor);
        UITheme.AddDetailRow(tbl, "Venue Name *",  _txtName);
        UITheme.AddDetailRow(tbl, "Location *",    _txtLocation);
        UITheme.AddDetailRow(tbl, "Capacity *",    _nudCapacity);
        UITheme.AddDetailRow(tbl, "Price (MMK) *", _nudPrice);
        UITheme.AddDetailRow(tbl, "Description",   _txtDesc);
        UITheme.AddDetailRow(tbl, "",              _chkActive);

        // Status label (bottom-docked, above button row)
        _lblStatus = new Label
        {
            AutoSize  = false,
            Dock      = DockStyle.Bottom,
            Height    = 28,
            Font      = UITheme.FontSmallBold,
            ForeColor = UITheme.TextPrimary,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding   = new Padding(20, 0, 0, 0)
        };

        _btnSave   = new Button { Text = "💾  Save",   Width = 110, Height = 36 };
        _btnClear  = new Button { Text = "✕  Clear",  Width = 100, Height = 36 };
        _btnDelete = new Button { Text = "🗑  Delete", Width = 100, Height = 36 };
        UITheme.StyleButton(_btnSave, UITheme.Accent);
        UITheme.StyleSecondaryButton(_btnClear);
        UITheme.StyleDangerButton(_btnDelete);
        _btnDelete.Enabled = false;
        _btnSave.Click   += BtnSave_Click;
        _btnClear.Click  += (_, _) => ClearDetail(isNew: false);
        _btnDelete.Click += BtnDelete_Click;

        UITheme.BuildActionButtonRow(scrollPanel, _btnSave, _btnClear, _btnDelete);
        scrollPanel.Controls.Add(_lblStatus);

        Controls.Add(split);
        Controls.Add(header);

        ResumeLayout(false);
        PerformLayout();
    }

    private static TextBox TB(bool multiLine = false)
    {
        var tb = new TextBox
        {
            Multiline  = multiLine,
            Height     = multiLine ? 64 : 28,
            ScrollBars = multiLine ? ScrollBars.Vertical : ScrollBars.None
        };
        UITheme.StyleTextBox(tb);
        return tb;
    }

    private void LoadVendorCombo()
    {
        _cbVendor.Items.Clear();
        var vendors = _vendorSvc.GetVendors();
        foreach (var v in vendors)
            _cbVendor.Items.Add(new ComboItem(v.VendorId, v.VendorName));
        if (_cbVendor.Items.Count > 0) _cbVendor.SelectedIndex = 0;
    }

    private void LoadGrid()
    {
        try
        {
            var venues = _service.GetVenues();
            _grid.DataSource = venues.Count > 0 ? venues : null;
        }
        catch (Exception ex) { SetStatus($"Load error: {ex.Message}", UITheme.Danger); }
    }

    private void Grid_SelectionChanged(object? sender, EventArgs e)
    {
        if (_grid.SelectedRows.Count == 0) return;
        var id = Convert.ToInt32(_grid.SelectedRows[0].Cells["VenueId"].Value);
        var v  = _service.GetVenue(id);
        if (v is null) return;

        _selectedId          = id;
        _detailHeading.Text  = "Edit Venue";
        SelectCombo(_cbVendor, v.VendorId);
        _txtName.Text        = v.VenueName;
        _txtLocation.Text    = v.Location;
        _nudCapacity.Value   = v.Capacity;
        _nudPrice.Value      = v.Price;
        _txtDesc.Text        = v.Description ?? "";
        _chkActive.Checked   = v.IsActive;
        _btnDelete.Enabled   = true;
        SetStatus("", Color.Transparent);
    }

    private void ClearDetail(bool isNew)
    {
        _selectedId          = null;
        _detailHeading.Text  = isNew ? "New Venue" : "Select a venue";
        if (_cbVendor.Items.Count > 0) _cbVendor.SelectedIndex = 0;
        _txtName.Text      = ""; _txtLocation.Text = "";
        _nudCapacity.Value = 0;  _nudPrice.Value   = 0;
        _txtDesc.Text      = ""; _chkActive.Checked = true;
        _btnDelete.Enabled = false;
        SetStatus("", Color.Transparent);
        _grid.ClearSelection();
        if (isNew) _txtName.Focus();
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (_cbVendor.SelectedItem is not ComboItem vendor ||
            string.IsNullOrWhiteSpace(_txtName.Text) ||
            string.IsNullOrWhiteSpace(_txtLocation.Text))
        {
            SetStatus("Vendor, Name and Location are required.", UITheme.Danger);
            return;
        }

        if (_selectedId.HasValue)
        {
            var req = new VenueUpdateRequestModel
            {
                VenueName   = _txtName.Text.Trim(),
                Location    = _txtLocation.Text.Trim(),
                Capacity    = (int)_nudCapacity.Value,
                Price       = _nudPrice.Value,
                Description = _txtDesc.Text.Trim(),
                IsActive    = _chkActive.Checked
            };
            var res = _service.UpdateVenue(_selectedId.Value, req);
            SetStatus(res.IsSuccess ? "✔  Updated." : $"✘  {res.Message}",
                      res.IsSuccess ? UITheme.Success : UITheme.Danger);
        }
        else
        {
            var req = new VenueCreateRequestModel
            {
                VendorId    = vendor.Id,
                VenueName   = _txtName.Text.Trim(),
                Location    = _txtLocation.Text.Trim(),
                Capacity    = (int)_nudCapacity.Value,
                Price       = _nudPrice.Value,
                Description = _txtDesc.Text.Trim(),
                IsActive    = _chkActive.Checked
            };
            var res = _service.CreateVenue(req);
            SetStatus(res.IsSuccess ? $"✔  Venue #{res.VenueId} created." : $"✘  {res.Message}",
                      res.IsSuccess ? UITheme.Success : UITheme.Danger);
            if (res.IsSuccess) ClearDetail(isNew: false);
        }
        LoadGrid();
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (!_selectedId.HasValue) return;
        if (MessageBox.Show("Deactivate this venue?", "Confirm",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        var res = _service.DeleteVenue(_selectedId.Value);
        SetStatus(res.IsSuccess ? "✔  Deactivated." : $"✘  {res.Message}",
                  res.IsSuccess ? UITheme.Success : UITheme.Danger);
        ClearDetail(isNew: false); LoadGrid();
    }

    private static void SelectCombo(ComboBox cb, int id)
    {
        for (int i = 0; i < cb.Items.Count; i++)
            if (cb.Items[i] is ComboItem item && item.Id == id) { cb.SelectedIndex = i; return; }
    }

    private void SetStatus(string msg, Color col)
    {
        _lblStatus.Text      = msg;
        _lblStatus.ForeColor = col == Color.Transparent ? UITheme.TextPrimary : col;
    }

    private static DataGridViewTextBoxColumn Col(string prop, string hdr, int w = 120, bool fill = true)
    {
        var c = new DataGridViewTextBoxColumn { Name = prop, DataPropertyName = prop, HeaderText = hdr, Width = w };
        if (fill) c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        return c;
    }

    protected override void Dispose(bool disposing) { if (disposing) _db.Dispose(); base.Dispose(disposing); }

    private record ComboItem(int Id, string Name) { public override string ToString() => Name; }
}
