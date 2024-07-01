using Microsoft.VisualStudio.TestTools.UnitTesting;
using MainApp.windows.adds;
using System.Windows.Controls;
using System.Windows;

namespace MSTest
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void TestTextboxesAreFillable()
        {
            // Arrange
            var window = new ClientsAddWindow();

            // Act
            var lastNameTextBox = window.FindName("LastName_tb") as TextBox;
            var firstNameTextBox = window.FindName("FirstName_tb") as TextBox;
            var middleNameTextBox = window.FindName("MiddleName_tb") as TextBox;
            var phoneTextBox = window.FindName("Phone_tb") as TextBox;
            var emailTextBox = window.FindName("Email_tb") as TextBox;
            var passportTextBox = window.FindName("Passport_tb") as TextBox;

            // Assert
            Assert.IsNotNull(lastNameTextBox);
            Assert.IsNotNull(firstNameTextBox);
            Assert.IsNotNull(middleNameTextBox);
            Assert.IsNotNull(phoneTextBox);
            Assert.IsNotNull(emailTextBox);
            Assert.IsNotNull(passportTextBox);

            lastNameTextBox.Text = "Doe";
            firstNameTextBox.Text = "John";
            middleNameTextBox.Text = "A.";
            phoneTextBox.Text = "1234567890";
            emailTextBox.Text = "john.doe@example.com";
            passportTextBox.Text = "1234567890";

            Assert.AreEqual("Doe", lastNameTextBox.Text);
            Assert.AreEqual("John", firstNameTextBox.Text);
            Assert.AreEqual("A.", middleNameTextBox.Text);
            Assert.AreEqual("1234567890", phoneTextBox.Text);
            Assert.AreEqual("john.doe@example.com", emailTextBox.Text);
            Assert.AreEqual("1234567890", passportTextBox.Text);
        }

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            Application.ResourceAssembly = typeof(MainApp.windows.adds.ClientsAddWindow).Assembly;
        }
    }
}
