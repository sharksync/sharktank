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
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Newtonsoft.Json;
using Amazon;

namespace SharkSync.IntegrationTests.UIAutomationTests
{
    [TestFixture("Chrome")]
    //[TestFixture("Firefox")]
    public partial class Tests
    {
        private const string TestingUrl = "https://www.testingallthethings.net";
        private const string LoginUrl = TestingUrl + "/Console/Login";
        private const string LoginCompleteUrl = TestingUrl + "/Console/LoginComplete";
        private const string AppsUrl = TestingUrl + "/Console/Apps";

        protected string browser;
        protected IWebDriver driver;
        protected WebDriverWait wait;

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

            wait = new WebDriverWait(driver, new TimeSpan(hours: 0, minutes: 0, seconds: 20));
        }

        [TearDown]
        public void CleanUp()
        {
            if (driver != null)
                driver.Quit();
        }

        private SecretsViewModel _secrets;
        protected SecretsViewModel Secrets
        {
            get
            {
                if (_secrets == null)
                {
                    var secretsManager = new AmazonSecretsManagerClient(RegionEndpoint.EUWest1);
                    var secretTask = secretsManager.GetSecretValueAsync(new GetSecretValueRequest() { SecretId = "arn:aws:secretsmanager:eu-west-1:429810410321:secret:SharkSync-Testing-z8gBv1" });
                    secretTask.Wait();

                    if (secretTask.Result == null || string.IsNullOrWhiteSpace(secretTask.Result.SecretString))
                        throw new Exception("Missing AWS SecretsManager value for \"SharkSync-Testing\" secret");

                    _secrets = JsonConvert.DeserializeObject<SecretsViewModel>(secretTask.Result.SecretString);
                }

                return _secrets;
            }
        }
    }

    public class SecretsViewModel
    {
        public string GithubUsername { get; set; }
        public string GithubPassword { get; set; }
        public string GithubTwoFactorCode { get; set; }
        public string GoogleUsername { get; set; }
        public string GooglePassword { get; set; }
        public string MicrosoftUsername { get; set; }
        public string MicrosoftPassword { get; set; }
    }
}
