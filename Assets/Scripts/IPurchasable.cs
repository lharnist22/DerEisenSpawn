public interface IPurchasable
{
    int Cost { get; }
    string Prompt { get; } // e.g., "Open Door ($750)"
    bool TryPurchase();     // returns true if bought & activated
}
