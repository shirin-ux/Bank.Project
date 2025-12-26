namespace Karizmah.Provider.Dtos
{
  public  class BaseResponse<T>
    {
        public List<ErrorMessage> errorMessages { get; set; }
        public T data { get; set; }
        public bool isSuccess { get; set; }
    }
    public class ErrorMessage
    {
        public int code { get; set; }
        public string message { get; set; }
    }
}
