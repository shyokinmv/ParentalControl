namespace ParentalControl.Interfaces
{
    public interface IUserInterface
    {
        public Task SendNotification(string message);

        public string GetComputerName();
    }
}
