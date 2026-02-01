using Microsoft.AspNetCore.Mvc;
using PagueVeloz.Application.Contracts;
using PagueVeloz.Application.Transactions;

namespace PagueVeloz.TransactionProcessor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost("")]
        public async Task<ActionResult<TransactionOutputDto>> Execute(TransactionInputDto input)
        {
            var transactions = await _transactionService.Handle(input);

            return Ok(transactions);
        }
    }
}
