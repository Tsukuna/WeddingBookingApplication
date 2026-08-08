using WeddingBookingApplication.WindowForm.Forms.Bookings;
using WeddingBookingApplication.WindowForm.Forms.Decorations;
using WeddingBookingApplication.WindowForm.Forms.ServicePackages;
using WeddingBookingApplication.WindowForm.Forms.Vendors;
using WeddingBookingApplication.WindowForm.Forms.Venues;
using WeddingBookingApplication.WindowForm.Helpers;

namespace WeddingBookingApplication.WindowForm.Forms;

/// <summary>
/// Top-level application shell: dark side-navigation + swappable content area.
/// AutoScaleMode.Dpi (baseline 96 F) ensures every ContainerControl scales
/// proportionally on high-DPI monitors, per the MS AutoScale documentation.
/// </summary>
public class MainShellForm : Form
{
    // ── Controls ────────────────────────────────────────────────────────────
    private Panel          _navPanel     = null!;
    private Panel          _contentPanel = null!;
    private Label          _statusLabel  = null!;
    private Button[]       _navBtns      = Array.Empty<Button>();
    private UserControl?   _currentView;
    private int            _activeIdx = -1;

    private readonly System.Windows.Forms.Timer _clock = new() { Interval = 1_000 };

    // ── Nav items ───────────────────────────────────────────────────────────
    private static readonly (string Icon, string Label)[] NavItems =
    {
        ("🏠", "Dashboard"),
        ("🏢", "Vendors"),
        ("🏛", "Venues"),
        ("🌸", "Decorations"),
        ("🎵", "Services"),
        ("📋", "Bookings")
    };

    public MainShellForm()
    {
        // AutoScaleMode and AutoScaleDimensions must be set BEFORE InitializeComponent
        // or any control is added, per the MS AutoScale documentation.
        AutoScaleMode       = AutoScaleMode.Dpi;
        AutoScaleDimensions = new SizeF(96F, 96F);   // design-time baseline = 100 % DPI

        BuildUI();
        NavigateTo(0);
        _clock.Tick += (_, _) => UpdateClock();
        _clock.Start();
    }

    // ────────────────────────────────────────────────────────────────────────
    // UI Construction
    // ────────────────────────────────────────────────────────────────────────
    private void BuildUI()
    {
        SuspendLayout();

        Text           = "💍 Wedding Booking Management";
        Size           = new Size(1340, 840);
        MinimumSize    = new Size(1100, 680);   // prevents controls becoming unusable
        StartPosition  = FormStartPosition.CenterScreen;
        BackColor      = UITheme.Background;
        ForeColor      = UITheme.TextPrimary;
        Font           = UITheme.FontBody;
        DoubleBuffered = true;

        _navPanel     = BuildNavPanel();
        _contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = UITheme.Background };
        var statusBar = BuildStatusBar();

        // Dock order matters: Fill must be added BEFORE Left/Bottom docked panels
        // so WinForms docking engine processes them in the correct z-order.
        Controls.Add(_contentPanel);   // Fill
        Controls.Add(statusBar);       // Bottom
        Controls.Add(_navPanel);       // Left

