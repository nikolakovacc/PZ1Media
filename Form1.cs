using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace GraphicsApp_PZ1
{
    public partial class Form1 : Form
    {

        // Стан вкладки 1: Примітиви та стилі

        private Color currentDrawColor = Color.Blue;
        private Color currentFillColor = Color.LightSkyBlue;
        private string currentPrimitive = "Пряма лінія";
        private DashStyle currentDashStyle = DashStyle.Solid;
        private string currentFillStyle = "Без заповнення";
        private Panel pnlDrawColorPreview = null!;
        private Panel pnlFillColorPreview = null!;


        // Стан вкладки 2: Афінні перетворення

        private PointF[] originalShape = new PointF[]
        {
            new PointF(250, 150),
            new PointF(350, 150),
            new PointF(350, 250),
            new PointF(380, 250),
            new PointF(300, 320), // Вершина стрілки
            new PointF(220, 250),
            new PointF(250, 250)
        };
        private PointF pivotPoint = new PointF(300, 230);
        private bool setPivotOnClick = false;
        private float transX = 0, transY = 0;
        private float scaleX = 1.0f, scaleY = 1.0f;
        private float rotationAngle = 0;


        // Стан вкладки 3: Зображення та колірні схеми

        private Bitmap baseImage = null!;
        private Bitmap processedImage = null!;

        public Form1()
        {
            InitializeComponent();
            SetupCustomUI();
        }

        private void SetupCustomUI()
        {
            this.Text = "ПЗ 1 - Комп'ютерна графіка, афінні перетворення та стиснення";
            this.Size = new Size(1150, 780);
            this.StartPosition = FormStartPosition.CenterScreen;

            TabControl tabControl = new TabControl { Dock = DockStyle.Fill };

            // 1. ВКЛАДКА: Примітиви, стилі та фрактал (п. 6, п. 10)
            TabPage tab1 = new TabPage("1. Примітиви, стилі та кольори (п. 6, 10)");
            SetupTab1(tab1);

            // 2. ВКЛАДКА: Афінні перетворення (п. 7)
            TabPage tab2 = new TabPage("2. Афінні перетворення (п. 7)");
            SetupTab2(tab2);

            // 3. ВКЛАДКА: Колір, яскравість та Side-by-Side (п. 11, 12)
            TabPage tab3 = new TabPage("3. Колірні схеми та яскравість (п. 11-12)");
            SetupTab3(tab3);

            // 4. ВКЛАДКА: Алгоритми стиснення (п. 13)
            TabPage tab4 = new TabPage("4. Дослідження стиснення (п. 13)");
            SetupTab4(tab4);

            tabControl.TabPages.Add(tab1);
            tabControl.TabPages.Add(tab2);
            tabControl.TabPages.Add(tab3);
            tabControl.TabPages.Add(tab4);

            this.Controls.Add(tabControl);
        }


        // РЕАЛІЗАЦІЯ ВКЛАДКИ 1: Примітиви, лінії, стилі, заповнення, текстури

        private PictureBox pbCanvas1 = null!;
        private void SetupTab1(TabPage tab)
        {
            Panel sidePanel = new Panel { Dock = DockStyle.Left, Width = 310, Padding = new Padding(10) };
            pbCanvas1 = new PictureBox { Dock = DockStyle.Fill, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

            int y = 10;
            Label lblPrim = new Label { Text = "Оберіть графічний примітив (п. 6, 10):", Location = new Point(10, y), AutoSize = true, Font = new Font(this.Font, FontStyle.Bold) };
            sidePanel.Controls.Add(lblPrim);
            y += 25;

            ComboBox cbPrimitives = new ComboBox { Location = new Point(10, y), Width = 280, DropDownStyle = ComboBoxStyle.DropDownList };
            cbPrimitives.Items.AddRange(new object[] {
                "Пряма лінія",
                "Коло",
                "Еліпс",
                "Крива Безьє",
                "Складна фігура (Будинок)",
                "Фігура (Зірка)",
                "Попіксельна растеризація Брезенгема (лінія + коло)"
            });
            cbPrimitives.SelectedIndex = 0;
            cbPrimitives.SelectedIndexChanged += (s, e) => {
                currentPrimitive = cbPrimitives.SelectedItem?.ToString() ?? "";
                pbCanvas1.Image = null; // Скидаємо фрактал при зміні примітиву
                pbCanvas1.Invalidate();
            };
            sidePanel.Controls.Add(cbPrimitives);
            y += 35;

            Label lblLineStyle = new Label { Text = "Стиль лінії (DashStyle):", Location = new Point(10, y), AutoSize = true, Font = new Font(this.Font, FontStyle.Bold) };
            sidePanel.Controls.Add(lblLineStyle);
            y += 22;

            ComboBox cbLineStyle = new ComboBox { Location = new Point(10, y), Width = 280, DropDownStyle = ComboBoxStyle.DropDownList };
            cbLineStyle.Items.AddRange(new object[] { "Суцільна (Solid)", "Штрихова (Dash)", "Пунктирна (Dot)", "Штрих-пунктир (DashDot)" });
            cbLineStyle.SelectedIndex = 0;
            cbLineStyle.SelectedIndexChanged += (s, e) =>
            {
                currentDashStyle = cbLineStyle.SelectedIndex switch
                {
                    1 => DashStyle.Dash,
                    2 => DashStyle.Dot,
                    3 => DashStyle.DashDot,
                    _ => DashStyle.Solid
                };
                pbCanvas1.Image = null;
                pbCanvas1.Invalidate();
            };
            sidePanel.Controls.Add(cbLineStyle);
            y += 35;

            Label lblFillStyle = new Label { Text = "Стиль заповнення (фігури / кола / еліпса):", Location = new Point(10, y), AutoSize = true, Font = new Font(this.Font, FontStyle.Bold) };
            sidePanel.Controls.Add(lblFillStyle);
            y += 22;

            ComboBox cbFillStyle = new ComboBox { Location = new Point(10, y), Width = 280, DropDownStyle = ComboBoxStyle.DropDownList };
            cbFillStyle.Items.AddRange(new object[] {
                "Без заповнення",
                "Суцільний колір",
                "Штрихування (Hatch сітка)",
                "Штрихування (Діагональне)",
                "Текстура (шахова сітка)"
            });
            cbFillStyle.SelectedIndex = 0;
            cbFillStyle.SelectedIndexChanged += (s, e) => {
                currentFillStyle = cbFillStyle.SelectedItem?.ToString() ?? "";
                pbCanvas1.Image = null;
                pbCanvas1.Invalidate();
            };
            sidePanel.Controls.Add(cbFillStyle);
            y += 35;

            // Вибір кольору лінії
            Button btnColor = new Button { Text = "🎨 Змінити колір контуру...", Location = new Point(10, y), Width = 230, Height = 32 };
            pnlDrawColorPreview = new Panel { Location = new Point(245, y + 2), Width = 45, Height = 28, BackColor = currentDrawColor, BorderStyle = BorderStyle.Fixed3D };
            btnColor.Click += (s, e) =>
            {
                using ColorDialog cd = new ColorDialog { Color = currentDrawColor };
                if (cd.ShowDialog() == DialogResult.OK)
                {
                    currentDrawColor = cd.Color;
                    pnlDrawColorPreview.BackColor = currentDrawColor;
                    pbCanvas1.Image = null;
                    pbCanvas1.Invalidate();
                }
            };
            sidePanel.Controls.Add(btnColor);
            sidePanel.Controls.Add(pnlDrawColorPreview);
            y += 40;

            // Вибір кольору заповнення
            Button btnFillColor = new Button { Text = "🖌️ Змінити колір заливки...", Location = new Point(10, y), Width = 230, Height = 32 };
            pnlFillColorPreview = new Panel { Location = new Point(245, y + 2), Width = 45, Height = 28, BackColor = currentFillColor, BorderStyle = BorderStyle.Fixed3D };
            btnFillColor.Click += (s, e) =>
            {
                using ColorDialog cd = new ColorDialog { Color = currentFillColor };
                if (cd.ShowDialog() == DialogResult.OK)
                {
                    currentFillColor = cd.Color;
                    pnlFillColorPreview.BackColor = currentFillColor;
                    pbCanvas1.Image = null;
                    pbCanvas1.Invalidate();
                }
            };
            sidePanel.Controls.Add(btnFillColor);
            sidePanel.Controls.Add(pnlFillColorPreview);
            y += 45;

            // Кнопка фракталу
            Button btnMandelbrot = new Button {
                Text = "✨ Побудувати фрактал Мандельброта",
                Location = new Point(10, y),
                Width = 280,
                Height = 42,
                BackColor = Color.DarkSlateBlue,
                ForeColor = Color.White,
                Font = new Font(this.Font, FontStyle.Bold)
            };
            btnMandelbrot.Click += (s, e) =>
            {
                Cursor = Cursors.WaitCursor;
                Bitmap fractal = GraphicsAlgorithms.GenerateMandelbrot(pbCanvas1.Width, pbCanvas1.Height, 80);
                pbCanvas1.Image = fractal;
                Cursor = Cursors.Default;
            };
            sidePanel.Controls.Add(btnMandelbrot);
            y += 50;

            Button btnClear = new Button { Text = "Очистити полотно", Location = new Point(10, y), Width = 280, Height = 32 };
            btnClear.Click += (s, e) => { pbCanvas1.Image = null; pbCanvas1.Invalidate(); };
            sidePanel.Controls.Add(btnClear);

            pbCanvas1.Paint += OnCanvas1Paint;

            tab.Controls.Add(pbCanvas1);
            tab.Controls.Add(sidePanel);
        }

        private void OnCanvas1Paint(object? sender, PaintEventArgs e)
        {
            if (pbCanvas1.Image != null) return; // Якщо відображається фрактал Мандельброта

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Створюємо перо з поточним стилем лінії та кольором
            using Pen pen = new Pen(currentDrawColor, 4) { DashStyle = currentDashStyle };
            Brush? fillBrush = GetCurrentBrush();

            int w = pbCanvas1.Width;
            int h = pbCanvas1.Height;

            switch (currentPrimitive)
            {
                case "Пряма лінія":
                    // Малюємо кілька ліній для демонстрації стилю та кольору
                    g.DrawLine(pen, 60, 100, w - 80, 100);
                    g.DrawLine(pen, 60, 200, w - 80, 300);
                    g.DrawLine(pen, 60, h - 100, w - 80, 150);
                    g.DrawString($"Прямі лінії зі стилем: {currentDashStyle}, колір: #{currentDrawColor.R:X2}{currentDrawColor.G:X2}{currentDrawColor.B:X2}", Font, Brushes.Black, 20, 20);
                    break;

                case "Коло":
                    int r = Math.Min(w, h) / 3;
                    Rectangle circleRect = new Rectangle(w / 2 - r, h / 2 - r, r * 2, r * 2);
                    if (fillBrush != null) g.FillEllipse(fillBrush, circleRect);
                    g.DrawEllipse(pen, circleRect);
                    g.DrawString($"Коло: стиль лінії = {currentDashStyle}, стиль заповнення = {currentFillStyle}", Font, Brushes.Black, 20, 20);
                    break;

                case "Еліпс":
                    Rectangle rect = new Rectangle(w / 4, h / 4, w / 2, h / 3);
                    if (fillBrush != null) g.FillEllipse(fillBrush, rect);
                    g.DrawEllipse(pen, rect);
                    g.DrawString($"Еліпс: стиль лінії = {currentDashStyle}, стиль заповнення = {currentFillStyle}", Font, Brushes.Black, 20, 20);
                    break;

                case "Крива Безьє":
                    Point p1 = new Point(60, h / 2);
                    Point p2 = new Point(w / 3, 60);
                    Point p3 = new Point(2 * w / 3, h - 60);
                    Point p4 = new Point(w - 60, h / 2);
                    g.DrawBezier(pen, p1, p2, p3, p4);
                    // Опорні точки Безьє
                    g.FillEllipse(Brushes.Red, p2.X - 5, p2.Y - 5, 10, 10);
                    g.FillEllipse(Brushes.Red, p3.X - 5, p3.Y - 5, 10, 10);
                    g.DrawLine(Pens.LightGray, p1, p2);
                    g.DrawLine(Pens.LightGray, p4, p3);
                    g.DrawString($"Крива Безьє 3-го порядку зі стилем: {currentDashStyle}", Font, Brushes.Black, 20, 20);
                    break;

                case "Складна фігура (Будинок)":
                    // Стіни будинку
                    Rectangle houseRect = new Rectangle(w / 2 - 140, h / 2 - 40, 280, 180);
                    if (fillBrush != null) g.FillRectangle(fillBrush, houseRect);
                    g.DrawRectangle(pen, houseRect);

                    // Дах будинку (трикутник)
                    Point[] roof = {
                        new Point(w / 2 - 160, h / 2 - 40),
                        new Point(w / 2, h / 2 - 160),
                        new Point(w / 2 + 160, h / 2 - 40)
                    };
                    if (fillBrush != null) g.FillPolygon(fillBrush, roof);
                    g.DrawPolygon(pen, roof);
                    g.DrawString($"Складна фігура (будинок): стиль лінії = {currentDashStyle}, заповнення = {currentFillStyle}", Font, Brushes.Black, 20, 20);
                    break;

                case "Фігура (Зірка)":
                    PointF[] star = CreateStarPoints(w / 2, h / 2, 140, 60, 5);
                    if (fillBrush != null) g.FillPolygon(fillBrush, star);
                    g.DrawPolygon(pen, star);
                    g.DrawString($"5-променева зірка: стиль лінії = {currentDashStyle}, заповнення = {currentFillStyle}", Font, Brushes.Black, 20, 20);
                    break;

                case "Попіксельна растеризація Брезенгема (лінія + коло)":
                    using (Bitmap bmp = new Bitmap(w, h))
                    {
                        GraphicsAlgorithms.DrawBresenhamLine(bmp, 60, 60, w - 80, h - 80, currentDrawColor);
                        GraphicsAlgorithms.DrawBresenhamLine(bmp, 60, h - 80, w - 80, 60, Color.Red);
                        GraphicsAlgorithms.DrawBresenhamCircle(bmp, w / 2, h / 2, Math.Min(w, h) / 3, currentDrawColor);
                        g.DrawImage(bmp, 0, 0);
                    }
                    g.DrawString("Низькорівневий алгоритм Брезенгема: растеризація відрізків і кола на рівні пікселів", Font, Brushes.Black, 20, 20);
                    break;
            }

            fillBrush?.Dispose();
        }

        private Brush? GetCurrentBrush()
        {
            return currentFillStyle switch
            {
                "Суцільний колір" => new SolidBrush(currentFillColor),
                "Штрихування (Hatch сітка)" => new HatchBrush(HatchStyle.Cross, currentDrawColor, currentFillColor),
                "Штрихування (Діагональне)" => new HatchBrush(HatchStyle.ForwardDiagonal, currentDrawColor, currentFillColor),
                "Текстура (шахова сітка)" => CreateCheckerBrush(currentFillColor, Color.White),
                _ => null
            };
        }

        private TextureBrush CreateCheckerBrush(Color c1, Color c2)
        {
            Bitmap bmp = new Bitmap(16, 16);
            using Graphics g = Graphics.FromImage(bmp);
            using Brush b1 = new SolidBrush(c1);
            using Brush b2 = new SolidBrush(c2);
            g.FillRectangle(b1, 0, 0, 8, 8);
            g.FillRectangle(b2, 8, 0, 8, 8);
            g.FillRectangle(b2, 0, 8, 8, 8);
            g.FillRectangle(b1, 8, 8, 8, 8);
            return new TextureBrush(bmp);
        }

        private PointF[] CreateStarPoints(float cx, float cy, float rOuter, float rInner, int numPoints)
        {
            PointF[] pts = new PointF[numPoints * 2];
            double step = Math.PI / numPoints;
            double angle = -Math.PI / 2;

            for (int i = 0; i < numPoints * 2; i++)
            {
                float r = (i % 2 == 0) ? rOuter : rInner;
                pts[i] = new PointF((float)(cx + r * Math.Cos(angle)), (float)(cy + r * Math.Sin(angle)));
                angle += step;
            }
            return pts;
        }

        // =============================================================
        // РЕАЛІЗАЦІЯ ВКЛАДКИ 2: Афінні перетворення
        // =============================================================
        private PictureBox pbCanvas2 = null!;
        private NumericUpDown nudTransX = null!;
        private NumericUpDown nudTransY = null!;
        private NumericUpDown nudScaleX = null!;
        private NumericUpDown nudScaleY = null!;
        private NumericUpDown nudAngle = null!;
        private Label lblPivotCoords = null!;

        private void SetupTab2(TabPage tab)
        {
            Panel sidePanel = new Panel { Dock = DockStyle.Left, Width = 300, Padding = new Padding(10) };
            pbCanvas2 = new PictureBox { Dock = DockStyle.Fill, BackColor = Color.WhiteSmoke, BorderStyle = BorderStyle.FixedSingle };

            int y = 10;
            Label title = new Label { Text = "Параметри афінних перетворень:", Location = new Point(10, y), AutoSize = true, Font = new Font(this.Font, FontStyle.Bold) };
            sidePanel.Controls.Add(title);
            y += 30;

            // Зсув
            Label lblT = new Label { Text = "1. Переміщення (dx, dy):", Location = new Point(10, y), AutoSize = true };
            sidePanel.Controls.Add(lblT);
            y += 22;

            nudTransX = new NumericUpDown { Location = new Point(10, y), Width = 110, Minimum = -500, Maximum = 500, Value = 0 };
            nudTransY = new NumericUpDown { Location = new Point(130, y), Width = 110, Minimum = -500, Maximum = 500, Value = 0 };
            nudTransX.ValueChanged += OnAffineParamsChanged;
            nudTransY.ValueChanged += OnAffineParamsChanged;
            sidePanel.Controls.Add(nudTransX);
            sidePanel.Controls.Add(nudTransY);
            y += 35;

            // Масштаб
            Label lblS = new Label { Text = "2. Масштабування (Sx, Sy):", Location = new Point(10, y), AutoSize = true };
            sidePanel.Controls.Add(lblS);
            y += 22;

            nudScaleX = new NumericUpDown { Location = new Point(10, y), Width = 110, Minimum = 0.1M, Maximum = 5.0M, DecimalPlaces = 2, Increment = 0.1M, Value = 1.0M };
            nudScaleY = new NumericUpDown { Location = new Point(130, y), Width = 110, Minimum = 0.1M, Maximum = 5.0M, DecimalPlaces = 2, Increment = 0.1M, Value = 1.0M };
            nudScaleX.ValueChanged += OnAffineParamsChanged;
            nudScaleY.ValueChanged += OnAffineParamsChanged;
            sidePanel.Controls.Add(nudScaleX);
            sidePanel.Controls.Add(nudScaleY);
            y += 35;

            // Поворот
            Label lblR = new Label { Text = "3. Кут повороту (градуси):", Location = new Point(10, y), AutoSize = true };
            sidePanel.Controls.Add(lblR);
            y += 22;

            nudAngle = new NumericUpDown { Location = new Point(10, y), Width = 230, Minimum = -360, Maximum = 360, Value = 0 };
            nudAngle.ValueChanged += OnAffineParamsChanged;
            sidePanel.Controls.Add(nudAngle);
            y += 35;

            // Точка обертання
            Label lblP = new Label { Text = "4. Точка повороту (Pivot):", Location = new Point(10, y), AutoSize = true, Font = new Font(this.Font, FontStyle.Bold) };
            sidePanel.Controls.Add(lblP);
            y += 22;

            lblPivotCoords = new Label { Text = $"X: {pivotPoint.X:F0}, Y: {pivotPoint.Y:F0}", Location = new Point(10, y), AutoSize = true, ForeColor = Color.DarkRed };
            sidePanel.Controls.Add(lblPivotCoords);
            y += 25;

            CheckBox cbClickPivot = new CheckBox { Text = "Встановити точку кліком на полотні", Location = new Point(10, y), Width = 260, AutoSize = true, Checked = true };
            cbClickPivot.CheckedChanged += (s, e) => setPivotOnClick = cbClickPivot.Checked;
            setPivotOnClick = true;
            sidePanel.Controls.Add(cbClickPivot);
            y += 40;

            Button btnReset = new Button { Text = "Скинути перетворення", Location = new Point(10, y), Width = 250, Height = 35 };
            btnReset.Click += (s, e) =>
            {
                nudTransX.Value = 0;
                nudTransY.Value = 0;
                nudScaleX.Value = 1.0M;
                nudScaleY.Value = 1.0M;
                nudAngle.Value = 0;
                pivotPoint = new PointF(300, 230);
                lblPivotCoords.Text = $"X: {pivotPoint.X:F0}, Y: {pivotPoint.Y:F0}";
                pbCanvas2.Invalidate();
            };
            sidePanel.Controls.Add(btnReset);

            pbCanvas2.Paint += OnCanvas2Paint;
            pbCanvas2.MouseClick += (s, e) =>
            {
                if (setPivotOnClick)
                {
                    pivotPoint = new PointF(e.X, e.Y);
                    lblPivotCoords.Text = $"X: {pivotPoint.X:F0}, Y: {pivotPoint.Y:F0}";
                    pbCanvas2.Invalidate();
                }
            };

            tab.Controls.Add(pbCanvas2);
            tab.Controls.Add(sidePanel);
        }

        private void OnAffineParamsChanged(object? sender, EventArgs e)
        {
            transX = (float)nudTransX.Value;
            transY = (float)nudTransY.Value;
            scaleX = (float)nudScaleX.Value;
            scaleY = (float)nudScaleY.Value;
            rotationAngle = (float)nudAngle.Value;
            pbCanvas2.Invalidate();
        }

        private void OnCanvas2Paint(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 1. Координатна сітка
            using Pen gridPen = new Pen(Color.FromArgb(220, 220, 220), 1);
            for (int x = 0; x < pbCanvas2.Width; x += 50) g.DrawLine(gridPen, x, 0, x, pbCanvas2.Height);
            for (int y = 0; y < pbCanvas2.Height; y += 50) g.DrawLine(gridPen, 0, y, pbCanvas2.Width, y);

            // 2. Оригінальна фігура (пунктирний сірий контур)
            using Pen origPen = new Pen(Color.Gray, 1) { DashStyle = DashStyle.Dash };
            g.DrawPolygon(origPen, originalShape);

            // 3. Афінне перетворення
            PointF[] transformed = GraphicsAlgorithms.ApplyAffineTransform(originalShape, transX, transY, scaleX, scaleY, rotationAngle, pivotPoint);

            // 4. Трансформована фігура
            using Brush fill = new SolidBrush(Color.FromArgb(180, Color.CornflowerBlue));
            using Pen shapePen = new Pen(Color.DarkBlue, 2);
            g.FillPolygon(fill, transformed);
            g.DrawPolygon(shapePen, transformed);

            // 5. Точка обертання (Pivot Point)
            g.FillEllipse(Brushes.Red, pivotPoint.X - 6, pivotPoint.Y - 6, 12, 12);
            g.DrawLine(Pens.Black, pivotPoint.X - 10, pivotPoint.Y, pivotPoint.X + 10, pivotPoint.Y);
            g.DrawLine(Pens.Black, pivotPoint.X, pivotPoint.Y - 10, pivotPoint.X, pivotPoint.Y + 10);
            g.DrawString($"Точка повороту ({pivotPoint.X:F0}, {pivotPoint.Y:F0})", Font, Brushes.DarkRed, pivotPoint.X + 10, pivotPoint.Y - 15);

            g.DrawString("Сірий пунктир = Початковий стан\nСиній полігон = Після афінних перетворень", Font, Brushes.Black, 15, 15);
        }

        // =============================================================
        // РЕАЛІЗАЦІЯ ВКЛАДКИ 3: Колір, яскравість та Side-by-Side (п. 11-12)
        // =============================================================
        private PictureBox pbOrig = null!;
        private PictureBox pbProcessed = null!;
        private TrackBar tbBrightness = null!;
        private TrackBar tbContrast = null!;
        private Label lblBrightVal = null!;
        private Label lblContrastVal = null!;

        private void SetupTab3(TabPage tab)
        {
            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 130, Padding = new Padding(10), BackColor = Color.FromArgb(245, 245, 245) };
            SplitContainer split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical };

            pbOrig = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };
            pbProcessed = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White };

            Label lblLeft = new Label { Text = "Оригінальне зображення (RGB)", Dock = DockStyle.Top, Height = 25, TextAlign = ContentAlignment.MiddleCenter, Font = new Font(this.Font, FontStyle.Bold) };
            Label lblRight = new Label { Text = "Конвертоване зображення / Зміна яскравості", Dock = DockStyle.Top, Height = 25, TextAlign = ContentAlignment.MiddleCenter, Font = new Font(this.Font, FontStyle.Bold) };

            split.Panel1.Controls.Add(pbOrig);
            split.Panel1.Controls.Add(lblLeft);
            split.Panel2.Controls.Add(pbProcessed);
            split.Panel2.Controls.Add(lblRight);

            // Кнопки вибору колірних схем
            int x = 10;
            Button btnLoad = new Button { Text = "📂 Завантажити фото...", Location = new Point(x, 15), Width = 150, Height = 30 };
            btnLoad.Click += (s, e) =>
            {
                using OpenFileDialog ofd = new OpenFileDialog { Filter = "Зображення (*.png;*.jpg;*.bmp)|*.png;*.jpg;*.bmp" };
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    baseImage = new Bitmap(ofd.FileName);
                    processedImage = new Bitmap(baseImage);
                    pbOrig.Image = baseImage;
                    pbProcessed.Image = processedImage;
                }
            };
            topPanel.Controls.Add(btnLoad);
            x += 160;

            Button btnGray = new Button { Text = "RGB → Grayscale", Location = new Point(x, 15), Width = 130, Height = 30 };
            btnGray.Click += (s, e) =>
            {
                if (baseImage == null) return;
                processedImage = GraphicsAlgorithms.ConvertToGrayscale(baseImage);
                pbProcessed.Image = processedImage;
            };
            topPanel.Controls.Add(btnGray);
            x += 140;

            Button btnHsv = new Button { Text = "RGB → HSV Візуалізація", Location = new Point(x, 15), Width = 160, Height = 30 };
            btnHsv.Click += (s, e) =>
            {
                if (baseImage == null) return;
                processedImage = GraphicsAlgorithms.ConvertToHSVRepresentation(baseImage);
                pbProcessed.Image = processedImage;
            };
            topPanel.Controls.Add(btnHsv);
            x += 170;

            Button btnGenTest = new Button { Text = "Згенерувати тестове зображення", Location = new Point(x, 15), Width = 210, Height = 30 };
            btnGenTest.Click += (s, e) =>
            {
                baseImage = GenerateTestCard(400, 300);
                processedImage = new Bitmap(baseImage);
                pbOrig.Image = baseImage;
                pbProcessed.Image = processedImage;
            };
            topPanel.Controls.Add(btnGenTest);

            // Повзунки яскравості та освітленості
            int y2 = 60;
            Label lblB = new Label { Text = "Яскравість (п. 11):", Location = new Point(10, y2), AutoSize = true };
            topPanel.Controls.Add(lblB);

            tbBrightness = new TrackBar { Location = new Point(130, y2 - 5), Width = 200, Minimum = -100, Maximum = 100, Value = 0, TickFrequency = 20 };
            lblBrightVal = new Label { Text = "0", Location = new Point(340, y2), AutoSize = true };
            tbBrightness.Scroll += OnLightnessChanged;
            topPanel.Controls.Add(tbBrightness);
            topPanel.Controls.Add(lblBrightVal);

            Label lblC = new Label { Text = "Контраст / Освітленість:", Location = new Point(400, y2), AutoSize = true };
            topPanel.Controls.Add(lblC);

            tbContrast = new TrackBar { Location = new Point(560, y2 - 5), Width = 200, Minimum = 2, Maximum = 30, Value = 10, TickFrequency = 5 };
            lblContrastVal = new Label { Text = "1.0x", Location = new Point(770, y2), AutoSize = true };
            tbContrast.Scroll += OnLightnessChanged;
            topPanel.Controls.Add(tbContrast);
            topPanel.Controls.Add(lblContrastVal);

            tab.Controls.Add(split);
            tab.Controls.Add(topPanel);

            // Початкове тестове зображення
            baseImage = GenerateTestCard(400, 300);
            processedImage = new Bitmap(baseImage);
            pbOrig.Image = baseImage;
            pbProcessed.Image = processedImage;
        }

        private void OnLightnessChanged(object? sender, EventArgs e)
        {
            if (baseImage == null) return;
            int b = tbBrightness.Value;
            float c = tbContrast.Value / 10.0f;
            lblBrightVal.Text = b.ToString();
            lblContrastVal.Text = $"{c:F1}x";

            processedImage = GraphicsAlgorithms.AdjustBrightnessContrast(baseImage, b, c);
            pbProcessed.Image = processedImage;
        }

        private Bitmap GenerateTestCard(int w, int h)
        {
            Bitmap bmp = new Bitmap(w, h);
            using Graphics g = Graphics.FromImage(bmp);
            Color[] colors = { Color.Red, Color.Green, Color.Blue, Color.Yellow, Color.Cyan, Color.Magenta, Color.Orange, Color.Purple };
            int step = w / colors.Length;
            for (int i = 0; i < colors.Length; i++)
            {
                using Brush b = new SolidBrush(colors[i]);
                g.FillRectangle(b, i * step, 0, step, h / 2);
            }
            using (LinearGradientBrush lgb = new LinearGradientBrush(new Rectangle(0, h / 2, w, h / 2), Color.Black, Color.White, 0f))
            {
                g.FillRectangle(lgb, 0, h / 2, w, h / 2);
            }
            g.FillEllipse(Brushes.White, w / 2 - 40, h / 2 - 40, 80, 80);
            g.DrawEllipse(Pens.Black, w / 2 - 40, h / 2 - 40, 80, 80);
            return bmp;
        }

        // =============================================================
        // РЕАЛІЗАЦІЯ ВКЛАДКИ 4: Дослідження алгоритмів стиснення (п. 13)
        // =============================================================
        private DataGridView dgvResults = null!;

        private void SetupTab4(TabPage tab)
        {
            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 70, Padding = new Padding(10) };
            dgvResults = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                ReadOnly = true,
                AllowUserToAddRows = false
            };

            dgvResults.Columns.Add("No", "№ п/п");
            dgvResults.Columns.Add("Algorithm", "Назва алгоритму стиснення");
            dgvResults.Columns.Add("Time", "Час роботи (мс)");
            dgvResults.Columns.Add("Sizes", "Розмір до стиску / Розмір після стискання");
            dgvResults.Columns.Add("Ratio", "Коефіцієнт стиснення (%)");
            dgvResults.Columns.Add("Library", "Мова програмування та бібліотеки");

            dgvResults.Columns[0].FillWeight = 30;
            dgvResults.Columns[1].FillWeight = 100;
            dgvResults.Columns[2].FillWeight = 60;
            dgvResults.Columns[3].FillWeight = 120;
            dgvResults.Columns[4].FillWeight = 70;
            dgvResults.Columns[5].FillWeight = 140;

            Button btnRunCompression = new Button
            {
                Text = "🚀 Запустити бенчмарк алгоритмів стиснення (RLE, LZW, JPEG)",
                Location = new Point(10, 15),
                Width = 450,
                Height = 40,
                BackColor = Color.ForestGreen,
                ForeColor = Color.White,
                Font = new Font(this.Font, FontStyle.Bold)
            };
            btnRunCompression.Click += (s, e) => RunCompressionBenchmark();
            topPanel.Controls.Add(btnRunCompression);

            tab.Controls.Add(dgvResults);
            tab.Controls.Add(topPanel);
        }

        private void RunCompressionBenchmark()
        {
            dgvResults.Rows.Clear();
            Cursor = Cursors.WaitCursor;

            Bitmap testBmp = GenerateTestCard(600, 400);

            byte[] rawBytes;
            using (MemoryStream ms = new MemoryStream())
            {
                testBmp.Save(ms, ImageFormat.Bmp);
                rawBytes = ms.ToArray();
            }

            int originalSize = rawBytes.Length;

            // 1. Тест RLE
            Stopwatch sw = Stopwatch.StartNew();
            byte[] rleCompressed = GraphicsAlgorithms.CompressRLE(rawBytes);
            sw.Stop();
            long rleTime = sw.ElapsedMilliseconds;
            double rleRatio = (1.0 - (double)rleCompressed.Length / originalSize) * 100.0;

            dgvResults.Rows.Add(
                "1",
                "Алгоритм RLE (Групове кодування)",
                $"{rleTime} мс",
                $"{originalSize / 1024.0:F1} КБ / {rleCompressed.Length / 1024.0:F1} КБ",
                $"{rleRatio:F1}%",
                "C# (.NET 10.0), Власна реалізація на базі потоків MemoryStream"
            );

            // 2. Тест LZW
            sw.Restart();
            byte[] lzwCompressed = GraphicsAlgorithms.CompressLZW(rawBytes);
            sw.Stop();
            long lzwTime = sw.ElapsedMilliseconds;
            double lzwRatio = (1.0 - (double)lzwCompressed.Length / originalSize) * 100.0;

            dgvResults.Rows.Add(
                "2",
                "Алгоритм LZW (Словниковий метод)",
                $"{lzwTime} мс",
                $"{originalSize / 1024.0:F1} КБ / {lzwCompressed.Length / 1024.0:F1} КБ",
                $"{lzwRatio:F1}%",
                "C# (.NET 10.0), Власна реалізація словника Dictionary<string, int>"
            );

            // 3. Тест JPEG
            sw.Restart();
            byte[] jpegCompressed = GraphicsAlgorithms.CompressJPEG(testBmp, 75);
            sw.Stop();
            long jpegTime = sw.ElapsedMilliseconds;
            double jpegRatio = (1.0 - (double)jpegCompressed.Length / originalSize) * 100.0;

            dgvResults.Rows.Add(
                "3",
                "Алгоритм JPEG (ДКП + Квантування + Хаффман, Q=75)",
                $"{jpegTime} мс",
                $"{originalSize / 1024.0:F1} КБ / {jpegCompressed.Length / 1024.0:F1} КБ",
                $"{jpegRatio:F1}%",
                "C# (.NET 10.0), System.Drawing.Imaging (GDI+ ImageCodecInfo)"
            );

            Cursor = Cursors.Default;
            MessageBox.Show("Тестування стиснення успішно виконано! Дані додано до таблиці.", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
