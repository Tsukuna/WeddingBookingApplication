using System.Drawing;
using System.Windows.Forms;

namespace WeddingBookingApplication.WindowForm.Helpers;

/// <summary>
/// Central design system: colours, fonts, and shared control-styling helpers.
/// </summary>
public static class UITheme
{
    // ── Colour Palette ──────────────────────────────────────────────────────
    public static readonly Color Background   = Color.FromArgb(15,  17,  23);
    public static readonly Color Surface      = Color.FromArgb(28,  31,  46);
    public static readonly Color SurfaceLight = Color.FromArgb(38,  42,  62);
    public static readonly Color Accent       = Color.FromArgb(124, 92,  191);
    public static readonly Color AccentLight  = Color.FromArgb(192, 132, 252);
    public static readonly Color AccentHover  = Color.FromArgb(100, 70,  165);
    public static readonly Color Success      = Color.FromArgb(34,  197, 94);
    public static readonly Color Warning      = Color.FromArgb(245, 158, 11);
    public static readonly Color Danger       = Color.FromArgb(239, 68,  68);
    public static readonly Color DangerHover  = Color.FromArgb(200, 40,  40);
    public static readonly Color TextPrimary  = Color.FromArgb(241, 245, 249);
    public static readonly Color TextMuted    = Color.FromArgb(148, 163, 184);
    public static readonly Color Border       = Color.FromArgb(46,  50,  80);
    public static readonly Color NavActive    = Color.FromArgb(45,  35,  75);

    // ── Fonts ───────────────────────────────────────────────────────────────
    public static readonly Font FontBody       = new("Segoe UI", 9.5f,  FontStyle.Regular);
    public static readonly Font FontBodyBold   = new("Segoe UI", 9.5f,  FontStyle.Bold);
    public static readonly Font FontSmall      = new("Segoe UI", 8.5f,  FontStyle.Regular);
    public static readonly Font FontSmallBold  = new("Segoe UI", 8.5f,  FontStyle.Bold);
    public static readonly Font FontSubHeading = new("Segoe UI", 11f,   FontStyle.Bold);
    public static readonly Font FontHeading    = new("Segoe UI", 15f,   FontStyle.Bold);
    public static readonly Font FontLarge      = new("Segoe UI", 20f,   FontStyle.Bold);

    // ── Control Stylers ─────────────────────────────────────────────────────

    public static void StyleTextBox(TextBox tb)
    {
        tb.BackColor   = SurfaceLight;
        tb.ForeColor   = TextPrimary;
        tb.BorderStyle = BorderStyle.FixedSingle;
        tb.Font        = FontBody;
    }

    public static void StyleNumericUpDown(NumericUpDown nud)
    {
        nud.BackColor = SurfaceLight;
        nud.ForeColor = TextPrimary;
        nud.Font      = FontBody;
    }

    public static void StyleComboBox(ComboBox cb)
    {
        cb.BackColor  = SurfaceLight;
        cb.ForeColor  = TextPrimary;
        cb.FlatStyle  = FlatStyle.Flat;
        cb.Font       = FontBody;
        cb.DropDownStyle = ComboBoxStyle.DropDownList;
    }

    public static void StyleDateTimePicker(DateTimePicker dtp)
    {
        dtp.CalendarForeColor       = TextPrimary;
        dtp.CalendarMonthBackground = Surface;
        dtp.CalendarTitleBackColor  = Accent;
        dtp.CalendarTitleForeColor  = TextPrimary;
        dtp.Font = FontBody;
    }

    public static void StyleButton(Button btn, Color? bg = null, Color? hover = null)
    {
        var backColor  = bg    ?? Accent;
        var hoverColor = hover ?? AccentHover;
        btn.BackColor  = backColor;
        btn.ForeColor  = TextPrimary;
        btn.FlatStyle  = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize           = 0;
        btn.FlatAppearance.MouseOverBackColor   = hoverColor;
        btn.FlatAppearance.MouseDownBackColor   = Color.FromArgb(
            Math.Max(0, hoverColor.R - 20),
            Math.Max(0, hoverColor.G - 20),
            Math.Max(0, hoverColor.B - 20));
        btn.Font   = FontBodyBold;
        btn.Cursor = Cursors.Hand;
    }

