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

namespace TVP_Projekat2_2._0
{
    public partial class VoziloForm : Form
    {

        _RentACarDB__1_DataSet _RentACarDB__1; //ds
        _RentACarDB__1_DataSetTableAdapters.VoziloTableAdapter voziloTableAdapter; //da
        _RentACarDB__1_DataSetTableAdapters.KategorijaTableAdapter kategorijaTableAdapter;
        DataGridViewRow selektovanoVozilo;
        public VoziloForm(DataGridViewRow vozilo)
        {
            InitializeComponent();
            _RentACarDB__1 = new _RentACarDB__1_DataSet();
            voziloTableAdapter = new _RentACarDB__1_DataSetTableAdapters.VoziloTableAdapter();
            kategorijaTableAdapter = new _RentACarDB__1_DataSetTableAdapters.KategorijaTableAdapter();
            selektovanoVozilo = vozilo;
        }

        private void VoziloForm_Load(object sender, EventArgs e)
        {
            PopuniComboBox(comboBox1);
            if(selektovanoVozilo != null)
            {
                textBox1.Text = selektovanoVozilo.Cells["naziv"].Value.ToString();
                textBox2.Text = selektovanoVozilo.Cells["marka"].Value.ToString();
                textBox3.Text = selektovanoVozilo.Cells["model"].Value.ToString();
                textBox4.Text = selektovanoVozilo.Cells["godina_proizvodnje"].Value.ToString();
                textBox5.Text = selektovanoVozilo.Cells["cena_po_satu"].Value.ToString();
                comboBox1.SelectedValue = Convert.ToInt32(selektovanoVozilo.Cells["id_kategorije"].Value);
                button1.Text = "Izmeni";
            }
        }

        public void PopuniComboBox(ComboBox cb)
        {
            kategorijaTableAdapter.Fill(_RentACarDB__1.Kategorija);
            cb.DataSource = _RentACarDB__1.Kategorija;
            cb.DisplayMember = "naziv";
            cb.ValueMember = "id_kategorije";
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //MessageBox.Show(comboBox1.SelectedValue.ToString());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GreskaLabel.Text = "";
            String naziv, model, marka;
            int cenaPoSatu, godinaProizvodnje;
            naziv = textBox1.Text.Trim();
            model = textBox2.Text.Trim();
            marka = textBox3.Text.Trim();
            if(string.IsNullOrWhiteSpace(naziv) || string.IsNullOrWhiteSpace(model) || string.IsNullOrWhiteSpace(marka) || string.IsNullOrWhiteSpace(textBox4.Text.Trim()) || string.IsNullOrWhiteSpace(textBox5.Text.Trim()))
            {
                GreskaLabel.Text = "Sva polja moraju biti popunjena!";
                return;
            }
            if(!Int32.TryParse(textBox5.Text.Trim(),out cenaPoSatu) || !Int32.TryParse(textBox4.Text.Trim(), out godinaProizvodnje))
            {
                GreskaLabel.Text = "Polja cenaPoSatu i godinaProizvodnje moraju biti brojevi!";
                return;
            }
            if(cenaPoSatu <= 0 || godinaProizvodnje <= 1900 || godinaProizvodnje > DateTime.Now.Year)
            {
                GreskaLabel.Text = "Proverite da li ste dobro uneli brojeve!";
                return;
            }

            if (comboBox1.SelectedValue == null)
            {
                GreskaLabel.Text = "Prvo dodajte i izaberite kategoriju vozila.";
                return;
            }
            int rez;
            if (selektovanoVozilo != null)
            {
                rez = voziloTableAdapter.Update(
                    (int)comboBox1.SelectedValue, 
                    naziv, 
                    model, 
                    marka, 
                    Convert.ToInt32(godinaProizvodnje), 
                    Convert.ToInt32(cenaPoSatu),
                    Convert.ToInt32(selektovanoVozilo.Cells["id_vozila"].Value),
                    Convert.ToInt32(selektovanoVozilo.Cells["id_kategorije"].Value),
                    Convert.ToInt32(selektovanoVozilo.Cells["godina_proizvodnje"].Value),
                    Convert.ToInt32(selektovanoVozilo.Cells["cena_po_satu"].Value)
                    );
                if (rez > 0)
                {
                    MessageBox.Show("Uspesno ste izmenili vozilo!");
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
                rez = voziloTableAdapter.Insert(
                (int)comboBox1.SelectedValue,
                naziv,
                model,
                marka, 
                Convert.ToInt32(godinaProizvodnje),
                Convert.ToInt32(cenaPoSatu)
                );
                if (rez > 0)
                {
                    MessageBox.Show("Uspesno ste dodali vozilo!");
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
