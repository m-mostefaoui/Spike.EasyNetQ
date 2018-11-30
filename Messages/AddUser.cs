namespace Messages
{
    public class AddUser : IEvent
    {
        public string Username { get; private set; }

        public AddUser(string username)
        {
            Username = username;
        }
    }
}