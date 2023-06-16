﻿using SkEditorPlus.Functionalities;
using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using MessageBox = HandyControl.Controls.MessageBox;

namespace SkEditorPlus.Utilities
{
    public class FunctionalityLoader
    {
        public void LoadAll(SkEditorAPI skEditor)
        {
            var types = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(IFunctionality).IsAssignableFrom(t))
                .Where(w => w.IsClass && !w.IsAbstract);
            foreach (var type in types)
            {
                try
                {
                    var funcInstance = (IFunctionality)Activator.CreateInstance(type);
                    funcInstance.OnEnable(skEditor);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error has been occurred while enabling one of functionalities.\n\nError message:\n" + ex.Message,
                        "SkEditor+", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
