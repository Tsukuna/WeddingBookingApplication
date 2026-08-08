using WeddingBookingApplication.Database.AppDbContextModels;
using WeddingBookingApplication.Domain.Features.ServicePackage;
using WeddingBookingApplication.Domain.Features.Vendor;
using WeddingBookingApplication.Domain.Models.Service;
using WeddingBookingApplication.WindowForm.Helpers;

namespace WeddingBookingApplication.WindowForm.Forms.ServicePackages;

/// <summary>
/// Full CRUD panel for Service Packages.
///
/// Responsive changes:
/// • Header buttons anchored Right|Top via UITheme.AddHeaderButton.
/// • Detail panel uses BuildDetailForm (scroll + TableLayoutPanel).
/// • Action buttons in BuildActionButtonRow (FlowLayoutPanel, Dock=Bottom).
/// </summary>
public class ServicePackageForm : UserControl
{
    private readonly AppDbContext  _db        = DbContextFactory.Create();
    private ServicePackageService  _service   = null!;
    private VendorService          _vendorSvc = null!;

    private int? _selectedId;

    private DataGridView  _grid          = null!;
    private Label         _detailHeading = null!;
    private ComboBox      _cbVendor      = null!;
    private TextBox       _txtName       = null!;
    private NumericUpDown _nudPrice      = null!;
    private TextBox       _txtDesc       = null!;
    private CheckBox      _chkActive     = null!;
    private Button        _btnSave       = null!;
    private Button        _btnDelete     = null!;
    private Button        _btnClear      = null!;
    private Label         _lblStatus     = null!;

    public ServicePackageForm()
    {
        _service   = new ServicePackageService(_db);
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
        var header = UITheme.BuildPageHeader("🎵", "Service Packages", "Manage service packages for bookings");
        var btnAdd = UITheme.AddHeaderButton(header, "+  Add New",  UITheme.Accent,       130);
        var btnRef = UITheme.AddHeaderButton(header, "↺  Refresh",  UITheme.SurfaceLight, 244);
        btnAdd.Click += (_, _) => ClearDetail(true);
        btnRef.Click += (_, _) => LoadGrid();

        // ── Split ────────────────────────────────────────────────────────
        var split = UITheme.BuildSplit(out var gridPanel, out var detailHost);

        // Grid
        _grid = new DataGridView { Dock = DockStyle.Fill };
        UITheme.StyleDataGridView(_grid);
        _grid.Columns.AddRange(
            TxtCol("ServicePackageId", "ID",          55,  false),
            TxtCol("VendorId",         "Vendor ID",   80,  false),
            TxtCol("PackageName",      "Service",     180),
            TxtCol("Price",            "Price",       100, false),
            TxtCol("Description",      "Description", 200),
            TxtCol("IsActive",         "Active",      55,  false)
        );
        _grid.SelectionChanged += Grid_SelectionChanged;
        gridPanel.Controls.Add(_grid);

        // Detail — scrollable + TableLayoutPanel
        var (scrollPanel, tbl) = UITheme.BuildDetailForm(detailHost);

        _detailHeading = new Label
        {
            Text      = "Select a service",
            Font      = UITheme.FontSubHeading,
            ForeColor = UITheme.AccentLight,
            AutoSize  = true,
            Dock      = DockStyle.Top,
            Padding   = new Padding(20, 16, 0, 8)
        };
        scrollPanel.Controls.Add(_detailHeading);

        _cbVendor = new ComboBox();
        UITheme.StyleComboBox(_cbVendor);

        _txtName = new TextBox { Height = 28 };
        UITheme.StyleTextBox(_txtName);

        _nudPrice = new NumericUpDown { Maximum = 99999999, DecimalPlaces = 2, Height = 28 };
        UITheme.StyleNumericUpDown(_nudPrice);

        _txtDesc = new TextBox { Height = 68, Multiline = true, ScrollBars = ScrollBars.Vertical };
        UITheme.StyleTextBox(_txtDesc);

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

        UITheme.AddDetailRow(tbl, "Vendor *",         _cbVendor);
        UITheme.AddDetailRow(tbl, "Service Name *",   _txtName);
        UITheme.AddDetailRow(tbl, "Price (MMK) *",    _nudPrice);
        UITheme.AddDetailRow(tbl, "Description",      _txtDesc);
        UITheme.AddDetailRow(tbl, "",                 _chkActive);

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
        _btnClear.Click  += (_, _) => ClearDetail(false);
        _btnDelete.Click += BtnDelete_Click;

        UITheme.BuildActionButtonRow(scrollPanel, _btnSave, _btnClear, _btnDelete);
        scrollPanel.Controls.Add(_lblStatus);

        Controls.Add(split);
        Controls.Add(header);

        ResumeLayout(false);
        PerformLayout();
    }

