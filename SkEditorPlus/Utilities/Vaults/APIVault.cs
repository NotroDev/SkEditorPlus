using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkEditorPlus.Utilities.Vaults
{
    public static class APIVault
    {
        private static SkEditorAPI instance;

        public static void SetAPIInstance(SkEditorAPI api)
        {
            instance = api;
        }

        public static SkEditorAPI GetAPIInstance()
        {
            return instance;
        }
    }
}