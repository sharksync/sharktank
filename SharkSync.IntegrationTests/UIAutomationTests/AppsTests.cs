using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text;
using SeleniumExtras.WaitHelpers;
using OtpNet;

namespace SharkSync.IntegrationTests.UIAutomationTests
{
    public partial class Tests
    {
        [Test]
        public void AppTests_ListApps()
        {
            AppHelpers.GotoAppUrl(driver, wait);

            LoginHelpers.SignIsUsingGoogle(driver, wait, secrets);

            wait.Until(e => ExpectedConditions.InvisibilityOfElementLocated(By.CssSelector(".pageLoading")));

            var table = driver.FindElement(By.CssSelector(".table"));
            var rows = table.FindElements(By.TagName("tr"));
            Assert.AreEqual(1, rows.Count);

            var tds = rows[0].FindElements(By.TagName("td"));
            Assert.AreEqual(1, tds.Count);
            Assert.AreEqual("Static App 1", tds[0].Text);
            Assert.AreEqual("28cb6d7f-e981-4a2e-9135-12120e0de4fd", tds[0].Text);
            Assert.AreEqual("46f2a24e-c127-47c4-9e03-57a72c2291ef", tds[0].Text);
        }

    }
}
