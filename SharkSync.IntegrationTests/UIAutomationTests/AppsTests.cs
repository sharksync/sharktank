using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OpenQA.Selenium;
using SeleniumExtras.WaitHelpers;
using SharkSync.PostgreSQL;
using SharkSync.PostgreSQL.Entities;
using System;
using System.Linq;

namespace SharkSync.IntegrationTests.UIAutomationTests
{
    public partial class Tests
    {
        private readonly Guid testAccountId = new Guid("e86606db-c70b-4708-865a-13d0d1e5b827");
        private readonly Guid testAppId = new Guid("28cb6d7f-e981-4a2e-9135-12120e0de4fd");
        private readonly Guid testAccessKey = new Guid("46f2a24e-c127-47c4-9e03-57a72c2291ef");
        private readonly string testAppName = "Static App 1";

        [Test]
        public void AppTests_ListApps()
        {
            ResetTestAppRecords();

            AppHelpers.GotoAppUrl(driver, wait);

            LoginHelpers.SignIsUsingGoogle(driver, wait, secrets);

            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".table")));

            var table = driver.FindElement(By.CssSelector(".table"));
            var rows = table.FindElements(By.TagName("tr"));
            Assert.AreEqual(2, rows.Count);

            var headersCells = rows[0].FindElements(By.TagName("th"));
            Assert.AreEqual(4, headersCells.Count);
            Assert.AreEqual("Name", headersCells[0].Text);
            Assert.AreEqual("App Id", headersCells[1].Text);
            Assert.AreEqual("Access Key", headersCells[2].Text);

            var firstRowCells = rows[1].FindElements(By.TagName("td"));
            Assert.AreEqual(4, firstRowCells.Count);
            Assert.AreEqual(testAppName, firstRowCells[0].Text);
            Assert.AreEqual(testAppId.ToString(), firstRowCells[1].Text);
            Assert.AreEqual(testAccessKey.ToString(), firstRowCells[2].Text);
        }

        [Test]
        public void AppTests_AddApp()
        {
            ResetTestAppRecords();

            AppHelpers.GotoAppUrl(driver, wait);

            LoginHelpers.SignIsUsingGoogle(driver, wait, secrets);

            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".table")));

            driver.FindElement(By.CssSelector(".btn-primary")).Click();

            driver.FindElement(By.Id("newAppName")).SendKeys("New app");

            driver.FindElement(By.CssSelector(".btn-success")).Click();

            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.CssSelector(".btn-success")));

            var table = driver.FindElement(By.CssSelector(".table"));
            var rows = table.FindElements(By.TagName("tr"));
            Assert.AreEqual(3, rows.Count);

            var firstRowCells = rows[2].FindElements(By.TagName("td"));
            Assert.AreEqual(4, firstRowCells.Count);
            Assert.AreEqual("New app", firstRowCells[0].Text);
        }

        [Test]
        public void AppTests_DeleteApp()
        {
            ResetTestAppRecords();

            AppHelpers.GotoAppUrl(driver, wait);

            LoginHelpers.SignIsUsingGoogle(driver, wait, secrets);

            wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".table")));

            driver.FindElement(By.CssSelector(".btn-danger")).Click();

            driver.FindElement(By.CssSelector(".swal2-confirm")).Click();

            wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.CssSelector(".btn-danger")));

            var table = driver.FindElement(By.CssSelector(".table"));
            var rows = table.FindElements(By.TagName("tr"));
            Assert.AreEqual(1, rows.Count);
        }

        public void ResetTestAppRecords()
        {
            using (var db = serviceProvider.GetService<DataContext>())
            {
                var apps = db.Applications.Where(a => a.AccountId == testAccountId);

                db.Applications.RemoveRange(apps);

                db.Applications.Add(new Application
                {
                    AccountId = testAccountId,
                    Id = testAppId,
                    AccessKey = testAccessKey,
                    Name = testAppName
                });

                db.SaveChanges();
            }

        }

    }
}
