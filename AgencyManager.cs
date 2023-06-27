using OpenQA.Selenium;
using System.Text.RegularExpressions;

namespace CianAgencyComplaint
{
    public class AgencyManager
    {
        private IWebDriver _driver;
        // Основной метод (точка входа)
        public void RunComplaintProcess(string agencyName)
        {
            // Получаем driver
            _driver = WebDriverFactory.GetDriver();

            // Переходим на страницу агентств
            _driver.Navigate().GoToUrl("https://tomsk.cian.ru/agentstva/?regionId=4620&page=1");
            _driver.Manage().Window.Maximize();

            Thread.Sleep(5000);

            AcceptCookies(_driver);
            // Получаем общее количество страниц
            int totalPages = GetTotalPages(_driver);

            for (int currentPage = 1; currentPage <= totalPages; currentPage++)
            {
                // Получаем список агентств на текущей странице
                List<IWebElement> agencyCards = _driver.FindElements(By.CssSelector("[data-name='AgencyCard']")).ToList();

                foreach (IWebElement agencyCard in agencyCards)
                {
                    // Получаем название агентства
                    IWebElement nameElement = agencyCard.FindElement(By.CssSelector("._9400a595a7--name--HPTnh > span"));
                    string agencyNameText = nameElement.Text;

                    if (agencyNameText.ToLower() == agencyName.ToLower())
                    {
                        // Плавно прокручиваем к агентству и кликаем
                        ScrollToElement(_driver, agencyCard);
                        agencyCard.Click();

                        Thread.Sleep(10000);
                        SwitchToNewTab(_driver);




                        //int totalCountForComplaint = GetTotalOffers(_driver);
                        // Здесь можно добавить дополнительную логику для работы с найденным агентством
                        // ...

                        // Выходим из метода, так как наше агентство было найдено
                        return;
                    }
                }

                if (currentPage < totalPages)
                {
                    // Прокручиваем к следующей странице и кликаем
                    IWebElement paginationNext = _driver.FindElement(By.CssSelector("[data-testid='pagination-next']"));
                    ScrollToElement(_driver, paginationNext);
                    paginationNext.Click();

                    // Ожидаем, чтобы страница загрузилась полностью перед продолжением работы
                    Thread.Sleep(2000); // Можно настроить время ожидания по необходимости
                }
            }

            // Если агентство не было найдено на всех страницах, можно добавить соответствующую обработку
        }

        // Переключаюсь на новую вкладку
        private static void SwitchToNewTab(IWebDriver driver)
        {
            // Получаем все открытые вкладки
            IReadOnlyCollection<string> windowHandles = driver.WindowHandles;

            // Переключаемся на последнюю (новую) вкладку
            string newTabHandle = windowHandles.Last();
            driver.SwitchTo().Window(newTabHandle);

            Thread.Sleep(2000);

            ClickViewAllOffersLink(driver);

            int totalCountForComplaint = GetTotalOffers(driver);
        }

        // Получаю число страниц которое надо будет обойти при поиске агенства
        private static int GetTotalPages(IWebDriver driver)
        {
            // Находим последний элемент списка страниц
            IWebElement pagesElement = driver.FindElement(By.CssSelector("._9400a595a7--items--F8vxh > div:last-child"));

            // Получаем элемент <span> с номером последней страницы
            IWebElement lastPageElement = pagesElement.FindElement(By.TagName("span"));

            // Получаем текст из элемента <span> и преобразуем его в число
            int totalPages = int.Parse(lastPageElement.Text);

            return totalPages;
        }

        // Скроллинг
        private static void ScrollToElement(IWebDriver driver, IWebElement element)
        {
            // Используем JavaScriptExecutor для выполнения скрипта прокрутки
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

            // Выполняем скрипт для прокрутки к элементу с использованием smooth behavior
            js.ExecuteScript("arguments[0].scrollIntoView({ behavior: 'smooth' });", element);

            Thread.Sleep(1000);
        }

        // Возвращаю число объявлений на которые надо будет пожаловаться
        private static int GetTotalOffers(IWebDriver driver)
        {
            // Ожидаем, пока загрузится страница с предложениями
            //WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            //wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("div[data-name='SummaryHeader'] h5")));

            // Находим элемент с информацией о количестве предложений
            IWebElement totalOffersElement = driver.FindElement(By.CssSelector("div[data-name='SummaryHeader'] h5"));

            // Получаем текст элемента
            string totalOffersText = totalOffersElement.Text;

            // Извлекаем число из текста
            string numberString = Regex.Match(totalOffersText, @"\d+").Value;

            // Преобразуем число в int и возвращаем
            int totalOffers = int.Parse(numberString);
            return totalOffers;
        }

        // Кликаю на элементе - показать все предложения этого агенства
        private static void ClickViewAllOffersLink(IWebDriver driver)
        {
            // Находим ссылку "Смотреть все предложения"
            IWebElement viewAllOffersLink = driver.FindElement(By.CssSelector("[data-ga-action='open_all_offers']"));

            // Прокручиваем страницу к ссылке
            ScrollToElement(driver, viewAllOffersLink);

            // Кликаем на ссылку
            viewAllOffersLink.Click();
        }

        // Кликаю на  - принять куки
        private static void AcceptCookies(IWebDriver driver)
        {
            // Ищем элемент кнопки "Принять куки"
            IWebElement acceptButton = driver.FindElement(By.CssSelector("button._25d45facb5--button--KVooB._25d45facb5--button--gs5R_._25d45facb5--M--I5Xj6._25d45facb5--button--DsA7r > span._25d45facb5--text--V2xLI"));

            // Проверяем наличие кнопки "Принять куки"
            if (acceptButton != null)
            {
                // Кликаем на кнопку "Принять куки"
                acceptButton.Click();
            }
        }


    }
}
