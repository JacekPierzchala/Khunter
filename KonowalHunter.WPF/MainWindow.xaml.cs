using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;
namespace KonowalHunter.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public string SelectedOrg { get; set; }
        public List<string> Orgs { get; set; } = new List<string>();
        public string SelectedVoivod { get; set; }
        public List<string> Voivods { get; set; } = new List<string>();
        public string Specialisation { get; set; }
        public string City { get; set; }
        private bool _pending;

        public bool Pending
        {
            get { return _pending; }
            set { _pending = value; OnPropertyChanges(); }
        }

       

        public MainWindow()
        {
            try
            {
                Loadlists();
            }
            catch (Exception ex)
            {

                MessageBox.Show("Error occured plese restart:" + ex.Message);
            }
          
            InitializeComponent();
            Pending = true;
            DataContext = this;
        }

       

        public void Loadlists()
        {
            
            string mainPage = "https://rpwdl.csioz.gov.pl/RPZ/SearchEx?institutionType=L";
            IWebDriver webDriver = new FirefoxDriver();
            webDriver.Manage().Window.Minimize();
            webDriver.Url = mainPage;
           
           
            webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            var orgSelect = new SelectElement(webDriver.FindElement(By.Id("InstitutionId")));
            foreach (var item in orgSelect.Options.OrderBy(e=>e.Text))
            {
                Orgs.Add(item.Text);
            }

            var voivodSelect = new SelectElement(webDriver.FindElement(By.Id("Voivodship")));
            foreach (var item in voivodSelect.Options.OrderBy(e => e.Text))
            {
                Voivods.Add(item.Text);
            }

            webDriver.Close();
            webDriver.Quit();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SelectedOrg) || string.IsNullOrWhiteSpace(SelectedVoivod) || string.IsNullOrWhiteSpace(Specialisation))
            {
                MessageBox.Show("Określ wojewodztwo, Izbę Lekarską i specjalizację!!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                Pending = false;
                Task.Run(() =>
                {
                    string[] args = { SelectedOrg, SelectedVoivod, Specialisation, City };
                    Program.Main(args);
                    Pending = true;
                    MessageBox.Show("File is ready", "", MessageBoxButton.OK, MessageBoxImage.Information);
                });
           
            
            }
            catch (Exception ex)
            {

                MessageBox.Show("Error:" + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
                Pending = true;
                return;
            }

        
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanges(string property=null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs (property) );
        }
    }
}
