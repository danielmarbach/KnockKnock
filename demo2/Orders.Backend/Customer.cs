using System;

namespace Orders.Backend
{
    public class Customer
    {
        private string shortId;
        public Guid CustomerId { get; set; }

        public string ShortId
        {
            get
            {
                if (shortId != null)
                {
                    return shortId;
                }

                var customerId = CustomerId.ToString();
                shortId = customerId.Substring(customerId.Length - 7, 7);
                return shortId;
            }
        }

        public bool GiveDiscount { get; set; }
    }
}