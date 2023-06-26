using EPM.Mouser.Interview.Data;
using EPM.Mouser.Interview.Models;
using Microsoft.AspNetCore.Mvc;

namespace EPM.Mouser.Interview.Web.Controllers
{
    [Route("api/warehouse")]
    public class WarehouseApi : Controller
    {
        private readonly IWarehouseRepository _warehouseRepository;

        public WarehouseApi(IWarehouseRepository warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }
        /*
         *  Action: GET
         *  Url: api/warehouse/id
         *  This action should return a single product for an Id
         */
        [HttpGet]
        [Route("{id}")]
        public async Task<JsonResult> GetProductAsync(long id)
        {
            return Json(await _warehouseRepository.Get(id));
        }

        /*
         *  Action: GET
         *  Url: api/warehouse
         *  This action should return a collection of products in stock
         *  In stock means In Stock Quantity is greater than zero and In Stock Quantity is greater than the Reserved Quantity
         */
        [HttpGet]
        public async Task<JsonResult> GetPublicInStockProductsAsync()
        {
            Func<Product, bool> ProductsInStock = (product) => product.InStockQuantity > product.ReservedQuantity;
            return Json(await _warehouseRepository.Query(ProductsInStock));
        }

        /*
         *  Action: GET
         *  Url: api/warehouse/order
         *  This action should return a EPM.Mouser.Interview.Models.UpdateResponse
         *  This action should have handle an input parameter of EPM.Mouser.Interview.Models.UpdateQuantityRequest in JSON format in the body of the request
         *       {
         *           "id": 1,
         *           "quantity": 1
         *       }
         *
         *  This action should increase the Reserved Quantity for the product requested by the amount requested
         *
         *  This action should return failure (success = false) when:
         *     - ErrorReason.NotEnoughQuantity when: The quantity being requested would increase the Reserved Quantity to be greater than the In Stock Quantity.
         *     - ErrorReason.QuantityInvalid when: A negative number was requested
         *     - ErrorReason.InvalidRequest when: A product for the id does not exist
        */
        [Route("order")]
        public async Task<JsonResult> OrderItemAsync([FromBody]UpdateQuantityRequest request)
        {
            var response = new UpdateResponse();
            response.Success = true;

            if (request.Quantity < 0)
            {
                response.Success = false;
                response.ErrorReason = ErrorReason.QuantityInvalid;
            }

            var item = await _warehouseRepository.Get(request.Id);
            if (response.Success && item == null)
            {
                response.Success = false;
                response.ErrorReason = ErrorReason.InvalidRequest;
            }
            if (response.Success && (item?.ReservedQuantity + request.Quantity) > item?.InStockQuantity)
            {
                response.Success = false;
                response.ErrorReason = ErrorReason.NotEnoughQuantity;
            }
            if(response.Success)
            {
                item.ReservedQuantity += request.Quantity;
                await _warehouseRepository.UpdateQuantities(item);
            }

            return Json(response);
        }

        /*
         *  Url: api/warehouse/ship
         *  This action should return a EPM.Mouser.Interview.Models.UpdateResponse
         *  This action should have handle an input parameter of EPM.Mouser.Interview.Models.UpdateQuantityRequest in JSON format in the body of the request
         *       {
         *           "id": 1,
         *           "quantity": 1
         *       }
         *
         *
         *  This action should:
         *     - decrease the Reserved Quantity for the product requested by the amount requested to a minimum of zero.
         *     - decrease the In Stock Quantity for the product requested by the amount requested
         *
         *  This action should return failure (success = false) when:
         *     - ErrorReason.NotEnoughQuantity when: The quantity being requested would cause the In Stock Quantity to go below zero.
         *     - ErrorReason.QuantityInvalid when: A negative number was requested
         *     - ErrorReason.InvalidRequest when: A product for the id does not exist
        */
        [Route("ship")]
        public async Task<JsonResult> ShipItemAsync([FromBody]UpdateQuantityRequest request)
        {
            var response = new UpdateResponse();
            response.Success = true;

            if (request.Quantity < 0)
            {
                response.Success = false;
                response.ErrorReason = ErrorReason.QuantityInvalid;
            }

            var item = await _warehouseRepository.Get(request.Id);
            if (response.Success && item == null)
            {
                response.Success = false;
                response.ErrorReason = ErrorReason.InvalidRequest;
            }
            if (response.Success && ((item?.InStockQuantity - request.Quantity) < 0))
            {
                response.Success = false;
                response.ErrorReason = ErrorReason.NotEnoughQuantity;
            }
            if (response.Success)
            {
                item.ReservedQuantity -= request.Quantity;
                if (item.ReservedQuantity < 0) { item.ReservedQuantity = 0; }
                item.InStockQuantity -= request.Quantity;
                await _warehouseRepository.UpdateQuantities(item);
            }
            return Json(response);
        }

