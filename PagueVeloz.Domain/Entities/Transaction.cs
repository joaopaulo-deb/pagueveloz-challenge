using PagueVeloz.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PagueVeloz.Domain.Entities
{
    public class Transaction : BaseEntity
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OperationType Operation { get; private set; }
        public int AccountId { get; private set; }
        public Account Account { get; private set; }
        public int Amount { get; private set; }
        public string currency { get; set; }
        public string ReferenceId { get; set; }
        public string Description { get; set; }

        public Transaction()
        {
            
        }

        public Transaction(OperationType operation, int accountId, int amount, string Currency, string referenceId, string description)
        {
            Operation = operation;
            AccountId = accountId;
            Amount = amount;
            currency = Currency;
            ReferenceId = referenceId;
            Description = description;

        }


    }
}
