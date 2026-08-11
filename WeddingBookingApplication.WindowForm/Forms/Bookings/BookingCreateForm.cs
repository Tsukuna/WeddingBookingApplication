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

public class BookingCreateForm : Form
{
    private readonly BookingService _bookingSvc;
    private readonly VendorService _vendorSvc;
    private readonly VenueService _venueSvc;
    private readonly DecorationService _decorSvc;
    private readonly ServicePackageService _svcPkgSvc;

    private int _step = 0;
    private const int TotalSteps = 5;

    private Panel[] _stepPanels = null!;
    private Panel _body = null!;

    private Button _btnBack = null!;
    private Button _btnNext = null!;
    private FlowLayoutPanel _dotsPanel = null!;

    private TextBox _txtCustName = null!;
    private TextBox _txtCustPhone = null!;
    private TextBox _txtCustEmail = null!;

    private ComboBox _cbVendor = null!;
    private ComboBox _cbVenue = null!;
    private DateTimePicker _dtpDate = null!;
    private NumericUpDown _nudGuests = null!;
    private Label _lblVenuePrice = null!;

    private CheckedListBox _clbDecor = null!;

    private CheckedListBox _clbSvc = null!;

    private Label _sumCustomer = null!;
    private Label _sumVendor = null!;
    private Label _sumVenue = null!;
    private Label _sumDate = null!;
    private Label _sumGuests = null!;
    private Label _sumDecor = null!;
    private Label _sumSvc = null!;
    private Label _sumTotal = null!;
    private Label _lblResult = null!;

    private List<VendorResponseModel> _vendors = [];
    private List<VenueResponseModel> _venues = [];
    private List<DecorationResponseModel> _decorations = [];
    private List<ServiceResponseModel> _services = [];

    public BookingCreateForm(AppDbContext db)
    {
        _bookingSvc = new BookingService(db);
        _vendorSvc = new VendorService(db);
        _venueSvc = new VenueService(db);
        _decorSvc = new DecorationService(db);
        _svcPkgSvc = new ServicePackageService(db);

        BuildUI();
        LoadStep0Data();
        GoToStep(0);
    }


    private void BuildUI()
    {
        Text = "Create New Booking";
        Size = new Size(640, 640);
        MinimumSize = new Size(580, 560);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = UITheme.Background;
        ForeColor = UITheme.TextPrimary;
        Font = UITheme.FontBody;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;

        var topStrip = new Panel
        {
            Dock = DockStyle.Top,
            Height = 78,
            BackColor = UITheme.Surface,
            Padding = new Padding(20, 10, 20, 6)
        };
        topStrip.Paint += (_, e) =>
        {
            using var p = new Pen(UITheme.Border, 1);
            e.Graphics.DrawLine(p, 0, topStrip.Height - 1, topStrip.Width, topStrip.Height - 1);
        };

        var title = new Label
        {
            Text = "💍 New Booking",
            Font = UITheme.FontSubHeading,
            ForeColor = UITheme.AccentLight,
            AutoSize = true,
            Dock = DockStyle.Top,
            Padding = new Padding(0, 0, 0, 4)
        };

        _dotsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 28,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoSize = false,
            BackColor = Color.Transparent
        };

        string[] stepNames = { "Customer", "Venue", "Decor", "Services", "Confirm" };
        for (int i = 0; i < TotalSteps; i++)
        {
            var dot = new Label
            {
                Tag = i,
                Text = $"● {stepNames[i]}",
                Font = new Font("Segoe UI", 8f),
                ForeColor = UITheme.TextMuted,
                AutoSize = true,
                Margin = new Padding(0, 4, 16, 0)
            };
            _dotsPanel.Controls.Add(dot);
        }

        topStrip.Controls.Add(title);
        topStrip.Controls.Add(_dotsPanel);

