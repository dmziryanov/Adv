using System;

namespace DAL
{
    public class Adv
    {
        public string Currency { get; set; }
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
        public int Country { get; set; }
        public DateTime Created { get; set; }
    }

    public class CarAdv : Adv
    {
        public int MileAge { get; set; }
        public int Year { get; set; }
        public int customs { get; set; }
        public bool guarantee { get; set; }
        public string PTS { get; set; }
        public string VIN { get; set; }
        public int Brand { get; set; }
        public int Model { get; set; }
        public int CarType { get; set; }
        public int CarModel { get; set; }
        
    }
}