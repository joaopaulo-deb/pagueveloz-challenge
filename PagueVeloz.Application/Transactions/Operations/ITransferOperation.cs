using PagueVeloz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PagueVeloz.Application.Transactions.Operations
{
    public interface ITransferOperation : IOperation
    {
        Task<TransactionOutputDto> ExecuteAsync(
            Account sourceAccount,
            Account destinationAccount,
            TransactionInputDto dto
        );
    }

}
