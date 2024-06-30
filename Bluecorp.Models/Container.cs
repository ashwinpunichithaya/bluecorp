using System.ComponentModel.DataAnnotations;

namespace Bluecorp.Models
{
    public class Container
    {
        [Required]
        public string loadId { get; set; }

        [Required]
        public string containerType { get; set; }

        [Required]
        public List<Item> items { get; set; }
    }
}