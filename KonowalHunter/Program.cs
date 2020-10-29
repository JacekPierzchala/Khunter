using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.Extensions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Tesseract;

namespace KonowalHunter
{
    public class Program
    {
        public static ReadOnlyCollection<IWebElement> Rows { get; set; }
        public static List<Medic> Medics { get; set; } = new List<Medic>();
        static void Main(string[] args)
        {
            string org = "Okręgowa Izba Lekarska w Warszawie";
            string voidvod = "MAZOWIECKIE";
            string speciality = "Ginekolog";
            string registy = "https://rpwdl.csioz.gov.pl/RPZ/RegistryList";
            string mainPage = "https://rpwdl.csioz.gov.pl/RPZ/SearchEx?institutionType=L";
            IWebDriver webDriver = new FirefoxDriver();
            webDriver.Url = mainPage;
            webDriver.Manage().Window.Maximize();
            webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            WaitUntilDocumentIsReady(webDriver, 2);
            System.Threading.Thread.Sleep(1000);
            var acceptCookies = webDriver.FindElements(By.TagName("a")).FirstOrDefault(e => e.Text == "Akceptuje");
            if (acceptCookies != null)
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

            var countertxt = webDriver.FindElements(By.ClassName("form-left")).FirstOrDefault(e => e.Text.Contains("Liczba znalezionych ksiąg:"));
            double counter = double.Parse(countertxt.Text.Substring(countertxt.Text.IndexOf(":") + 1, countertxt.Text.Length - (countertxt.Text.IndexOf(":") + 1)).Trim());
            int totalNrOfPages = (int)Math.Ceiling(counter / 15);

            for (int i = 2; i <= totalNrOfPages; i++)
            {
                registy = webDriver.Url;
                ReadPage(registy, webDriver);
               webDriver.FindElements(By.ClassName("pagination"))[0]
                     .FindElements(By.TagName("li"))
                     .FirstOrDefault(e => e.Text == i.ToString())
                     .FindElements(By.TagName("a"))[0].Click();
                System.Threading.Thread.Sleep(500);
                //.Click();
            }

            Medics.ForEach(e =>
                   Console.WriteLine("Name:" + e.Name  +" phone:" + e.Phone)
            );
     
            #region old
            //int rowIndex = 0;
            //foreach (var row in Rows)
            //{
            //    if (rowIndex > 0)
            //    {

            //        string name = row.FindElements(By.TagName("td"))[1].Text;
            //        row.FindElements(By.TagName("td"))[3]
            //            .FindElements(By.TagName("a")).FirstOrDefault(e => e.Text == "Wyświetl").Click();
            //        System.Threading.Thread.Sleep(1000);

            //        string phone = webDriver.FindElements
            //            (By.ClassName("odstep")).
            //            FirstOrDefault(e => e.Text.StartsWith("Rubryka 18. Adres i numer telefonu miejsca udzielania świadczeń zdrowotnych")).
            //            FindElements(By.TagName("tr")).FirstOrDefault(r => r.Text.StartsWith("7. Numer telefonu")).
            //            FindElements(By.ClassName("rightThick"))[0].Text;

            //        //var b=webDriver.FindElements(By.ClassName("btn-default")).FirstOrDefault(e => e.Text == "Wróć");
            //        //b.Click();
            //        webDriver.Url = registy;
            //        waitTillPageReady(webDriver, registy);

            //        RecordsTableInit(webDriver);
            //        //System.Threading.Thread.Sleep(1000);
            //    }
            //    rowIndex++;
            //}
            // System.Threading.Thread.Sleep(1000);
            //15 per page
            //https://rpwdl.csioz.gov.pl/RPZ/RegistryList
            //NotFiniteNumberException           
            //inline form - left bottom - space
            //Liczba znalezionych ksiąg: 1136 
            //table registry-list-table table-striped
            #endregion


        }

        private static void ReadPage(string registy, IWebDriver webDriver)
        {
            RecordsTableInit(webDriver);

            for (int i = 0; i < Rows.Count; i++)
            {
                if (i > 0)
                {
                    string name = Rows[i].FindElements(By.TagName("td"))[1].Text;
                    Rows[i].FindElements(By.TagName("td"))[3]
                       .FindElements(By.TagName("a")).FirstOrDefault(e => e.Text == "Wyświetl").Click();
                    System.Threading.Thread.Sleep(500);
                    string phone=null;
                    retry: ;
                    try
                    {
                      phone = webDriver.FindElements
                      (By.ClassName("odstep")).
                      FirstOrDefault(e => e.Text.StartsWith("Rubryka 18. Adres i numer telefonu miejsca udzielania świadczeń zdrowotnych")).
                      FindElements(By.TagName("tr")).FirstOrDefault(r => r.Text.StartsWith("7. Numer telefonu")).
                      FindElements(By.ClassName("rightThick"))[0].Text;
                    }
                    catch (Exception ex)
                    {
                        while (webDriver.FindElements
                                (By.ClassName("odstep")).
                                FirstOrDefault(e => e.Text.StartsWith("Rubryka 18. Adres i numer telefonu miejsca udzielania świadczeń zdrowotnych"))==null)
                                {
                           
                                }
                        goto retry;
                        //string filePath = @"C:\Users\Zaneta\Desktop\programowanie\";
                        //var remElement = webDriver.FindElement(By.Id("CaptchaImage"));
                        //Point location = remElement.Location;
                        //var screenshot = (webDriver as FirefoxDriver).GetScreenshot();

                        //using (MemoryStream stream = new MemoryStream(screenshot.AsByteArray))
                        //{
                        //    using (Bitmap bitmap = new Bitmap(stream))
                        //    {
                        //        RectangleF part = new RectangleF(location.X, location.Y, remElement.Size.Width, remElement.Size.Height);
                        //        using (Bitmap bn = bitmap.Clone(part, bitmap.PixelFormat))
                        //        {
                        //            bn.Save(filePath + "Generate.gif", System.Drawing.Imaging.ImageFormat.Png);
                        //        }
                        //    }

                        //}

                        //using (var engine = new TesseractEngine(filePath, "eng", EngineMode.Default))
                        //{

                        //    Page ocrPage = engine.Process(Pix.LoadFromFile(filePath + "Generate.png"), PageSegMode.AutoOnly);
                        //    var captchatext = ocrPage.GetText();
                        //}
                    }


                    Medics.Add(new Medic { Name = name, Phone=phone });

                    webDriver.Url = registy;
                    waitTillPageReady(webDriver, registy);

                    RecordsTableInit(webDriver);
                }

            }
        }

        private static void RecordsTableInit(IWebDriver webDriver)
        {
            var table = webDriver.FindElement(By.ClassName("registry-list-table"));
            Rows = table.FindElements(By.TagName("tr"));
            
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
