using System.ComponentModel.DataAnnotations;

namespace DBModels
{
    public class Profile
    {
        [Key]
        public string ID { get; set; }
        public long UserType { get; set; }
    }
}
