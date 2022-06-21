namespace RESTfullPusher
{
    public interface IDestination
    {
        string Name { get; }
        void Send(dynamic data);
    }
}