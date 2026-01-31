using PagueVeloz.Application.Common;
using PagueVeloz.Application.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PagueVeloz.Application.Transactions
{
    public interface ITransactionService
    {
        Task<TransactionOutputDto> Handle(TransactionInputDto input);
    }
}
