﻿#pragma checksum "..\..\..\..\Windows\GenerateWindow.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "6D019130C90DE8B104F03CB2BC4B0819D12535B3"
//------------------------------------------------------------------------------
// <auto-generated>
//     Ten kod został wygenerowany przez narzędzie.
//     Wersja wykonawcza:4.0.30319.42000
//
//     Zmiany w tym pliku mogą spowodować nieprawidłowe zachowanie i zostaną utracone, jeśli
//     kod zostanie ponownie wygenerowany.
// </auto-generated>
//------------------------------------------------------------------------------

using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Expression.Media;
using HandyControl.Expression.Shapes;
using HandyControl.Interactivity;
using HandyControl.Media.Animation;
using HandyControl.Media.Effects;
using HandyControl.Properties.Langs;
using HandyControl.Themes;
using HandyControl.Tools;
using HandyControl.Tools.Converter;
using HandyControl.Tools.Extension;
using SkEditorPlus;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace SkEditorPlus.Windows {
    
    
    /// <summary>
    /// GenerateWindow
    /// </summary>
    public partial class GenerateWindow : HandyControl.Controls.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 1 "..\..\..\..\Windows\GenerateWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal SkEditorPlus.Windows.GenerateWindow generatorWindow;
        
        #line default
        #line hidden
        
        
        #line 11 "..\..\..\..\Windows\GenerateWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button commandButton;
        
        #line default
        #line hidden
        
        
        #line 13 "..\..\..\..\Windows\GenerateWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button guiButton;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "6.0.9.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/SkEditorPlus;V1.0.0.0;component/windows/generatewindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Windows\GenerateWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "6.0.9.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.generatorWindow = ((SkEditorPlus.Windows.GenerateWindow)(target));
            
            #line 9 "..\..\..\..\Windows\GenerateWindow.xaml"
            this.generatorWindow.KeyUp += new System.Windows.Input.KeyEventHandler(this.OnKey);
            
            #line default
            #line hidden
            return;
            case 2:
            this.commandButton = ((System.Windows.Controls.Button)(target));
            
            #line 11 "..\..\..\..\Windows\GenerateWindow.xaml"
            this.commandButton.Click += new System.Windows.RoutedEventHandler(this.CommandClick);
            
            #line default
            #line hidden
            return;
            case 3:
            this.guiButton = ((System.Windows.Controls.Button)(target));
            
            #line 13 "..\..\..\..\Windows\GenerateWindow.xaml"
            this.guiButton.Click += new System.Windows.RoutedEventHandler(this.GUIClick);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
