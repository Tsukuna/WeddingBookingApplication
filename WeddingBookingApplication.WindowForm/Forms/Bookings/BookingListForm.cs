using WeddingBookingApplication.Database.AppDbContextModels;
using WeddingBookingApplication.Domain.Features.Booking;
using WeddingBookingApplication.Domain.Models.Booking;
using WeddingBookingApplication.WindowForm.Helpers;

namespace WeddingBookingApplication.WindowForm.Forms.Bookings;

/// <summary>
/// Booking management panel: search by phone/email, view all bookings,
/// approve / reject / cancel / complete from the grid, and create new bookings.
///
/// Responsive changes:
/// • Header buttons use UITheme.AddHeaderButton (Anchor=Right|Top).
/// • Search bar controls replaced by a FlowLayoutPanel — no more absolute Left coords.
/// • Action toolbar buttons placed in a FlowLayoutPanel (Dock=Left inside actionBar).
/// • Booking detail info panel uses UITheme.BuildDetailInfoTable (3-col TableLayoutPanel)
///   so all three columns share equal width and scale with the panel.
/// • Status label (search results count) anchored Right inside the search bar.
/// </summary>
public class BookingListForm : UserControl
{
    private readonly AppDbContext _db      = DbContextFactory.Create();
    private BookingService        _service = null!;

    // ── Controls ─────────────────────────────────────────────────────────────
    private DataGridView _grid        = null!;
    private TextBox      _txtPhone    = null!;
    private TextBox      _txtEmail    = null!;
    private Label        _lblStatus   = null!;
    private Button       _btnApprove  = null!;
    private Button       _btnReject   = null!;
    private Button       _btnCancel   = null!;
    private Button       _btnComplete = null!;

    // Detail expansion panel
    private Panel _detailPanel   = null!;
    private bool  _detailVisible = false;

    // Labels inside detail panel
    private Label _dCustomer    = null!;
    private Label _dVendor      = null!;
    private Label _dVenue       = null!;
    private Label _dDate        = null!;
    private Label _dGuests      = null!;
    private Label _dTotal       = null!;
    private Label _dStatus      = null!;
    private Label _dDecorations = null!;
    private Label _dServices    = null!;

    private int? _selectedBookingId;

    public BookingListForm()
    {
        _service = new BookingService(_db);
        BuildUI();
        LoadGrid();
    }

    // ────────────────────────────────────────────────────────────────────────
    // Build UI
    // ────────────────────────────────────────────────────────────────────────
    private void BuildUI()
    {
        SuspendLayout();
        BackColor      = UITheme.Background;
        DoubleBuffered = true;

        // ── Page Header ──────────────────────────────────────────────────
        var header = UITheme.BuildPageHeader("📋", "Bookings", "Search, review and manage all bookings");
        var btnNew     = UITheme.AddHeaderButton(header, "+  New Booking", UITheme.Accent,       150);
        var btnRefresh = UITheme.AddHeaderButton(header, "↺  Refresh",    UITheme.SurfaceLight, 264);
        btnNew.Width     = 130;   // slightly wider for longer label
        btnRefresh.Width = 100;
        btnNew.Click     += (_, _) => OpenCreateBooking();
        btnRefresh.Click += (_, _) => LoadGrid();

        // ── Search Bar ───────────────────────────────────────────────────
        // BEFORE: Labels and TextBoxes at absolute Left = 0, 52, 228, 272, 490, 608 → clips at high DPI.
        // AFTER:  FlowLayoutPanel (LeftToRight, WrapContents=false) — the engine places
        //         each control sequentially; the status label is Anchor=Right.
        var searchBar = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 54,
            BackColor = UITheme.Surface,
            Padding   = new Padding(20, 0, 20, 0)
        };
        searchBar.Paint += (_, e) =>
        {
            using var p = new Pen(UITheme.Border, 1);
            e.Graphics.DrawLine(p, 0, searchBar.Height - 1, searchBar.Width, searchBar.Height - 1);
        };

