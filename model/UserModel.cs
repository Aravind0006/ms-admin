using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace ms_admin.model
{
    public class Register
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public long Mobile { get; set; }
    }

    public class Login
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public long Mobile { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class ProductDto
    {
        [Key]
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductComplaint { get; set; }
        public byte[]? ProductImage { get; set; }
        public string UserName { get; set; }
        public string Status { get; set; }
        public DateTime UploadDate { get; set; }
    }

    public class ProductUploadModel
    {
        [Required]
        public string ProductName { get; set; }

        [Required]
        public string ProductComplaint { get; set; }

        [Required(ErrorMessage = "The ProductImage field is required.")]
        public IFormFile ProductImage { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Status { get; set; }
        public DateTime UploadDate { get; set; }
    }


    ///////////////////////////////////accessories model / //////////////////////////////////////////

    public class Productaccessories
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string ProductImage { get; set; }
        public string Starrating { get; set; }
        public int oldprice {  get; set; }
        public int newprice { get; set; }

    }

    ///////////////////////////////for mobile upload /////////////////
    
    public class refurbishedmobile
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string ProductImage { get; set; }
    }

    ///////////////////////////////for news feed  //////////////////////////////
    
    public class newsfeed
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string ProductImage { get; set; }
        public string? url {  get; set; }
    }
}
