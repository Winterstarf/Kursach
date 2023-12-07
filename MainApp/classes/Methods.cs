using RandomStringCreator;

namespace MainApp.classes
{
    internal class Methods
    {
        public string username, password, code;

        public string GenerateCode()
        {
            RandomStringCreator.StringCreator stringCreator = new RandomStringCreator.StringCreator("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*");
            return stringCreator.Get(8);
        }
    }
}
