namespace Applications.DTO
{
    public struct NotificationEvent
    {
        public readonly string Message;

        public NotificationEvent(string message)
        {
            Message = message;
        }
    }
}