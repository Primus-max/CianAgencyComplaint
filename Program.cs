using OpenQA.Selenium;

namespace CianAgencyComplaint
{
    class Program
    {
        static async Task Main(string[] args)
        {
            int delayHours = GetDelayHours();
            string agencyName = GetAgencyName();


            while (true)
            {
                IWebDriver driver = WebDriverFactory.GetDriver();

                Task task = Task.Run(() =>
                {

                    AgencyManager agencyManager = new AgencyManager();
                    agencyManager.RunComplaintProcess(agencyName, driver);
                });

                // Ожидаем заданное время в часах перед следующей итерацией
                Console.WriteLine($"Ожидание перед следующей итерацией: {delayHours} ч.");
                await Task.Delay(delayHours * 60 * 60 * 1000); // Преобразуем часы в миллисекунды
                driver.Quit();
            }
        }


        // Задержка работы программы.
        static int GetDelayHours()
        {
            int delayHours = 0;
            bool isValidInput = false;

            while (!isValidInput)
            {
                Console.WriteLine("Введите время задержки работы программы (в часах):");
                string input = Console.ReadLine();

                if (int.TryParse(input, out delayHours) && delayHours >= 0)
                {
                    isValidInput = true;
                }
                else
                {
                    Console.WriteLine("Некорректный ввод. Время задержки должно быть положительным целым числом. Попробуйте снова.");
                }
            }

            return delayHours;
        }

        // Получаю название агенства
        static string GetAgencyName()
        {
            string agencyName = null;

            while (string.IsNullOrEmpty(agencyName))
            {
                Console.WriteLine("Введите название агентства:");
                agencyName = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(agencyName))
                {
                    Console.WriteLine("Название агентства не может быть пустым. Попробуйте снова.");
                }
            }

            return agencyName;
        }


    }
}