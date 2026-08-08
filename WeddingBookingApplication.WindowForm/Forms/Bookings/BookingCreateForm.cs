using WeddingBookingApplication.Database.AppDbContextModels;
using WeddingBookingApplication.Domain.Features.Booking;
using WeddingBookingApplication.Domain.Features.Decoration;
using WeddingBookingApplication.Domain.Features.ServicePackage;
using WeddingBookingApplication.Domain.Features.Vendor;
using WeddingBookingApplication.Domain.Features.Venue;
using WeddingBookingApplication.Domain.Models.Booking;
using WeddingBookingApplication.Domain.Models.Decoration;
using WeddingBookingApplication.Domain.Models.Service;
using WeddingBookingApplication.Domain.Models.Vendor;
using WeddingBookingApplication.Domain.Models.Venue;
using WeddingBookingApplication.WindowForm.Helpers;

namespace WeddingBookingApplication.WindowForm.Forms.Bookings;

/// <summary>
/// Multi-step dialog for creating a new booking.
/// Steps: Customer Info → Vendor/Venue/Date → Decorations → Services → Summary
/// </summary>
public class BookingCreateForm : Form
{
    // ── Services ─────────────────────────────────────────────────────────────
    private readonly BookingService        _bookingSvc;
    private readonly VendorService         _vendorSvc;
    private readonly VenueService          _venueSvc;
    private readonly DecorationService     _decorSvc;
    private readonly ServicePackageService _svcPkgSvc;

    // ── Wizard state ─────────────────────────────────────────────────────────
    private int   _step = 0;
    private const int TotalSteps = 5;

    // ── Step panels ──────────────────────────────────────────────────────────
    private Panel[]   _stepPanels = null!;
    private Panel     _body       = null!;

    // ── Navigation ───────────────────────────────────────────────────────────
    private Button _btnBack  = null!;
    private Button _btnNext  = null!;
    private Label  _lblStep  = null!;

    // ── Step 1 – Customer ─────────────────────────────────────────────────────
    private TextBox _txtCustName  = null!;
    private TextBox _txtCustPhone = null!;
    private TextBox _txtCustEmail = null!;

    // ── Step 2 – Vendor / Venue / Date ────────────────────────────────────────
    private ComboBox      _cbVendor   = null!;
    private ComboBox      _cbVenue    = null!;
    private DateTimePicker _dtpDate   = null!;
    private NumericUpDown _nudGuests  = null!;
    private Label         _lblVenuePrice = null!;

    // ── Step 3 – Decorations ──────────────────────────────────────────────────
    private CheckedListBox _clbDecor  = null!;

    // ── Step 4 – Services ─────────────────────────────────────────────────────
    private CheckedListBox _clbSvc    = null!;

    // ── Step 5 – Summary ──────────────────────────────────────────────────────
    private Label _sumCustomer  = null!;
    private Label _sumVendor    = null!;
    private Label _sumVenue     = null!;
    private Label _sumDate      = null!;
    private Label _sumGuests    = null!;
    private Label _sumDecor     = null!;
    private Label _sumSvc       = null!;
    private Label _sumTotal     = null!;
    private Label _lblResult    = null!;

    // ── Cached data ───────────────────────────────────────────────────────────
    private List<VendorResponseModel>     _vendors     = [];
    private List<VenueResponseModel>      _venues      = [];
    private List<DecorationResponseModel> _decorations = [];
    private List<ServiceResponseModel>    _services    = [];

    // ────────────────────────────────────────────────────────────────────────
    public BookingCreateForm(AppDbContext db)
    {
        _bookingSvc = new BookingService(db);
        _vendorSvc  = new VendorService(db);
        _venueSvc   = new VenueService(db);
        _decorSvc   = new DecorationService(db);
        _svcPkgSvc  = new ServicePackageService(db);

        BuildUI();
        LoadStep0Data();
        GoToStep(0);
    }

