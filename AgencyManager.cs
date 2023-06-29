using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using Serilog;
using Serilog.Core;
using System.Drawing;

namespace CianAgencyComplaint
{
    public class AgencyManager
    {
        public static List<string> phoneNumbers = new List<string>();
        public static string? curPhoneNumber;
        public static ILogger? logger = null;

        // Основной метод (точка входа)
        public void RunComplaintProcess(string agencyName)
        {
            // Инициализация логера
            logger = LoggingHelper.ConfigureLogger();

            // Получаем driver
            IWebDriver _driver = WebDriverFactory.GetDriver();

            // Переходим на страницу агентств
            _driver.Navigate().GoToUrl("https://tomsk.cian.ru/agentstva/?regionId=4620&page=1");
            _driver.Manage().Window.Maximize();

            // Ожидаем полной загрузки страницы
            WaitForDOMReady(_driver);

            //Принимаю куки
            AcceptCookies(_driver);

            Thread.Sleep(300);

            // Авторизация
            AuthorizationHelper authHelper = new AuthorizationHelper();
            authHelper.Login(_driver);

            AcceptCookies(_driver);

            // Ожидаем полной загрузки страницы
            WaitForDOMReady(_driver);

            // Получаю и переключась на агенство
            FindAgencyByName(_driver, agencyName);

            IWebElement? agencyCard = null;
            try
            {
                // Получаю список агентств на текущей странице
                agencyCard = _driver.FindElement(By.CssSelector("[data-name='AgencyCard']"));
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Ошибка в получении названия агенства: {ErrorMessage}", ex.Message);
            }


            // Плавно прокручиваем к агентству и кликаем
            ScrollToElement(_driver, agencyCard);

            //WaitForDOMReady(_driver);

            //foundedCard.Click();

            ClickElement(_driver, agencyCard);

            Thread.Sleep(3000);

            SwitchToNewTab(_driver);

            ClickViewAllOffersLink(_driver);

            SwitchToNewTab(_driver);

            ProcessAllOffers(_driver);
        }

        // Метод поиска по названию агенства
        public static void FindAgencyByName(IWebDriver driver, string agencyName)
        {
            IWebElement? inputElement = null;

            try
            {
                // Находим элемент по указанным атрибутам
                inputElement = driver.FindElement(By.CssSelector("input[data-testid='search_text_input']"));
            }
            catch (Exception ex)
            {
                logger.Error($"Не удалось получить поле для ввода названия агенства: {ex.Message} ");
            }

            // Очищаем поле ввода и вводим текст
            ClearAndEnterText(inputElement, agencyName);

            try
            {
                // Выполняем нажатие клавиши Enter или отправку формы
                inputElement.SendKeys(Keys.Enter); // Или inputElement.Submit();
            }
            catch (Exception ex)
            {
                logger.Error($"Не удалось ввести название агенства: {ex.Message} ");
            }
        }

        // Получаю и нажимаю на кнопку - Пожаловаться
        private static void ProcessAllOffers(IWebDriver driver)
        {
            // Ожидаем полной загрузки страницы
            WaitForDOMReady(driver);

            HashSet<IWebElement>? clickedElements = new HashSet<IWebElement>();
            IWebElement? paginationElement = null;
            IWebElement? lastPageElement;
            IWebElement? nextPageElement = null;
            string? lastPageText;
            int maxPage;

            IReadOnlyCollection<IWebElement> offerElements = null;

            while (true)
            {

                try
                {
                    // Получаем элементы с предложениями на текущей странице
                    offerElements = driver.FindElements(By.CssSelector("[data-name='CardComponent']"));
                }
                catch (Exception ex)
                {
                    logger?.Error(ex, "Ошибка в получении CardComponent: {ErrorMessage}", ex.Message);
                }

                // Проходим по каждому элементу и кликаем на него
                foreach (IWebElement offerElement in offerElements)
                {
                    //curPhoneNumber = string.Empty;

                    curPhoneNumber = GetPhoneNumber(offerElement, driver);

                    if (phoneNumbers.Contains(curPhoneNumber))
                    {
                        continue;
                    }

                    ScrollToElement(driver, offerElement);

                    Thread.Sleep(2000);

                    // Наводим курсор на элемент
                    Actions actions = new Actions(driver);
                    actions.MoveToElement(offerElement).Perform();

                    Thread.Sleep(1000);

                    try
                    {
                        IWebElement? complaintButton = driver.FindElement(By.CssSelector("[data-mark='ComplainControl']"));
                        // Изменяем стиль элемента на "display: block" с использованием JavaScript
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].style.display = 'block';", complaintButton);

                        Thread.Sleep(500);
                        ClickElement(driver, complaintButton);
                    }
                    catch (Exception ex)
                    {
                        logger?.Error(ex, "Ошибка в получении кнопки - ПОЖАЛОВАТЬСЯ: {ErrorMessage}", ex.Message);
                    }

                    // Отправляю жалобу
                    SendComplaint(driver);

                    Thread.Sleep(2000);
                }


                try
                {
                    IWebElement? nextButton = driver.FindElement(By.CssSelector("a._93444fe79c--button--Cp1dl._93444fe79c--link-button--Pewgf._93444fe79c--M--T3GjF._93444fe79c--button--dh5GL"));

                    // Проверяем, является ли кнопка активной (не disabled)
                    if (!nextButton.GetAttribute("disabled").Equals("disabled"))
                    {
                        // Выполняем клик на кнопке
                        ClickElement(driver, nextButton);
                    }
                    else
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    logger?.Error(ex, "Ошибка в получении кнопки - ДАЛЬШЕ: {ErrorMessage}", ex.Message);
                }

                // Ожидаем загрузки новой страницы
                Thread.Sleep(2000);
            }
        }