    private void LoadVendorCombo()
    {
        _cbVendor.Items.Clear();
        foreach (var v in _vendorSvc.GetVendors())
            _cbVendor.Items.Add(new ComboItem(v.VendorId, v.VendorName));
        if (_cbVendor.Items.Count > 0) _cbVendor.SelectedIndex = 0;
    }

    private void LoadGrid()
    {
        try
        {
            var pkgs = _service.GetServicePackages();
            _grid.DataSource = pkgs.Count > 0 ? pkgs : null;
        }
        catch (Exception ex) { SetStatus($"Load error: {ex.Message}", UITheme.Danger); }
    }

    private void Grid_SelectionChanged(object? sender, EventArgs e)
    {
        if (_grid.SelectedRows.Count == 0) return;
        var id  = Convert.ToInt32(_grid.SelectedRows[0].Cells["ServicePackageId"].Value);
        var pkg = _service.GetServicePackage(id);
        if (pkg is null) return;

        _selectedId         = id;
        _detailHeading.Text = "Edit Service";
        SelectCombo(_cbVendor, pkg.VendorId);
        _txtName.Text      = pkg.PackageName;
        _nudPrice.Value    = pkg.Price;
        _txtDesc.Text      = pkg.Description ?? "";
        _chkActive.Checked = pkg.IsActive;
        _btnDelete.Enabled = true;
        SetStatus("", Color.Transparent);
    }

    private void ClearDetail(bool isNew)
    {
        _selectedId         = null;
        _detailHeading.Text = isNew ? "New Service" : "Select a service";
        if (_cbVendor.Items.Count > 0) _cbVendor.SelectedIndex = 0;
        _txtName.Text = ""; _nudPrice.Value = 0; _txtDesc.Text = "";
        _chkActive.Checked = true; _btnDelete.Enabled = false;
        SetStatus("", Color.Transparent);
        _grid.ClearSelection();
        if (isNew) _txtName.Focus();
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (_cbVendor.SelectedItem is not ComboItem vendor || string.IsNullOrWhiteSpace(_txtName.Text))
        { SetStatus("Vendor and Service Name are required.", UITheme.Danger); return; }

        if (_selectedId.HasValue)
        {
            var res = _service.UpdateServicePackage(_selectedId.Value, new ServiceUpdateRequestModel
            {
                VendorId = vendor.Id, PackageName = _txtName.Text.Trim(),
                Price = _nudPrice.Value, Description = _txtDesc.Text.Trim(), IsActive = _chkActive.Checked
            });
            SetStatus(res.IsSuccess ? "✔  Updated." : $"✘  {res.Message}", res.IsSuccess ? UITheme.Success : UITheme.Danger);
        }
        else
        {
            var res = _service.CreateServicePackage(new ServiceCreateRequestModel
            {
                VendorId = vendor.Id, PackageName = _txtName.Text.Trim(),
                Price = _nudPrice.Value, Description = _txtDesc.Text.Trim(), IsActive = _chkActive.Checked
            });
            SetStatus(res.IsSuccess ? $"✔  Service #{res.ServicePackageId} created." : $"✘  {res.Message}",
                      res.IsSuccess ? UITheme.Success : UITheme.Danger);
            if (res.IsSuccess) ClearDetail(false);
        }
        LoadGrid();
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (!_selectedId.HasValue) return;
        if (MessageBox.Show("Deactivate this service package?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        var res = _service.DeleteServicePackage(_selectedId.Value);
        SetStatus(res.IsSuccess ? "✔  Deactivated." : $"✘  {res.Message}", res.IsSuccess ? UITheme.Success : UITheme.Danger);
        ClearDetail(false); LoadGrid();
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

    private static DataGridViewTextBoxColumn TxtCol(string prop, string hdr, int w = 120, bool fill = true)
    {
        var c = new DataGridViewTextBoxColumn { Name = prop, DataPropertyName = prop, HeaderText = hdr, Width = w };
        if (fill) c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        return c;
    }

    protected override void Dispose(bool disposing) { if (disposing) _db.Dispose(); base.Dispose(disposing); }

    private record ComboItem(int Id, string Name) { public override string ToString() => Name; }
}