    // ────────────────────────────────────────────────────────────────────────
    // Form Shell
    // ────────────────────────────────────────────────────────────────────────
    private void BuildUI()
    {
        Text            = "Create New Booking";
        Size            = new Size(620, 620);
        MinimumSize     = new Size(580, 560);
        StartPosition   = FormStartPosition.CenterParent;
        BackColor       = UITheme.Background;
        ForeColor       = UITheme.TextPrimary;
        Font            = UITheme.FontBody;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;

        // ── Top strip: title + step indicator ────────────────────────────
        var topStrip = new Panel { Dock = DockStyle.Top, Height = 72, BackColor = UITheme.Surface };
        topStrip.Paint += (_, e) => { using var p = new Pen(UITheme.Border, 1); e.Graphics.DrawLine(p, 0, topStrip.Height - 1, topStrip.Width, topStrip.Height - 1); };

        var title = new Label { Text = "💍  New Booking", Font = UITheme.FontSubHeading, ForeColor = UITheme.AccentLight, AutoSize = true, Left = 20, Top = 16 };

        // Step dots
        var dotsPanel = new Panel { BackColor = Color.Transparent, Width = 220, Height = 24, Left = 20, Top = 44 };
        string[] stepNames = { "Customer", "Venue", "Decor", "Services", "Confirm" };
        for (int i = 0; i < TotalSteps; i++)
        {
            var idx = i;
            var dot = new Label
            {
                Tag       = idx,
                Text      = $"● {stepNames[i]}",
                Font      = new Font("Segoe UI", 7.5f),
                ForeColor = UITheme.TextMuted,
                AutoSize  = true,
                Left      = i * 100,
                Top       = 4
            };
            dotsPanel.Controls.Add(dot);
        }
        dotsPanel.Width = TotalSteps * 100;

        _lblStep = new Label
        {
            Text      = "Step 1 of 5",
            Font      = UITheme.FontSmall,
            ForeColor = UITheme.TextMuted,
            AutoSize  = true,
            Left      = 20,
            Top       = 44,
            Visible   = false
        };

        topStrip.Controls.AddRange([title, dotsPanel]);

        // ── Body (swapped per step) ───────────────────────────────────────
        _body = new Panel { Dock = DockStyle.Fill, BackColor = UITheme.Background, Padding = new Padding(30, 20, 30, 0) };

        // Build all step panels
        _stepPanels = [
            BuildStep1_Customer(),
            BuildStep2_VenueDate(),
            BuildStep3_Decorations(),
            BuildStep4_Services(),
            BuildStep5_Summary()
        ];

        // ── Bottom navigation ─────────────────────────────────────────────
        var navBar = new Panel { Dock = DockStyle.Bottom, Height = 56, BackColor = UITheme.Surface, Padding = new Padding(16, 10, 16, 10) };
        navBar.Paint += (_, e) => { using var p = new Pen(UITheme.Border, 1); e.Graphics.DrawLine(p, 0, 0, navBar.Width, 0); };

        _btnBack = new Button { Text = "← Back", Width = 110, Height = 36, Dock = DockStyle.Left };
        UITheme.StyleSecondaryButton(_btnBack);
        _btnBack.Click += (_, _) => GoToStep(_step - 1);

        _btnNext = new Button { Text = "Next →", Width = 120, Height = 36, Dock = DockStyle.Right };
        UITheme.StyleButton(_btnNext);
        _btnNext.Click += BtnNext_Click;

        navBar.Controls.AddRange([_btnBack, _btnNext]);

        Controls.Add(_body);
        Controls.Add(navBar);
        Controls.Add(topStrip);
    }

    // ────────────────────────────────────────────────────────────────────────
    // Step Panels
    // ────────────────────────────────────────────────────────────────────────

