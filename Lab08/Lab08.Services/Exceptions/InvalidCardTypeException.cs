namespace Lab08.Services.Exceptions
{
    public class InvalidCardTypeException : ApplicationException
    {
        public InvalidCardTypeException(string cardType)
        {
            this.cardType = cardType;
        }

        public override string ToString()
        {
            return $"The given promotion card {cardType} does not exist.";
        }

        public string cardType { get; set; }
    }
}
