using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace FreakyFashionUmbraco.API;

[Route("api/[controller]")]


public class ProductsController : UmbracoApiController
{
    private readonly IMediaService mediaService;
    private readonly IPublishedContentQuery publishedContentQuery;
    private readonly IContentService contentService;
    public ProductsController(IPublishedContentQuery publishedContentQuery,
        IContentService contentService, IMediaService mediaService)
    {
        this.publishedContentQuery = publishedContentQuery;
        this.contentService = contentService;
        this.mediaService = mediaService;

    }

    
    [HttpGet]
    public IEnumerable<ProductDto> GetProducts()
    {

        var productDetailsPages = publishedContentQuery
            .ContentAtRoot()
            .First()
            .Descendants<ProductDetailsPage>();

        var products = productDetailsPages.Select(x => new ProductDto()
        {
            Name = x.ProductName,
            Image = x.ProductImage?.Url(),
            Info = x.ProductInfo,
            Price = x.ProductPrice,
           

        });

        return products;
    }

    // Lägg till ny produkt
    [HttpPost]
    public ActionResult CreateProduct(CreateProductRequest createProductRequest)
    {
        //var products = publishedContentQuery.ContentSingleAtXPath("//productsDetailsPage");

        var products = publishedContentQuery.ContentAtRoot().First().Descendant<ProductPage>();

        var content = contentService.Create(
          name: createProductRequest.Name,
          parentId: products.Key,
          documentTypeAlias: ProductDetailsPage.ModelTypeAlias);



        content.SetValue("productName", createProductRequest.Name);
        content.SetValue("productInfo", createProductRequest.Info);
        content.SetValue("productPrice", createProductRequest.Price);

        var imageProperty = mediaService.GetMediaByPath(createProductRequest.Image);

        if (imageProperty != null)
        {
            
            content.SetValue("productImage", imageProperty.GetUdi());
        }

        contentService.SaveAndPublish(content);

        return Created("", null);
    }

    public class CreateProductRequest
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public string Info { get; set; }
        public string Price { get; set; }
    }

    [HttpGet("search")]
    public ActionResult<IEnumerable<ProductDto>> SearchProducts(string q)
    {
        if (string.IsNullOrEmpty(q))
        {
            return Ok(new List<ProductDto>());
        }

        var products = GetProducts();

        var searchResults = products.Where(p =>
            p.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
            p.Info.Contains(q, StringComparison.OrdinalIgnoreCase)
            );


        return Ok(searchResults);
    }
}


public class ProductDto
{
    public string Image { get; set; }
    public string Name { get; set; }

    public string Info { get; set; }

    public string Price { get; set; }

}

