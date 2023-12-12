using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace FreakyFashion.API;

[Route("api/[controller]")]
[ApiController]
public class PagesController : UmbracoApiController
{
    private readonly UmbracoHelper umbracoHelper;


    public PagesController(UmbracoHelper umbracoHelper)
    {
        this.umbracoHelper = umbracoHelper;
    }



    [HttpGet("init")]
    public object GetInit()
    {
        var home = umbracoHelper.ContentAtRoot().OfType<HomePage>().First();

        var products = home.Descendants<ProductDetailsPage>();

        var response = new
        {
            products = products?.Select(x => new
            {
                name = x.ProductName,
                price = x.ProductPrice,
                description = x.ProductInfo,
                image = x.ProductImage?.Url(),
                link = x.Url()
            }),


            link = home.Link?.Select(x => new
            {
                link = x.Content.Value("link"),
                name = x.Content.Value("name")
            }),



            hero = new
            {
                heading = home.Heading,
                message = home.Message,
                image = home.Image.Url(),
                button = home.Button,
                buttonLink = home.ButtonLink
            },

            spots = home.Spots?.Select(x => new
            {
                heading = x.Content.Value("heading"),
                spotImage = x.Content.Value<IPublishedContent>("spotImage")?.Url(),
                url = x.Content.Value("url")
            }),

        };

        return (response);
    }

    //För att hämta en product
    [HttpGet("products/{slug}")]
    public object GetProduct(string slug)
    {
        var home = umbracoHelper.ContentAtRoot().OfType<HomePage>().First();

        var product = home
            .Descendants<ProductDetailsPage>()
            .FirstOrDefault(x => x.UrlSegment == slug);

        if (product is null)
            return NotFound();

        var result = new
        {
            name = product.ProductName,
            description = product.ProductInfo.ToString(),
            price = product.ProductPrice,
            image = product.ProductImage.Url(),
        };

        return result;
    }
}