        /*
        *  Url: api/warehouse/restock
        *  This action should return a EPM.Mouser.Interview.Models.UpdateResponse
        *  This action should have handle an input parameter of EPM.Mouser.Interview.Models.UpdateQuantityRequest in JSON format in the body of the request
        *       {
        *           "id": 1,
        *           "quantity": 1
        *       }
        *
        *
        *  This action should:
        *     - increase the In Stock Quantity for the product requested by the amount requested
        *
        *  This action should return failure (success = false) when:
        *     - ErrorReason.QuantityInvalid when: A negative number was requested
        *     - ErrorReason.InvalidRequest when: A product for the id does not exist
        */
        [Route("restock")]
        public async Task<JsonResult> RestockItemAsync([FromBody]UpdateQuantityRequest request)
        {
            var response = new UpdateResponse();
            response.Success = true;

            if (request.Quantity < 0)
            {
                response.Success = false;
                response.ErrorReason = ErrorReason.QuantityInvalid;
            }
            var item = await _warehouseRepository.Get(request.Id);
            if (response.Success && item == null)
            {
                response.Success = false;
                response.ErrorReason = ErrorReason.InvalidRequest;
            }
            if (response.Success)
            {
                item.InStockQuantity += request.Quantity;
                await _warehouseRepository.UpdateQuantities(item);
            }

            return Json(response);
        }

        /*
        *  Url: api/warehouse/add
        *  This action should return a EPM.Mouser.Interview.Models.CreateResponse<EPM.Mouser.Interview.Models.Product>
        *  This action should have handle an input parameter of EPM.Mouser.Interview.Models.Product in JSON format in the body of the request
        *       {
        *           "id": 1,
        *           "inStockQuantity": 1,
        *           "reservedQuantity": 1,
        *           "name": "product name"
        *       }
        *
        *
        *  This action should:
        *     - create a new product with:
        *          - The requested name - But forced to be unique - see below
        *          - The requested In Stock Quantity
        *          - The Reserved Quantity should be zero
        *
        *       UNIQUE Name requirements
        *          - No two products can have the same name
        *          - Names should have no leading or trailing whitespace before checking for uniqueness
        *          - If a new name is not unique then append "(x)" to the name [like windows file system does, where x is the next avaiable number]
        *
        *
        *  This action should return failure (success = false) and an empty Model property when:
        *     - ErrorReason.QuantityInvalid when: A negative number was requested for the In Stock Quantity
        *     - ErrorReason.InvalidRequest when: A blank or empty name is requested
        */
        [Route("add")]
        public async Task<JsonResult> AddNewProductAsync([FromBody]Product product)
        {
            var response = new CreateResponse<Product>();
            response.Success = true;
            if (product.InStockQuantity < 0)
            {
                response.Success = false;
                response.ErrorReason = ErrorReason.QuantityInvalid;
            }
            if (response.Success && string.IsNullOrWhiteSpace(product.Name))
            {
                response.Success = false;
                response.ErrorReason = ErrorReason.InvalidRequest;
            }
            if (response.Success)
            {
                response.Model = product;
                response.Model.ReservedQuantity = 0;
                response.Model.InStockQuantity = product.InStockQuantity;
                var productInSystem = await _warehouseRepository.List();
                response.Model.Name = ValidProductName(product.Name.Trim(), productInSystem);
            }

            return Json(response);
        }
        private string ValidProductName(string name, List<Product> productInSystem)
        {
            var validName = name;
            var matchedProduct = productInSystem.Where(x => x.Name == validName);
            if (matchedProduct.Any())
            {
                validName = validName + "x";
                validName = ValidProductName(validName, productInSystem);
            }

            return validName;
        }
    }
}