    public static void StyleDangerButton(Button btn)
        => StyleButton(btn, Danger, DangerHover);

    public static void StyleSuccessButton(Button btn)
        => StyleButton(btn, Success, Color.FromArgb(20, 160, 74));

    public static void StyleWarningButton(Button btn)
        => StyleButton(btn, Warning, Color.FromArgb(200, 130, 5));

    public static void StyleSecondaryButton(Button btn)
        => StyleButton(btn, SurfaceLight, Border);

    public static void StyleDataGridView(DataGridView dgv)
    {
        dgv.BackgroundColor  = Surface;
        dgv.BorderStyle      = BorderStyle.None;
        dgv.CellBorderStyle  = DataGridViewCellBorderStyle.SingleHorizontal;
        dgv.GridColor        = Border;
        dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

        dgv.DefaultCellStyle.BackColor         = Surface;
        dgv.DefaultCellStyle.ForeColor         = TextPrimary;
        dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(55, 124, 92, 191);
        dgv.DefaultCellStyle.SelectionForeColor = TextPrimary;
        dgv.DefaultCellStyle.Font              = FontBody;
        dgv.DefaultCellStyle.Padding           = new Padding(4, 0, 4, 0);

        dgv.ColumnHeadersDefaultCellStyle.BackColor        = Color.FromArgb(22, 18, 40);
        dgv.ColumnHeadersDefaultCellStyle.ForeColor        = AccentLight;
        dgv.ColumnHeadersDefaultCellStyle.Font             = FontSmallBold;
        dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(22, 18, 40);
        dgv.ColumnHeadersDefaultCellStyle.Padding          = new Padding(8, 0, 4, 0);

        dgv.AlternatingRowsDefaultCellStyle.BackColor = SurfaceLight;
        dgv.AlternatingRowsDefaultCellStyle.ForeColor = TextPrimary;

        dgv.EnableHeadersVisualStyles  = false;
        dgv.RowHeadersVisible          = false;
        dgv.SelectionMode              = DataGridViewSelectionMode.FullRowSelect;
        dgv.ReadOnly                   = true;
        dgv.AllowUserToAddRows         = false;
        dgv.AllowUserToDeleteRows      = false;
        dgv.MultiSelect                = false;
        dgv.ColumnHeadersHeight        = 38;
        dgv.RowTemplate.Height         = 34;
        dgv.AutoSizeColumnsMode        = DataGridViewAutoSizeColumnsMode.Fill;
        dgv.Cursor                     = Cursors.Hand;
    }

    public static void StyleCheckedListBox(CheckedListBox clb)
    {
        clb.BackColor      = SurfaceLight;
        clb.ForeColor      = TextPrimary;
        clb.BorderStyle    = BorderStyle.None;
        clb.Font           = FontBody;
        clb.CheckOnClick   = true;
    }

    /// <summary>Creates a styled section separator label.</summary>
    public static Label SectionLabel(string text) => new()
    {
        Text      = text,
        Font      = FontSmallBold,
        ForeColor = TextMuted,
        AutoSize  = true,
        BackColor = Color.Transparent
    };

    /// <summary>Creates a styled field label.</summary>
    public static Label FieldLabel(string text) => new()
    {
        Text      = text,
        Font      = FontSmall,
        ForeColor = TextMuted,
        AutoSize  = true,
        BackColor = Color.Transparent
    };

    /// <summary>Creates a badge Label with rounded feel (coloured back).</summary>
    public static Label StatusBadge(string text, Color back)
    {
        var lbl = new Label
        {
            Text      = $"  {text}  ",
            Font      = FontSmallBold,
            ForeColor = Color.White,
            BackColor = back,
            AutoSize  = true,
            TextAlign = ContentAlignment.MiddleCenter,
            Padding   = new Padding(2, 0, 2, 0)
        };
        return lbl;
    }