    // Step 1 – Customer Info
    private Panel BuildStep1_Customer()
    {
        var p = StepPanel("👤  Customer Information", "Enter the customer's contact details.");
        int y = 80;

        AddField(p, "Full Name *",     ref y, out _txtCustName);
        AddField(p, "Phone Number *",  ref y, out _txtCustPhone);
        AddField(p, "Email Address",   ref y, out _txtCustEmail);

        return p;
    }

    // Step 2 – Vendor / Venue / Date
    private Panel BuildStep2_VenueDate()
    {
        var p = StepPanel("🏛  Venue & Date", "Select the vendor, venue and booking date.");
        int y = 80;

        var lblV = UITheme.FieldLabel("Vendor *"); lblV.Top = y; lblV.Left = 0; y += 20;
        _cbVendor = new ComboBox { Top = y, Left = 0, Width = 520 };
        UITheme.StyleComboBox(_cbVendor);
        _cbVendor.SelectedIndexChanged += (_, _) => LoadVenues();
        y += 38;
        p.Controls.AddRange([lblV, _cbVendor]);

        var lblVn = UITheme.FieldLabel("Venue *"); lblVn.Top = y; lblVn.Left = 0; y += 20;
        _cbVenue = new ComboBox { Top = y, Left = 0, Width = 520 };
        UITheme.StyleComboBox(_cbVenue);
        _cbVenue.SelectedIndexChanged += (_, _) => UpdateVenuePrice();
        y += 38;
        p.Controls.AddRange([lblVn, _cbVenue]);

        _lblVenuePrice = new Label { Top = y, Left = 0, Width = 520, Height = 24, Font = UITheme.FontSmall, ForeColor = UITheme.TextMuted, BackColor = Color.Transparent, AutoSize = false };
        y += 32;
        p.Controls.Add(_lblVenuePrice);

        var lblDt = UITheme.FieldLabel("Booking Date *"); lblDt.Top = y; lblDt.Left = 0; y += 20;
        _dtpDate = new DateTimePicker { Top = y, Left = 0, Width = 240, Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(7) };
        UITheme.StyleDateTimePicker(_dtpDate);
        y += 38;
        p.Controls.AddRange([lblDt, _dtpDate]);

        var lblG = UITheme.FieldLabel("Guest Count *"); lblG.Top = y; lblG.Left = 0; y += 20;
        _nudGuests = new NumericUpDown { Top = y, Left = 0, Width = 160, Minimum = 1, Maximum = 5000, Value = 100 };
        UITheme.StyleNumericUpDown(_nudGuests);
        p.Controls.AddRange([lblG, _nudGuests]);

        return p;
    }

    // Step 3 – Decoration Packages
    private Panel BuildStep3_Decorations()
    {
        var p = StepPanel("🌸  Decoration Packages", "Select decoration packages (optional, multi-select).");
        _clbDecor = new CheckedListBox { Top = 80, Left = 0, Width = 520, Height = 320, BorderStyle = BorderStyle.None };
        UITheme.StyleCheckedListBox(_clbDecor);
        p.Controls.Add(_clbDecor);
        return p;
    }

    // Step 4 – Service Packages
    private Panel BuildStep4_Services()
    {
        var p = StepPanel("🎵  Service Packages", "Select service packages (optional, multi-select).");
        _clbSvc = new CheckedListBox { Top = 80, Left = 0, Width = 520, Height = 320, BorderStyle = BorderStyle.None };
        UITheme.StyleCheckedListBox(_clbSvc);
        p.Controls.Add(_clbSvc);
        return p;
    }