        // Отправка жалобы
        private static void SendComplaint(IWebDriver driver)
        {
            IReadOnlyCollection<IWebElement> complaintItems = null;
            IWebElement? randomComplaintItem = null;
            IWebElement? complaintForm = null;
            IWebElement? sendComplaintButton = null;


            // Ожидаем полной загрузки страницы
            Thread.Sleep(5000);

            while (true)
            {
                try
                {
                    // Получаем все элементы ComplaintItem
                    complaintItems = driver.FindElements(By.CssSelector("[data-name='ComplaintItem']"));
                }
                catch (Exception ex)
                {
                    logger?.Error(ex, "Ошибка в получении ComplaintItem: {ErrorMessage}", ex.Message);
                }

                // Получаем количество элементов ComplaintItem
                int complaintItemCount = complaintItems.Count;

                // Если нет элементов ComplaintItem, выходим из цикла
                if (complaintItemCount == 0)
                {
                    break;
                }


                // Генерируем случайное число от 0 до complaintItemCount - 1
                Random random = new();
                int randomIndex = random.Next(0, complaintItemCount);

                try
                {
                    // Выбираем случайный элемент ComplaintItem
                    randomComplaintItem = complaintItems.ElementAt(randomIndex);
                    // Делаем элемент видимым, установив свойство display в block
                    ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].style.display = 'block';", randomComplaintItem);

                    Thread.Sleep(300);

                    // Кликаем на элемент
                    randomComplaintItem.Click();
                    // ClickElement(driver, randomComplaintItem);
                }
                catch (Exception ex)
                {
                    logger?.Error(ex, "Не удалось отправить жалобу: {ErrorMessage}", ex.Message);
                }

                try
                {
                    // Ожидаем полной загрузки страницы
                    Thread.Sleep(2000);
                    // Находим форму ComplaintItemForm
                    complaintForm = driver.FindElement(By.CssSelector("[data-name='ComplaintItemForm']"));
                    // Находим кнопку отправки внутри формы
                    sendComplaintButton = complaintForm.FindElement(By.CssSelector("button._93444fe79c--button--Cp1dl._93444fe79c--button--IqIpq._93444fe79c--XS--Q3OqJ._93444fe79c--button--OhHnj"));
                    // Выполняем клик на кнопке отправки
                    sendComplaintButton.Click();

                    // При удачной отправке жалобы добавляю телефон
                    if (!string.IsNullOrEmpty(curPhoneNumber))
                    {
                        phoneNumbers.Add(curPhoneNumber);
                        logger?.Information($"Жалоба отправлена на номер: {curPhoneNumber}");
                    }

                }
                catch (Exception ex)
                {
                    logger?.Error(ex, "Не удалось отправить жалобу: {ErrorMessage}", ex.Message);
                }

                // Ожидаем полной загрузки страницы
                Thread.Sleep(7000);
            }

            // Ожидаем полной загрузки страницы
            Thread.Sleep(5000);

