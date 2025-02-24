namespace APILoggingElastic.Models
{
    public class LogModel
    {
        public string Application { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public object AdditionalData { get; set; }
        //public Dictionary<string, object> AdditionalData { get; set; }
    }
}
