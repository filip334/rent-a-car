using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
namespace TVP_Projekat2_2._0
{
    public partial class StatistikaForm : Form
    {
        private DataTable podaci = new DataTable();
        private readonly Color[] colors = { Color.SteelBlue, Color.Orange, Color.SeaGreen, Color.MediumPurple, Color.IndianRed, Color.Teal };
        public StatistikaForm()
        {
            InitializeComponent();
            DoubleBuffered = true;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }
        private void StatistikaForm_Load(object sender, EventArgs e) { RefreshStatistics(); }
        private void button1_Click(object sender, EventArgs e) { RefreshStatistics(); }
        private void RefreshStatistics()
        {
            using (var ds = new _RentACarDB__1_DataSet())
            using (var vehicles = new _RentACarDB__1_DataSetTableAdapters.VoziloTableAdapter())
            using (var reservations = new _RentACarDB__1_DataSetTableAdapters.RezervacijaTableAdapter())
            using (var categories = new _RentACarDB__1_DataSetTableAdapters.KategorijaTableAdapter())
            {
                vehicles.Fill(ds.Vozilo); reservations.Fill(ds.Rezervacija); categories.Fill(ds.Kategorija);
                podaci = new DataTable();
                podaci.Columns.Add("Kategorija", typeof(string));
                podaci.Columns.Add("Rezervacije", typeof(int));
                podaci.Columns.Add("Udeo (%)", typeof(double));
                var groups = (from r in ds.Rezervacija join v in ds.Vozilo on r.id_vozila equals v.id_vozila
                              group r by v.id_kategorije into g orderby g.Key
                              select new { Id = g.Key, Count = g.Count() }).ToList();
                int total = groups.Sum(g => g.Count);
                foreach (var g in groups)
                {
                    var category = ds.Kategorija.FindByid_kategorije(g.Id);
                    podaci.Rows.Add(category == null ? g.Id.ToString() : category.naziv, g.Count, Math.Round(100.0 * g.Count / total, 1));
                }
                dataGridView1.DataSource = podaci;
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    dataGridView1.Rows[i].DefaultCellStyle.BackColor = colors[i % colors.Length];
                Invalidate();
            }
        }
        private void StatistikaForm_Paint(object sender, PaintEventArgs e)
        {
            int total = podaci.AsEnumerable().Sum(r => r.Field<int>("Rezervacije"));
            if (total == 0) { e.Graphics.DrawString("Nema rezervacija.", Font, Brushes.Black, 10, 20); return; }
            float angle = 0;
            for (int i = 0; i < podaci.Rows.Count; i++)
            {
                float sweep = 360f * (int)podaci.Rows[i]["Rezervacije"] / total;
                using (var brush = new SolidBrush(colors[i % colors.Length]))
                    e.Graphics.FillPie(brush, new Rectangle(10, 10, 290, 290), angle, sweep);
                angle += sweep;
            }
        }
    }
}