    /// <summary>
    /// Builds a two-pane split layout. Panel minimum sizes and the initial
    /// splitter position are applied only once the container has a real width,
    /// avoiding the WinForms crash where a SplitContainer panel is given large
    /// min sizes while it still has its tiny default size.
    /// </summary>
    public static SplitContainer BuildSplit(out Panel left, out Panel right,
        int panel1MinSize = 360, int panel2MinSize = 280)
    {
        var split = new SplitContainer
        {
            Dock          = DockStyle.Fill,
            BorderStyle   = BorderStyle.None,
            SplitterWidth = 2,
            BackColor     = Border
        };
        split.Panel1.BackColor = Background;
        split.Panel1.Padding   = new Padding(20, 16, 8, 16);
        split.Panel2.BackColor = Background;
        split.Panel2.Padding   = new Padding(8, 16, 20, 16);

        bool positioned = false;
        split.Resize += (_, _) =>
        {
            if (split.Width <= panel1MinSize + panel2MinSize) return;

            if (split.Panel1MinSize != panel1MinSize)
            {
                split.Panel1MinSize = panel1MinSize;
                split.Panel2MinSize = panel2MinSize;
            }

            if (positioned) return;
            positioned = true;
            split.SplitterDistance = Math.Clamp(
                (int)(split.Width * 0.68),
                panel1MinSize,
                split.Width - panel2MinSize);
        };

        left  = split.Panel1;
        right = split.Panel2;
        return split;
    }

    /// <summary>Returns the badge colour for a booking status byte.</summary>
    public static Color BookingStatusColor(byte status) => status switch
    {
        1 => Warning,                              // Pending
        2 => Success,                              // Approved
        3 => Danger,                               // Rejected
        4 => Color.FromArgb(100, 100, 110),        // Cancelled
        5 => Color.FromArgb(56, 189, 248),         // Completed
        _ => TextMuted
    };

    // ── Shared Responsive Layout Helpers ────────────────────────────────────

