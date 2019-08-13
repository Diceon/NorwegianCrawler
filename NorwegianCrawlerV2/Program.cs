using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NorwegianCrawlerV2
{
    class Program
    {
        private static FirefoxDriverService driverService;
        private static FirefoxOptions driverOptions;
        private static WebDriverWait driverWait;
        private static IWebDriver driver;

        private static IList<Flight> flights = new List<Flight>();
        //private static String url = "https://www.norwegian.com/en/ipc/availability/avaday?D_City=OSL&A_City=RIX&TripType=1&D_Day=01&D_Month=201909&D_SelectedDay=01&R_Day=01&R_Month=201909&R_SelectedDay=01&IncludeTransit=false&AgreementCodeFK=-1&CurrencyCode=EUR&rnd=86981&processid=92537&mode=ab";


        static void Main(string[] args)
        {
            // Starting WebDriver
            StartDriver();

            for (int i = 1; i <= 30; i++)
            {
                Console.WriteLine($"Gathering data of day {i}");
                driver.Navigate().GoToUrl($"https://www.norwegian.com/en/ipc/availability/avaday?D_City=OSL&A_City=RIX&TripType=1&D_Day={i}&D_Month=201909&D_SelectedDay={i}&R_Day={i}&R_Month=201909&R_SelectedDay={i}&IncludeTransit=false&AgreementCodeFK=-1&CurrencyCode=EUR&mode=ab");
                GatherFlights();
            }

            // Closing WebDriver
            CloseDriver();

            PrintFlights();

            Console.ReadLine();
        }

        private static void StartDriver()
        {
            // Initialising driver service & options
            driverService = FirefoxDriverService.CreateDefaultService(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            driverOptions = new FirefoxOptions();
            driverOptions.BrowserExecutableLocation = @"C:\Program Files\Mozilla Firefox\firefox.exe";
            driverOptions.AddArgument("-headless");
            driverOptions.AcceptInsecureCertificates = true;

            // Initialising driver
            try
            {
                driver = new FirefoxDriver(driverService, driverOptions);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
                driverWait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            } catch (Exception ex)
            {
                Console.WriteLine("Failed to initialise driver");
                throw ex;
            }
        }

        private static void CloseDriver()
        {
            // Closing driver
            driver.Quit();
        }

        private static void GatherFlights()
        {
            IEnumerable<IWebElement> flightsTableRows = new List<IWebElement>();
            IWebElement row = null;

            try
            {
                flightsTableRows = driver.FindElements(By.XPath("//table[@class = 'avadaytable']/tbody/tr"));
            } catch (Exception)
            {
                Console.WriteLine("No flights");
                return;
            }
            
            String date = null;
            String departureTime = null;
            String arrivalTime = null;
            String duration = null;
            String price = null;
            String departureAirport = null;
            String arrivalAirport = null;
            String tax = null;

            for (int i = 0; i < flightsTableRows.Count(); i++)
            {
                try
                {
                    flightsTableRows = driver.FindElements(By.XPath("//table[@class = 'avadaytable']/tbody/tr"));
                }
                catch (Exception)
                {
                    Console.WriteLine("No flights");
                    return;
                }

                row = flightsTableRows.ElementAt(i);

                if (row.GetAttribute("class").Contains("rowinfo1"))
                {
                    date = driver.FindElement(By.XPath("/html/body/main/div[2]/form/div[3]/table/tbody/tr[4]/td/table/tbody/tr/td[1]/div[2]/div[2]/div/div/div/div[1]/table/tbody/tr/td[2]")).Text;
                    departureTime = row.FindElement(By.XPath(".//td[@class = 'depdest']/div")).Text;
                    arrivalTime = row.FindElement(By.XPath(".//td[@class = 'arrdest']/div")).Text;
                    price = row.FindElement(By.XPath(".//td[@class = 'fareselect standardlowfare']/div/label")).Text;

                    row.FindElement(By.XPath("//td/div/input")).Click();
                    tax = driver.FindElement(By.XPath("/html/body/main/div[2]/form/div[3]/table/tbody/tr[4]/td/table/tbody/tr/td[2]/div/div[1]/div/table/tbody/tr[18]/td[2]")).Text;
                }
                else if (row.GetAttribute("class").Contains("rowinfo2"))
                {
                    departureAirport = row.FindElement(By.XPath(".//td[@class = 'depdest']/div")).Text;
                    arrivalAirport = row.FindElement(By.XPath(".//td[@class = 'arrdest']/div")).Text;
                    duration = row.FindElement(By.XPath(".//td[@class = 'duration']/div")).Text.Substring(10);
                }
                else if (row.GetAttribute("class").Contains("lastrow"))
                {
                    flights.Add(new Flight
                    {
                        Date = date,
                        DepartureTime = departureTime,
                        ArrivalTime = arrivalTime,
                        Duration = duration,
                        Price = price,
                        DepartureAirport = departureAirport,
                        ArrivalAirport = arrivalAirport,
                        Tax = tax
                    });
                }

            }

        }

        private static void PrintFlights()
        {

            foreach (Flight flight in flights)
            {
                Console.WriteLine($"( {flight.Date.Trim()} ) {flight.DepartureAirport}({flight.DepartureTime}) -> {flight.ArrivalAirport}({flight.ArrivalTime}), duration: {flight.Duration}, price: {flight.Price}, tax: {flight.Tax}");
            }
        }

    }
}
