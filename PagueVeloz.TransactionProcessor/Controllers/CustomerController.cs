using Microsoft.AspNetCore.Mvc;
using PagueVeloz.Application.Common;
using PagueVeloz.Application.Contracts;
using PagueVeloz.Application.Customers;

namespace PagueVeloz.TransactionProcessor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;            
        }

        [HttpPost("CreateCustomer")]
        public async Task<ActionResult<Response<CustomerCreateOutputDto>>> createCustomer(CustomerCreateInputDto input)
        {
            var customers = await _customerService.Create(input);

            return Ok(customers);
        }
    }
}
