using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using SharkSync.PostgreSQL;
using System;
using System.IO;
using System.Reflection;

namespace SharkSync.IntegrationTests.UIAutomationTests
{
    [TestFixture("Chrome")]
    //[TestFixture("Firefox")]
    public partial class Tests
    {
        public const string TestingUrl = "https://www.testingallthethings.net";
        public const string LoginUrl = TestingUrl + "/Console/Login";
        public const string LoginCompleteUrl = TestingUrl + "/Console/LoginComplete";
        public const string AppsUrl = TestingUrl + "/Console/Apps";

        // Cache this to save round trips every test
        protected static SecretsViewModel secrets;

        protected string browser;
        protected IWebDriver driver;
        protected WebDriverWait wait;

        protected readonly ServiceProvider serviceProvider;
        protected DataContext db;

        public Tests(string browser)
        {
            serviceProvider = DIHelpers.GetServiceProvider();
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
            {
                driver = new ChromeDriver(unitTestPath);
            }
            else if (browser == "Firefox")
            {
                driver = new FirefoxDriver(unitTestPath, new FirefoxOptions { BrowserExecutableLocation = firefoxInstallPath });
            }
            else
            {
                throw new Exception("Unsupported browser driver");
            }

            wait = new WebDriverWait(driver, new TimeSpan(hours: 0, minutes: 0, seconds: 20));

            if (secrets == null)
            {
                var secretsManager = new AmazonSecretsManagerClient(RegionEndpoint.EUWest1);
                var secretTask = secretsManager.GetSecretValueAsync(new GetSecretValueRequest() { SecretId = "arn:aws:secretsmanager:eu-west-1:429810410321:secret:SharkSync-Testing-z8gBv1" });
                secretTask.Wait();

                if (secretTask.Result == null || string.IsNullOrWhiteSpace(secretTask.Result.SecretString))
                {
                    throw new Exception("Missing AWS SecretsManager value for \"SharkSync-Testing\" secret");
                }

                secrets = JsonConvert.DeserializeObject<SecretsViewModel>(secretTask.Result.SecretString);
            }
        }

        [TearDown]
        public void CleanUp()
        {
            if (driver != null)
            {
                driver.Quit();
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
        public string GoogleTwoFactorCode { get; set; }
        public string MicrosoftUsername { get; set; }
        public string MicrosoftPassword { get; set; }
    }
}
