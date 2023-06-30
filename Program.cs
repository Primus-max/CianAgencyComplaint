namespace CianAgencyComplaint
{
    class Program
    {
        static Task Main(string[] args)
        {
            int delayHours = GetDelayHours();

            AgencyManager agencyManager = new AgencyManager();
            string agencyName = GetAgencyName();

            while (true)
            {
                agencyManager.RunComplaintProcess(agencyName);

                // Ожидаем заданное время в часах перед следующей итерацией
                Console.WriteLine($"Ожидание перед следующей итерацией: {delayHours} ч.");
                Thread.Sleep(delayHours * 60 * 60 * 1000); // Преобразуем часы в миллисекунды
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