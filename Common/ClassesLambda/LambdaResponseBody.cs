namespace Common.ClassesLambda
{
    public class LambdaResponseBody
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public DateTimeOffset DateStart { get; set; }
        public DateTimeOffset DateEnd { get; set; }

        public LambdaResponseBody() 
        {
            Success = false;
            Message = string.Empty;
            DateStart = DateTimeOffset.Now;
            DateEnd = DateTimeOffset.MinValue;
        }

        public void SetSuccess(bool success, string message)
        {
            Success = success; 
            Message = message;
            DateEnd = DateTimeOffset.Now;
        }
    }
}
