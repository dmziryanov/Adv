using System;

namespace DAL
{
    public class AccountModel
    {
        public RegisterModel User { get; set; }
        public int ViewCount { get; set; }
        public int FavoriteCount { get; set; }
        public int AdvCount { get; set; }
        public DateTime LastLogin { get; set; }
    }
}