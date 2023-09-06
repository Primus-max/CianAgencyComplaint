using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using Serilog.Core;
using System.Drawing;
using System.Text.RegularExpressions;

namespace CianAgencyComplaint
{
    public class AgencyManager
    {
        public static List<string> phoneNumbers = new List<string>();
        public static string? curPhoneNumber;
        public static string? offerTitleText;
        public static Logger? logger = null;
        public static HashSet<IWebElement>? clickedElements = null;
        public static List<string> offerTitles = new List<string>();


        // Основной метод (точка входа)
        public void RunComplaintProcess(string agencyName, IWebDriver driver)
        {
            // Инициализация логера
            logger = LoggingHelper.ConfigureLogger();

            // Получаем driver
            IWebDriver _driver = driver;

            // Переходим на страницу агентств
            _driver.Navigate().GoToUrl("https://tomsk.cian.ru/agentstva/?dealType=sale&regionId=4620&page=1");
            _driver.Manage().Window.Maximize();

            // Ожидаем полной загрузки страницы
            WaitForDOMReady(_driver);

            //Принимаю куки
            AcceptCookies(_driver);

            Thread.Sleep(300);

            // Авторизация
            AuthorizationHelper authHelper = new AuthorizationHelper();
            authHelper.Login(_driver);

            Thread.Sleep(3000);

            AcceptCookies(_driver);

            // Ожидаем полной загрузки страницы
            WaitForDOMReady(_driver);

            // Получаю и переключась на агенство
            FindAgencyByName(_driver, agencyName);

            Thread.Sleep(3000);
            IWebElement? agencyCard = null;
            try
            {
                // Получаем список агентств на текущей странице
                var agencyCards = _driver.FindElements(By.CssSelector("[data-name='AgencyCard']"));

                // Выбираем первый элемент из списка (если он доступен)
                agencyCard = agencyCards.FirstOrDefault();
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

            // Выбираю категорию Продажа квартир (для выбора всех предложений)
            ClickSaleCategoryLink(_driver);

            //SwitchToNewTab(_driver);

            //ProcessAllOffers(_driver);
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
        private static async void ProcessAllOffers(IWebDriver driver)
        {
            // Ожидаем полной загрузки страницы
            WaitForDOMReady(driver);
            int pageNumber = 2;

            List<IWebElement> offerElements = null;


            try
            {
                // Получаем элементы с предложениями на текущей странице (в выдаче может быть больше одного)
                offerElements = driver.FindElements(By.CssSelector("._93444fe79c--container--Povoi._93444fe79c--cont--OzgVc")).ToList();

            }
            catch (Exception ex)
            {
                logger?.Error(ex, "Ошибка в получении CardComponent: {ErrorMessage}", ex.Message);
                Console.WriteLine($"Не удалось получить offerElements {ex.Message}");
            }

            int curElementCount = 0;
            int totalCountElementOnPage = 28;
            bool IsAllElementsOnOnePage = false;

            HashSet<int> visitedIndices = new HashSet<int>();
            Random random = new Random();

            while (visitedIndices.Count < offerElements.Count)
            {
                int randomIndex;

                do
                {
                    randomIndex = random.Next(offerElements.Count);
                } while (visitedIndices.Contains(randomIndex));

                visitedIndices.Add(randomIndex);
                IWebElement offerElement = offerElements.ElementAt(randomIndex);

                ClosePopup(driver);

                curElementCount++;

                if (offerElements.Count == curElementCount || offerElements.Count < totalCountElementOnPage)
                {
                    PerformPagination(driver, ref pageNumber);

                    Thread.Sleep(2000);
                    offerElements = driver.FindElements(By.CssSelector("._93444fe79c--container--Povoi._93444fe79c--cont--OzgVc")).ToList();
                    visitedIndices.Clear();
                    continue;
                }

                curPhoneNumber = GetPhoneNumber(offerElement, driver);

                try
                {
                    IWebElement offerTitleElement = null;

                    try
                    {
                        offerTitleElement = offerElement.FindElement(By.CssSelector("span[data-mark='OfferTitle']"));
                        offerTitleText = offerTitleElement.Text;
                    }
                    catch (Exception)
                    {
                        driver.Navigate().Refresh();
                        offerElements = driver.FindElements(By.CssSelector("._93444fe79c--container--Povoi._93444fe79c--cont--OzgVc")).ToList();
                        visitedIndices.Clear();
                        continue;
                    }

                    if (offerTitles.Contains(offerTitleText))
                    {
                        continue;
                    }

                    ScrollToElement(driver, offerElement);

                    Thread.Sleep(2000);

                    Actions actions = new Actions(driver);
                    actions.MoveToElement(offerElement).Perform();

                    Thread.Sleep(1000);

                    try
                    {
                        IWebElement complaintButton = driver.FindElement(By.CssSelector("[data-mark='ComplainControl']"));
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].style.display = 'block';", complaintButton);

                        Thread.Sleep(500);
                        ClickElement(driver, complaintButton);
                    }
                    catch (Exception ex)
                    {
                        logger?.Error(ex, "Ошибка в получении кнопки - ПОЖАЛОВАТЬСЯ: {ErrorMessage}", ex.Message);
                    }

                    await SendComplaintAsync(driver, offerElement);

                    Thread.Sleep(2000);
                }
                catch (StaleElementReferenceException)
                {
                    driver.Navigate().Refresh();
                    offerElements = driver.FindElements(By.CssSelector("._93444fe79c--container--Povoi._93444fe79c--cont--OzgVc")).ToList();
                    visitedIndices.Clear();
                    continue;
                }
            }

        }

        // Пагинация
        private static void PerformPagination(IWebDriver driver, ref int pageNumber)
        {
            try
            {
                // Проверка элемента наличие на странице, если нет, то пагинация закончилась
                IWebElement nextButtons = driver.FindElement(By.XPath("//a[contains(@class, '_93444fe79c--button--Cp1dl') and span[text()='Дальше']]"));

                // Получение текущего URL
                string currentUrl = driver.Url;
                string newUrl;

                if (currentUrl.Contains("&p="))
                {
                    // Заменяем значение параметра "&p=" на нужную страницу
                    newUrl = Regex.Replace(currentUrl, @"&p=\d+", "&p=" + pageNumber);
                }
                else
                {
                    // Добавляем новый параметр "&p=" со значением страницы
                    newUrl = currentUrl + "&p=" + pageNumber;
                }

                // Переходим по новому URL
                driver.Navigate().GoToUrl(newUrl);

                WaitForDOMReady(driver);

                pageNumber++;
            }
            catch (Exception)
            {
                // Если возникает исключение, пагинация закончилась
                // Можно выполнить соответствующие действия или просто выйти из метода
            }
        }

        // Отправка жалобы
        private static async Task SendComplaintAsync(IWebDriver driver, IWebElement offerCard)
        {
            IReadOnlyCollection<IWebElement> complaintItems = null;
            IWebElement? complaintForm = null;
            SubComplaintItem? randomSubComplaint = null;


            string jsonFilePath = "ComplaintsAndText.json"; // Путь к JSON файлу
            List<Complaint> complaints = LoadComplaintsFromJson(jsonFilePath);

            while (true)
            {
                // Проверяю открыто ли еще окно с выбором варианта жалобы
                try
                {
                    IWebElement complaintPopup = driver.FindElement(By.CssSelector("div._93444fe79c--window--nAL7V._93444fe79c--window--jN5vA"));
                }
                catch (Exception)
                {
                    // Если окно с вариантами жалоб не удалось получить, то выходим
                    break;
                }

                // Получаем все элементы ComplaintItem
                try
                {
                    complaintItems = driver.FindElements(By.CssSelector("[data-name='ComplaintItem']"));
                }
                catch (Exception ex)
                {
                    logger?.Error(ex, "Ошибка в получении ComplaintItem: {ErrorMessage}", ex.Message);
                    break;
                }

                // Если нет элементов ComplaintItem, выходим из цикла
                if (complaintItems.Count == 0) break;

                // Генерирую случайное число
                Random random = new Random();
                int randomIndex = random.Next(0, complaints.Count);

                // Выбираю случайную жалобу
                Complaint selectedComplaint = complaints[randomIndex];

                // Нахожу такую жалобу на сайте и кликаю
                try
                {
                    IWebElement correspondingComplaintItem = null;

                    foreach (var complaintItem in complaintItems)
                    {
                        var compText = complaintItem.Text;

                        if (complaintItem.Text.Contains(selectedComplaint.MainComplaint))
                        {
                            correspondingComplaintItem = complaintItem;
                            complaintItem.Click();
                            break;
                        }
                    }
                }
                catch (Exception)
                {
                    continue;
                }




                // Выбараю поджалобу
                try
                {
                    List<string> subComplaintsWithSubSubComplaints = new List<string>
                    {
                        "Позвонил. По телефону назвали другие параметры",
                        "Выяснилось при просмотре",
                        "Параметры объявления не соответствуют друг-другу или тексту описания"
                    };

                    // Получаю варианты жалоб с сайта
                    complaintItems = driver.FindElements(By.CssSelector("[data-name='ComplaintItem']"));

                    // Выбираю случайную поджалобу из выбранной жалобы
                    randomSubComplaint = selectedComplaint.SubComplaints[random.Next(selectedComplaint.SubComplaints.Count)];

                    // Нахожу такую поджалобу на сайте и кликаю
                    IWebElement correspondingSubComplaintItem = null;

                    foreach (var subComplaintItem in complaintItems)
                    {
                        var compText = subComplaintItem.Text;

                        if (subComplaintItem.Text.Contains(randomSubComplaint.SubComplaint))
                        {
                            correspondingSubComplaintItem = subComplaintItem;
                            subComplaintItem.Click();

                            if (randomSubComplaint.SubComplaint == "Цена больше, чем указано в объявлении")
                            {
                                // Получаю варианты жалоб с сайта
                                complaintItems = driver.FindElements(By.CssSelector("[data-name='ComplaintItem']"));

                                // Выбираю случайную поджалобу из выбранной жалобы
                                string randomSubSubComplaint = subComplaintsWithSubSubComplaints[random.Next(subComplaintsWithSubSubComplaints.Count)];

                                foreach (var subSubComplaintItem in complaintItems)
                                {
                                    var compTextt = subSubComplaintItem.Text;
                                    if (subSubComplaintItem.Text.Contains(randomSubSubComplaint))
                                    {
                                        subSubComplaintItem.Click();
                                        break;
                                    }
                                }

                                break;
                            }
                        }
                    }
                }
                catch (Exception)
                {

                }


                // Блок для вставки текста жалобы
                try
                {
                    // Ожидаем полной загрузки страницы
                    Thread.Sleep(1500);
                    // Находим форму ComplaintItemForm
                    complaintForm = driver.FindElement(By.CssSelector("[data-name='ComplaintItemForm'] textarea[name='message']"));

                    if (string.IsNullOrEmpty(complaintForm.Text))
                    {
                        // Выбираю случайный текст жалобы из поджалобы
                        string randomText = randomSubComplaint.Texts[random.Next(randomSubComplaint.Texts.Count)];
                        // Вставляю текст в поле
                        ClearAndEnterText(complaintForm, randomText);
                    }
                    complaintForm.Submit();
                    Thread.Sleep(1000);
                }
                catch (Exception)
                {
                    //continue;
                }

                // ВРЕМЕННО! получаю поле для вставки email
                try
                {
                    IWebElement emailInput = driver.FindElement(By.CssSelector("input._93444fe79c--input--MqKSA"));
                    EnterRandomEmail(emailInput);
                }
                catch (Exception)
                {

                }

                // Если есть окно со вставкой email                
                try
                {
                    // Проверяем наличие элемента
                    IWebElement sendButton = driver.FindElement(By.XPath("//button[contains(@class, '_93444fe79c--button--Cp1dl _93444fe79c--button--IqIpq _93444fe79c--XS--Q3OqJ _93444fe79c--button--OhHnj') and .//span[@class='_93444fe79c--text--rH6sj' and text()='Отправить']]"));

                    // Если элемент найден, кликаем на него
                    ClickElement(driver, sendButton);

                    // При удачной отправке жалобы добавляю телефон и заголовок в список
                    if (!string.IsNullOrEmpty(curPhoneNumber))
                    {
                        phoneNumbers.Add(curPhoneNumber);
                        offerTitles.Add(offerTitleText);
                        logger?.Information($"Жалоба отправлена на номер: {curPhoneNumber} || Объект: {offerTitleText}");

                        // Выходим из цикла, т.к. успешно отправили жалобу
                        break;
                    }
                }
                catch (Exception) { }
            }
        }

        // Метод для десериализации JSON
        static List<Complaint> LoadComplaintsFromJson(string jsonFilePath)
        {
            using (StreamReader reader = new StreamReader(jsonFilePath))
            {
                string jsonContent = reader.ReadToEnd();
                List<Complaint> complaints = JsonConvert.DeserializeObject<List<Complaint>>(jsonContent);
                return complaints;
            }
        }

        // Получаю номер телефона 
        private static string GetPhoneNumber(IWebElement offerElement, IWebDriver driver)
        {
            //ClosePopup(driver);

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
            string phoneValueText = phoneValueElement?.Text;

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
        }

        // Переключаюсь на новую вкладку
        private static void SwitchToNewTab(IWebDriver driver)
        {
            Random random = new();
            // Ожидаем полной загрузки страницы
            WaitForDOMReady(driver);

            //Если всплыли окна, закрываю
            ClosePopup(driver);

            // Получаем все открытые вкладки
            IReadOnlyCollection<string> windowHandles = driver.WindowHandles;

            // Переключаемся на последнюю (новую) вкладку
            string newTabHandle = windowHandles.Last();
            driver.SwitchTo().Window(newTabHandle);

            Thread.Sleep(random.Next(300, 700));
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

            // Параметры для плавной анимации прокрутки
            int stepCount = 150; // Количество шагов
            int delayMilliseconds = 50; // Задержка между шагами (в миллисекундах)

            // Выполняем плавную прокрутку страницы
            for (int i = 0; i <= stepCount; i++)
            {
                var yOffset = verticalScrollPosition * i / stepCount;
                js.ExecuteScript($"window.scrollTo(0, {yOffset});");
                Thread.Sleep(delayMilliseconds);
            }
        }


        // Метод перехода (получения) в категорию "Продажа квартир и комнат"    
        public static void ClickSaleCategoryLink(IWebDriver driver)
        {
            try
            {
                // Находим все элементы с классом "serp-list"
                var serpListElements = driver.FindElements(By.ClassName("serp-list"));


                foreach (var serpListElement in serpListElements)
                {
                    // Получаю список категорий (Аренда, продажа и т.д)
                    //try
                    //{
                    //    var profileSubheadingElement = serpListElement.FindElement(By.ClassName("profile__subheading"));
                    //}
                    //catch (Exception)
                    //{

                    //}

                    try
                    {
                        // Получаю ссылку
                        var linkElement = serpListElement.FindElement(By.TagName("a"));

                        // Сохраняем исходный хэндл текущей вкладки
                        string originalHandle = driver.CurrentWindowHandle;

                        // Открываю рубрику
                        linkElement.Click();

                        // Переключаюсь на вкладку если была открыта
                        SwitchToNewTab(driver);

                        // Остальная работа по отправке жалоб
                        ProcessAllOffers(driver);

                        // Переключаемся обратно на исходную вкладку
                        driver.SwitchTo().Window(originalHandle);
                    }
                    catch (Exception)
                    {
                        continue;
                    }


                }
            }
            catch (Exception) { }

        }

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

        // Метод ожидания загрузки DOM дерева
        private static void WaitForDOMReady(IWebDriver driver)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(90));
                wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
            }
            catch (Exception)
            {
                return;
            }
        }

        private static void ClickElement(IWebDriver driver, IWebElement element)
        {
            Random random = new Random();
            WaitForDOMReady(driver);

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

            Thread.Sleep(random.Next(500, 1500));
        }

        // Метод отчистки и вставки текста в Input
        private static void ClearAndEnterText(IWebElement element, string text)
        {
            Random random = new Random();
            // Используем JavaScriptExecutor для выполнения JavaScript-кода
            IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)((IWrapsDriver)element).WrappedDriver;

            // Очищаем поле ввода с помощью JavaScript
            jsExecutor.ExecuteScript("arguments[0].value = '';", element);

            // Вставляем текст по одному символу без изменений
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

                Thread.Sleep(random.Next(50, 150));  // Добавляем небольшую паузу между вводом каждого символа
            }
            Thread.Sleep(random.Next(300, 700));
        }

        public class SubComplaintItem
        {
            public string? SubComplaint { get; set; }
            public List<string>? Texts { get; set; }
        }

        public class Complaint
        {
            public string? MainComplaint { get; set; }
            public List<SubComplaintItem>? SubComplaints { get; set; }
        }


        public class TextsComplaints
        {
            public List<string>? TextComplaint { get; set; }
        }

    }
}
