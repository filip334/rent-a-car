using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TVP_Projekat2_2._0._RentACarDB__1_DataSetTableAdapters;

namespace TVP_Projekat2_2._0
{
    public partial class Form1 : Form
    {
        System.Windows.Forms.Timer animationTimer;
        Point p;
        
        _RentACarDB__1_DataSet _RentACarDB__1; //ds
        _RentACarDB__1_DataSetTableAdapters.VoziloTableAdapter voziloTableAdapter; //da
        _RentACarDB__1_DataSetTableAdapters.KategorijaTableAdapter kategorijaTableAdapter;
        _RentACarDB__1_DataSetTableAdapters.RezervacijaTableAdapter rezervacijaTableAdapter;

        DataGridViewRow selektovanoVozilo;
        DataGridViewRow selektovanaKategorija;
        public Form1()
        {
            InitializeComponent();
            _RentACarDB__1  = new _RentACarDB__1_DataSet(); 
            voziloTableAdapter = new _RentACarDB__1_DataSetTableAdapters.VoziloTableAdapter();
            kategorijaTableAdapter = new _RentACarDB__1_DataSetTableAdapters.KategorijaTableAdapter();
            rezervacijaTableAdapter = new _RentACarDB__1_DataSetTableAdapters.RezervacijaTableAdapter();
            DataGridViewRow selektovanoVozilo = new DataGridViewRow();
            this.DoubleBuffered = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DoubleBuffered = true;
            UcitajPodatke();
            PopuniComboBox(comboBox1);

            animationTimer = new System.Windows.Forms.Timer { Interval = 20 };
            animationTimer.Tick += (o, args) => Animacija();
            animationTimer.Start();
        }

        private void FilterButton_Click(object sender, EventArgs e)
        {
            int cenaOD,cenaDO;
            int kategorijaID;
            cenaDO = Convert.ToInt32(numericUpDown2.Value);
            cenaOD = Convert.ToInt32(numericUpDown1.Value);
            if(cenaDO < cenaOD)
            {
                numericUpDown1.Value = cenaDO;
                cenaOD = cenaDO;
            }
            kategorijaID = Convert.ToInt32(comboBox1.SelectedValue);
            DataTable filtrirano = FiltrirajVozila(cenaOD,cenaDO,dateTimePicker1.Value,dateTimePicker2.Value,kategorijaID);
            dataGridView1.DataSource = filtrirano;
            dataGridView1.ClearSelection();
            selektovanoVozilo = null;
            DodajRezButton.Enabled = false;
        }

        private void PrikaziSveButton_Click(object sender, EventArgs e)
        {
            UcitajPodatke();
            dataGridView1.DataSource = _RentACarDB__1.Vozilo;
        }

        public void PopuniComboBox(ComboBox cb)
        {
            kategorijaTableAdapter.Fill(_RentACarDB__1.Kategorija);
            DataTable pomocniTable = _RentACarDB__1.Kategorija.Copy();
            pomocniTable.Rows.InsertAt(pomocniTable.NewRow(), 0);
            pomocniTable.Rows[0]["id_kategorije"] = -1;
            pomocniTable.Rows[0]["naziv"] = "Svi automobili";

            cb.DataSource = pomocniTable;
            cb.DisplayMember = "naziv";
            cb.ValueMember = "id_kategorije";
            cb.SelectedIndex = 0;
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if(e.RowIndex >= 0)
            {
                dataGridView2.ClearSelection();
                selektovanoVozilo = dataGridView1.SelectedRows[0];
                DodajRezButton.Enabled = true;
            }
        }

        private void DodajRezButton_Click(object sender, EventArgs e)
        {
            if (selektovanoVozilo == null) return;
            RezervacijaForm form = new RezervacijaForm(selektovanoVozilo);
            form.ShowDialog();
            UcitajPodatke();
        }

        private void Form1_Click(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
            DodajRezButton.Enabled = false;
            selektovanoVozilo = null;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            VoziloForm vf = new VoziloForm(selektovanoVozilo);
            vf.ShowDialog();
            UcitajPodatke();
        }

        private void UcitajPodatke()
        {
            voziloTableAdapter.Fill(_RentACarDB__1.Vozilo);
            kategorijaTableAdapter.Fill(_RentACarDB__1.Kategorija);
            PopuniComboBox(comboBox1);
            rezervacijaTableAdapter.Fill(_RentACarDB__1.Rezervacija);
            dataGridView1.DataSource = _RentACarDB__1.Vozilo;
            dataGridView2.DataSource = _RentACarDB__1.Kategorija;
            dataGridView1.ClearSelection();
            dataGridView2.ClearSelection();
            selektovanoVozilo = null;
            selektovanaKategorija = null;
            DodajRezButton.Enabled = false;
            NadjiNajviseRezervisanoVozilo();
            PodesiDatume();

            double maxCena = nadjiMaxCenu();
            numericUpDown1.Maximum = Convert.ToInt32(maxCena);
            numericUpDown2.Maximum = Convert.ToInt32(maxCena);
            numericUpDown2.Value = Decimal.Parse(maxCena.ToString());

            comboBox1.SelectedValue = -1;
        }