    // Step 5 – Summary & Confirm
    private Panel BuildStep5_Summary()
    {
        var p = StepPanel("📋  Booking Summary", "Review your booking details before confirming.");
        int y = 80;

        _sumCustomer = SumLine(p, ref y);
        _sumVendor   = SumLine(p, ref y);
        _sumVenue    = SumLine(p, ref y);
        _sumDate     = SumLine(p, ref y);
        _sumGuests   = SumLine(p, ref y);
        _sumDecor    = SumLine(p, ref y);
        _sumSvc      = SumLine(p, ref y);

        y += 10;
        var sep = new Panel { Top = y, Left = 0, Width = 520, Height = 1, BackColor = UITheme.Border }; y += 12;
        p.Controls.Add(sep);

        _sumTotal = new Label { Top = y, Left = 0, Width = 520, Height = 28, Font = UITheme.FontSubHeading, ForeColor = UITheme.AccentLight, AutoSize = false, BackColor = Color.Transparent };
        y += 36;
        p.Controls.Add(_sumTotal);

        _lblResult = new Label { Top = y, Left = 0, Width = 520, Height = 28, Font = UITheme.FontBodyBold, ForeColor = UITheme.TextPrimary, AutoSize = false, BackColor = Color.Transparent };
        p.Controls.Add(_lblResult);

        return p;
    }

    // ────────────────────────────────────────────────────────────────────────
    // Step Navigation
    // ────────────────────────────────────────────────────────────────────────
    private void GoToStep(int step)
    {
        if (step < 0 || step >= TotalSteps) return;

        _step = step;
        _body.SuspendLayout();
        _body.Controls.Clear();
        var panel = _stepPanels[step];
        panel.Dock = DockStyle.Fill;
        _body.Controls.Add(panel);
        _body.ResumeLayout();

        _btnBack.Enabled = step > 0;
        _btnNext.Text    = step == TotalSteps - 1 ? "✔  Confirm Booking" : "Next  →";

        // Pre-fill summary on last step
        if (step == TotalSteps - 1) BuildSummary();
    }

    private void BtnNext_Click(object? sender, EventArgs e)
    {
        if (!ValidateStep(_step)) return;

        if (_step == TotalSteps - 1)
        {
            // Final step — submit
            SubmitBooking();
        }
        else
        {
            // Pre-load data for next step
            if (_step == 1) { LoadDecorations(); LoadServices(); }
            GoToStep(_step + 1);
        }
    }

    // ────────────────────────────────────────────────────────────────────────
    // Validation
    // ────────────────────────────────────────────────────────────────────────
    private bool ValidateStep(int step) => step switch
    {
        0 => ValidateCustomer(),
        1 => ValidateVenueDate(),
        _ => true
    };

    private bool ValidateCustomer()
    {
        if (string.IsNullOrWhiteSpace(_txtCustName.Text))
        { Flash(_txtCustName, "Customer name is required."); return false; }
        if (string.IsNullOrWhiteSpace(_txtCustPhone.Text))
        { Flash(_txtCustPhone, "Phone number is required."); return false; }
        return true;
    }

