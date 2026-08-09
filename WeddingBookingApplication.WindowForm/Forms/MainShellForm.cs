using WeddingBookingApplication.WindowForm.Forms.Bookings;
using WeddingBookingApplication.WindowForm.Forms.Decorations;
using WeddingBookingApplication.WindowForm.Forms.ServicePackages;
using WeddingBookingApplication.WindowForm.Forms.Vendors;
using WeddingBookingApplication.WindowForm.Forms.Venues;
using WeddingBookingApplication.WindowForm.Helpers;

namespace WeddingBookingApplication.WindowForm.Forms;


public class MainShellForm : Form
{
    private Panel          _navPanel     = null!;
    private Panel          _contentPanel = null!;
    private Label          _statusLabel  = null!;
    private Button[]       _navBtns      = Array.Empty<Button>();
    private UserControl?   _currentView;
    private int            _activeIdx = -1;

    private readonly System.Windows.Forms.Timer _clock = new() { Interval = 1_000 };

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

        AutoScaleMode       = AutoScaleMode.Dpi;
        AutoScaleDimensions = new SizeF(96F, 96F);  

        BuildUI();
        NavigateTo(0);
        _clock.Tick += (_, _) => UpdateClock();
        _clock.Start();
    }

    private void BuildUI()
    {
        SuspendLayout();

        Text           = "💍 Wedding Booking Management";
        Size           = new Size(1340, 840);
        MinimumSize    = new Size(1100, 680);   
        StartPosition  = FormStartPosition.CenterScreen;
        BackColor      = UITheme.Background;
        ForeColor      = UITheme.TextPrimary;
        Font           = UITheme.FontBody;
        DoubleBuffered = true;

        _navPanel     = BuildNavPanel();
        _contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = UITheme.Background };
        var statusBar = BuildStatusBar();


        Controls.Add(_contentPanel);   
        Controls.Add(statusBar);       
        Controls.Add(_navPanel);       

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

        foreach (var b in _navBtns.Reverse()) btnContainer.Controls.SetChildIndex(b, 0);

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
            Dock      = DockStyle.Fill,   
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

        bar.Controls.AddRange([rightLbl, _statusLabel]);
        return bar;
    }

    private void UpdateClock() =>
        _statusLabel.Text = DateTime.Now.ToString("dddd, MMMM dd yyyy   •   HH:mm:ss");

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

    private UserControl BuildDashboard()
    {
        var dash = new UserControl { BackColor = UITheme.Background };
        dash.SuspendLayout();

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
        headingPanel.Controls.Add(sub);
        headingPanel.Controls.Add(heading);

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
        var card = new Panel
        {
            Width = 260,
            Height = 140,
            BackColor = UITheme.Surface,
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 0, 20, 20)
        };

        var localAccent = accent;

        card.Paint += (_, e) =>
        {
            var g = e.Graphics;
            using var borderPen = new Pen(UITheme.Border, 1);
            g.DrawRectangle(borderPen, 0, 0, card.Width - 1, card.Height - 1);
            using var accentBrush = new SolidBrush(localAccent);
            g.FillRectangle(accentBrush, 0, card.Height - 4, card.Width, 4);
        };

        var header = new TableLayoutPanel
        {
            ColumnCount = 2,
            RowCount = 1,
            Dock = DockStyle.Top,
            Height = 52,
            Padding = new Padding(14, 12, 12, 0),
            BackColor = Color.Transparent,
            Margin = new Padding(0)
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42F)); // fixed icon column
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        var lblIcon = new Label
        {
            Text = icon,
            Font = new Font("Segoe UI", 18f),
            ForeColor = UITheme.TextPrimary,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.Transparent
        };

        var lblTitle = new Label
        {
            Text = title,
            Font = UITheme.FontSubHeading,
            ForeColor = UITheme.TextPrimary,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = Color.Transparent,
            AutoEllipsis = true
        };

        header.Controls.Add(lblIcon, 0, 0);
        header.Controls.Add(lblTitle, 1, 0);

        var lblDesc = new Label
        {
            Text = desc,
            Font = UITheme.FontSmall,
            ForeColor = UITheme.TextMuted,
            AutoSize = false,
            Dock = DockStyle.Top,
            Height = 40,
            Padding = new Padding(16, 4, 12, 0),
            BackColor = Color.Transparent,
            AutoEllipsis = true
        };

        var lblGo = new Label
        {
            Text = "Click to manage →",
            Font = new Font("Segoe UI", 8f, FontStyle.Bold),
            ForeColor = localAccent,
            AutoSize = false,
            Dock = DockStyle.Bottom,
            Height = 28,
            Padding = new Padding(16, 0, 12, 8),
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = Color.Transparent
        };

        card.Controls.Add(lblGo);     
        card.Controls.Add(lblDesc);
        card.Controls.Add(header);    

        void WireClick(Control c)
        {
            c.Click += (_, _) => NavigateTo(navIdx);
            foreach (Control child in c.Controls)
                WireClick(child);
        }
        WireClick(card);

        return card;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _clock.Stop();
        base.OnFormClosing(e);
    }
}
