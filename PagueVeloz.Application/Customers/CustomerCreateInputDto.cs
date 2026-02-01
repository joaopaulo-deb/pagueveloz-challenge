using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PagueVeloz.Application.Customers
{
    public class CustomerCreateInputDto
    {
        [Required]
        public string ClientId { get; set; }
    }
}
