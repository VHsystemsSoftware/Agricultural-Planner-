using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VHS.OPC.Models
{
	public class OPCModelAttribute : Attribute
	{
		public string Name { get; set; }
	}
}
