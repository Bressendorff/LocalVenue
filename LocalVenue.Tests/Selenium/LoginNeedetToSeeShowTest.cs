using LocalVenue.Tests.Helpers;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace LocalVenue.Tests.Selenium;

public class LoginNeedetToSeeShowTest : IDisposable
{
    public IWebDriver driver { get; private set; }
    public IDictionary<String, Object> vars { get; private set; }
    public IJavaScriptExecutor js { get; private set; }
    private string baseUrl = "https://localvenue-webapp-frbr0006.azurewebsites.net";

    public LoginNeedetToSeeShowTest()
    {
        driver = new ChromeDriver();
        js = (IJavaScriptExecutor)driver;
        vars = new Dictionary<String, Object>();
    }

    public void Dispose()
    {
        driver.Quit();
    }

    [IgnoreOnBuildServerFact]
    public void TestLoginNeededToSeeShow()
    {
        driver.Navigate().GoToUrl(baseUrl);
        driver.Manage().Window.Size = new System.Drawing.Size(1936, 1048);

        // Wait until the element is visible and clickable
        WebDriverWait wait = new(driver, TimeSpan.FromSeconds(10));
        var firstShowLink = wait.Until(
            ExpectedConditions.ElementToBeClickable(By.CssSelector("#home-shows-grid .col"))
        );

        firstShowLink.Click();

        wait.Until(d => d.Url.Contains("/Account/Login"));

        Assert.Contains($"{baseUrl}/Account/Login", driver.Url);
    }
}
