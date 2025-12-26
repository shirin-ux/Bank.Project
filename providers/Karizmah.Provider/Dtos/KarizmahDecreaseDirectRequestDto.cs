namespace Karizmah.Provider.Dtos
{
    public sealed class KarizmahDecreaseDirectRequestDto
    {
        public decimal amount { get; set; }
        public long policyId { get; set; }
        public long traceId { get; set; }
        public string? description { get; set; }
        public string? phoneNumber { get; set; }//بستگی به بزینس دارد
        //اگر سرویس گیرنده نیاز به رمز یکبار مصرف نداشته باشد این اختیاری است 
        public string bankAccountNumber { get; set; }
    }
}