        ResumeLayout(false);
        PerformLayout();
    }

    private Panel BuildNavPanel()
    {
        var nav = new Panel
        {
            Width     = 220,
            Dock      = DockStyle.Left,
            BackColor = UITheme.Surface,
        };
        nav.Paint += (_, e) =>
        {
            using var p = new Pen(UITheme.Border, 1);
            e.Graphics.DrawLine(p, nav.Width - 1, 0, nav.Width - 1, nav.Height);
        };

        // ── Logo area ─────────────────────────────────────────────────────
        // BEFORE: three Labels with absolute Left/Top → clipped at high DPI.
        // AFTER:  a TableLayoutPanel (2-col: icon | text stack) so the layout
        //         engine handles positioning and DPI scaling automatically.
        var logoPanel = new Panel
        {
            Height    = 72,
            Dock      = DockStyle.Top,
            BackColor = Color.Transparent,
            Padding   = new Padding(12, 8, 8, 0)
        };
        logoPanel.Paint += (_, e) =>
        {
            using var p = new Pen(UITheme.Border, 1);
            e.Graphics.DrawLine(p, 16, logoPanel.Height - 1, logoPanel.Width - 16, logoPanel.Height - 1);
        };

        // Two-column TLP: [icon col 44 px fixed] [text col fills rest]
        var logoTlp = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            ColumnCount = 2,
            RowCount    = 1,
            BackColor   = Color.Transparent,
            Margin      = new Padding(0)
        };
        logoTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 44F));
        logoTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        logoTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var logoIcon = new Label
        {
            Text      = "💍",
            Font      = new Font("Segoe UI", 22f),
            ForeColor = UITheme.AccentLight,
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent
        };

        // Vertical stack for title + subtitle
        var textStack = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            ColumnCount = 1,
            RowCount    = 2,
            BackColor   = Color.Transparent,
            Margin      = new Padding(0)
        };
        textStack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        textStack.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));
        textStack.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));

        var logoText = new Label
        {
            Text      = "WeddingBook",
            Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
            ForeColor = UITheme.TextPrimary,
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomLeft,
            BackColor = Color.Transparent,
            AutoEllipsis = false
        };
        var logoSub = new Label
        {
            Text      = "Management System",
            Font      = new Font("Segoe UI", 7.5f),
            ForeColor = UITheme.TextMuted,
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.TopLeft,
            BackColor = Color.Transparent,
            AutoEllipsis = false
        };
        textStack.Controls.Add(logoText, 0, 0);
        textStack.Controls.Add(logoSub,  0, 1);

        logoTlp.Controls.Add(logoIcon,  0, 0);
        logoTlp.Controls.Add(textStack, 1, 0);
        logoPanel.Controls.Add(logoTlp);

        // ── Nav Buttons ───────────────────────────────────────────────────
        // Buttons use Dock = Top so they already stretch to the nav panel width.
        _navBtns = new Button[NavItems.Length];
        var btnContainer = new Panel
        {
            Dock      = DockStyle.Fill,
            BackColor = Color.Transparent,
            Padding   = new Padding(8, 12, 8, 0)
        };

        for (int i = 0; i < NavItems.Length; i++)
        {
            var (icon, label) = NavItems[i];
            var idx = i;
            var btn = new Button
            {
                Text      = $"  {icon}  {label}",
                Dock      = DockStyle.Top,
                Height    = 46,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Font      = UITheme.FontBody,
                ForeColor = UITheme.TextMuted,
                BackColor = Color.Transparent,
                Cursor    = Cursors.Hand,
                Margin    = new Padding(0, 0, 0, 4)
            };
            btn.FlatAppearance.BorderSize         = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(35, UITheme.Accent);
            btn.Click += (_, _) => NavigateTo(idx);
            _navBtns[i] = btn;
            btnContainer.Controls.Add(btn);
        }

        // Reverse child index so DockStyle.Top stacks them top-to-bottom
        foreach (var b in _navBtns.Reverse()) btnContainer.Controls.SetChildIndex(b, 0);

        // ── Version Label ─────────────────────────────────────────────────
        var ver = new Label
        {
            Text      = "v1.0.0  •  net8.0-windows",
            Font      = new Font("Segoe UI", 7f),
            ForeColor = Color.FromArgb(55, 148, 163, 184),
            Dock      = DockStyle.Bottom,
            Height    = 24,
            TextAlign = ContentAlignment.MiddleCenter
        };

        nav.Controls.Add(btnContainer);
        nav.Controls.Add(ver);
        nav.Controls.Add(logoPanel);
        return nav;
    }

    private Panel BuildStatusBar()
    {
        var bar = new Panel
        {
            Dock      = DockStyle.Bottom,
            Height    = 30,
            BackColor = UITheme.Surface
        };
        bar.Paint += (_, e) =>
        {
            using var p = new Pen(UITheme.Border, 1);
            e.Graphics.DrawLine(p, 0, 0, bar.Width, 0);
        };

        _statusLabel = new Label
        {
            AutoSize  = false,
            Dock      = DockStyle.Fill,   // CHANGED: Fill instead of fixed Width so it uses all leftover space
            Height    = 30,
            Font      = new Font("Segoe UI", 8f),
            ForeColor = UITheme.TextMuted,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding   = new Padding(10, 0, 0, 0)
        };
        UpdateClock();

        var rightLbl = new Label
        {
            Text      = "Wedding Booking Management  •  All rights reserved",
            AutoSize  = false,
            Dock      = DockStyle.Right,
            Width     = 360,
            Height    = 30,
            Font      = new Font("Segoe UI", 7.5f),
            ForeColor = Color.FromArgb(55, 148, 163, 184),
            TextAlign = ContentAlignment.MiddleRight,
            Padding   = new Padding(0, 0, 12, 0)
        };

        // Right label must be added first so Dock=Right takes its slice before Fill
        bar.Controls.AddRange([rightLbl, _statusLabel]);
        return bar;
    }

    private void UpdateClock() =>
        _statusLabel.Text = DateTime.Now.ToString("dddd, MMMM dd yyyy   •   HH:mm:ss");

    // ────────────────────────────────────────────────────────────────────────
    // Navigation
    // ────────────────────────────────────────────────────────────────────────
    public void NavigateTo(int index)
    {
        if (_activeIdx == index) return;
        _activeIdx = index;

        for (int i = 0; i < _navBtns.Length; i++)
        {
            if (i == index)
            {
                _navBtns[i].BackColor = UITheme.NavActive;
                _navBtns[i].ForeColor = UITheme.AccentLight;
                _navBtns[i].Font      = UITheme.FontBodyBold;
            }
            else
            {
                _navBtns[i].BackColor = Color.Transparent;
                _navBtns[i].ForeColor = UITheme.TextMuted;
                _navBtns[i].Font      = UITheme.FontBody;
            }
        }

        SwapContent(index switch
        {
            0 => BuildDashboard(),
            1 => new VendorForm(),
            2 => new VenueForm(),
            3 => new DecorationForm(),
            4 => new ServicePackageForm(),
            5 => new BookingListForm(),
            _ => BuildDashboard()
        });
    }

    private void SwapContent(UserControl next)
    {
        _contentPanel.SuspendLayout();

        if (_currentView is not null)
        {
            _contentPanel.Controls.Remove(_currentView);
            _currentView.Dispose();
        }

        next.Dock = DockStyle.Fill;
        _contentPanel.Controls.Add(next);
        _currentView = next;

        _contentPanel.ResumeLayout();
    }

    // ────────────────────────────────────────────────────────────────────────
    // Dashboard
    // ────────────────────────────────────────────────────────────────────────
    private UserControl BuildDashboard()
    {
        var dash = new UserControl { BackColor = UITheme.Background };
        dash.SuspendLayout();

        // ── Heading strip (Top-docked, fixed height) ─────────────────────
        // BEFORE: heading/sub were absolutely positioned at Top = 40/80.
        // AFTER:  a dedicated top panel so the FlowLayoutPanel card area
        //         starts cleanly below and does not overlap the heading.
        var headingPanel = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 110,
            BackColor = UITheme.Background,
            Padding   = new Padding(36, 32, 36, 0)
        };

        var heading = new Label
        {
            Text      = "Welcome to WeddingBook",
            Font      = UITheme.FontLarge,
            ForeColor = UITheme.TextPrimary,
            AutoSize  = true,
            Dock      = DockStyle.Top
        };
        var sub = new Label
        {
            Text      = "Manage vendors, venues, packages and bookings — all in one place.",
            Font      = new Font("Segoe UI", 11f),
            ForeColor = UITheme.TextMuted,
            AutoSize  = true,
            Dock      = DockStyle.Top
        };
        // Sub added first so it appears below heading (DockStyle.Top stacks)
        headingPanel.Controls.Add(sub);
        headingPanel.Controls.Add(heading);

        // ── Cards — FlowLayoutPanel (replaces fixed-coordinate grid) ─────
        // BEFORE: Left = startX + col * (cardW + colGap) — static, never reflows.
        // AFTER:  FlowLayoutPanel WrapContents=true → cards reflow to a new row
        //         automatically when the content area is too narrow.
        //         Per the MS FlowLayoutPanel walkthrough docs.
        var cardFlow = new FlowLayoutPanel
        {
            Dock          = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents  = true,
            AutoScroll    = true,
            BackColor     = UITheme.Background,
            Padding       = new Padding(36, 20, 16, 16)
        };

        (string Icon, string Title, string Desc, int NavIdx, Color Accent)[] cards =
        {
            ("🏢", "Vendors",     "Manage your wedding vendors",          1, Color.FromArgb(124, 92,  191)),
            ("🏛", "Venues",      "Browse & manage event venues",         2, Color.FromArgb(56,  189, 248)),
            ("🌸", "Decorations", "Decoration package catalogue",         3, Color.FromArgb(236, 72,  153)),
            ("🎵", "Services",    "Service package catalogue",            4, Color.FromArgb(34,  197, 94)),
            ("📋", "Bookings",    "All bookings & status management",     5, Color.FromArgb(245, 158, 11))
        };

        foreach (var (icon, title, desc, navIdx, accent) in cards)
        {
            var card = BuildDashboardCard(icon, title, desc, navIdx, accent);
            cardFlow.Controls.Add(card);
        }

        dash.Controls.Add(cardFlow);      // Fill — add before Top-docked panel
        dash.Controls.Add(headingPanel);  // Top

        dash.ResumeLayout(false);
        return dash;
    }

    private Panel BuildDashboardCard(string icon, string title, string desc, int navIdx, Color accent)
    {
        // Cards keep a fixed Width/Height (260×140) — they don't grow individually.
        // The FlowLayoutPanel wraps them to new rows instead.
        // Margin provides the column gap (right=20) and row gap (bottom=20).
        var card = new Panel
        {
            Width     = 260,
            Height    = 140,
            BackColor = UITheme.Surface,
            Cursor    = Cursors.Hand,
            Margin    = new Padding(0, 0, 20, 20)
        };
        var localAccent = accent;
        card.Paint += (_, e) =>
        {
            var g = e.Graphics;
            using var borderPen  = new Pen(UITheme.Border, 1);
            g.DrawRectangle(borderPen, 0, 0, card.Width - 1, card.Height - 1);
            using var accentBrush = new SolidBrush(localAccent);
            g.FillRectangle(accentBrush, 0, card.Height - 4, card.Width, 4);
        };

        var lblIcon = new Label
        {
            Text      = icon,
            Font      = new Font("Segoe UI", 20f),
            ForeColor = UITheme.TextPrimary,
            AutoSize  = true,
            Left      = 16,
            Top       = 14,
            BackColor = Color.Transparent
        };
        var lblTitle = new Label
        {
            Text         = title,
            Font         = UITheme.FontSubHeading,
            ForeColor    = UITheme.TextPrimary,
            AutoSize     = true,      // CHANGED: AutoSize so text never clips
            Left         = 72,
            Top          = 20,
            BackColor    = Color.Transparent,
            AutoEllipsis = false
        };
        var lblDesc = new Label
        {
            Text         = desc,
            Font         = UITheme.FontSmall,
            ForeColor    = UITheme.TextMuted,
            AutoSize     = false,
            Width        = card.Width - 28,  // leave 12 px margin each side
            Height       = 36,
            Left         = 12,
            Top          = 68,
            BackColor    = Color.Transparent,
            AutoEllipsis = true             // graceful fallback if text is very long
        };
        var lblGo = new Label
        {
            Text      = "Click to manage  →",
            Font      = new Font("Segoe UI", 8f, FontStyle.Bold),
            ForeColor = localAccent,
            AutoSize  = true,
            Left      = 12,
            Top       = 112,
            BackColor = Color.Transparent
        };

        card.Controls.AddRange([lblIcon, lblTitle, lblDesc, lblGo]);

        // Wire click on every child label so the whole card is clickable
        foreach (Control c in card.Controls)
            c.Click += (_, _) => NavigateTo(navIdx);
        card.Click += (_, _) => NavigateTo(navIdx);

        return card;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _clock.Stop();
        base.OnFormClosing(e);
    }
}
