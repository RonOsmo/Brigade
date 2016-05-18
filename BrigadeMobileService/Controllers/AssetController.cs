using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.WindowsAzure.Mobile.Service;
using BrigadeMobileService.DataObjects;
using BrigadeMobileService.Models;

namespace BrigadeMobileService.Controllers
{
    public class AssetController : TableController<Asset>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<Asset>(context, Request, Services);
        }

        // GET tables/Asset
        public IQueryable<Asset> GetAllAsset()
        {
            return Query(); 
        }

        // GET tables/AssetChildren/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public IQueryable<Asset> GetAssetChildren(string id)
        {
            return Query().Where(asset => asset.Id.StartsWith(id + Asset.SEPARATOR));
        }

        // GET tables/Asset/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<Asset> GetAsset(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/Asset/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<Asset> PatchAsset(string id, Delta<Asset> patch)
        {
            var container = patch.GetChangedPropertyNames().Where(propertyName => propertyName == "Container").FirstOrDefault();

            if (!string.IsNullOrEmpty(container))
            {
                throw new System.NotImplementedException("Haven't implemented patch updates for Container yet!");
            }
            return UpdateAsync(id, patch);
        }

        // POST tables/Asset
        public async Task<IHttpActionResult> PostAsset(Asset item)
        {
            item.SetId();
            Asset current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/Asset/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteAsset(string id)
        {
             return DeleteAsync(id);
        }

    }
}