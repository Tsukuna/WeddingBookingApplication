using WeddingBookingApplication.Database.AppDbContextModels;
using WeddingBookingApplication.Domain.Features.Vendor;
using WeddingBookingApplication.Domain.Models.Vendor;
using WeddingBookingApplication.WindowForm.Helpers;

namespace WeddingBookingApplication.WindowForm.Forms.Vendors;


public class VendorForm : UserControl
{
    private readonly AppDbContext   _db      = DbContextFactory.Create();
    private VendorService           _service = null!;

    private int? _selectedId;

    private DataGridView _grid          = null!;
    private Label        _detailHeading = null!;

    private TextBox  _txtName    = null!;
    private TextBox  _txtEmail   = null!;
    private TextBox  _txtPhone   = null!;
    private TextBox  _txtAddress = null!;
    private TextBox  _txtDesc    = null!;
    private ComboBox _cbStatus   = null!;

    private Button _btnSave   = null!;
    private Button _btnDelete = null!;
    private Button _btnClear  = null!;
    private Label  _lblStatus = null!;

    public VendorForm()
    {
        _service = new VendorService(_db);
        BuildUI();
        LoadGrid();
    }

   
    private void BuildUI()
    {
        SuspendLayout();
        BackColor      = UITheme.Background;
        DoubleBuffered = true;

        var header  = UITheme.BuildPageHeader("🏢", "Vendors", "Manage wedding vendors");
        var btnAdd     = UITheme.AddHeaderButton(header, "+  Add New",   UITheme.Accent,       130);
        var btnRefresh = UITheme.AddHeaderButton(header, "↺  Refresh",   UITheme.SurfaceLight, 244);
        btnAdd.Click     += (_, _) => ClearDetail(isNew: true);
        btnRefresh.Click += (_, _) => LoadGrid();

        var split = UITheme.BuildSplit(out var gridPanel, out var detailHost,
            panel1MinSize: 400, panel2MinSize: 300);

        _grid = new DataGridView { Dock = DockStyle.Fill };
        UITheme.StyleDataGridView(_grid);
        _grid.AllowUserToResizeRows = false;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
        _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        _grid.DefaultCellStyle.WrapMode = DataGridViewTriState.False; 
        _grid.Columns.AddRange(
        Col("VendorId", "ID", 55, DataGridViewAutoSizeColumnMode.None),
        Col("VendorName", "Vendor Name", 160, DataGridViewAutoSizeColumnMode.None),
        Col("Phone", "Phone", 115, DataGridViewAutoSizeColumnMode.None),
        Col("Email", "Email", 180, DataGridViewAutoSizeColumnMode.None),
        Col("Address", "Address", 140, DataGridViewAutoSizeColumnMode.None),
        Col("StatusName", "Status", 90, DataGridViewAutoSizeColumnMode.Fill)  // only this one fills
        );
        _grid.SelectionChanged += Grid_SelectionChanged;
        gridPanel.Controls.Add(_grid);

      
        var (scrollPanel, tbl) = UITheme.BuildDetailForm(detailHost);

        _detailHeading = new Label
        {
            Text      = "Select a vendor",
            Font      = UITheme.FontSubHeading,
            ForeColor = UITheme.AccentLight,
            AutoSize  = true,
            Dock      = DockStyle.Top,
            Padding   = new Padding(20, 16, 0, 8)
        };
        scrollPanel.Controls.Add(_detailHeading);   // sits above the TableLayoutPanel

        _txtName    = MakeTextBox();
        _txtEmail   = MakeTextBox();
        _txtPhone   = MakeTextBox();
        _txtAddress = MakeTextBox();
        _txtDesc    = MakeTextBox(multiLine: true);
        _cbStatus   = MakeComboBox();
        _cbStatus.Items.AddRange(["Active (1)", "Inactive (0)"]);
        _cbStatus.SelectedIndex = 0;

        UITheme.AddDetailRow(tbl, "Vendor Name *",  _txtName);
        UITheme.AddDetailRow(tbl, "Email *",        _txtEmail);
        UITheme.AddDetailRow(tbl, "Phone *",        _txtPhone);
        UITheme.AddDetailRow(tbl, "Address",        _txtAddress);
        UITheme.AddDetailRow(tbl, "Description",    _txtDesc);
        UITheme.AddDetailRow(tbl, "Status *",       _cbStatus);

        _lblStatus = new Label
        {
            AutoSize  = false,
            Dock      = DockStyle.Top,
            Height    = 32,
            Font      = UITheme.FontSmallBold,
            ForeColor = UITheme.TextPrimary,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding   = new Padding(20, 0, 0, 0)
        };


        _btnSave   = new Button { Text = "💾  Save",    Width = 110, Height = 36 };
        _btnClear  = new Button { Text = "✕  Clear",   Width = 100, Height = 36 };
        _btnDelete = new Button { Text = "🗑  Delete",  Width = 100, Height = 36 };
        UITheme.StyleButton(_btnSave);
        UITheme.StyleSecondaryButton(_btnClear);
        UITheme.StyleDangerButton(_btnDelete);
        _btnDelete.Enabled = false;
        _btnSave.Click   += BtnSave_Click;
        _btnClear.Click  += (_, _) => ClearDetail(isNew: false);
        _btnDelete.Click += BtnDelete_Click;

        UITheme.BuildActionButtonRow(scrollPanel, _btnSave, _btnClear, _btnDelete);


        _lblStatus.Dock = DockStyle.Bottom;
        scrollPanel.Controls.Add(_lblStatus);

        Controls.Add(split);
        Controls.Add(header);

        ResumeLayout(false);
        PerformLayout();
    }

