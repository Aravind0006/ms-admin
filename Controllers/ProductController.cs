using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ms_admin.Dbconnection;
using ms_admin.model;
using ms_admin.Services;

namespace ms_admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase 
    {
        private readonly userdbconnection _context;
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductService productService, userdbconnection context, ILogger<ProductController> logger)
        {
            _productService = productService;
            _context = context;
            _logger = logger;

        }


        [HttpPost("UploadProduct"), DisableRequestSizeLimit]
        public async Task<IActionResult> UploadProduct([FromForm] ProductUploadModel model, [FromServices] userdbconnection dbContext)
        {
            // Check if the model is valid
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if a file is attached
            if (model.ProductImage == null || model.ProductImage.Length == 0)
            {
                ModelState.AddModelError("ProductImage", "The ProductImage field is required.");
                return BadRequest(ModelState);
            }

            try
            {
                // Check if the user exists in the Register table
                var user = dbContext.Register.SingleOrDefault(u => u.Username == model.UserName);
                if (user == null)
                {
                    // User does not exist, return a response indicating the issue
                    ModelState.AddModelError("UserName", "The specified username does not exist.");
                    return BadRequest(ModelState);
                }

                using (var stream = new MemoryStream())
                {
                    await model.ProductImage.CopyToAsync(stream);

                    var productDto = new ProductDto
                    {
                        ProductName = model.ProductName,
                        ProductComplaint = model.ProductComplaint,
                        UserName = model.UserName,
                        Status = model.Status,
                        ProductImage = stream.ToArray(),
                        UploadDate = DateTime.Now
                    };

                    // Save productDto to the database
                    dbContext.ProductDtos.Add(productDto);
                    dbContext.SaveChanges();
                }

                return Ok(new { Message = "Product uploaded successfully", Username = model.UserName });
            }
            catch (Exception ex)
            {
                // Handle exceptions, log the error, and return an error response
                return StatusCode(500, new { Message = "Internal server error", Error = ex.Message });
            }
        }


        [HttpGet, DisableRequestSizeLimit]
        public IActionResult GetProducts(string username, [FromServices] userdbconnection dbContext)
        {
            // Perform validation or error handling as needed

            // Assuming you have a DbSet<ProductDto> in your DbContext
            var userProducts = dbContext.ProductDtos.Where(p => p.UserName == username).ToList();

            if (userProducts.Count == 0)
            {
                return NotFound($"No products found for the user with username: {username}");
            }

            return Ok(userProducts);
        }
        

       [HttpPut("UpdateProductStatus/{productId}"), DisableRequestSizeLimit]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductDto productDto, IFormFile productImage)
        {
            // Retrieve the existing product from the database
            var existingProduct = await _context.ProductDtos.FindAsync(id);

            if (existingProduct == null)
            {
                return NotFound();
            }

            // Update other properties
            existingProduct.ProductName = productDto.ProductName;
            existingProduct.ProductComplaint = productDto.ProductComplaint;
            existingProduct.UserName = productDto.UserName;
            existingProduct.Status = productDto.Status;

            // Handle the file upload
            if (productImage != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await productImage.CopyToAsync(memoryStream);
                    existingProduct.ProductImage = memoryStream.ToArray();
                }
            }

            _context.ProductDtos.Update(existingProduct);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpGet("user-products"), DisableRequestSizeLimit]
        [Authorize]
        public IActionResult GetUserProducts()
        {
            try
            {
                var loggedInUsername = User.Identity.Name;

                if (string.IsNullOrEmpty(loggedInUsername))
                {
                    return BadRequest("Unable to retrieve logged-in user information.");
                }

                var userProducts = _context.ProductDtos
                    .Where(p => p.UserName == loggedInUsername)
                    .ToList();

                return Ok(userProducts);
            }
            catch (Exception ex)
            {
                // Log the exception for troubleshooting
                _logger.LogError($"Error retrieving user products: {ex}");
                return StatusCode(500, "Internal server error");
            }
        }


        // GET: api/Product/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            var product = await _context.ProductDtos.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // PUT: api/Product/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, ProductDto productDto)
        {

            if (id != productDto.ProductId)
            {
                return BadRequest();
            }

            _context.Entry(productDto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Product/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.ProductDtos.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.ProductDtos.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return _context.ProductDtos.Any(e => e.ProductId == id);
        }


        //////////////////////////////////////product accesoreis ///////////////////////
        
        [HttpGet("GetAccessoriesproduct")]
        public async Task<ActionResult<IEnumerable<Productaccessories>>> GetProducts()
        {
            var products = await _context.ProductAccessories.OrderByDescending(n => n.Id).ToListAsync();
            return Ok(products);
        }

        [HttpPost("Postaccessoriesproducts")]
        public async Task<ActionResult<Productaccessories>> CreateProduct([FromBody] Productaccessories product)
        {
            if (product == null)
            {
                return BadRequest("Product cannot be null");
            }

            // Add the product to the context.
            _context.ProductAccessories.Add(product);

            // Save changes to the database.
            await _context.SaveChangesAsync();

            // Return the newly created product.
            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
        }

        [HttpGet("Getacessoriesby{id}")]
        public async Task<ActionResult<Productaccessories>> GetProductById(int id)
        {
            var product = await _context.ProductAccessories.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        // PUT: api/ProductAccessories/UpdateProduct/5
        [HttpPut("UpdateaccessoriesProduct/{id}")]
        public async Task<IActionResult> UpdateaccessoriesProduct(int id, [FromBody] Productaccessories updatedProduct)
        {
            if (id != updatedProduct.Id)
            {
                return BadRequest("Mismatched Id in the request body and URL");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Entry(updatedProduct).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ProductAccessories.Any(e => e.Id == id))
                {
                    return NotFound("Product not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // 204 No Content
        }

        // DELETE: api/ProductAccessories/DeleteProduct/5
        [HttpDelete("DeleteaccessoriesProduct/{id}")]
        public async Task<IActionResult> DeleteaccessoriesProduct(int id)
        {
            var product = await _context.ProductAccessories.FindAsync(id);

            if (product == null)
            {
                return NotFound("Product not found");
            }

            _context.ProductAccessories.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent(); // 204 No Content
        }

        ////////////////////////////////////////////refurbished mobile /////////////////////////////////////////////

        [HttpGet("Getrefurbishedmobile")]
        public async Task<ActionResult<IEnumerable<refurbishedmobile>>> GetrefurbishedProducts()
        {
            var products = await _context.refurbishedmobiles.OrderByDescending(n => n.Id).ToListAsync();
            return Ok(products);
        }

        [HttpPost("Postrefurbishedmobile")]
        public async Task<ActionResult<refurbishedmobile>> CreaterefurbishedProduct([FromBody] refurbishedmobile product)
        {
            if (product == null)
            {
                return BadRequest("Product cannot be null");
            }

            // Add the product to the context.
            _context.refurbishedmobiles.Add(product);

            // Save changes to the database.
            await _context.SaveChangesAsync();

            // Return the newly created product.
            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
        }

        [HttpGet("Getrefurbishedmobileby{id}")]
        public async Task<ActionResult<refurbishedmobile>> GetrefurbishedProductById(int id)
        {
            var product = await _context.ProductAccessories.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        // PUT: api/RefurbishedMobile/UpdateRefurbishedProduct/5
        [HttpPut("UpdateRefurbishedProduct/{id}")]
        public async Task<IActionResult> UpdateRefurbishedProduct(int id, [FromBody] refurbishedmobile updatedProduct)
        {
            if (id != updatedProduct.Id)
            {
                return BadRequest("Mismatched Id in the request body and URL");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Entry(updatedProduct).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.refurbishedmobiles.Any(e => e.Id == id))
                {
                    return NotFound("Refurbished Mobile not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // 204 No Content
        }

        // DELETE: api/RefurbishedMobile/DeleteRefurbishedProduct/5
        [HttpDelete("DeleteRefurbishedProduct/{id}")]
        public async Task<IActionResult> DeleteRefurbishedProduct(int id)
        {
            var product = await _context.refurbishedmobiles.FindAsync(id);

            if (product == null)
            {
                return NotFound("Refurbished Mobile not found");
            }

            _context.refurbishedmobiles.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent(); // 204 No Content
        }

        ///////////////////////////////////for new feed //////////////////////////////////////////

        [HttpGet("GetNewsfeeds")]
        public async Task<ActionResult<IEnumerable<newsfeed>>> GetNewsfeeds()
        {
            var newsfeeds = await _context.newsfeeds.OrderByDescending(n => n.Id).ToListAsync();
            return Ok(newsfeeds);
        }



        [HttpPost("Postnewsfeed")]
        public async Task<ActionResult<newsfeed>> Createnewsfeed([FromBody] newsfeed product)
        {
            if (product == null)
            {
                return BadRequest("Product cannot be null");
            }

            // Add the product to the context.
            _context.newsfeeds.Add(product);

            // Save changes to the database.
            await _context.SaveChangesAsync();

            // Return the newly created product.
            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
        }

        [HttpGet("Getnewsfeedby{id}")]
        public async Task<ActionResult<refurbishedmobile>> GetnewfeedById(int id)
        {
            var product = await _context.newsfeeds.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        // PUT: api/Newsfeed/UpdateNewsfeed/5
        [HttpPut("UpdateNewsfeed/{id}")]
        public async Task<IActionResult> UpdateNewsfeed(int id, [FromBody] newsfeed updatedNewsfeed)
        {
            if (id != updatedNewsfeed.Id)
            {
                return BadRequest("Mismatched Id in the request body and URL");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Entry(updatedNewsfeed).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.newsfeeds.Any(e => e.Id == id))
                {
                    return NotFound("Newsfeed not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // 204 No Content
        }

        // DELETE: api/Newsfeed/DeleteNewsfeed/5
        [HttpDelete("DeleteNewsfeed/{id}")]
        public async Task<IActionResult> DeleteNewsfeed(int id)
        {
            var newsfeed = await _context.newsfeeds.FindAsync(id);

            if (newsfeed == null)
            {
                return NotFound("Newsfeed not found");
            }

            _context.newsfeeds.Remove(newsfeed);
            await _context.SaveChangesAsync();

            return NoContent(); // 204 No Content
        }

    }
}