        var searchFlow = new FlowLayoutPanel
        {
            Dock          = DockStyle.Left,
            Width         = 680,         // generous enough for the five items
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents  = false,
            BackColor     = Color.Transparent,
            AutoSize      = false,
            Padding       = new Padding(0, 10, 0, 0)
        };

        searchFlow.Controls.Add(MakeSearchLabel("Phone:"));
        _txtPhone = MakeSearchBox(160);
        searchFlow.Controls.Add(_txtPhone);
        searchFlow.Controls.Add(MakeSearchLabel("Email:"));
        _txtEmail = MakeSearchBox(200);
        searchFlow.Controls.Add(_txtEmail);

        var btnSearch = new Button { Text = "🔍  Search", Width = 110, Height = 34 };
        UITheme.StyleButton(btnSearch);
        btnSearch.Margin = new Padding(12, 0, 0, 0);
        btnSearch.Click  += (_, _) => LoadGrid();
        searchFlow.Controls.Add(btnSearch);

        var btnAll = new Button { Text = "All", Width = 60, Height = 34 };
        UITheme.StyleSecondaryButton(btnAll);
        btnAll.Margin = new Padding(6, 0, 0, 0);
        btnAll.Click += (_, _) => { _txtPhone.Text = ""; _txtEmail.Text = ""; LoadGrid(); };
        searchFlow.Controls.Add(btnAll);

        // Status label — anchored to the right of the search bar
        _lblStatus = new Label
        {
            AutoSize  = false,
            Dock      = DockStyle.Fill,   // Fill takes the remaining space to the right of searchFlow
            Height    = 34,
            Font      = UITheme.FontSmall,
            ForeColor = UITheme.TextPrimary,
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleRight,
            Padding   = new Padding(0, 0, 4, 0)
        };

        searchBar.Controls.Add(_lblStatus);    // Fill — add first
        searchBar.Controls.Add(searchFlow);    // Left — add after Fill

