using System;
using System.Windows.Forms;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Diagnostics;
using System.IO;

namespace BotTube
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        ChromeDriverService servis = ChromeDriverService.CreateDefaultService();
        ChromeDriver tarayici; ChromeOptions options;

        private void Form1_Load(object sender, EventArgs e)
        {
            string kullaniciAdi = SystemInformation.UserName;
            options = new ChromeOptions(); servis.HideCommandPromptWindow = true;
            options.AddArgument(@"user-data-dir=C:/Users/" + kullaniciAdi + "/AppData/Local/Google/Chrome/User Data");
            //tarayici = new ChromeDriver(servis, options);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dosyaSec.ShowDialog();
        }

        bool tarayiciAcik;
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (tarayiciAcik) tarayici.Quit();
        }

        private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DosyaBilgileri(dosyaSec.FileName);
        }

        bool buton = true;
        private void button2_Click(object sender, EventArgs e)
        {
            if (buton)
            {
                DialogResult cevap = MessageBox.Show($"Başlamadan önce tüm Chrome pencereleri kapatılacak, yaptığınız değişiklikler kaydedilmemiş olabilir yine de devam etmek istiyor musunuz?", "Başlat", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (cevap == DialogResult.Yes)
                {
                    buton = false; yukle.Text = "Durdur";
                    if (!tarayiciAcik) { ChromeKapat(); ChromeAc(); }
                    //VideoyuPaylas();
                    sistem.Start();
                }
            }
            else
            {
                buton = true; yukle.Text = "Yeniden Başlat";
                tarayici.Close(); tarayiciAcik = false;
            }
        }

        void DosyaBilgileri(string yol)
        {
            FileInfo dosya = new FileInfo(yol);
            float dosyaBoyutu = (float)dosya.Length / (1024 * 1024);
            label1.Text = $"Video: {dosyaSec.SafeFileName}\nBoyut: {Math.Round(dosyaBoyutu, 2)} MB";
            label1.Visible = true;
            yukle.Enabled = true;
        }

        string Sec(string komut)
        {
            switch (komut)
            {
                case "Hayır":
                    return "/html/body/ytcp-uploads-dialog/tp-yt-paper-dialog/div/ytcp-animatable[1]/ytcp-ve/ytcp-video-metadata-editor/div/ytcp-video-metadata-editor-basics/div[5]/ytkc-made-for-kids-select/div[4]/tp-yt-paper-radio-group/tp-yt-paper-radio-button[2]/div[2]/ytcp-ve";
                case "İleri":
                    return "/html/body/ytcp-uploads-dialog/tp-yt-paper-dialog/div/ytcp-animatable[2]/div/div[2]/ytcp-button[2]";
                case "Herkes":
                    return "/html/body/ytcp-uploads-dialog/tp-yt-paper-dialog/div/ytcp-animatable[1]/ytcp-uploads-review/div[2]/div[1]/ytcp-video-visibility-select/div[1]/tp-yt-paper-radio-group/tp-yt-paper-radio-button[3]";
                case "Kaydet":
                    return "/html/body/ytcp-uploads-dialog/tp-yt-paper-dialog/div/ytcp-animatable[2]/div/div[2]/ytcp-button[3]";
                case "Kalan":
                    return "/html/body/ytcp-uploads-still-processing-dialog/ytcp-dialog/tp-yt-paper-dialog/div[2]/div/ytcp-video-upload-progress/span";
                default: return "";
            }
        }

        void Bekle(int milisaniye) => System.Threading.Thread.Sleep(milisaniye);

        void ButonaTikla(string yol) 
        {
            try { tarayici.FindElement(By.XPath(yol)).Click(); } catch { }
        }

        void DosyaYukle()
        {
            try { tarayici.FindElement(By.XPath("//input[@type='file']")).SendKeys(dosyaSec.FileName); } catch { }
        }

        int YuklemeYuzdesi()
        {
            string metin = tarayici.FindElement(By.XPath(Sec("Kalan"))).GetAttribute("innerText");
            if (metin.Split(' ')[0] != "İşlenme")
            {
                string yuzde = metin.Split(' ')[2];
                int deger = Convert.ToInt32(yuzde.Remove(0, 1));
                return deger;
            } else return 100;
        }

        void ChromeKapat()
        {
            Process[] uygulamalar = Process.GetProcessesByName("Chrome");
            foreach (var chrome in uygulamalar) chrome.Kill();
        }

        void ChromeAc()
        {
            tarayici = new ChromeDriver(servis, options); tarayiciAcik = true;
        }

        void VideoyuPaylas()
        {
            tarayici.Navigate().GoToUrl("https://www.youtube.com/upload");
            try { var uyari = tarayici.SwitchTo().Alert(); uyari.Accept(); } catch { }
            Bekle(10000);
            DosyaYukle();
            Bekle(5000);
            ButonaTikla(Sec("Hayır"));
            ButonaTikla(Sec("İleri"));
            ButonaTikla(Sec("İleri"));
            ButonaTikla(Sec("İleri"));
            Bekle(5000);
            ButonaTikla(Sec("Herkes"));
            ButonaTikla(Sec("Kaydet"));
            Bekle(5000);
            timer1.Start();
            label3.Text = ++paylasilanVideo + ". Video Yükleniyor"; label3.Visible = true;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            
        }

        uint paylasilanVideo = 0;
        string yuklenenVideo = "";
        private void timer1_Tick(object sender, EventArgs e)
        {
            progressBar1.Value = YuklemeYuzdesi();
            yuzde.Text = "%" + progressBar1.Value.ToString();

            string metin = tarayici.FindElement(By.XPath(Sec("Kalan"))).GetAttribute("innerText");
            try { label4.Text = $"Tahmini Süre: {metin.Split(' ')[4]} {metin.Split(' ')[5]}\n{yuklenenVideo}"; } catch { label4.Text = yuklenenVideo; }

            if (YuklemeYuzdesi() == 100)
            {
                timer1.Stop();
                paylasilanVideo++;
                videoSec.Text = paylasilanVideo.ToString();
                if (paylasilanVideo == numericUpDown1.Value + 1)
                {
                    label4.Visible = false;
                    label3.Text = "Tüm Videolar Yüklendi ✅";
                }
                else
                {
                    yuklenenVideo = paylasilanVideo + ". Video Yüklendi ✅";
                    progressBar1.Value = 0;
                    yuzde.Text = "%0";
                    sistem.Start(); 
                }
            } else {  }
        }

        uint adim = 0;
        uint hata = 20;
        private void sistem_Tick(object sender, EventArgs e)
        {
            switch (adim)
            {
                case 0:
                    tarayici.Navigate().GoToUrl("https://www.youtube.com/upload");
                    try { var uyari = tarayici.SwitchTo().Alert(); uyari.Accept(); } catch { }
                    adim++;
                    break;
                case 1:
                    try { if (tarayici.FindElement(By.XPath("//input[@type='file']")).Enabled) { DosyaYukle(); adim++; hata = 20; } }
                    catch
                    {
                        hata--; Text = hata.ToString(); if (hata == 0)
                        { sistem.Stop(); MessageBox.Show("Yüklenecek konum bulunamadı lütfen tekrar yüklemeyi deneyin.", "Konum bulunamadı", MessageBoxButtons.OK, MessageBoxIcon.Error); break; } 
                    }
                    break;
                case 2:
                    try { if (tarayici.FindElement(By.XPath(Sec("Hayır"))).Enabled) { ButonaTikla(Sec("Hayır")); adim++; hata = 20; } }
                    catch
                    {
                        hata--; Text = hata.ToString(); if (hata == 0)
                        { sistem.Stop(); MessageBox.Show("Yüklenecek konum bulunamadı lütfen tekrar yüklemeyi deneyin.", "Konum bulunamadı", MessageBoxButtons.OK, MessageBoxIcon.Error); break; } 
                    }
                    break;
                case 3:
                    ButonaTikla(Sec("İleri"));
                    ButonaTikla(Sec("İleri"));
                    ButonaTikla(Sec("İleri"));
                    adim++;
                    break;
                case 4:
                    try { if (tarayici.FindElement(By.XPath(Sec("Kaydet"))).GetAttribute("aria-disabled").ToString() == "false") { ButonaTikla(Sec("Herkes")); ButonaTikla(Sec("Kaydet")); adim++; hata = 20; } }
                    catch
                    {
                        hata--; Text = hata.ToString(); if (hata == 0)
                        { sistem.Stop(); MessageBox.Show("İşlem yapılacak yol bulunamadı lütfen tekrar yüklemeyi deneyin.", "Yol bulunamadı", MessageBoxButtons.OK, MessageBoxIcon.Error); break; }
                    }
                    break;
                case 5:
                    try 
                    { 
                        if (tarayici.FindElement(By.XPath(Sec("Kalan"))).Enabled) 
                        { 
                            sistem.Stop(); 
                            timer1.Start(); 
                            adim = 0; hata = 20; 
                            label3.Text = 1 + paylasilanVideo + ". Video Yükleniyor"; 
                            label3.Visible = true; label4.Visible = true;
                            progressBar1.Visible = true; yuzde.Visible = true;
                        } 
                    }
                    catch
                    {
                        hata--; Text = hata.ToString(); if (hata == 0)
                        { sistem.Stop(); MessageBox.Show("İşlem yapılacak yol bulunamadı lütfen tekrar yüklemeyi deneyin.", "Yol bulunamadı", MessageBoxButtons.OK, MessageBoxIcon.Error); break; }
                    }
                    break;
                default:
                    break;
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {
            yukle.Text = tarayici.FindElement(By.XPath(Sec("Kaydet"))).GetAttribute("aria-disabled").ToString();
        }
    }
}