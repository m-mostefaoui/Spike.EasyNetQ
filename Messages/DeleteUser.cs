namespace Messages
{
    public class DeleteUser : IEvent
    {
        public string Username { get; private set; }

        public DeleteUser(string username)
        {
            Username = username;
        }
    }
}