        // ── Action Toolbar ───────────────────────────────────────────────
        // BEFORE: buttons at absolute Left = 0, 124, 248, 372.
        // AFTER:  FlowLayoutPanel (Dock=Left, LeftToRight) — buttons stack correctly at any DPI.
        var actionBar = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 52,
            BackColor = UITheme.Background,
            Padding   = new Padding(16, 8, 16, 8)
        };

        var actionFlow = new FlowLayoutPanel
        {
            Dock          = DockStyle.Left,
            AutoSize      = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents  = false,
            BackColor     = Color.Transparent,
            Padding       = new Padding(0)
        };

        _btnApprove  = ActionBtn("✔  Approve",  UITheme.Success);
        _btnReject   = ActionBtn("✘  Reject",   UITheme.Danger);
        _btnCancel   = ActionBtn("⊘  Cancel",   Color.FromArgb(80, 80, 100));
        _btnComplete = ActionBtn("★  Complete", Color.FromArgb(56, 189, 248));

        DisableActionBtns();

        _btnApprove.Click  += (_, _) => DoStatusAction(s => s.ApproveBooking(_selectedBookingId!.Value),  "approved");
        _btnReject.Click   += (_, _) => DoStatusAction(s => s.RejectBooking(_selectedBookingId!.Value),   "rejected");
        _btnCancel.Click   += (_, _) => DoStatusAction(s => s.CancelBooking(_selectedBookingId!.Value),   "cancelled");
        _btnComplete.Click += (_, _) => DoStatusAction(s => s.CompleteBooking(_selectedBookingId!.Value), "completed");

        actionFlow.Controls.AddRange([_btnApprove, _btnReject, _btnCancel, _btnComplete]);
        actionBar.Controls.Add(actionFlow);

        // ── Detail Panel (collapsible, bottom) ───────────────────────────
        _detailPanel = BuildDetailPanel();

        // ── Grid ─────────────────────────────────────────────────────────
        _grid = new DataGridView { Dock = DockStyle.Fill };
        UITheme.StyleDataGridView(_grid);
        _grid.Columns.AddRange(
            TxtCol("BookingId",     "ID",       55,  false),
            TxtCol("CustomerName",  "Customer", 140),
            TxtCol("CustomerPhone", "Phone",    110, false),
            TxtCol("VendorName",    "Vendor",   120),
            TxtCol("VenueName",     "Venue",    130),
            TxtCol("BookingDate",   "Date",      90, false),
            TxtCol("GuestCount",    "Guests",    60, false),
            TxtCol("TotalAmount",   "Total",     90, false),
            TxtCol("StatusName",    "Status",    80, false)
        );
        _grid.CellFormatting   += Grid_CellFormatting;
        _grid.SelectionChanged += Grid_SelectionChanged;
        _grid.CellDoubleClick  += (_, _) => ToggleDetail();

        // Body container — Fill, holds grid + detail panel
        var bodyContainer = new Panel
        {
            Dock    = DockStyle.Fill,
            Padding = new Padding(16, 8, 16, 12)
        };
        bodyContainer.Controls.Add(_grid);        // Fill
        bodyContainer.Controls.Add(_detailPanel); // Bottom

        // Assemble — reverse order for top-docking
        Controls.Add(bodyContainer);
        Controls.Add(actionBar);
        Controls.Add(searchBar);
        Controls.Add(header);

        ResumeLayout(false);
        PerformLayout();
    }

    // ────────────────────────────────────────────────────────────────────────
    // Detail Panel Builder
    // ────────────────────────────────────────────────────────────────────────
    private Panel BuildDetailPanel()
    {
        var panel = new Panel
        {
            Dock      = DockStyle.Bottom,
            Height    = 0,           // starts collapsed
            BackColor = UITheme.Surface,
            Visible   = false,
            Padding   = new Padding(16, 12, 16, 12)
        };
        panel.Paint += (_, e) =>
        {
            using var p = new Pen(UITheme.Accent, 2);
            e.Graphics.DrawLine(p, 0, 0, panel.Width, 0);
        };

        // Close button (anchored top-right)
        var btnClose = new Button
        {
            Text   = "✕",
            Width  = 28,
            Height = 24,
            Top    = 8,
            Anchor = AnchorStyles.Right | AnchorStyles.Top,
            FlatStyle = FlatStyle.Flat
        };
        UITheme.StyleSecondaryButton(btnClose);
        btnClose.Click += (_, _) => HideDetail();
        panel.Controls.Add(btnClose);
        panel.Resize += (_, _) => btnClose.Left = panel.Width - 44;

        // Heading
        var hdr = new Label
        {
            Text      = "Booking Detail",
            Font      = UITheme.FontSubHeading,
            ForeColor = UITheme.AccentLight,
            AutoSize  = true,
            Top       = 10,
            Left      = 0
        };
        panel.Controls.Add(hdr);

        // ── 3-column info grid (TableLayoutPanel) ─────────────────────────
        // BEFORE: InfoLabel with absolute Left = 0, 260, 520 → third column clips.
        // AFTER:  BuildDetailInfoTable → 3 equal columns via SizeType.Percent 33%.
        //         Each label uses Dock=Fill + AutoEllipsis=true for graceful overflow.
        var infoTable = UITheme.BuildDetailInfoTable(panel, cols: 3, rows: 3);
        infoTable.Top    = 38;    // below heading; table is Dock=Fill but we set Top manually
        infoTable.Dock   = DockStyle.None;
        infoTable.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
        infoTable.Left   = 0;
        infoTable.Height = panel.Height - 42;
        panel.Resize += (_, _) =>
        {
            infoTable.Width  = panel.Width - 32;
            infoTable.Height = Math.Max(0, panel.Height - 42);
        };

        _dCustomer    = InfoCell(infoTable, 0, 0);
        _dVendor      = InfoCell(infoTable, 1, 0);
        _dVenue       = InfoCell(infoTable, 2, 0);
        _dDate        = InfoCell(infoTable, 0, 1);
        _dGuests      = InfoCell(infoTable, 1, 1);
        _dTotal       = InfoCell(infoTable, 2, 1);
        _dStatus      = InfoCell(infoTable, 0, 2);
        _dDecorations = InfoCell(infoTable, 1, 2);
        _dServices    = InfoCell(infoTable, 2, 2);

        return panel;
    }

    /// <summary>Creates an info label and places it in the TableLayoutPanel cell.</summary>
    private static Label InfoCell(TableLayoutPanel tbl, int col, int row)
    {
        var lbl = new Label
        {
            Dock         = DockStyle.Fill,
            Font         = UITheme.FontSmall,
            ForeColor    = UITheme.TextMuted,
            AutoSize     = false,
            AutoEllipsis = true,      // graceful clip with "…" if text still overflows
            BackColor    = Color.Transparent,
            TextAlign    = ContentAlignment.MiddleLeft,
            Padding      = new Padding(2, 0, 4, 0)
        };
        tbl.Controls.Add(lbl, col, row);
        return lbl;
    }

    // ────────────────────────────────────────────────────────────────────────
    // Data
    // ────────────────────────────────────────────────────────────────────────
    private void LoadGrid()
    {
        try
        {
            string? phone = string.IsNullOrWhiteSpace(_txtPhone.Text) ? null : _txtPhone.Text.Trim();
            string? email = string.IsNullOrWhiteSpace(_txtEmail.Text) ? null : _txtEmail.Text.Trim();

            var bookings = _service.GetBookingHistory(phone, email);
            _grid.DataSource = bookings.Count > 0 ? bookings : null;
            SetStatus(bookings.Count > 0
                ? $"{bookings.Count} booking(s) found."
                : "No bookings found.", UITheme.TextMuted);

            DisableActionBtns();
            HideDetail();
            _selectedBookingId = null;
        }
        catch (Exception ex) { SetStatus($"Error: {ex.Message}", UITheme.Danger); }
    }

    private void Grid_SelectionChanged(object? sender, EventArgs e)
    {
        if (_grid.SelectedRows.Count == 0) { DisableActionBtns(); return; }

        var id        = Convert.ToInt32(_grid.SelectedRows[0].Cells["BookingId"].Value);
        var statusVal = _grid.SelectedRows[0].Cells["StatusName"].Value?.ToString();
        _selectedBookingId = id;

        UpdateActionButtons(statusVal);
        if (_detailVisible) ShowDetail(id);
    }

    private void Grid_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (_grid.Columns[e.ColumnIndex].Name != "StatusName") return;
        if (e.Value is string status && e.CellStyle is not null)
        {
            e.CellStyle.ForeColor = status switch
            {
                "Pending"   => UITheme.Warning,
                "Approved"  => UITheme.Success,
                "Rejected"  => UITheme.Danger,
                "Cancelled" => UITheme.TextMuted,
                "Completed" => Color.FromArgb(56, 189, 248),
                _           => UITheme.TextPrimary
            };
            e.CellStyle.Font = UITheme.FontSmallBold;
        }
    }

    // ────────────────────────────────────────────────────────────────────────
    // Status Actions
    // ────────────────────────────────────────────────────────────────────────
    private void DoStatusAction(Func<BookingService, BookingStatusUpdateResponseModel> action, string verb)
    {
        if (_selectedBookingId is null) return;
        if (MessageBox.Show($"Mark booking #{_selectedBookingId} as {verb}?",
            "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

        var res = action(_service);
        SetStatus(res.IsSuccess ? $"✔  Booking {verb}." : $"✘  {res.Message}",
                  res.IsSuccess ? UITheme.Success : UITheme.Danger);
        if (res.IsSuccess) LoadGrid();
    }

    private void UpdateActionButtons(string? statusName)
    {
        _btnApprove.Enabled  = false;
        _btnReject.Enabled   = false;
        _btnCancel.Enabled   = false;
        _btnComplete.Enabled = false;

        switch (statusName)
        {
            case "Pending":
                _btnApprove.Enabled = true;
                _btnReject.Enabled  = true;
                _btnCancel.Enabled  = true;
                break;
            case "Approved":
                _btnCancel.Enabled   = true;
                _btnComplete.Enabled = true;
                break;
        }
    }

    private void DisableActionBtns()
    {
        _btnApprove.Enabled  = false;
        _btnReject.Enabled   = false;
        _btnCancel.Enabled   = false;
        _btnComplete.Enabled = false;
    }

    // ────────────────────────────────────────────────────────────────────────
    // Detail Panel
    // ────────────────────────────────────────────────────────────────────────
    private void ToggleDetail()
    {
        if (_selectedBookingId is null) return;
        if (_detailVisible) HideDetail();
        else ShowDetail(_selectedBookingId.Value);
    }

    private void ShowDetail(int id)
    {
        var b = _service.GetBookingDetail(id);
        if (b is null) return;

        _dCustomer.Text    = $"Customer:   {b.CustomerName}  ({b.CustomerPhone})";
        _dVendor.Text      = $"Vendor:      {b.VendorName}";
        _dVenue.Text       = $"Venue:       {b.VenueName}";
        _dDate.Text        = $"Date:         {b.BookingDate:yyyy-MM-dd}";
        _dGuests.Text      = $"Guests:      {b.GuestCount}";
        _dTotal.Text       = $"Total:         {b.TotalAmount:N0} MMK";
        _dStatus.Text      = $"Status:       {b.StatusName}";
        _dStatus.ForeColor = UITheme.BookingStatusColor(b.Status);
        _dDecorations.Text = "Decorations:  " + (b.Decorations.Count == 0 ? "None"
            : string.Join(", ", b.Decorations.Select(d => d.PackageName)));
        _dServices.Text    = "Services:      " + (b.Services.Count == 0 ? "None"
            : string.Join(", ", b.Services.Select(s => s.PackageName)));

        _detailPanel.Height  = 130;
        _detailPanel.Visible = true;
        _detailVisible       = true;
    }

    private void HideDetail()
    {
        _detailPanel.Height  = 0;
        _detailPanel.Visible = false;
        _detailVisible       = false;
    }

    // ────────────────────────────────────────────────────────────────────────
    // New Booking
    // ────────────────────────────────────────────────────────────────────────
    private void OpenCreateBooking()
    {
        using var form = new BookingCreateForm(_db);
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            SetStatus("✔  New booking created successfully.", UITheme.Success);
            LoadGrid();
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    private Button ActionBtn(string text, Color bg)
    {
        var b = new Button { Text = text, Width = 116, Height = 36 };
        b.Margin = new Padding(0, 0, 8, 0);
        UITheme.StyleButton(b, bg);
        b.Enabled = false;
        return b;
    }

    private static Label MakeSearchLabel(string text) => new()
    {
        Text      = text,
        Font      = UITheme.FontSmall,
        ForeColor = UITheme.TextMuted,
        AutoSize  = true,
        Margin    = new Padding(8, 8, 4, 0),
        TextAlign = ContentAlignment.MiddleLeft
    };

    private static TextBox MakeSearchBox(int width)
    {
        var tb = new TextBox { Width = width, Height = 28, Margin = new Padding(0, 5, 0, 0) };
        UITheme.StyleTextBox(tb);
        return tb;
    }

    private void SetStatus(string msg, Color col)
    {
        _lblStatus.Text      = msg;
        _lblStatus.ForeColor = col;
    }

    private static DataGridViewTextBoxColumn TxtCol(string prop, string hdr, int w = 120, bool fill = true)
    {
        var c = new DataGridViewTextBoxColumn { Name = prop, DataPropertyName = prop, HeaderText = hdr, Width = w };
        if (fill) c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        return c;
    }

    protected override void Dispose(bool disposing) { if (disposing) _db.Dispose(); base.Dispose(disposing); }
}
