using PagueVeloz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PagueVeloz.Application.Transactions.Operations
{
    public interface ISingleAccountOperation : IOperation
    {
        Task<TransactionOutputDto> ExecuteAsync(
            Account account,
            TransactionInputDto dto
        );
    }

}
