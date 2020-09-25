using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KonowalHunter
{
    class Program
    {
        static void Main(string[] args)
        {
            string org = "Okręgowa Izba Lekarska w Warszawie";
            string voidvod = "MAZOWIECKIE";
            string speciality = "Ginekolog";
            string registy= "https://rpwdl.csioz.gov.pl/RPZ/RegistryList";
            string mainPage = "https://rpwdl.csioz.gov.pl/RPZ/SearchEx?institutionType=L";
            IWebDriver webDriver = new FirefoxDriver();
            webDriver.Url = mainPage;
            webDriver.Manage().Window.Maximize();
            webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            WaitUntilDocumentIsReady(webDriver,2);
            System.Threading.Thread.Sleep(1000);
            var acceptCookies = webDriver.FindElements(By.TagName("a")).FirstOrDefault(e => e.Text == "Akceptuje");
            if(acceptCookies!=null)
            {
                acceptCookies.Click();
                System.Threading.Thread.Sleep(1000);
            }

            var orgSelect = new SelectElement(webDriver.FindElement(By.Id("InstitutionId")));
            orgSelect.SelectByText(org);
         
            var voidSelect = new SelectElement(webDriver.FindElement(By.Id("Voivodship")));
            voidSelect.SelectByText(voidvod);

            webDriver.FindElement(By.Id("Speciality")).SendKeys(speciality);

           webDriver.FindElement(By.ClassName("btn-primary")).Click();

            waitTillPageReady(webDriver, registy);

            var countertxt=webDriver.FindElements(By.ClassName("form-left")).FirstOrDefault(e => e.Text.Contains("Liczba znalezionych ksiąg:"));
            double counter= double.Parse(countertxt.Text.Substring(countertxt.Text.IndexOf(":") + 1, countertxt.Text.Length - (countertxt.Text.IndexOf(":") + 1)).Trim());
            int nrOfPages =(int) Math.Ceiling(counter/ 15);

            var table = webDriver.FindElement(By.ClassName("registry-list-table"));
            var rows = table.FindElements(By.TagName("tr"));
            // System.Threading.Thread.Sleep(1000);
            //15 per page
            //https://rpwdl.csioz.gov.pl/RPZ/RegistryList
                //NotFiniteNumberException           
            //inline form - left bottom - space
            //Liczba znalezionych ksiąg: 1136 
            //table registry-list-table table-striped

        }

        private static void waitTillPageReady(IWebDriver webDriver, string expectedUrl)
        {
            while(webDriver.Url!= expectedUrl)
            {
                WaitUntilDocumentIsReady(webDriver, 1);
                //var r=webDriver.ExecuteJavaScript("return document.readyState");
                //((JavascriptExecutor)webDriver).executeScript("return document.readyState").equals("complete");
            }
        }


        public static void WaitUntilDocumentIsReady( IWebDriver driver, int timeoutInSeconds)
        {
            var javaScriptExecutor = driver as IJavaScriptExecutor;
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));

            try
            {
                Func<IWebDriver, bool> readyCondition = webDriver => (bool)javaScriptExecutor.ExecuteScript("return (document.readyState == 'complete' && jQuery.active == 0)");
                wait.Until(readyCondition);
            }
            catch (InvalidOperationException)
            {
                wait.Until(wd => javaScriptExecutor.ExecuteScript("return document.readyState").ToString() == "complete");
            }
        }
    }
}
