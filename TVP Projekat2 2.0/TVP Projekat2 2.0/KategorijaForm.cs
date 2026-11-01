using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TVP_Projekat2_2._0._RentACarDB__1_DataSetTableAdapters;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TVP_Projekat2_2._0
{
    public partial class KategorijaForm : Form
    {
        _RentACarDB__1_DataSet _RentACarDB__1;
        _RentACarDB__1_DataSetTableAdapters.KategorijaTableAdapter kategorijaTableAdapter;
        DataGridViewRow selektovanaKategorija;
        public KategorijaForm(DataGridViewRow kategorija)
        {
            InitializeComponent();
            _RentACarDB__1 = new _RentACarDB__1_DataSet();
            kategorijaTableAdapter = new _RentACarDB__1_DataSetTableAdapters.KategorijaTableAdapter();
            selektovanaKategorija = kategorija;
        }

        private void KategorijaForm_Load(object sender, EventArgs e)
        {
            if (selektovanaKategorija != null)
            {
                textBox1.Text = selektovanaKategorija.Cells["naziv"].Value.ToString();
                textBox2.Text = selektovanaKategorija.Cells["opis"].Value.ToString();
                button1.Text = "Izmeni";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GreskaLabel.Text = "";
            String naziv, opis;
           
            naziv = textBox1.Text.Trim();
            opis = textBox2.Text.Trim();
            if (string.IsNullOrWhiteSpace(naziv) || string.IsNullOrWhiteSpace(opis))
            {
                GreskaLabel.Text = "Sva polja moraju biti popunjena!";
                return;
            }
            

            int rez;
            if (selektovanaKategorija != null)
            {
                rez = kategorijaTableAdapter.Update(naziv, opis, Convert.ToInt32(selektovanaKategorija.Cells["id_kategorije"].Value));

                if (rez > 0)
                {
                    MessageBox.Show("Uspesno ste izmenili kategoriju!");
                    this.Close();
                }
                else
                {
                    MessageBox.Show("GRESKA!!!");
                    return;
                }
            }
            else
            {
                rez = kategorijaTableAdapter.Insert(naziv, opis);

                if (rez > 0)
                {
                    MessageBox.Show("Uspesno ste dodali kategoriju!");
                    this.Close();
                }
                else
                {
                    MessageBox.Show("GRESKA!!!");
                    return;
                }
            }
        }
    }
}
