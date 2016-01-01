namespace DAL
{
    public class Adv
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public double Price { get; set; }

        public int Location { get; set; }

        public int Category { get; set; }

        public bool Negotiable { get; set; }

        public int Condition { get; set; }
        public int type { get; set; }
        public int SubCategory { get; set; }
        public int IsFeatured { get; set; }

        public string SellerName { get; set; }
        public string SellerPhone { get; set; }

        public string SellerEmail { get; set; }

        public string SellerDistrict { get; set; }
        public int? SellerId { get; set; }
    }
}