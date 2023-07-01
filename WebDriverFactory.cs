using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;

namespace CianAgencyComplaint
{
    public class WebDriverFactory
    {
        public static IWebDriver GetDriver()
        {
            // Проверяем наличие драйвера браузера и скачиваем его автоматически, если он отсутствует
            new DriverManager().SetUpDriver(new ChromeConfig());

            // Создаем экземпляр драйвера
            ChromeDriver driver = new ChromeDriver();

            // Установка глобального ожидания
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(120));

            return driver;
        }

    }
}
