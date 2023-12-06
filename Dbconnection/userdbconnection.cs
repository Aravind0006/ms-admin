using Microsoft.EntityFrameworkCore;
using ms_admin.model;

namespace ms_admin.Dbconnection
{
    public class userdbconnection : DbContext
    {
        public userdbconnection(DbContextOptions<userdbconnection> options) : base(options)
        {
        }
        public DbSet<Register> Register { get; set; }
        public DbSet<Login> Login { get; set; }
        public DbSet<ProductDto> ProductDtos { get; set; }
       public DbSet<Productaccessories> ProductAccessories {  get; set; }
       public DbSet<refurbishedmobile> refurbishedmobiles { get; set; }
        public DbSet<newsfeed> newsfeeds { get; set; }
    }
}
