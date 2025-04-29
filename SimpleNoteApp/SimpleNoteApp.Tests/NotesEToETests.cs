using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleNoteApp.NUnitTests
{
    [TestFixture]
    public class NotesApiUITests
    {
        private IWebDriver? _driver;
        private Process? _serverProcess;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // Start the server manually
            _serverProcess = Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "run --project ../../../../SimpleNoteApp/SimpleNoteApp.csproj",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

            // Wait for server to boot
            Thread.Sleep(5000);

            var options = new ChromeOptions();
            options.AddArgument("--headless");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            _driver = new ChromeDriver(options);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _driver?.Quit();

            if (_serverProcess!=null && !_serverProcess.HasExited)
            {
                _serverProcess.Kill();
            }
        }

        [Test]
        public async Task CanCreateNoteThroughStaticPage()
        {
            if (_driver == null)
            {
                Assert.Fail("WebDriver not initialized");
            }

            _driver.Navigate().GoToUrl("http://localhost:5140/");

            var wait = new WebDriverWait(new SystemClock(), _driver, TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(500));

            // --- Before creating a note ---
            var notesBefore = _driver.FindElements(By.CssSelector(".note")).Count;

            // Find input boxes
            var titleInput = wait.Until(driver => driver.FindElement(By.Id("title")));
            var contentInput = wait.Until(driver => driver.FindElement(By.Id("content")));

            string testTitle = "Test Note Title " + Guid.NewGuid();  // Unique title every time!
            string testContent = "Test note content.";

            titleInput.Clear();
            titleInput.SendKeys(testTitle);

            contentInput.Clear();
            contentInput.SendKeys(testContent);

            // Click Create button
            var createButton = wait.Until(driver => driver.FindElement(By.XPath("//button[contains(text(), 'Create')]")));
            createButton.Click();

            // Wait until number of notes increases
            wait.Until(driver => driver.FindElements(By.CssSelector(".note")).Count == notesBefore + 1);

            var notesAfter = _driver.FindElements(By.CssSelector(".note"));

            // Check if any new note contains the new title
            bool noteFound = false;
            foreach (var note in notesAfter)
            {
                if (note.Text.Contains(testTitle) && note.Text.Contains(testContent))
                {
                    noteFound = true;
                    break;
                }
            }

            Assert.That(noteFound, "Newly created note was not found on the page.");
        }


    }
}
