using System.ComponentModel.DataAnnotations;

namespace Bluecorp.Models
{
    public class DispatchEvent
    {
        [Required]
        public int controlNumber { get; set; }

        [Required]
        public string salesOrder { get; set; }

        [Required]
        public List<Container> containers { get; set; }

        [Required]
        public DeliveryAddress deliveryAddress { get; set; }
    }
}