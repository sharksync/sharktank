﻿using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Text;
using SeleniumExtras.WaitHelpers;

namespace SharkSync.IntegrationTests.UIAutomationTests
{
    public partial class Tests
    {
        //[Test]
        //public void LoginTests_Github()
        //{
        //    driver.Navigate().GoToUrl(TestingUrl);

        //    driver.FindElement(By.CssSelector(".navbar-nav")).Click();
        //    wait.Until(ExpectedConditions.UrlToBe(LoginUrl));

        //    driver.FindElement(By.CssSelector(".github-login")).Click();
        //    wait.Until(ExpectedConditions.UrlContains("https://github.com/login"));

        //    driver.FindElement(By.Id("login_field")).SendKeys(Secrets.GithubUsername);
        //    driver.FindElement(By.Id("password")).SendKeys(Secrets.GithubPassword);
        //    driver.FindElement(By.CssSelector("input[type=submit]")).Click();

        //    try
        //    {
        //        wait.Until(ExpectedConditions.UrlToBe(AppsUrl));
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Ended on URL: " + driver.Url, ex);
        //    }
        //}

        //[Test]
        //public void LoginTests_Google()
        //{
        //    driver.Navigate().GoToUrl(TestingUrl);

        //    driver.FindElement(By.CssSelector(".navbar-nav")).Click();
        //    wait.Until(ExpectedConditions.UrlToBe(LoginUrl));

        //    driver.FindElement(By.CssSelector(".google-login")).Click();
        //    wait.Until(ExpectedConditions.UrlContains("https://accounts.google.com/signin"));

        //    driver.FindElement(By.CssSelector("input[type=email]")).SendKeys(Secrets.GoogleUsername);
        //    driver.FindElement(By.Id("identifierNext")).Click();

        //    wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[type=password]")));
        //    driver.FindElement(By.CssSelector("input[type=password]")).SendKeys(Secrets.GooglePassword);
        //    driver.FindElement(By.Id("passwordNext")).Click();

        //    try
        //    {
        //        wait.Until(ExpectedConditions.UrlToBe(AppsUrl));
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Ended on URL: " + driver.Url, ex);
        //    }
        //}

        //[Test]
        //public void LoginTests_Microsoft()
        //{
        //    driver.Navigate().GoToUrl(TestingUrl);

        //    driver.FindElement(By.CssSelector(".navbar-nav")).Click();
        //    wait.Until(ExpectedConditions.UrlToBe(LoginUrl));

        //    driver.FindElement(By.CssSelector(".microsoft-login")).Click();
        //    wait.Until(ExpectedConditions.UrlContains("https://login.microsoftonline.com"));

        //    driver.FindElement(By.CssSelector("input[type=email]")).SendKeys(Secrets.MicrosoftUsername);
        //    driver.FindElement(By.CssSelector("input[type=submit]")).Click();

        //    wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("input[type=password]")));
        //    driver.FindElement(By.CssSelector("input[type=password]")).SendKeys(Secrets.MicrosoftPassword);
        //    driver.FindElement(By.CssSelector("input[type=submit]")).Click();

        //    try
        //    {
        //        wait.Until(ExpectedConditions.UrlToBe(AppsUrl));
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Ended on URL: " + driver.Url, ex);
        //    }
        //}
    }
}