    private bool ValidateVenueDate()
    {
        if (_cbVendor.SelectedItem is null)
        { MessageBox.Show("Please select a vendor.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
        if (_cbVenue.SelectedItem is null)
        { MessageBox.Show("Please select a venue.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
        if (_dtpDate.Value.Date <= DateTime.Today)
        { MessageBox.Show("Booking date must be in the future.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
        return true;
    }

    private static void Flash(TextBox tb, string msg)
    {
        tb.BackColor = Color.FromArgb(80, 239, 68, 68);
        tb.Focus();
        MessageBox.Show(msg, "Required Field", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        tb.BackColor = UITheme.SurfaceLight;
    }

    // ────────────────────────────────────────────────────────────────────────
    // Data Loading
    // ────────────────────────────────────────────────────────────────────────
    private void LoadStep0Data()
    {
        _vendors = _vendorSvc.GetVendors();
        _cbVendor.Items.Clear();
        foreach (var v in _vendors)
            _cbVendor.Items.Add(new ComboItem(v.VendorId, v.VendorName));
        if (_cbVendor.Items.Count > 0) _cbVendor.SelectedIndex = 0;
    }

    private void LoadVenues()
    {
        _cbVenue.Items.Clear();
        _lblVenuePrice.Text = "";
        if (_cbVendor.SelectedItem is not ComboItem vendor) return;

        _venues = _venueSvc.GetVenues().Where(v => v.VendorId == vendor.Id).ToList();
        foreach (var v in _venues)
            _cbVenue.Items.Add(new ComboItem(v.VenueId, $"{v.VenueName}  –  {v.Location}  (cap: {v.Capacity})"));
        if (_cbVenue.Items.Count > 0) _cbVenue.SelectedIndex = 0;
    }

    private void UpdateVenuePrice()
    {
        if (_cbVenue.SelectedItem is not ComboItem venueItem) return;
        var venue = _venues.FirstOrDefault(v => v.VenueId == venueItem.Id);
        _lblVenuePrice.Text = venue is not null ? $"  💰  Venue Price: {venue.Price:N0} MMK" : "";
    }

    private void LoadDecorations()
    {
        _clbDecor.Items.Clear();
        if (_cbVendor.SelectedItem is not ComboItem vendor) return;
        _decorations = _decorSvc.GetDecorationPackages().Where(d => d.VendorId == vendor.Id).ToList();
        foreach (var d in _decorations)
            _clbDecor.Items.Add(new PackageItem(d.DecorationPackageId, $"{d.PackageName}  —  {d.Price:N0} MMK"));
    }

    private void LoadServices()
    {
        _clbSvc.Items.Clear();
        if (_cbVendor.SelectedItem is not ComboItem vendor) return;
        _services = _svcPkgSvc.GetServicePackages().Where(s => s.VendorId == vendor.Id).ToList();
        foreach (var s in _services)
            _clbSvc.Items.Add(new PackageItem(s.ServicePackageId, $"{s.PackageName}  —  {s.Price:N0} MMK"));
    }

    // ────────────────────────────────────────────────────────────────────────
    // Summary
    // ────────────────────────────────────────────────────────────────────────
    private void BuildSummary()
    {
        var vendor = _cbVendor.SelectedItem as ComboItem;
        var venue  = _cbVenue.SelectedItem  as ComboItem;
        var venueModel = _venues.FirstOrDefault(v => v.VenueId == venue?.Id);

        var selectedDecorIds = _clbDecor.CheckedItems.OfType<PackageItem>().Select(x => x.Id).ToList();
        var selectedSvcIds   = _clbSvc.CheckedItems.OfType<PackageItem>().Select(x => x.Id).ToList();

        var decorTotal = _decorations.Where(d => selectedDecorIds.Contains(d.DecorationPackageId)).Sum(d => d.Price);
        var svcTotal   = _services.Where(s => selectedSvcIds.Contains(s.ServicePackageId)).Sum(s => s.Price);
        var total      = (venueModel?.Price ?? 0) + decorTotal + svcTotal;

        _sumCustomer.Text = $"👤  Customer    :  {_txtCustName.Text.Trim()}  ({_txtCustPhone.Text.Trim()})";
        _sumVendor.Text   = $"🏢  Vendor        :  {vendor?.Name ?? "—"}";
        _sumVenue.Text    = $"🏛  Venue          :  {venueModel?.VenueName ?? "—"}  ({venueModel?.Price:N0} MMK)";
        _sumDate.Text     = $"📅  Date            :  {_dtpDate.Value:yyyy-MM-dd}";
        _sumGuests.Text   = $"👥  Guests          :  {_nudGuests.Value}";
        _sumDecor.Text    = selectedDecorIds.Count == 0 ? "🌸  Decorations :  None"
            : $"🌸  Decorations :  {string.Join(", ", _clbDecor.CheckedItems.OfType<PackageItem>().Select(x => x.Name.Split('—')[0].Trim()))}  ({decorTotal:N0} MMK)";
        _sumSvc.Text      = selectedSvcIds.Count == 0 ? "🎵  Services       :  None"
            : $"🎵  Services       :  {string.Join(", ", _clbSvc.CheckedItems.OfType<PackageItem>().Select(x => x.Name.Split('—')[0].Trim()))}  ({svcTotal:N0} MMK)";
        _sumTotal.Text    = $"💰  Total Amount :  {total:N0} MMK";
        _lblResult.Text   = "";
    }

    // ────────────────────────────────────────────────────────────────────────
    // Submit
    // ────────────────────────────────────────────────────────────────────────
    private void SubmitBooking()
    {
        var vendor = _cbVendor.SelectedItem as ComboItem;
        var venue  = _cbVenue.SelectedItem  as ComboItem;
        if (vendor is null || venue is null) return;

        var decorIds = _clbDecor.CheckedItems.OfType<PackageItem>().Select(x => x.Id).ToList();
        var svcIds   = _clbSvc.CheckedItems.OfType<PackageItem>().Select(x => x.Id).ToList();

        var req = new BookingCreateRequestModel
        {
            CustomerName        = _txtCustName.Text.Trim(),
            CustomerPhone       = _txtCustPhone.Text.Trim(),
            CustomerEmail       = _txtCustEmail.Text.Trim(),
            VendorId            = vendor.Id,
            VenueId             = venue.Id,
            BookingDate         = DateOnly.FromDateTime(_dtpDate.Value),
            GuestCount          = (int)_nudGuests.Value,
            DecorationPackageIds = decorIds,
            ServicePackageIds   = svcIds
        };

        try
        {
            var res = _bookingSvc.CreateBooking(req);
            if (res.IsSuccess)
            {
                _lblResult.ForeColor = UITheme.Success;
                _lblResult.Text      = $"✔  Booking #{res.BookingId} created successfully!";
                _btnNext.Enabled     = false;
                DialogResult         = DialogResult.OK;
            }
            else
            {
                _lblResult.ForeColor = UITheme.Danger;
                _lblResult.Text      = $"✘  {res.Message}";
            }
        }
        catch (Exception ex)
        {
            _lblResult.ForeColor = UITheme.Danger;
            _lblResult.Text      = $"✘  Error: {ex.Message}";
        }
    }

    // ────────────────────────────────────────────────────────────────────────
    // Layout Helpers
    // ────────────────────────────────────────────────────────────────────────
    private static Panel StepPanel(string title, string sub)
    {
        var p = new Panel { BackColor = UITheme.Background, Padding = new Padding(0) };

        var lblTitle = new Label { Text = title, Font = UITheme.FontSubHeading, ForeColor = UITheme.AccentLight, AutoSize = true, Left = 0, Top = 0 };
        var lblSub   = new Label { Text = sub,   Font = UITheme.FontSmall,      ForeColor = UITheme.TextMuted,  AutoSize = true, Left = 0, Top = 28 };
        var sep      = new Panel { Left = 0, Top = 52, Width = 520, Height = 1, BackColor = UITheme.Border };

        p.Controls.AddRange([lblTitle, lblSub, sep]);
        return p;
    }

    private static void AddField(Panel p, string label, ref int y, out TextBox tb)
    {
        var lbl = UITheme.FieldLabel(label); lbl.Top = y; lbl.Left = 0; y += 20;
        tb = new TextBox { Left = 0, Top = y, Width = 520 };
        UITheme.StyleTextBox(tb); y += 38;
        p.Controls.AddRange([lbl, tb]);
    }

    private static Label SumLine(Panel p, ref int y)
    {
        var lbl = new Label { Top = y, Left = 0, Width = 520, Height = 22, Font = UITheme.FontBody, ForeColor = UITheme.TextPrimary, AutoSize = false, BackColor = Color.Transparent };
        y += 24;
        p.Controls.Add(lbl);
        return lbl;
    }

    private record ComboItem(int Id, string Name) { public override string ToString() => Name; }
    private record PackageItem(int Id, string Name) { public override string ToString() => Name; }
}
