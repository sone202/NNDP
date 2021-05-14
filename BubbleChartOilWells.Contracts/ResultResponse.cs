namespace BubbleChartOilWells.Contracts
{
    /// <summary>
    /// Service response model.
    /// </summary>
    public class ResultResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public T Data { get; set; }

        public static ResultResponse<T> GetSuccessResponse()
        {
            return new ResultResponse<T> { IsSuccess = true, ErrorMessage = string.Empty, Data = default(T) };
        }

        public static ResultResponse<T> GetSuccessResponse(T data)
        {
            return new ResultResponse<T> { IsSuccess = true, ErrorMessage = string.Empty, Data = data };
        }

        public static ResultResponse<T> GetErrorResponse(string errorMessage)
        {
            return new ResultResponse<T> { IsSuccess = false, ErrorMessage = errorMessage, Data = default(T) };
        }
    }
}