        private void NadjiNajviseRezervisanoVozilo()
        {
            var rez = _RentACarDB__1.Rezervacija
            .GroupBy(v => v.id_vozila)
            .Select(g => new
            {
                id_vozila = g.Key,
                BrojRezervacija = g.Count()
            })
            .OrderByDescending(g => g.BrojRezervacija);

            var red = rez.FirstOrDefault();
            //return rentCar.Vozilo.FindByid_vozila(red.id_vozila);
            var vozilo = red == null ? null : _RentACarDB__1.Vozilo.FindByid_vozila(red.id_vozila);
            NajRezLabel.Text = vozilo == null ? "NEMA REZERVACIJA" : "NAJPOPULARNIJE VOZILO : " + vozilo.naziv;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            KategorijaForm kt = new KategorijaForm(selektovanaKategorija);
            kt.ShowDialog();
            UcitajPodatke();
        }

        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dataGridView1.ClearSelection();
                selektovanaKategorija = dataGridView2.SelectedRows[0];
                DodajRezButton.Enabled = false;
            }
        }

        private double nadjiMaxCenu()
        {
            double maxCena = 0;

            foreach (DataRow row in _RentACarDB__1.Vozilo.Rows)
            {
                double cenaVozila;
                if (Double.TryParse(row["cena_po_satu"].ToString(), out cenaVozila))
                {
                    if (cenaVozila > maxCena)
                    {
                        maxCena = cenaVozila;
                    }
                }
            }

            return maxCena;
        }

        private void PodesiDatume()
        {
            dateTimePicker2.Value = DateTime.Now.AddHours(1);
            dateTimePicker1.Value = DateTime.Now;
            //dateTimePicker1.MinDate = DateTime.Now;
            //dateTimePicker2.MinDate = DateTime.Now.AddHours(1);
        }

        /*private void DohvatiRezervacije()
        {
            var redovi = _RentACarDB__1.Rezervacija.Where(v => v.id_vozila == Convert.ToInt32(selektovanoVozilo.Cells["id_vozila"].Value));
            rezervacije = _RentACarDB__1.Rezervacija.Copy();
            rezervacije.Clear();
            foreach (var red in redovi)
            {
                DataRow novi = rezervacije.NewRow();
                for (int i = 0; i < _RentACarDB__1.Rezervacija.Columns.Count; i++)
                    novi[i] = red[i];

                //novi["datumVreme_pocetka"] = red.datumVreme_pocetka;
                //novi["datumVreme_kraja"] = red.datumVreme_kraja;
                rezervacije.Rows.Add(novi);
            }

            dataGridView3.DataSource = rezervacije;
            dataGridView3.ClearSelection();
        }*/

        public DataTable FiltrirajVozila(int cenaOD, int cenaDO, DateTime datumOD, DateTime datumDO, int kategorijaID)
        {
            var vozilaRedovi = from v in _RentACarDB__1.Vozilo select v;
            if (kategorijaID < 0)
            {
                vozilaRedovi = _RentACarDB__1.Vozilo.Where(v => !v.Iscena_po_satuNull() && v.cena_po_satu >= cenaOD && v.cena_po_satu <= cenaDO);
            }
            else
            {
                vozilaRedovi = _RentACarDB__1.Vozilo.Where(v => !v.Iscena_po_satuNull() && v.cena_po_satu >= cenaOD && v.cena_po_satu <= cenaDO && v.id_kategorije == kategorijaID);
            }

            DataTable filtriranaVozila = _RentACarDB__1.Vozilo.Copy();
            filtriranaVozila.Clear();

            foreach (var vozilo in vozilaRedovi)
            {
                var rezervacijeRedovi = _RentACarDB__1.Rezervacija
                    .Where(r => r.id_vozila == vozilo.id_vozila &&
                                BookingRules.Overlaps(datumOD, datumDO, r.datumVreme_pocetka, r.datumVreme_kraja))
                    .ToList();

                if (rezervacijeRedovi.Count == 0)
                {
                    DataRow noviRed = filtriranaVozila.NewRow();
                    for (int i = 0; i < _RentACarDB__1.Vozilo.Columns.Count; i++)
                    {
                        noviRed[i] = vozilo[i];
                    }
                    filtriranaVozila.Rows.Add(noviRed);
                }
            }

            return filtriranaVozila;
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            if ((dateTimePicker2.Value - dateTimePicker1.Value).TotalHours < 1)
            {
                dateTimePicker2.Value = dateTimePicker1.Value.AddHours(1);
            }
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            if ((dateTimePicker2.Value - dateTimePicker1.Value).TotalHours < 1)
            {
                dateTimePicker1.Value = dateTimePicker2.Value.AddHours(-1);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            StatistikaForm sf = new StatistikaForm();
            sf.ShowDialog();

        }

        public void Animacija()
        {
            if (p.X >= panel2.Width) p.X = -NajRezLabel.Width;
            p.X += 2;
            panel2.Invalidate();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (animationTimer != null) animationTimer.Dispose();
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            NajRezLabel.Left = p.X;
        }
    }
}
