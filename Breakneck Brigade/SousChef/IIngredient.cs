using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public interface IIngredient
    {
        IngredientType Type { get; set; }
        int Cleanliness { get; set; }
    }
}
