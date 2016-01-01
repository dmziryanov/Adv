using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DAL
{
    public class AdvPhoto
    {
        [Key, Column(Order = 0)]
        public int PhotoId { get; set; }

        [Key, Column(Order = 1)]
        public int AdvId { get; set; }
    }
}