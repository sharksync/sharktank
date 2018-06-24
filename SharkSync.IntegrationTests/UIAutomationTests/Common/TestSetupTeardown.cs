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
    [TestFixture("Firefox")]
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
            string unitTestPath = null;
            string firefoxInstallPath = null;
#if DEBUG
            unitTestPath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(Tests)).Location);
            firefoxInstallPath = @"C:\Program Files\Mozilla Firefox\firefox.exe";
#endif
            if (browser == "Chrome")
                driver = new ChromeDriver(unitTestPath);
            else if (browser == "Firefox")
                driver = new FirefoxDriver(unitTestPath, new FirefoxOptions { BrowserExecutableLocation = firefoxInstallPath });
            else
                throw new Exception("Unsupported browser driver");
        }

        [TearDown]
        public void CleanUp()
        {
            if (driver != null)
                driver.Quit();
        }
    }
}
