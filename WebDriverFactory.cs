using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace CianAgencyComplaint
{
    public class WebDriverFactory
    {
        public static IWebDriver GetDriver()
        {
            ChromeDriver driver = null!;
            try
            {
                // Создаем экземпляр драйвера
                driver = new ChromeDriver();

                // Установка глобального ожидания
                WebDriverWait wait = new(driver, TimeSpan.FromSeconds(120));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"С драйвером возникли проблемки {ex.Message}");
                Console.WriteLine("Для продолжения нажми любую кнопку");
                Console.ReadKey();
            }

            return driver;
        }

    }
}
