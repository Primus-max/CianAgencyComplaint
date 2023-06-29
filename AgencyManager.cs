using AngleSharp.Dom;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System.Drawing;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace CianAgencyComplaint
{
    public class AgencyManager
    {
        public static List<string> phoneNumbers = new List<string>();
        public static string? curPhoneNumber;

        // Основной метод (точка входа)
        public void RunComplaintProcess(string agencyName)
        {
            // Получаем driver
            IWebDriver _driver = WebDriverFactory.GetDriver();

            // Переходим на страницу агентств
            _driver.Navigate().GoToUrl("https://tomsk.cian.ru/agentstva/?regionId=4620&page=1");
            _driver.Manage().Window.Maximize();

            // Ожидаем полной загрузки страницы
            WaitForDOMReady(_driver);


            // Авторизация
            AuthorizationHelper authHelper = new AuthorizationHelper();
            authHelper.Login(_driver);

            AcceptCookies(_driver);
            // Получаем общее количество страниц
            int totalPages = GetTotalPages(_driver);

            // Ожидаем полной загрузки страницы
            WaitForDOMReady(_driver);

            for (int currentPage = 1; currentPage <= totalPages; currentPage++)
            {
                // Получаем список агентств на текущей странице
                List<IWebElement> agencyCards = _driver.FindElements(By.CssSelector("[data-name='AgencyCard']")).ToList();

                foreach (IWebElement agencyCard in agencyCards)
                {
                    IWebElement? nameElement = null;
                    string? agencyNameText = string.Empty;
                    try
                    {
                        // Получаем название агентства
                        nameElement = agencyCard.FindElement(By.CssSelector("._9400a595a7--name--HPTnh > span"));
                        agencyNameText = nameElement.Text;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }

                    if (agencyNameText.ToLower() == agencyName.ToLower())
                    {
                        // Плавно прокручиваем к агентству и кликаем
                        ScrollToElement(_driver, agencyCard);
                        ClickElement(_driver, agencyCard);


                        SwitchToNewTab(_driver);

                        ClickViewAllOffersLink(_driver);

                        SwitchToNewTab(_driver);

                        //int totalCountForComplaint = GetTotalOffers(_driver);

                        ProcessAllOffers(_driver);


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


        // Получаю и нажимаю на кнопку - Пожаловаться
        private static void ProcessAllOffers(IWebDriver driver)
        {
            // Ожидаем полной загрузки страницы
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

            HashSet<IWebElement> clickedElements = new HashSet<IWebElement>();
            IWebElement paginationElement = null;
            IWebElement lastPageElement;
            IWebElement nextPageElement = null;
            string lastPageText;
            int maxPage;

            IReadOnlyCollection<IWebElement> offerElements = null;

            // Блок на случай если пагинации нет, скорее всего одня страница
            //try
            //{
            //    // Находим элемент с пагинацией
            //    paginationElement = driver.FindElement(By.CssSelector("ul._93444fe79c--pages-list--gsEUE"));
            //    // Находим последний элемент пагинации и получаем текст
            //    lastPageElement = paginationElement.FindElements(By.TagName("li")).Last();
            //    lastPageText = lastPageElement.Text;
            //    // Парсим текст в число, чтобы получить максимальное количество страниц
            //    maxPage = int.Parse(lastPageText);
            //}
            //catch (Exception)
            //{
            //    maxPage = 1;
            //}

            // Цикл для прохода по всем страницам
            while (true)
            {

                try
                {
                    // Получаем элементы с предложениями на текущей странице
                    offerElements = driver.FindElements(By.CssSelector("[data-name='CardComponent']"));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Что то пошло не так с получением элементов {ex.Message}");
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
                        IWebElement complaintButton = driver.FindElement(By.CssSelector("[data-mark='ComplainControl']"));
                        // Изменяем стиль элемента на "display: block" с использованием JavaScript
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].style.display = 'block';", complaintButton);

                        Thread.Sleep(500);
                        complaintButton.Click();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Что то пошло не так с получением кнопки для жалобы {ex.Message}");
                    }

                    // Отправляю жалобу
                    SendComplaint(offerElement, driver);

                    Thread.Sleep(2000);
                }


                try
                {
                    IWebElement nextButton = driver.FindElement(By.CssSelector("a._93444fe79c--button--Cp1dl._93444fe79c--link-button--Pewgf._93444fe79c--M--T3GjF._93444fe79c--button--dh5GL"));

                    // Проверяем, является ли кнопка активной (не disabled)
                    if (!nextButton.GetAttribute("disabled").Equals("disabled"))
                    {
                        // Выполняем клик на кнопке
                        nextButton.Click();
                    }
                    else
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Не получилось получить кнопку - ДАЛЬШЕ {ex.Message}");
                }

                // Ожидаем загрузки новой страницы
                Thread.Sleep(2000);
            }
        }

        // Отправка жалобы
        private static void SendComplaint(IWebElement offerElement, IWebDriver driver)
        {
            ClosePopup(driver);

            IReadOnlyCollection<IWebElement> complaintItems = null;
            IWebElement randomComplaintItem = null;
            IWebElement complaintForm = null;
            IWebElement sendComplaintButton = null;


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
                    Console.WriteLine($"Что то пошло не так с получением варианта жалобы {ex.Message}");
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
                    // Кликаем на элемент
                    randomComplaintItem.Click();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Что то пошло не так с кликом на рандомную причину жалобы {ex.Message}");
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
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Что то пошло не так с отправкой жалобы {ex.Message}");
                    //return false;
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
                IWebElement emailInput = driver.FindElement(By.CssSelector("input[name='email']"));
                EnterRandomEmail(emailInput);
            }
            catch (Exception ex) { }


            // Закрываем Popup
            Actions actions = new Actions(driver);
            actions.SendKeys(Keys.Escape).Perform();
            Thread.Sleep(1000);
            try
            {
                // Проверяем наличие элемента
                IWebElement closeButton = driver.FindElement(By.CssSelector("button._93444fe79c--button--Cp1dl._93444fe79c--button--IqIpq._93444fe79c--XS--Q3OqJ._93444fe79c--button--OhHnj"));

                // Если элемент найден, кликаем на него
                closeButton.Click();
            }
            catch (Exception)
            {
                // Элемент не найден, продолжаем выполнение программы
            }


            // Ожидаем полной загрузки страницы
            Thread.Sleep(5000);
        }

        // Получаю номер телефона 
        private static string GetPhoneNumber(IWebElement offerElement, IWebDriver driver)
        {
            ClosePopup(driver);

            IWebElement phoneValueElement = null;
            try
            {
                // Находим элемент data-mark="PhoneButton"
                IWebElement phoneButtonElement = offerElement.FindElement(By.CssSelector("[data-mark='PhoneButton']"));
                // Изменяем стиль элемента на "display: block" с использованием JavaScript
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].style.display = 'block';", phoneButtonElement);
                // Если атрибут "onclick" отсутствует, выполняем клик на элементе
                Thread.Sleep(500);
                phoneButtonElement.Click();
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
                "john.doe@gmail.com",
                "emma.johnson@yahoo.com"
            };


            // Генерируем случайный индекс для выбора адреса электронной почты из списка
            Random random = new Random();
            int randomIndex = random.Next(0, emailList.Count);
            string randomEmail = emailList[randomIndex];

            // Очищаем поле ввода перед вставкой нового адреса
            emailInput.Clear();

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
            ClosePopup(driver);
            // Ожидаем полной загрузки страницы
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

            // Получаем все открытые вкладки
            IReadOnlyCollection<string> windowHandles = driver.WindowHandles;

            // Переключаемся на последнюю (новую) вкладку
            string newTabHandle = windowHandles.Last();
            driver.SwitchTo().Window(newTabHandle);
        }

        // Получаю число страниц которое надо будет обойти при поиске агенства
        private static int GetTotalPages(IWebDriver driver)
        {
            ClosePopup(driver);
            // Ожидаем полной загрузки страницы
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

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
            ClosePopup(driver);
            // Ожидаем полной загрузки страницы
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

            IWebElement viewAllOffdfdfrsLink = driver.FindElement(By.CssSelector(".serp-list"));


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
            ClosePopup(driver);

            // Ожидаем полной загрузки страницы
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

            // Ищем элемент кнопки "Принять куки"
            IWebElement acceptButton = driver.FindElement(By.CssSelector("button._25d45facb5--button--KVooB._25d45facb5--button--gs5R_._25d45facb5--M--I5Xj6._25d45facb5--button--DsA7r > span._25d45facb5--text--V2xLI"));

            // Проверяем наличие кнопки "Принять куки"
            if (acceptButton != null)
            {
                // Кликаем на кнопку "Принять куки"
                acceptButton.Click();
            }
        }

        private static void WaitForDOMReady(IWebDriver driver)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
        }

        private static void ClickElement(IWebDriver driver, IWebElement element)
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
    }


}