    private static TextBox MakeTextBox(bool multiLine = false)
    {
        var tb = new TextBox
        {
            Multiline  = multiLine,
            Height     = multiLine ? 68 : 28,
            ScrollBars = multiLine ? ScrollBars.Vertical : ScrollBars.None
        };
        UITheme.StyleTextBox(tb);
        return tb;
    }

    private static ComboBox MakeComboBox()
    {
        var cb = new ComboBox();
        UITheme.StyleComboBox(cb);
        return cb;
    }


    private static DataGridViewTextBoxColumn Col(
     string prop,
     string header,
     int width,
     DataGridViewAutoSizeColumnMode autoSize = DataGridViewAutoSizeColumnMode.None)
    {
        return new DataGridViewTextBoxColumn
        {
            Name = prop,
            DataPropertyName = prop,
            HeaderText = header,
            Width = width,
            AutoSizeMode = autoSize,
            MinimumWidth = 40          
        };
    }


    private void LoadGrid()
    {
        try
        {
            var vendors = _service.GetVendors();
            var rows = vendors.Select(v => new
            {
                v.VendorId,
                v.VendorName,
                v.Phone,
                v.Email,
                Address    = v.Address ?? "—",
                StatusName = v.Status != 0 ? "Active" : "Inactive"
            }).ToList();

            _grid.DataSource = rows.Count > 0 ? rows : null;
            SetStatus("", Color.Transparent);
        }
        catch (Exception ex)
        {
            SetStatus($"Load error: {ex.Message}", UITheme.Danger);
        }
    }

    private void Grid_SelectionChanged(object? sender, EventArgs e)
    {
        if (_grid.SelectedRows.Count == 0) return;
        var row   = _grid.SelectedRows[0];
        var idVal = row.Cells["VendorId"].Value;
        if (idVal is null) return;

        int id     = Convert.ToInt32(idVal);
        var vendor = _service.GetVendor(id);
        if (vendor is null) return;

        _selectedId             = id;
        _detailHeading.Text     = "Edit Vendor";
        _txtName.Text           = vendor.VendorName;
        _txtEmail.Text          = vendor.Email;
        _txtPhone.Text          = vendor.Phone;
        _txtAddress.Text        = vendor.Address ?? "";
        _txtDesc.Text           = vendor.Description ?? "";
        _cbStatus.SelectedIndex = vendor.Status != 0 ? 0 : 1;
        _btnDelete.Enabled      = true;
        SetStatus("", Color.Transparent);
    }

    private void ClearDetail(bool isNew)
    {
        _selectedId             = null;
        _detailHeading.Text     = isNew ? "New Vendor" : "Select a vendor";
        _txtName.Text           = "";
        _txtEmail.Text          = "";
        _txtPhone.Text          = "";
        _txtAddress.Text        = "";
        _txtDesc.Text           = "";
        _cbStatus.SelectedIndex = 0;
        _btnDelete.Enabled      = false;
        SetStatus("", Color.Transparent);
        _grid.ClearSelection();
        if (isNew) _txtName.Focus();
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_txtName.Text) ||
            string.IsNullOrWhiteSpace(_txtEmail.Text) ||
            string.IsNullOrWhiteSpace(_txtPhone.Text))
        {
            SetStatus("Name, Email and Phone are required.", UITheme.Danger);
            return;
        }

        byte status = (byte)(_cbStatus.SelectedIndex == 0 ? 1 : 0);

        if (_selectedId.HasValue)
        {
            var req = new VendorUpdateRequestModel
            {
                VendorName  = _txtName.Text.Trim(),
                Email       = _txtEmail.Text.Trim(),
                Phone       = _txtPhone.Text.Trim(),
                Address     = _txtAddress.Text.Trim(),
                Description = _txtDesc.Text.Trim(),
                Status      = status
            };
            var res = _service.UpdateVendor(_selectedId.Value, req);
            SetStatus(res.IsSuccess ? "✔  Updated successfully." : $"✘  {res.Message}",
                      res.IsSuccess ? UITheme.Success : UITheme.Danger);
        }
        else
        {
            var req = new VendorCreateRequestModel
            {
                VendorName  = _txtName.Text.Trim(),
                Email       = _txtEmail.Text.Trim(),
                Phone       = _txtPhone.Text.Trim(),
                Address     = _txtAddress.Text.Trim(),
                Description = _txtDesc.Text.Trim(),
                Status      = status
            };
            var res = _service.CreateVendor(req);
            SetStatus(res.IsSuccess ? $"✔  Vendor #{res.VendorId} created." : $"✘  {res.Message}",
                      res.IsSuccess ? UITheme.Success : UITheme.Danger);
            if (res.IsSuccess) ClearDetail(isNew: false);
        }

        LoadGrid();
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (!_selectedId.HasValue) return;
        var confirm = MessageBox.Show(
            "Soft-delete this vendor? (Status → Inactive)",
            "Confirm Delete",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes) return;

        var res = _service.DeleteVendor(_selectedId.Value);
        SetStatus(res.IsSuccess ? "✔  Vendor deactivated." : $"✘  {res.Message}",
                  res.IsSuccess ? UITheme.Success : UITheme.Danger);
        ClearDetail(isNew: false);
        LoadGrid();
    }

    private void SetStatus(string msg, Color bg)
    {
        _lblStatus.Text      = msg;
        _lblStatus.BackColor = Color.Transparent;
        _lblStatus.ForeColor = bg == Color.Transparent ? UITheme.TextPrimary : bg;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _db.Dispose();
        base.Dispose(disposing);
    }
}
