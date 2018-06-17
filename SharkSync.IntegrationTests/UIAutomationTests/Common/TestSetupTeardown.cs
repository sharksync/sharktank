using NUnit.Framework;
using System;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using System.Reflection;
using System.Drawing;
using System.Collections.Generic;

namespace SharkSync.IntegrationTests.UIAutomationTests
{
    [TestFixture("Chrome")]
    //[TestFixture("Firefox")]
    public partial class Tests
    {
        protected IWebDriver driver;

        private string browser;

        public Tests(string browser)
        {
            this.browser = browser;
        }

        [SetUp]
        public void Init()
        {
            string unitTestPath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(Tests)).Location);
            string solutionPath = unitTestPath.Replace("SharkSync.IntegrationTests/bin/Debug/netcoreapp2.0/SharkSync.IntegrationTests.dll", "");

            if (browser == "Chrome")
            {
                string chromePath = null;
#if !DEBUG 
                chromePath = Path.Combine(solutionPath, "node_modules/puppeteer/.local-chromium/linux-564778");
#endif
                var options = new ChromeOptions() { BinaryLocation = chromePath };
                driver = new ChromeDriver(unitTestPath, options);
            }
            else if (browser == "Firefox")
            {
                driver = new FirefoxDriver(unitTestPath);
            }
            else
            {
                throw new Exception("Unsupported browser driver for local");
            }
        }

        [TearDown]
        public void CleanUp()
        {
            driver.Quit();
        }
    }
}
