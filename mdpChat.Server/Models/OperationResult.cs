namespace mdpChat.Server.Models
{
    public class OperationResult
    {
        public bool Successful { get { return ErrorMessage.Length == 0; } }
        public string ErrorMessage { get; set; }  
    }
}