using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CianAgencyComplaint
{
    public class AgencyManager
    {
        public void RunComplaintProcess(string agencyName)
        {
            // Получаем driver
            IWebDriver _driver = WebDriverFactory.GetDriver();

            // Переходим на страницу агентств
            _driver.Navigate().GoToUrl("https://tomsk.cian.ru/agentstva/?regionId=4620&page=1");

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

                    if (agencyNameText == agencyName)
                    {
                        // Плавно прокручиваем к агентству и кликаем
                        ScrollToElement(_driver, agencyCard);
                        //agencyCard.Click();

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



        private static void ScrollToElement(IWebDriver driver, IWebElement element)
        {
            // Используем JavaScriptExecutor для выполнения скрипта прокрутки
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

            // Выполняем скрипт для прокрутки к элементу с использованием smooth behavior
            js.ExecuteScript("arguments[0].scrollIntoView({ behavior: 'smooth' });", element);
        }


    }
}
