using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkEditorPlus.Functionalities
{
    public interface IFunctionality
    {
        public void onEnable(SkEditorAPI skEditor);
    }
}
