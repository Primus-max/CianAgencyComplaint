using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
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
            IWebDriver driver = new ChromeDriver();

            return driver;
        }
    }
}
