using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PagueVeloz.Application.Contracts;
using PagueVeloz.Application.Accounts;
using PagueVeloz.Application.Common;

namespace PagueVeloz.TransactionProcessor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("")]
        public async Task<ActionResult<Response<AccountCreateOutputDto>>> create([FromBody] AccountCreateInputDto input)
        {
            var accounts = await _accountService.Create(input);

            return Ok(accounts);
        }
    }
}