        _body = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UITheme.Background,
            Padding = new Padding(28, 12, 28, 8)
        };

        _stepPanels =
        [
            BuildStep1_Customer(),
            BuildStep2_VenueDate(),
            BuildStep3_Decorations(),
            BuildStep4_Services(),
            BuildStep5_Summary()
        ];

        var navBar = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 58,
            BackColor = UITheme.Surface,
            Padding = new Padding(16, 10, 16, 10)
        };
        navBar.Paint += (_, e) =>
        {
            using var p = new Pen(UITheme.Border, 1);
            e.Graphics.DrawLine(p, 0, 0, navBar.Width, 0);
        };

        _btnBack = new Button { Text = "← Back", Width = 110, Height = 36, Dock = DockStyle.Left };
        UITheme.StyleSecondaryButton(_btnBack);
        _btnBack.Click += (_, _) => GoToStep(_step - 1);

        _btnNext = new Button { Text = "Next →", Width = 150, Height = 36, Dock = DockStyle.Right };
        UITheme.StyleButton(_btnNext);
        _btnNext.Click += BtnNext_Click;

        navBar.Controls.AddRange([_btnBack, _btnNext]);

        Controls.Add(_body);
        Controls.Add(navBar);
        Controls.Add(topStrip);
    }


    private Panel BuildStep1_Customer()
    {
        var p = CreateStepShell("👤 Customer Information", "Enter the customer's contact details.");

        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            Padding = new Padding(0, 4, 0, 0)
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

        AddLabeledField(table, 0, "Full Name *", out _txtCustName);
        AddLabeledField(table, 2, "Phone Number *", out _txtCustPhone);
        AddLabeledField(table, 4, "Email Address", out _txtCustEmail);

        p.Controls.Add(table);
        table.BringToFront();
        return p;
    }

    private Panel BuildStep2_VenueDate()
    {
        var p = CreateStepShell("🏛 Venue & Date", "Select the vendor, venue and booking date.");

        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 9,
            Padding = new Padding(0, 4, 0, 0)
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

        table.Controls.Add(UITheme.FieldLabel("Vendor *"), 0, 0);
        _cbVendor = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        UITheme.StyleComboBox(_cbVendor);
        _cbVendor.SelectedIndexChanged += (_, _) => LoadVenues();
        table.Controls.Add(_cbVendor, 0, 1);

        table.Controls.Add(UITheme.FieldLabel("Venue *"), 0, 2);
        _cbVenue = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        UITheme.StyleComboBox(_cbVenue);
        _cbVenue.SelectedIndexChanged += (_, _) => UpdateVenuePrice();
        table.Controls.Add(_cbVenue, 0, 3);

        _lblVenuePrice = new Label
        {
            Dock = DockStyle.Fill,
            Font = UITheme.FontSmall,
            ForeColor = UITheme.TextMuted,
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true
        };
        table.Controls.Add(_lblVenuePrice, 0, 4);

        table.Controls.Add(UITheme.FieldLabel("Booking Date *"), 0, 5);
        _dtpDate = new DateTimePicker
        {
            Dock = DockStyle.Left,
            Width = 240,
            Format = DateTimePickerFormat.Short,
            Value = DateTime.Today.AddDays(7)
        };
        UITheme.StyleDateTimePicker(_dtpDate);
        table.Controls.Add(_dtpDate, 0, 6);

        table.Controls.Add(UITheme.FieldLabel("Guest Count *"), 0, 7);
        _nudGuests = new NumericUpDown
        {
            Dock = DockStyle.Left,
            Width = 160,
            Minimum = 1,
            Maximum = 5000,
            Value = 100
        };
        UITheme.StyleNumericUpDown(_nudGuests);
        table.Controls.Add(_nudGuests, 0, 8);

        p.Controls.Add(table);
        table.BringToFront();
        return p;
    }

    private Panel BuildStep3_Decorations()
    {
        var p = CreateStepShell("🌸 Decorations Packages", "Select decoration packages (optional, multi-select).");

        _clbDecor = new CheckedListBox
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None,
            CheckOnClick = true
        };
        UITheme.StyleCheckedListBox(_clbDecor);
        p.Controls.Add(_clbDecor);
        _clbDecor.BringToFront();
        return p;
    }

    private Panel BuildStep4_Services()
    {
        var p = CreateStepShell("🎵 Service Packages", "Select service packages (optional, multi-select).");

        _clbSvc = new CheckedListBox
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.None,
            CheckOnClick = true
        };
        UITheme.StyleCheckedListBox(_clbSvc);
        p.Controls.Add(_clbSvc);
        _clbSvc.BringToFront();
        return p;
    }

    private Panel BuildStep5_Summary()
    {
        var p = CreateStepShell("📋 Booking Summary", "Review your booking details before confirming.");

        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 11,
            Padding = new Padding(0, 4, 0, 0)
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        for (int i = 0; i < 11; i++)
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _sumCustomer = MakeSummaryLabel(); table.Controls.Add(_sumCustomer, 0, 0);
        _sumVendor = MakeSummaryLabel(); table.Controls.Add(_sumVendor, 0, 1);
        _sumVenue = MakeSummaryLabel(); table.Controls.Add(_sumVenue, 0, 2);
        _sumDate = MakeSummaryLabel(); table.Controls.Add(_sumDate, 0, 3);
        _sumGuests = MakeSummaryLabel(); table.Controls.Add(_sumGuests, 0, 4);
        _sumDecor = MakeSummaryLabel(); table.Controls.Add(_sumDecor, 0, 5);
        _sumSvc = MakeSummaryLabel(); table.Controls.Add(_sumSvc, 0, 6);

        var sep = new Panel
        {
            Height = 1,
            Dock = DockStyle.Top,
            BackColor = UITheme.Border,
            Margin = new Padding(0, 8, 0, 8)
        };
        table.Controls.Add(sep, 0, 7);

        _sumTotal = new Label
        {
            Dock = DockStyle.Fill,
            Font = UITheme.FontSubHeading,
            ForeColor = UITheme.AccentLight,
            AutoSize = false,
            Height = 32,
            AutoEllipsis = true
        };
        table.Controls.Add(_sumTotal, 0, 8);

        _lblResult = new Label
        {
            Dock = DockStyle.Fill,
            Font = UITheme.FontBodyBold,
            ForeColor = UITheme.TextPrimary,
            AutoSize = false,
            Height = 28,
            AutoEllipsis = true
        };
        table.Controls.Add(_lblResult, 0, 9);

        p.Controls.Add(table);
        table.BringToFront();
        return p;
    }


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
        _btnNext.Text = step == TotalSteps - 1 ? "✔ Confirm Booking" : "Next →";
        _btnNext.Enabled = true;

        foreach (Control c in _dotsPanel.Controls)
        {
            if (c is Label lbl && lbl.Tag is int idx)
                lbl.ForeColor = idx == step ? UITheme.AccentLight : UITheme.TextMuted;
        }

        if (step == TotalSteps - 1)
            BuildSummary();
    }

    private void BtnNext_Click(object? sender, EventArgs e)
    {
        if (!ValidateStep(_step)) return;

        if (_step == TotalSteps - 1)
        {
            SubmitBooking();
        }
        else
        {
            if (_step == 1)
            {
                LoadDecorations();
                LoadServices();
            }
            GoToStep(_step + 1);
        }
    }


    private bool ValidateStep(int step) => step switch
    {
        0 => ValidateCustomer(),
        1 => ValidateVenueDate(),
        _ => true
    };

    private bool ValidateCustomer()
    {
        if (string.IsNullOrWhiteSpace(_txtCustName.Text))
        {
            Flash(_txtCustName, "Customer name is required.");
            return false;
        }
        if (string.IsNullOrWhiteSpace(_txtCustPhone.Text))
        {
            Flash(_txtCustPhone, "Phone number is required.");
            return false;
        }
        return true;
    }

    private bool ValidateVenueDate()
    {
        if (_cbVendor.SelectedItem is null)
        {
            MessageBox.Show("Please select a vendor.", "Validation",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        if (_cbVenue.SelectedItem is null)
        {
            MessageBox.Show("Please select a venue.", "Validation",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        if (_dtpDate.Value.Date <= DateTime.Today)
        {
            MessageBox.Show("Booking date must be in the future.", "Validation",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        return true;
    }

    private static void Flash(TextBox tb, string msg)
    {
        tb.BackColor = Color.FromArgb(80, 239, 68, 68);
        tb.Focus();
        MessageBox.Show(msg, "Required Field", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        tb.BackColor = UITheme.SurfaceLight;
    }


    private void LoadStep0Data()
    {
        _vendors = _vendorSvc.GetVendors();
        _cbVendor.Items.Clear();
        foreach (var v in _vendors)
            _cbVendor.Items.Add(new ComboItem(v.VendorId, v.VendorName));
        if (_cbVendor.Items.Count > 0)
            _cbVendor.SelectedIndex = 0;
    }

    private void LoadVenues()
    {
        _cbVenue.Items.Clear();
        _lblVenuePrice.Text = "";
        if (_cbVendor.SelectedItem is not ComboItem vendor) return;

        _venues = _venueSvc.GetVenues().Where(v => v.VendorId == vendor.Id).ToList();
        foreach (var v in _venues)
            _cbVenue.Items.Add(new ComboItem(v.VenueId,
                $"{v.VenueName} – {v.Location} (cap: {v.Capacity})"));
        if (_cbVenue.Items.Count > 0)
            _cbVenue.SelectedIndex = 0;
    }

    private void UpdateVenuePrice()
    {
        if (_cbVenue.SelectedItem is not ComboItem venueItem) return;
        var venue = _venues.FirstOrDefault(v => v.VenueId == venueItem.Id);
        _lblVenuePrice.Text = venue is not null
            ? $"💰 Venue Price: {venue.Price:N0} MMK"
            : "";
    }

    private void LoadDecorations()
    {
        _clbDecor.Items.Clear();
        if (_cbVendor.SelectedItem is not ComboItem vendor) return;

        _decorations = _decorSvc.GetDecorationPackages()
            .Where(d => d.VendorId == vendor.Id).ToList();
        foreach (var d in _decorations)
            _clbDecor.Items.Add(new PackageItem(d.DecorationPackageId,
                $"{d.PackageName} — {d.Price:N0} MMK"));
    }

    private void LoadServices()
    {
        _clbSvc.Items.Clear();
        if (_cbVendor.SelectedItem is not ComboItem vendor) return;

        _services = _svcPkgSvc.GetServicePackages()
            .Where(s => s.VendorId == vendor.Id).ToList();
        foreach (var s in _services)
            _clbSvc.Items.Add(new PackageItem(s.ServicePackageId,
                $"{s.PackageName} — {s.Price:N0} MMK"));
    }

    private void BuildSummary()
    {
        var vendor = _cbVendor.SelectedItem as ComboItem;
        var venue = _cbVenue.SelectedItem as ComboItem;
        var venueModel = _venues.FirstOrDefault(v => v.VenueId == venue?.Id);

        var selectedDecorIds = _clbDecor.CheckedItems.OfType<PackageItem>().Select(x => x.Id).ToList();
        var selectedSvcIds = _clbSvc.CheckedItems.OfType<PackageItem>().Select(x => x.Id).ToList();

        var decorTotal = _decorations
            .Where(d => selectedDecorIds.Contains(d.DecorationPackageId)).Sum(d => d.Price);
        var svcTotal = _services
            .Where(s => selectedSvcIds.Contains(s.ServicePackageId)).Sum(s => s.Price);
        var total = (venueModel?.Price ?? 0) + decorTotal + svcTotal;

        _sumCustomer.Text = $"👤 Customer : {_txtCustName.Text.Trim()} ({_txtCustPhone.Text.Trim()})";
        _sumVendor.Text = $"🏢 Vendor : {vendor?.Name ?? "—"}";
        _sumVenue.Text = $"🏛 Venue : {venueModel?.VenueName ?? "—"} ({venueModel?.Price:N0} MMK)";
        _sumDate.Text = $"📅 Date : {_dtpDate.Value:yyyy-MM-dd}";
        _sumGuests.Text = $"👥 Guests : {_nudGuests.Value}";
        _sumDecor.Text = selectedDecorIds.Count == 0
            ? "🌸 Decorations : None"
            : $"🌸 Decorations : {string.Join(", ", _clbDecor.CheckedItems.OfType<PackageItem>().Select(x => x.Name.Split('—')[0].Trim()))} ({decorTotal:N0} MMK)";
        _sumSvc.Text = selectedSvcIds.Count == 0
            ? "🎵 Services : None"
            : $"🎵 Services : {string.Join(", ", _clbSvc.CheckedItems.OfType<PackageItem>().Select(x => x.Name.Split('—')[0].Trim()))} ({svcTotal:N0} MMK)";
        _sumTotal.Text = $"💰 Total Amount : {total:N0} MMK";
        _lblResult.Text = "";
    }

    private void SubmitBooking()
    {
        var vendor = _cbVendor.SelectedItem as ComboItem;
        var venue = _cbVenue.SelectedItem as ComboItem;
        if (vendor is null || venue is null) return;

        var decorIds = _clbDecor.CheckedItems.OfType<PackageItem>().Select(x => x.Id).ToList();
        var svcIds = _clbSvc.CheckedItems.OfType<PackageItem>().Select(x => x.Id).ToList();

        var req = new BookingCreateRequestModel
        {
            CustomerName = _txtCustName.Text.Trim(),
            CustomerPhone = _txtCustPhone.Text.Trim(),
            CustomerEmail = _txtCustEmail.Text.Trim(),
            VendorId = vendor.Id,
            VenueId = venue.Id,
            BookingDate = DateOnly.FromDateTime(_dtpDate.Value),
            GuestCount = (int)_nudGuests.Value,
            DecorationPackageIds = decorIds,
            ServicePackageIds = svcIds
        };

        try
        {
            var res = _bookingSvc.CreateBooking(req);
            if (res.IsSuccess)
            {
                _lblResult.ForeColor = UITheme.Success;
                _lblResult.Text = $"✔ Booking #{res.BookingId} created successfully!";
                _btnNext.Enabled = false;
                DialogResult = DialogResult.OK;
            }
            else
            {
                _lblResult.ForeColor = UITheme.Danger;
                _lblResult.Text = $"✘ {res.Message}";
            }
        }
        catch (Exception ex)
        {
            _lblResult.ForeColor = UITheme.Danger;
            _lblResult.Text = $"✘ Error: {ex.Message}";
        }
    }

    private static Panel CreateStepShell(string title, string sub)
    {
        var p = new Panel { Dock = DockStyle.Fill, BackColor = UITheme.Background };

        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 56,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 0, 0, 4)
        };

        var lblTitle = new Label
        {
            Text = title,
            Font = UITheme.FontSubHeading,
            ForeColor = UITheme.AccentLight,
            AutoSize = true,
            Dock = DockStyle.Top
        };

        var lblSub = new Label
        {
            Text = sub,
            Font = UITheme.FontSmall,
            ForeColor = UITheme.TextMuted,
            AutoSize = true,
            Dock = DockStyle.Top,
            Padding = new Padding(0, 2, 0, 0)
        };

        var sep = new Panel
        {
            Height = 1,
            Dock = DockStyle.Bottom,
            BackColor = UITheme.Border
        };

        header.Controls.Add(sep);
        header.Controls.Add(lblSub);
        header.Controls.Add(lblTitle);

        p.Controls.Add(header);
        return p;
    }

    private static void AddLabeledField(TableLayoutPanel table, int row, string label, out TextBox tb)
    {
        var lbl = UITheme.FieldLabel(label);
        table.Controls.Add(lbl, 0, row);

        tb = new TextBox { Dock = DockStyle.Fill };
        UITheme.StyleTextBox(tb);
        table.Controls.Add(tb, 0, row + 1);
    }

    private static Label MakeSummaryLabel()
    {
        return new Label
        {
            Dock = DockStyle.Fill,
            Font = UITheme.FontBody,
            ForeColor = UITheme.TextPrimary,
            AutoSize = false,
            Height = 26,
            AutoEllipsis = true,
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    private record ComboItem(int Id, string Name)
    {
        public override string ToString() => Name;
    }

    private record PackageItem(int Id, string Name)
    {
        public override string ToString() => Name;
    }
}