    /// <summary>
    /// Builds the standard page header panel (Dock=Top, Height=64) with icon,
    /// title and subtitle. Returns the panel so callers can add action buttons.
    /// Per MS docs: header is Top-docked so it never participates in resize math.
    /// </summary>
    public static Panel BuildPageHeader(string icon, string title, string sub)
    {
        var hdr = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 64,
            BackColor = Surface,
            Padding   = new Padding(20, 0, 20, 0)
        };
        hdr.Paint += (_, e) =>
        {
            using var p = new Pen(Border, 1);
            e.Graphics.DrawLine(p, 0, hdr.Height - 1, hdr.Width, hdr.Height - 1);
        };
        hdr.Controls.AddRange([
            new Label { Text = icon,  Font = new Font("Segoe UI", 18f), ForeColor = TextPrimary, AutoSize = true, Top = 14, Left = 20 },
            new Label { Text = title, Font = FontHeading,               ForeColor = TextPrimary, AutoSize = true, Top = 18, Left = 72 },
            new Label { Text = sub,   Font = FontSmall,                 ForeColor = TextMuted,   AutoSize = true, Top = 44, Left = 72 }
        ]);
        return hdr;
    }

    /// <summary>
    /// Builds an anchor-aware header action button and wires up the header's
    /// Resize event so the button stays right-aligned as the panel stretches.
    /// rightOffset: pixels from the right edge (first btn = 130, second = 244).
    /// </summary>
    public static Button AddHeaderButton(Panel header, string text, Color bg, int rightOffset)
    {
        var btn = new Button
        {
            Text   = text,
            Width  = rightOffset == 130 ? 110 : 100,
            Height = 34,
            Top    = 15,
            // Anchor keeps it glued to the right edge automatically
            Anchor = AnchorStyles.Right | AnchorStyles.Top
        };
        StyleButton(btn, bg);
        header.Controls.Add(btn);
        // Position on first layout; Anchor handles subsequent resizes
        header.Resize += (_, _) => btn.Left = header.Width - rightOffset;
        // Trigger once immediately so position is correct before first paint
        btn.Left = header.Width > 0 ? header.Width - rightOffset : 0;
        return btn;
    }

    /// <summary>
    /// Builds the right-side detail container: a Panel with AutoScroll=true and
    /// a vertically-stacked TableLayoutPanel inside it so all fields stretch to
    /// fill the panel width regardless of the SplitContainer position.
    /// Per MS Layout docs: use TableLayoutPanel for predictable column stretching.
    /// </summary>
    public static (Panel scrollPanel, TableLayoutPanel table) BuildDetailForm(Panel host)
    {
        var scroll = new Panel
        {
            Dock        = DockStyle.Fill,
            BackColor   = Surface,
            AutoScroll  = true,
            Padding     = new Padding(0)
        };
        scroll.Paint += (_, e) =>
        {
            using var pen = new Pen(Border, 1);
            e.Graphics.DrawRectangle(pen, 0, 0, scroll.Width - 1, scroll.Height - 1);
        };

        // Single-column TableLayoutPanel: column stretches to 100% of scroll panel width.
        var tbl = new TableLayoutPanel
        {
            Dock        = DockStyle.Top,       // grows downward; scroll panel clips it
            AutoSize    = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            Padding     = new Padding(20, 16, 20, 16),
            BackColor   = Color.Transparent,
        };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        scroll.Controls.Add(tbl);
        host.Controls.Add(scroll);
        return (scroll, tbl);
    }

    /// <summary>
    /// Adds a label+control pair as consecutive rows in a detail TableLayoutPanel.
    /// The control gets Dock=Fill so it always stretches to the panel width.
    /// </summary>
    public static T AddDetailRow<T>(TableLayoutPanel tbl, string labelText, T control)
        where T : Control
    {
        var lbl = FieldLabel(labelText);
        lbl.Margin = new Padding(0, 8, 0, 2);
        control.Dock   = DockStyle.Fill;
        control.Margin = new Padding(0, 0, 0, 0);

        tbl.RowCount++;
        tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tbl.Controls.Add(lbl);

        tbl.RowCount++;
        tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        tbl.Controls.Add(control);

        return control;
    }

    /// <summary>
    /// Builds a bottom-docked FlowLayoutPanel for action buttons (Save/Clear/Delete
    /// or Approve/Reject/Cancel/Complete). Buttons flow left-to-right with a gap.
    /// Per MS docs: FlowLayoutPanel with WrapContents=false keeps buttons on one row.
    /// </summary>
    public static FlowLayoutPanel BuildActionButtonRow(Panel parent, params Button[] buttons)
    {
        var flow = new FlowLayoutPanel
        {
            Dock          = DockStyle.Bottom,
            Height        = 52,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents  = false,
            BackColor     = Color.Transparent,
            Padding       = new Padding(20, 8, 8, 8),
            AutoSize      = false
        };
        foreach (var btn in buttons)
        {
            btn.Margin = new Padding(0, 0, 8, 0);
            btn.Anchor = AnchorStyles.None;  // let FlowLayoutPanel manage position
            flow.Controls.Add(btn);
        }
        parent.Controls.Add(flow);
        return flow;
    }

    /// <summary>
    /// Builds a TableLayoutPanel with equal-width columns for the Booking detail
    /// info section. Each cell gets Dock=Fill labels so text wraps rather than clips.
    /// Per MS docs: SizeType.Percent with equal values gives equal stretching columns.
    /// </summary>
    public static TableLayoutPanel BuildDetailInfoTable(Panel parent, int cols, int rows)
    {
        var tbl = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            ColumnCount = cols,
            RowCount    = rows,
            BackColor   = Color.Transparent,
            Padding     = new Padding(0, 8, 0, 0),
            CellBorderStyle = TableLayoutPanelCellBorderStyle.None
        };
        float pct = 100f / cols;
        for (int c = 0; c < cols; c++)
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, pct));
        for (int r = 0; r < rows; r++)
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 26f));
        parent.Controls.Add(tbl);
        return tbl;
    }
}