            // Вставляю рандомную почту
            try
            {
                // Находим элемент <input> по атрибуту name
                IWebElement? emailInput = driver.FindElement(By.CssSelector("input[name='email']"));
                EnterRandomEmail(emailInput);
            }
            catch (Exception ex)
            {
                logger?.Error(ex, "Не удалось отправить вставить email: {ErrorMessage}", ex.Message);
            }


            // Закрываем Popup
            Actions actions = new Actions(driver);
            actions.SendKeys(Keys.Escape).Perform();
            Thread.Sleep(1000);
            try
            {
                // Проверяем наличие элемента
                IWebElement? closeButton = driver.FindElement(By.CssSelector("button._93444fe79c--button--Cp1dl._93444fe79c--button--IqIpq._93444fe79c--XS--Q3OqJ._93444fe79c--button--OhHnj"));

                // Если элемент найден, кликаем на него
                ClickElement(driver, closeButton);
            }
            catch (Exception) { }


            // Ожидаем полной загрузки страницы
            Thread.Sleep(5000);
        }

        // Получаю номер телефона 
        private static string GetPhoneNumber(IWebElement offerElement, IWebDriver driver)
        {
            ClosePopup(driver);

            IWebElement? phoneValueElement = null;
            try
            {
                // Находим элемент data-mark="PhoneButton"
                IWebElement? phoneButtonElement = offerElement.FindElement(By.CssSelector("[data-mark='PhoneButton']"));

                // Если атрибут "onclick" отсутствует, выполняем клик на элементе
                Thread.Sleep(500);
                ClickElement(driver, phoneButtonElement);
            }
            catch (Exception)
            {

            }
            // Ожидаем появления элемента data-mark="PhoneValue"
            Thread.Sleep(1000);

            try
            {
                phoneValueElement = offerElement.FindElement(By.CssSelector("[data-mark='PhoneValue']"));

            }
            catch (Exception)
            {

            }
            // Получаем текст внутри элемента
            string phoneValueText = phoneValueElement.Text;

            // Возвращаем текст
            return phoneValueText;
        }

        // Закрываю вплывающие окна
        private static void ClosePopup(IWebDriver driver)
        {
            // Закрываем Popup
            Actions actions = new(driver);
            actions.SendKeys(Keys.Escape).Perform();
        }

        // Метод для вставки рандомной почты в поле после отправки жалобы
        private static void EnterRandomEmail(IWebElement emailInput)
        {
            // Список случайных почт
            List<string> emailList = new List<string>()
            {
                "viktorovaalysa@yandex.ru",
                "oskris8@yandex.ru"
            };


            // Генерируем случайный индекс для выбора адреса электронной почты из списка
            Random random = new Random();
            int randomIndex = random.Next(0, emailList.Count);
            string randomEmail = emailList[randomIndex];

            // Очищаем поле ввода перед вставкой нового адреса
            ClearAndEnterText(emailInput, randomEmail);

            // Вводим адрес электронной почты по одной букве
            foreach (char letter in randomEmail)
            {
                emailInput.SendKeys(letter.ToString());
                Thread.Sleep(random.Next(100, 300));  // Добавляем небольшую паузу между вводом каждой буквы
            }
        }


        // Переключаюсь на новую вкладку
        private static void SwitchToNewTab(IWebDriver driver)
        {
            // Ожидаем полной загрузки страницы
            WaitForDOMReady(driver);

            //Если всплыли окна, закрываю
            ClosePopup(driver);

            // Получаем все открытые вкладки
            IReadOnlyCollection<string> windowHandles = driver.WindowHandles;

            // Переключаемся на последнюю (новую) вкладку
            string newTabHandle = windowHandles.Last();
            driver.SwitchTo().Window(newTabHandle);
        }


        // Получаю число страниц которое надо будет обойти при поиске агенства
        private static int GetTotalPages(IWebDriver driver)
        {
            IWebElement? pagesElement = null;
            IWebElement? lastPageElement = null;
            // Ожидаем полной загрузки страницы
            WaitForDOMReady(driver);

            //Если всплыли окна, закрываю
            ClosePopup(driver);

            try
            {
                // Находим последний элемент списка страниц
                pagesElement = driver.FindElement(By.CssSelector("._9400a595a7--items--F8vxh > div:last-child"));
            }
            catch (Exception ex)
            {
                logger?.Error(ex, "Не удалось получить последний элемент в списке страниц: {ErrorMessage}", ex.Message);
            }

            try
            {
                // Получаем элемент <span> с номером последней страницы
                lastPageElement = pagesElement.FindElement(By.TagName("span"));
            }
            catch (Exception ex)
            {
                logger?.Error(ex, "Не удалось получить цифру с номером последней страници: {ErrorMessage}", ex.Message);
            }



            // Получаем текст из элемента <span> и преобразуем его в число
            int totalPages = int.Parse(lastPageElement.Text);

            return totalPages;
        }

        // Скроллинг
        private static void ScrollToElement(IWebDriver driver, IWebElement element)
        {
            // Ожидаем полной загрузки страницы
            WaitForDOMReady(driver);

            // Используем JavaScriptExecutor для выполнения скрипта получения позиции элемента
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

            // Получаем позицию элемента на странице с использованием JavaScript
            var boundingRect = (Dictionary<string, object>)js.ExecuteScript("return arguments[0].getBoundingClientRect();", element);
            var elementLocation = new Point(Convert.ToInt32(boundingRect["x"]), Convert.ToInt32(boundingRect["y"]));


            // Получаем размеры окна браузера
            var windowSize = driver.Manage().Window.Size;

            // Вычисляем вертикальную позицию для прокрутки, чтобы элемент был посередине экрана
            var verticalScrollPosition = elementLocation.Y - windowSize.Height / 2;

            // Выполняем скрипт для прокрутки страницы по вертикали
            js.ExecuteScript($"window.scrollTo(0, {verticalScrollPosition});");

            Thread.Sleep(1000);
        }

        // Кликаю на элементе - показать все предложения этого агенства
        private static void ClickViewAllOffersLink(IWebDriver driver)
        {
            IWebElement? viewAllOffersLink = null;
            // Ожидаем полной загрузки страницы
            WaitForDOMReady(driver);

            //Если всплыли окна, закрываю
            ClosePopup(driver);

            try
            {
                // Находим ссылку "Смотреть все предложения"
                viewAllOffersLink = driver.FindElement(By.CssSelector("[data-ga-action='open_all_offers']"));
                // Прокручиваем страницу к ссылке
                ScrollToElement(driver, viewAllOffersLink);

                // Кликаем на ссылку
                ClickElement(driver, viewAllOffersLink);
            }
            catch (Exception ex)
            {
                logger?.Error(ex, "Не удалось получить или кликнуть на элемент - Смотреть все предложения: {ErrorMessage}", ex.Message);
            }


        }

        // Кликаю на  - принять куки
        private static void AcceptCookies(IWebDriver driver)
        {
            IWebElement? acceptButton = null;
            // Ожидаем полной загрузки страницы
            WaitForDOMReady(driver);

            //Если всплыли окна, закрываю
            ClosePopup(driver);

            try
            {
                // Ищем элемент кнопки "Принять куки"
                acceptButton = driver.FindElement(By.CssSelector("button._25d45facb5--button--KVooB._25d45facb5--button--gs5R_._25d45facb5--M--I5Xj6._25d45facb5--button--DsA7r > span._25d45facb5--text--V2xLI"));

                // Кликаем на кнопку "Принять куки"
                ClickElement(driver, acceptButton);
            }
            catch (Exception) { }
        }

        private static void WaitForDOMReady(IWebDriver driver)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
            wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
        }

        private static void ClickElement(IWebDriver driver, IWebElement element)
        {
            WaitForDOMReady(driver);
            //Thread.Sleep(3000);
            try
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].style.display = 'block';", element);
                Thread.Sleep(300);
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", element);
            }
            catch (Exception ex)
            {
                logger?.Error(ex, "Не удалось кликнуть на элементе: {ErrorMessage}", ex.Message);
            }
        }

        private static void ClearAndEnterText(IWebElement element, string text)
        {
            Random random = new Random();

            // Вводим текст по одному символу
            foreach (char letter in text)
            {
                if (letter == '\b')
                {
                    // Если символ является символом backspace, удаляем последний введенный символ
                    element.SendKeys(Keys.Backspace);
                }
                else
                {
                    // Вводим символ
                    element.SendKeys(letter.ToString());
                }

                Thread.Sleep(random.Next(100, 300));  // Добавляем небольшую паузу между вводом каждого символа
            }
        }

    }


}
