using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;
using System.Text.Json;

public class AuthorizationHelper
{
    private string authFilePath = "auth.json"; // Путь к файлу с данными авторизации

    public void Login(IWebDriver driver)
    {
        // Чтение данных авторизации из файла
        AuthData authData = ReadAuthData();

        try
        {
            // Находим кнопку "Войти"
            IWebElement loginButton = driver.FindElement(By.CssSelector("a[href*='/authenticate/']"));
            ClickElement(driver, loginButton);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        // Ждем загрузки DOM
        WaitForDOMReady(driver);

        try
        {
            // Находим кнопку "Другим способом"
            IWebElement switchToEmailButton = driver.FindElement(By.CssSelector("button[data-name='SwitchToEmailAuthBtn']"));
            ClickElement(driver, switchToEmailButton);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        try
        {
            // Получаем поле "Email или id"
            IWebElement usernameField = driver.FindElement(By.CssSelector("input[name='username']"));
            EnterTextWithDelay(usernameField, authData.Id);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        try
        {
            // Находим и кликаем кнопку "Продолжить"
            IWebElement continueButton = driver.FindElement(By.CssSelector("button[data-name='ContinueAuthBtn']"));
            ClickElement(driver, continueButton);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        try
        {
            // Получаем поле "Пароль"
            IWebElement passwordField = driver.FindElement(By.CssSelector("input[name='password']"));
            EnterTextWithDelay(passwordField, authData.Password);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        try
        {
            // Находим и кликаем кнопку "Войти"
            IWebElement loginButton2 = driver.FindElement(By.CssSelector("button[data-name='ContinueAuthBtn']"));
            ClickElement(driver, loginButton2);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private AuthData ReadAuthData()
    {
        string json = string.Empty;
        try
        {
            json = File.ReadAllText(authFilePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Не удалось провитать файл {ex.Message}");
        }
        return JsonSerializer.Deserialize<AuthData>(json);
    }

    private void ClickElement(IWebDriver driver, IWebElement element)
    {
        try
        {
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", element);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private void EnterTextWithDelay(IWebElement element, string text)
    {
        foreach (char c in text)
        {
            element.SendKeys(c.ToString());
            Delay(100); // Задержка для имитации печати
        }
    }

    private void WaitForDOMReady(IWebDriver driver)
    {
        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
    }

    private void Delay(int milliseconds)
    {
        System.Threading.Thread.Sleep(milliseconds);
    }
}

public class AuthData
{
    public string? Id { get; set; }
    public string? Password { get; set; }
}
