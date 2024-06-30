using System.ComponentModel.DataAnnotations;

namespace Bluecorp.Models
{
    public class DeliveryAddress
    {
        [Required]
        public string street { get; set; }

        [Required]
        public string city { get; set; }

        [Required]
        public string state { get; set; }

        [Required]
        public string postalCode { get; set; }

        [Required]
        public string country { get; set; }
    }
}